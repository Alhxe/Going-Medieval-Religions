# Contributing

Thanks for considering a contribution.

## Repository layout

See [`docs/architecture.md`](docs/architecture.md) for the full layout and conventions.

Short version:

- `Assets/` — Unity project (asset sources, AddressableBuilder).
- `ModBuild/ReligionsExpanded/` — the mod itself: JSON definitions, localization, compiled bundles.
- `tools/` — build and validation scripts.
- `docs/` — design docs, modding references.

## Adding a new religion

See [`docs/adding-a-religion.md`](docs/adding-a-religion.md).

## Translating

See [`docs/translation-guide.md`](docs/translation-guide.md).

## Conventions

- **JSON style**: 4-space indentation, trailing newline, lowercase `snake_case` IDs.
- **Modular files**: one feature per file (e.g. `crosses.json`, `pews.json`). The build script merges them.
- **No vanilla overrides** unless strictly necessary; prefer additive entries.
- **Commits**: Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`...).
- **Branches**: `main` is stable. Feature branches `feat/<name>`.

## Build & test

```powershell
pwsh tools/build-mod.ps1
```

This validates JSON, merges modular files, and copies the result to `Documents/Foxy Voxel/Going Medieval/Mods/ReligionsExpanded/`. Then launch Going Medieval and enable the mod.

## Pull requests

- One feature per PR.
- Update `CHANGELOG.md` under `[Unreleased]`.
- Include screenshot or short description of in-game behavior.
