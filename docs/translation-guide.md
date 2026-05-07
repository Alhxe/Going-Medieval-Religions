# Translation guide

The mod ships with English (`en`) and Spanish (`es`). Adding a new language is just one file.

## How localization works in this mod

Game definitions in `Data/Models/` reference **localization keys** instead of hard-coded strings:

```json
{
    "id": "cross_floor_christian",
    "nameKey": "building_name_cross_floor_christian",
    "infoKey": "building_info_cross_floor_christian"
}
```

Strings live in `Localization/<lang>.json`:

```json
{
    "language": "English",
    "strings": {
        "building_name_cross_floor_christian": "Floor cross",
        "building_info_cross_floor_christian": "A wooden cross planted in the ground. Marks holy ground."
    }
}
```

The build script (`tools/build_mod.py`) merges these into the inline `locKeys` format the game expects.

## Adding a new language

1. Copy `Localization/en.json` to `Localization/<your_lang>.json`.
2. Set `"language"` to the value the game expects (`"English"`, `"Spanish"`, `"German"`, `"French"`, `"Portuguese (Brazil)"`, etc. — see `Going Medieval_Data/StreamingAssets/Language Enum.txt` in your install).
3. Translate every value, leaving keys untouched.
4. Run `tools/build_mod.py`.
5. In-game, switch language; mod strings should follow.

## Glossary

For consistency across translators, here are the canonical translations for recurring religious terms.

| English | Spanish |
|---|---|
| Christianity | Cristianismo |
| Cross | Cruz |
| Crucifix | Crucifijo |
| Chapel | Capilla |
| Church | Iglesia |
| Cathedral | Catedral |
| Bell tower | Campanario |
| Confessional | Confesionario |
| Altar | Altar |
| Pew | Banco de iglesia |
| Mass | Misa |
| Baptism | Bautismo |
| Confession | Confesión |
| Bible | Biblia |
| Priest robe | Sotana |
| Stained glass | Vidriera |

Extend this table as you add new vocabulary.

## Override vanilla strings

To rename a vanilla string (e.g. "Restitutionist" → "Christianity"), use the **same key the game uses internally**. Keys like `religion_christian_name` will replace the vanilla value when the mod is loaded.

A list of vanilla keys we override lives in `Localization/_vanilla_overrides.md`.
