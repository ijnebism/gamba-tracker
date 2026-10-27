# Backend — GambaTracker API

## Project Overview

The REST API, auth and persistence for Gamba Tracker. See the root `CLAUDE.md` for the overall design.

Responsibilities:

- **Auth**: email + password (done), Google login (planned). Issues JWTs. Per-user API keys for external sources.
- **CRUD**: folders (nested), trackers and events with their drops, all scoped to the logged-in user.
- **Ingest**: an endpoint that external sources (APIs, mods) post events to. It auto-creates any missing folder/tracker in the given path.

Metrics (item rates) and the Recent tab are computed on the frontend from the event list and locally stored history, not by the backend.

### Solution layout

- `GambaTracker.Api` — web host
  - `Program.cs` — DI registration, JWT and API key auth setup, DbUp migrations on startup
  - `Authentication/` — API key auth handler and key hashing
  - `Controllers/` — API controllers
  - `Services/` — business logic and app services (e.g. `TokenService`)
  - `Scripts/` — DbUp SQL migrations (embedded resources, run in filename order)
- `GambaTracker.Core` — domain, no infrastructure dependencies
  - `Models/` — DB entities (User, Folder, Tracker, Event, Drop, ApiKey)
  - `DTOs/` — request/response shapes
  - `Interfaces/` — repository contracts
- `GambaTracker.Data` — data access
  - `DapperContext.cs` — creates PostgreSQL connections
  - `Repositories/` — Dapper implementations of `Core/Interfaces`

Dependency direction: Api → Data → Core. Core references nothing.

### Current features

**Database**: DbUp migrations `001`–`005` create the `users`, `folders`, `trackers`, `events` and `drops` tables. Child rows `ON DELETE CASCADE` from their parent.

**Nested folders** (`006_add_folder_parent.sql`)

- `folders.ParentFolderId` is nullable. Null means the folder is at the user's root.
- Deleting a folder cascades to its subfolders, trackers, events and drops.
- Folder names are unique within their parent (and within the user's root), so a path like `Skyblock/Dungeons` resolves to exactly one folder.
- Tracker names are unique within their folder (`007_unique_tracker_name.sql`).

**Auth** (`AuthController`, `TokenService`)

- `POST /api/auth/register` creates a user with a BCrypt-hashed password and returns `AuthResponse(Token, Email, Name)`. It rejects duplicate emails.
- `POST /api/auth/login` checks the email and password and returns the same `AuthResponse`.
- JWTs carry the user id (`ClaimTypes.NameIdentifier`), email and name. They expire after `Jwt:ExpiryHours`.
- `User.GetUserId()` (`Api/Extensions/ClaimsPrincipalExtensions.cs`) reads the caller's id in `[Authorize]` endpoints. It works for both JWT and API key requests.
- Swagger's "Authorize" button accepts a JWT and an API key.

**API keys** (`008_create_api_keys.sql`, `Authentication/`, `ApiKeyController` → `Services/ApiKeyService.cs`)

- Keys look like `gt_<43 random chars>`. Only a SHA-256 hash and an 11-character display prefix are stored. The full key is returned once, at creation.
- `ApiKeyAuthenticationHandler` (scheme `ApiKey`) reads the `X-Api-Key` header, resolves the key's user and updates `LastUsedAt`.
- JWT stays the default scheme. Plain `[Authorize]` endpoints don't accept API keys; only endpoints that name the `ApiKey` scheme do (currently just ingest).
- Key management is JWT-only, so a leaked key can't create or revoke keys.

| Method | Route               | Action                                                    |
| ------ | ------------------- | --------------------------------------------------------- |
| GET    | `/api/apikey`       | List the caller's keys (prefix, created, last used; never the key) |
| POST   | `/api/apikey`       | Create (`name`). 201 with the full `key`, shown only this once |
| DELETE | `/api/apikey/{id}`  | Revoke. 204, 404 if not the caller's                      |

**Ingest** (`IngestController` → `Services/IngestService.cs` → `Data/Repositories/IngestRepository.cs`, DTOs in `Core/DTOs/IngestDto.cs`)

`POST /api/ingest` accepts an `X-Api-Key` header or a JWT, and returns 201 with `{ folderId, trackerId, event }`.

Body: `{ "path": "Skyblock/Dungeons/F7", "tracker": "Chest drops", "timestamp": "...", "drops": [{ "itemName": "Wheat", "quantity": 1 }] }`.

- `path` is the folder path from the user's root, split on `/`. Segments are trimmed and empty ones ignored. It needs 1–20 segments of at most 100 characters each, otherwise 400.
- `timestamp` is optional and defaults to the time the request arrives. Drops follow the same rules as event create.
- One transaction finds or creates each folder, then the tracker, then inserts the event and drops. It uses `INSERT ... ON CONFLICT DO NOTHING` plus a `SELECT` against the unique name indexes, so repeated or concurrent requests for the same path reuse the same folders and tracker.
- `Data/EventWriter.cs` is the shared event + drops insert used by both `EventRepository` and `IngestRepository`.

**Repositories** (`GambaTracker.Data/Repositories`): async CRUD for all models, plus `IngestRepository`, all registered in DI.

- User: get by id/email, list all, create, update.
- Folder: every method takes or checks the user id. `GetChildFoldersAsync(userId, parentFolderId)` lists one level, where a null parent means root.
- Tracker: get by id and delete check the user through the tracker's folder. List-by-folder and update expect the caller to have checked ownership first.
- Event:
  - Get by id and delete check the user through tracker → folder.
  - `CreateEventAsync` inserts the event and all its `Drops` in one transaction.
  - `GetEventsWithDropsAsync(trackerId, from, to)` loads events newest first with their drops, using two queries.
- Drop: get by id, list by event, create, update, delete. Not scoped to the user; only reached through an event that's already been checked.
- ApiKey: get by id, list and delete are scoped to the user. Get by hash is only for the auth handler.
- `Get*ByIdAsync` returns `null` when the row doesn't exist.

**Services** return `OperationResult<T>` (`Services/OperationResult.cs`) for writes: `Success`, `NotFound` or `NameTaken`. Controllers map these to 2xx, 404 or 409. A unique-index violation (`PostgresErrorCodes.UniqueViolation`) is what produces `NameTaken`.

**Folder endpoints** (`FolderController` → `Services/FolderService.cs`, DTOs in `Core/DTOs/FolderDto.cs`). All require `[Authorize]`, and another user's folder returns 404.

| Method | Route               | Action                                                                      |
| ------ | ------------------- | --------------------------------------------------------------------------- |
| GET    | `/api/folder/root`  | Root contents: `FolderContentsResponse` with `folder: null` and root folders |
| GET    | `/api/folder/{id}`  | The folder, its child folders and its trackers (one call per Browse screen)  |
| POST   | `/api/folder`       | Create (`name`, optional `parentFolderId`). 201, 404 bad parent, 409 duplicate name |
| PUT    | `/api/folder/{id}`  | Rename (`name`). 409 on duplicate name                                      |
| DELETE | `/api/folder/{id}`  | Delete the folder and everything inside it. 204                             |

**Tracker endpoints** (`TrackerController` → `Services/TrackerService.cs`, DTOs in `Core/DTOs/TrackerDto.cs`). All require `[Authorize]`, and a tracker in another user's folder returns 404. A folder's trackers are listed by `GET /api/folder/{id}`.

| Method | Route                | Action                                                              |
| ------ | -------------------- | ------------------------------------------------------------------- |
| GET    | `/api/tracker/{id}`  | Get a tracker                                                       |
| POST   | `/api/tracker`       | Create (`name`, `folderId`). 201, 404 unknown folder, 409 duplicate name |
| PUT    | `/api/tracker/{id}`  | Rename (`name`). 409 on duplicate name                              |
| DELETE | `/api/tracker/{id}`  | Delete the tracker and its events and drops. 204                    |

Folder and tracker names are trimmed and limited to 100 characters. Blank names return 400.

**Event endpoints** (`EventController` → `Services/EventService.cs`, DTOs in `Core/DTOs/EventDto.cs`). All require `[Authorize]`, and an event or tracker of another user returns 404.

| Method | Route                                         | Action                                                              |
| ------ | --------------------------------------------- | ------------------------------------------------------------------- |
| GET    | `/api/tracker/{trackerId}/events?from=&to=`   | The tracker's events, newest first, each with its drops. `from`/`to` are optional and inclusive |
| POST   | `/api/tracker/{trackerId}/events`             | Create an event with its drops. 201, 404 unknown tracker            |
| GET    | `/api/event/{id}`                             | One event with its drops                                            |
| DELETE | `/api/event/{id}`                             | Delete the event and its drops. 204                                 |

Create body: `{ "timestamp": "2026-10-28T10:00:00Z", "drops": [{ "itemName": "Wheat", "quantity": 1 }] }`.

- `timestamp` is a `DateTimeOffset` and is stored as UTC. Responses return UTC (`...Z`).
- At least one drop is required. `quantity` must be 1 or more, and `itemName` is required and limited to 100 characters (trimmed). Breaking a rule returns 400.
- Drops in a response are sorted by item name.

`UserController` exists but has no endpoints yet.

### In progress

Next on the roadmap: Google login.

- The event list has no paging, only the `from`/`to` range. Add paging if trackers get large.
- There's no endpoint to edit an event or its drops.
- There's no endpoint to move a tracker to another folder.
- There's no endpoint to move a folder to a new parent. If one is added, it must reject moving a folder into itself or one of its descendants.
- Google login is not implemented.
- Ingest has no rate limiting.

## Tech Stack

- .NET 10, ASP.NET Core (controllers)
- PostgreSQL via Dapper + Npgsql
- DbUp for migrations
- JWT bearer auth, BCrypt for password hashing, custom `X-Api-Key` scheme for external sources
- Swashbuckle (Swagger) in Development

## Implementation conventions

- Keep nullable reference types on. Avoid `!` unless justified.
- Follow good practices and conventions, ensure that changes can be humanly readable.
- Follow the existing structure and naming:
  - The interface goes in `Core/Interfaces`, the implementation in `Data/Repositories`, and it's registered as scoped in `Program.cs`.
  - Controllers use `[Route("api/[controller]")]` and `[ApiController]`, with dependencies via constructor injection.
- Keep business logic out of controllers. Once it's more than a single repository call, move it to `Services/`.
- Controllers take and return DTOs, never raw models.
- Validate external input (especially ingest payloads) at the controller boundary.
- Data access:
  - Parameterised SQL only (`@Param`), never string-concatenated values.
  - Async throughout.
  - One connection per method via `DapperContext.CreateConnection()`.
- Prefer existing packages and patterns before adding new ones.
- Keep changes focused on the requested task.

## Commands

Run from `backend/`:

- `dotnet build GambaTracker.sln`
- `dotnet run --project GambaTracker.Api`
- `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<conn>" --project GambaTracker.Api`
- Swagger UI: `/swagger` (Development only)

## Rules

- Schema changes only through a new numbered script `Scripts/NNN_<description>.sql`. Never edit an applied script.
- The `Jwt:Key` in `appsettings.json` is temporary. Never commit real secrets or connection strings.
- Every endpoint that touches user data is `[Authorize]` and scoped to the caller's user id.

## Git

Follow the Git rules in the root `CLAUDE.md`. An optional scope is fine, e.g. `feat(backend):`.
