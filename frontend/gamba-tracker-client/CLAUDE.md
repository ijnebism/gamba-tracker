@AGENTS.md

# Frontend — Gamba Tracker Client

## Project Overview

The Gamba Tracker client for web, iOS and Android. See the root `CLAUDE.md` for the overall design and how the client connects to the backend.

### Navigation

A bottom tab bar with three tabs:

- **Recent**
  - The most recently opened trackers. Tracked on the device (no backend endpoint).
  - A dynamic search.
- **Browse**
  - The user's folder tree, starting from root.
  - Header: a back button (top left), the current folder's name, and an ellipsis menu with sort, new folder and delete folder.
- **Profile**
  - Settings and account info.
  - Notifications.

### Other screens

- **Login**: email + password, or Google.
- **Tracker detail**: the tracker's events and metrics, e.g. item rates per minute. Metrics are computed in the app from `GET /api/tracker/{id}/events`; the backend has no metrics endpoint.
- **API keys** (likely under Profile): create, list and revoke keys for mods and scripts that post to `/api/ingest`. A new key is only shown once, so display it with a copy button.

### Current features

Currently it is in the default React Native Expo code.

## Tech Stack

- Expo SDK 56, React Native 0.85, React 19.2, TypeScript 6.
- react-native-web for the web target.
- Navigation: _TBD (likely Expo Router tabs)_.
- State / data fetching: _TBD_.

## Implementation conventions

- Use TypeScript and avoid `any` unless justified.
- Follow good practices and conventions, ensure that changes can be humanly readable.
- Follow existing project structure and naming conventions.
- Keep business logic separate from screen components.
- Validate external API responses at the integration boundary.
- Prefer existing dependencies and patterns before adding new ones.
- Keep changes focused on the requested task.

Project specifics:

- Screens stay thin. API calls and business logic live in separate modules (e.g. `src/api/`, `src/hooks/`). Record the final folder layout here once it exists.
- API response types mirror the backend DTOs (Folder, Tracker, Event, Drop).
- Functional components with hooks. Styles via `StyleSheet.create`.

## Commands

- `npm start` — Expo dev server
- `npm run android` / `npm run ios` / `npm run web`
- `npx tsc --noEmit` — type check

## Rules

- Read the Expo v56 docs before using any Expo API (see AGENTS.md).
- Never hardcode the API base URL, tokens or credentials.
- Any `any` needs a comment explaining why.

## Git

Follow the Git rules in the root `CLAUDE.md`. An optional scope is fine, e.g. `feat(frontend):`.
