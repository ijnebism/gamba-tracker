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

**Auth** (`AuthController`, `TokenService`)

- `POST /register` creates a user with a BCrypt-hashed password and returns `AuthResponse(Token, Email, Name)`. It rejects duplicate emails.
- `POST /login` checks the email and password and returns the same `AuthResponse`.
- JWTs carry the user id (`ClaimTypes.NameIdentifier`), email and name. They expire after `Jwt:ExpiryHours`.

**Repositories** (`GambaTracker.Data/Repositories`): full async CRUD for all five models.

- User: get by id/email, list all, create, update.
- Folder, Tracker, Event, Drop: get by id, list by parent, create, update, delete.

**Endpoints**: all require `[Authorize]`. The controllers are written, but their repositories aren't registered in DI yet (see In progress).

| Method | Route                                 | Action                    |
| ------ | ------------------------------------- | ------------------------- |
| GET    | `/api/folder`                         | List the caller's folders |
| GET    | `/api/folder/{id}`                    | Get a folder              |
| POST   | `/api/folder`                         | Create a folder (`Name`)  |
| GET    | `/api/folder/{folderId}/tracker`      | List a folder's trackers  |
| GET    | `/api/folder/{folderId}/tracker/{id}` | Get a tracker             |
| POST   | `/api/folder/{folderId}/tracker`      | Create a tracker (`Name`) |
| GET    | `/api/tracker/{trackerId}/event`      | List a tracker's events   |
| GET    | `/api/tracker/{trackerId}/event/{id}` | Get an event              |
| POST   | `/api/tracker/{trackerId}/event`      | Create an event (`Date`)  |

`UserController` exists but has no endpoints yet. There are no drop endpoints yet.

### In progress

- Nested folders: `Folder` and `002_create_folders.sql` have no `ParentFolderId` yet. This needs a new migration.
- `AuthController` lacks `[Route("api/[controller]")]` / `[ApiController]`, so its routes resolve to `/login` and `/register` instead of `/api/auth/...`.
- Google login is not implemented. `User.GoogleId` is non-nullable in C# but nullable in the database.
- Only `IUserRepository` is registered in DI so far. The Folder, Tracker and Event controllers fail to resolve until their repositories are registered in `Program.cs`.
- `FolderController.GetFolder` takes `id` as a `string` and looks up the folder with the caller's `userId` instead of `id`.
- The Tracker and Event endpoints don't check that the parent folder or tracker belongs to the caller.
- Events are created without drops. There's no drop endpoint, and nothing creates a drop together with its event.

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
