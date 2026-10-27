# Gamba Tracker

## Project Overview

An item drop tracker, mainly designed for Hypixel Skyblock item drops. It receives game events from external sources, files them into folders and trackers, and shows metrics on them.

- **Platforms**: web, iOS and Android, from a single Expo codebase.
- **Event sources**: backend APIs we call (primary) and possibly Minecraft mods.
- **Auto-filing**: each incoming event names its folder/tracker path. If the path doesn't exist, the missing folders and tracker are created and start filling.
- **Auth**: email + password, or Google login.
- **Metrics**: item rates, e.g. wheat per minute.

### Domain model

```
User
└── Folder              (folders can contain other folders)
    └── Tracker
        └── Event       (timestamp)
            └── Drop    (item name + quantity, e.g. 1 wheat, 3 seeds)
```

- Backend: `Models/` (DB shape) and `DTOs/` (API shape) in `GambaTracker.Core`.
- Frontend: matching TypeScript types.

### Repo map

- `backend/` — ASP.NET Core Web API. See `backend/CLAUDE.md`.
- `frontend/gamba-tracker-client/` — Expo app. See `frontend/gamba-tracker-client/CLAUDE.md`.

Each sub-folder's CLAUDE.md loads automatically when working in that folder.

### How it links together

```
Expo app -----------+
                    +-- HTTP/JSON + JWT --> Backend API (/api/<controller>) -- Dapper --> PostgreSQL
Ingest source ------+
(APIs / mods)
```

- Clients talk to the backend only via the REST API at `/api/<controller>`.
- Clients log in via the auth endpoints, receive a JWT, and send it as `Authorization: Bearer <token>`.
- The backend owns the database. Nothing else touches it directly.

## Tech Stack

| Layer    | Stack                                                        |
| -------- | ------------------------------------------------------------ |
| Frontend | Expo SDK 56, React Native 0.85, React 19.2, TypeScript       |
| Backend  | ASP.NET Core on .NET 10 (controllers)                        |
| Data     | PostgreSQL, Dapper + Npgsql, DbUp migrations                 |
| Auth     | JWT bearer tokens, BCrypt password hashing, Google (planned) |

## Implementation conventions

- An API contract change (route or DTO) updates backend and frontend in the same change.
- Domain names stay identical across layers: Folder, Tracker, Event, Drop.
- Follow the existing project structure and naming conventions of each side.
- Prefer existing dependencies and patterns before adding new ones.
- Keep changes focused on the requested task and readable by a human.

## Commands

Run the full stack locally:

1. Start PostgreSQL and set the API connection string (see `backend/CLAUDE.md`).
2. `dotnet run --project backend/GambaTracker.Api` — DbUp applies migrations on startup.
3. `cd frontend/gamba-tracker-client && npm start`

## Rules

- Never commit secrets, real connection strings or production keys.
- Never edit a database migration that has already been applied. Add a new one.
- Ask before adding a dependency or changing the data model.
- Don't change the other half of the repo unless the change crosses the API contract.

## Git

- Main branch: `main`. Use a feature branch for larger work.
- Conventional commit prefixes: `feat:`, `fix:`, `chore:`, `docs:`, `refactor:`. An optional scope is fine, e.g. `feat(backend):`.
- Small, focused commits. Don't mix unrelated changes.
