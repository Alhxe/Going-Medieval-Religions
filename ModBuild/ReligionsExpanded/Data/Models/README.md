# Modular game definitions

Each subfolder corresponds to a Going Medieval repository file. The build script (`tools/build-mod.ps1`) merges every `*.json` in a subfolder into the single repository file the game expects.

| Folder | Compiles to |
|---|---|
| `religions/` | `ReligionConfig.json` |
| `buildings/` | `BaseBuildingRepository.json` |
| `items/` | `Resources.json` |
| `rooms/` | `RoomTypes.json` |
| `research/` | `Research.json` |
| `events/` | `GameEventSettingsRepository.json` |
| `stats/` | `StatsRepository.json` (only if creating custom stats) |

## Conventions

- One feature per file (e.g. `crosses.json` holds floor cross + wall crucifix; `pews.json` holds pew variants).
- Each file's root is `{ "repository": [ ... ] }` so the build can concatenate `repository` arrays directly.
- IDs use `snake_case` and are prefixed by religion when applicable: `cross_floor_christian`, `altar_christian`.
- All user-visible strings reference localization keys (see [`docs/translation-guide.md`](../../../../docs/translation-guide.md)).
