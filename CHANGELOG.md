# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Tooling
- Initial repo scaffolding (Unity project + modular ModBuild structure).
- ModInfo for `Religions Expanded` mod.
- Modular JSON folder layout under `ModBuild/ReligionsExpanded/Data/Models/`.
- Localization scaffolding (English, Spanish).
- Python build script `tools/build_mod.py`: validates JSON, expands `nameKey`/`infoKey`/`descKey` into the inline `locKeys` array Going Medieval expects, merges modular files into the single repository file the game loads, copies AddressableAssets, deploys to `Documents/Foxy Voxel/Going Medieval/Mods/`.

#### Christianity content (definitions only — meshes / textures pending)
- **Religion config** override for `christian`: adds `grand_altar_christian` to its shrine list and registers the new room types.
- **Buildings**: floor cross, wall crucifix, wooden pew, padded pew, grand altar, confessional, bell tower, three religious paintings (Christ / Mary / Saint), three stained-glass windows (red / blue / gold).
- **Items**: bible, portable crucifix, consecrated wine (reuses vanilla wine assets).
- **Rooms**: `church_christian` (mid-tier), `cathedral_christian` (top-tier — replaces the dormant vanilla `advanced_chapel_christian`), `bell_tower_room_christian`.
- **Research**: `liturgy` (unlocks church + most decorations + items) and `cathedral_architecture` (unlocks cathedral, grand altar, stained glass, bell tower).
- **Localization**: English + Spanish strings for every new entry, plus vanilla overrides (`Restitutionist` -> `Christianity`, `Restitutionist Relic` -> `Holy Relic`).

### Notes
- Targeting Going Medieval `0.23.3`.
- Targeting Unity `2022.3.46f1` (matches Foxy Voxel modding template).
- Triggered events (Sunday Mass, Baptism, Confession, Bell Chime) are documented but not implemented — the C# `className` for scheduled / triggered religious events is unknown and likely needs in-game testing or a code mod.
- Priest robe (visual + Speechcraft bonus) deferred — Going Medieval lacks an obvious wearable-apparel system separate from the priest role's built-in visuals.
