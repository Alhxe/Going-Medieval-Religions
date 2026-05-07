# Modding references

## Official

- [Foxy Voxel modding template](https://github.com/FoxyVoxel/going-medieval-modding) — Unity project for Addressables.
- Required Unity version: `2022.3.46f1` (see `ProjectSettings/ProjectVersion.txt`).
- Game version targeted: `0.23.3`.

## Vanilla data

Game JSONs live at:

```
C:\Program Files (x86)\Steam\steamapps\common\Going Medieval\Going Medieval_Data\StreamingAssets\
```

Key files for religion work:

| File | Contains |
|---|---|
| `Data/ReligionConfig.json` | Religions, alignment ranges, shrines, mood effectors. |
| `Data/RoomTypes.json` | Room definitions (chapel_christian, advanced_chapel_christian, etc.). |
| `Constructables/BaseBuildingRepository.json` | Every building. |
| `Resources/Resources.json` | Every resource / item. |
| `Roles/Role.json` | Roles (priest, shaman). |
| `Research/Research.json` | Research tree. |
| `GameEventSystem/GameEventSettingsRepository.json` | Game events. |
| `Language Enum.txt` | Valid `languageName` values. |

## Mod folder

User-installed mods live at:

```
C:\Users\<user>\Documents\Foxy Voxel\Going Medieval\Mods\<ModName>\
```

The game logs mod loading to:

```
C:\Users\<user>\AppData\LocalLow\Foxy Voxel\Going Medieval\Player.log
```

Look for `[ModdingUtils]` and `[ModInstance]` entries when debugging.
