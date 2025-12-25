# Repository Guidelines

## Project Structure & Module Organization
- `Assets/` holds all game content. Project-specific scenes live in `Assets/Scenes/`, settings in `Assets/Settings/`, and tilemap content in `Assets/TileMap/`.
- Third-party art and scripts are vendored under `Assets/Assets/` (e.g., `Cainos/`, `Hero Knight - Pixel Art/`). Avoid editing vendor content unless necessary.
- Unity project configuration lives in `ProjectSettings/` and package dependencies in `Packages/manifest.json`.

## Build, Test, and Development Commands
- Open the project in Unity Hub (this is a Unity project; no CLI build scripts are defined).
- Use **File > Build Settings** in the Unity Editor to build for your target platform.
- Use **Window > General > Test Runner** to run EditMode/PlayMode tests (Unity Test Framework is installed).

## Coding Style & Naming Conventions
- C# scripts should follow Unity conventions: PascalCase class names, matching file names (e.g., `PlayerController.cs`).
- Indentation: 4 spaces; avoid tabs. Keep methods short and focused on single behaviors.
- Keep `.meta` files alongside assets; do not delete or rename assets without their `.meta` pair.

## Testing Guidelines
- The Unity Test Framework package is present, but no tests are committed yet.
- New tests should go under `Assets/Tests/` with separate `EditMode` and `PlayMode` folders.
- Name tests with clear behavior intent, e.g., `PlayerMovementTests.cs`.

## Commit & Pull Request Guidelines
- Git history only contains `init commit`, so there is no established commit style. Use short, imperative summaries (e.g., `Add jump buffering`), and include scope in the body if helpful.
- PRs should include: a concise summary, linked issues (if any), and screenshots or GIFs for visual changes (scenes, sprites, UI).
- Note any asset imports, package changes, or configuration edits in the PR description.

## Configuration Tips
- Prefer project-level settings in `ProjectSettings/` over per-user settings.
- If you add packages, update `Packages/manifest.json` and commit any generated lock files Unity creates.
