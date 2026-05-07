# Religions Expanded

A religion framework mod for [Going Medieval](https://store.steampowered.com/app/1029780/). Adds new buildings, items, rooms, research and rituals around the existing religions, and provides the runtime hooks for adding more religions in future versions.

> **Status**: alpha (0.2.0). JSON content in place; BepInEx plugin renames the two vanilla religions and lays the groundwork for a multi-religion selector.

## What's in 0.2.0

### JSON content (Christianity expansion)
- **Buildings**: floor cross, wall crucifix, wooden pew, padded pew, grand altar, confessional, bell tower, three religious paintings (Christ / Mary / Saint), three stained-glass windows.
- **Items**: bible, portable crucifix, consecrated wine.
- **Rooms**: church (mid-tier), cathedral (top-tier — replaces the dormant vanilla `advanced_chapel_christian`), bell tower.
- **Research**: `liturgy` (unlocks the church and most decorations) and `cathedral_architecture` (unlocks cathedral, grand altar, stained glass, bell tower). Both wired into the vanilla research tree under `religious_structures_lvl3` via `nextNodesIDs` override.
- **Religion config**: appends `grand_altar_christian` to the Christian shrine list and registers `church_christian` / `cathedral_christian` as Christian room types via `#APPEND`.
- **Localization**: bilingual EN/ES.

### BepInEx plugin
- Renames `general_christian` / `general_pagan` so the scenario UI shows **Cristiano / Pagano** instead of the cryptic vanilla "Restitucionista" / "Fiel del Roble".
- Discovers every religion registered in `ReligionRepository` at runtime (vanilla + JSON content from any mod). The infrastructure for the upcoming multi-religion selector.

## Roadmap

| Version | Scope |
|---|---|
| **0.2** | Rename + discovery infrastructure (current). |
| **0.3** | Extend renames to room types, tooltips, scenario detail strings. |
| **0.4** | Replace the bipolar slider with a religion selector backed by `ReligionDiscovery`. |
| **0.5** | Triggered events: Sunday mass, baptism, confession, hourly bell chime. |
| **0.6** | Priest robe apparel + Speechcraft bonus. |
| **1.0** | Custom meshes (Blender) and Workshop release. |

## Install

The mod ships in two parts. Both are required.

1. **JSON content** -> drag `ModBuild/ReligionsExpanded/` into `Documents/Foxy Voxel/Going Medieval/Mods/`. Enable in the in-game **Mods** panel.
2. **BepInEx plugin** -> install [BepInEx 5 (x64)](https://github.com/BepInEx/BepInEx/releases) into the Going Medieval install folder, run the game once so it generates `BepInEx/plugins/`, then drop `ReligionsExpanded.Plugin.dll` into `BepInEx/plugins/ReligionsExpanded/`.

`tools/build_mod.py` does both deploys for you on a dev machine.

## Build from source

Requirements:

- Unity Editor **2022.3.46f1** with **Windows Build Support (IL2CPP)** (only needed once meshes are added).
- .NET SDK 8 (for the BepInEx plugin).
- Python **3.10+** (build script).
- Going Medieval **0.23.3** installed.

```bash
# JSON merge + deploy
python tools/build_mod.py

# C# plugin build + deploy
dotnet build plugin/ReligionsExpanded.Plugin.csproj -c Release
```

The plugin's `.csproj` deploys the DLL straight into `<game>/BepInEx/plugins/ReligionsExpanded/`. Override `GameDir` if your install lives somewhere else:

```bash
dotnet build plugin/ReligionsExpanded.Plugin.csproj -c Release -p:GameDir="D:\Games\Going Medieval"
```

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md). Translations especially welcome.

## License

[MIT](LICENSE).
