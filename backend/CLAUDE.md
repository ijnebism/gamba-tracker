# Backend — GambaTracker API

## Project Overview

The REST API, auth and persistence for Gamba Tracker. See the root `CLAUDE.md` for the overall design.

Responsibilities:

- **Auth**: email + password (done), Google login (planned). Issues JWTs.
- **CRUD**: folders (nested), trackers and events with their drops, all scoped to the logged-in user.
- **Ingest**: an endpoint that external sources (APIs, mods) post events to. It auto-creates any missing folder/tracker in the given path.
- **Metrics**: queries such as item rate per minute for a tracker.

### Solution layout

- `GambaTracker.Api` — web host
  - `Program.cs` — DI registration, JWT setup, DbUp migrations on startup
  - `Controllers/` — API controllers
  - `Services/` — business logic and app services (e.g. `TokenService`)
  - `Scripts/` — DbUp SQL migrations (embedded resources, run in filename order)
- `GambaTracker.Core` — domain, no infrastructure dependencies
  - `Models/` — DB entities (User, Folder, Tracker, Event, Drop)
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
- `User.GetUserId()` (`Api/Extensions/ClaimsPrincipalExtensions.cs`) reads the caller's id in `[Authorize]` endpoints.
- Swagger has an "Authorize" button for pasting a JWT.

**Repositories** (`GambaTracker.Data/Repositories`): async CRUD for all five models, all registered in DI.

- User: get by id/email, list all, create, update.
- Folder: every method takes or checks the user id. `GetChildFoldersAsync(userId, parentFolderId)` lists one level, where a null parent means root.
- Tracker: get by id and delete check the user through the tracker's folder. List-by-folder and update expect the caller to have checked ownership first.
- Event, Drop: get by id, list by parent, create, update, delete. Not yet scoped to the user.
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

`UserController` exists but has no endpoints yet.

### In progress

Following the roadmap in order: 5 events with drops → 6 metrics → 7 ingest + API keys → 8 recent trackers → 9 Google login.

- Event and Drop repository queries aren't scoped to the user yet. Ownership checks come with Step 5's endpoints.
- There's no endpoint to move a tracker to another folder.
- There's no endpoint to move a folder to a new parent. If one is added, it must reject moving a folder into itself or one of its descendants.
- Google login is not implemented.

## Tech Stack

- .NET 10, ASP.NET Core (controllers)
- PostgreSQL via Dapper + Npgsql
- DbUp for migrations
- JWT bearer auth, BCrypt for password hashing
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
