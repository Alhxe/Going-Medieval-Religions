# Religions Expanded

Religion mod framework for [Going Medieval](https://store.steampowered.com/app/1029780/) — adds new religions with custom structures, items and rituals. Currently includes Christianity.

> **Status**: alpha (0.1.0). Setting up scaffolding.

## Features (planned)

### Christianity (first religion)

- **Buildings**: floor cross, wall crucifix, pews, grand altar, confessional, stained glass, religious paintings (multiple variants), bell tower.
- **Items**: bible, portable crucifix, priest robe (visual + charisma bonus), consecrated wine.
- **Rooms**: church (mid tier), cathedral (unlocks dormant vanilla `advanced_chapel_christian`), bell tower.
- **Rituals**: Sunday mass, baptism, confession, hourly bell chime.
- **Research**: cathedral architecture, liturgy.
- **Localization**: English, Spanish (more welcome — see [translation guide](docs/translation-guide.md)).

### Framework

The mod is built so adding more religions is mostly a matter of dropping new JSON files. See [`docs/adding-a-religion.md`](docs/adding-a-religion.md).

## Install

> The mod is not yet released. These are the dev-time install instructions.

1. Build the mod (see below).
2. Launch Going Medieval → menu → **Mods** → enable **Religions Expanded**.

## Build

Requirements:

- Unity Editor **2022.3.46f1** with **Windows Build Support (IL2CPP)**.
- PowerShell 7+.
- Going Medieval **0.23.3** installed.

```powershell
# Compile assets in Unity:
#   1. Open this project in Unity Hub.
#   2. Going Medieval menu → AddressableBuilder → Build.
#
# Then merge JSONs and deploy:
pwsh tools/build-mod.ps1
```

The script validates JSON, merges modular files, and copies the result to `Documents/Foxy Voxel/Going Medieval/Mods/ReligionsExpanded/`.

## Repo layout

See [`docs/architecture.md`](docs/architecture.md).

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md). Translations especially welcome.

## License

[MIT](LICENSE).
