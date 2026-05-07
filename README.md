# Christianity Expanded

A content mod for [Going Medieval](https://store.steampowered.com/app/1029780/) that expands the in-game Christian religion with new buildings, items, rooms and research.

> **Status**: alpha (0.1.0). JSON content done; meshes / textures pending.

## Why "Christianity Expanded" and not "more religions"?

Going Medieval's religion system is hard-coded to a **bipolar 0–100 axis**: 0–49 Pagan ("Followers of the Oak"), 50–100 Christian ("Restitutionist"). Adding a third religion would require code patches against `Assembly-CSharp.dll` (BepInEx + Harmony). We chose to stay within JSON / asset modding for now and pour effort into making the existing Christian religion *much* richer.

## Features

### Buildings
- Floor cross + wall crucifix
- Wooden pew + padded pew
- Grand altar (cathedral focal piece)
- Confessional booth (required in churches and cathedrals)
- Bell tower (anchors the Bell Tower room)
- Three religious paintings (Christ, Virgin Mary, generic saint)
- Three stained-glass windows (red, blue, gold)

### Items
- Bible (readable; trains Speechcraft)
- Portable crucifix
- Consecrated wine (reuses vanilla wine assets)

### Rooms
- **Church** (mid-tier worship room)
- **Cathedral** (top-tier — replaces dormant vanilla `advanced_chapel_christian`)
- **Bell tower** (room around the bell-tower structure)

### Research
- **Liturgy** — unlocks church, pews, paintings, confessional, items.
- **Cathedral architecture** — unlocks cathedral, grand altar, stained glass, bell tower.

### Localization
- English, Spanish. See [`docs/translation-guide.md`](docs/translation-guide.md) to add a language.
- Renames the vanilla "Restitutionist" to "Christianity" / "Cristianismo".

## Install

```bash
python tools/build_mod.py
```

Then launch Going Medieval → menu → **Mods** → enable **Christianity Expanded**.

## Build

Requirements:

- Unity Editor **2022.3.46f1** with **Windows Build Support (IL2CPP)**.
- Python **3.10+**.
- Going Medieval **0.23.3** installed.

```bash
# Compile assets in Unity (only when meshes / textures change):
#   1. Open this project in Unity Hub.
#   2. Going Medieval menu -> AddressableBuilder -> Build.
#
# Merge JSONs and deploy:
python tools/build_mod.py
```

The script validates JSON, expands localization keys into the inline format Going Medieval expects, generates a CSV for the global I2 Localization table, merges modular files, and copies the result to `Documents/Foxy Voxel/Going Medieval/Mods/ChristianityExpanded/`.

## Repo layout

See [`docs/architecture.md`](docs/architecture.md).

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md). Translations especially welcome.

## License

[MIT](LICENSE).
