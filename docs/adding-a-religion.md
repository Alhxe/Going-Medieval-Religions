# Adding a new religion

> Status: scaffolding — full guide pending implementation of the framework.

## Overview

A religion in this mod is a coherent set of:

1. A `religions/<name>.json` definition (alignment range, mood effectors, room types, shrines).
2. Building entries in `buildings/` (its altars, decorations, etc.).
3. Item entries in `items/` (its relic, vestments, sacred consumables).
4. Room types in `rooms/` (chapel and advanced chapel).
5. Research nodes in `research/` (unlock progression).
6. Optional rituals in `events/`.
7. Localization strings in every `Localization/<lang>.json`.

## Steps

1. Pick a unique `id` (lowercase, snake_case — e.g. `christianity`, `norse_pagan`).
2. Decide alignment range (numeric band, must not collide with other religions).
3. Create the files above using the existing Christianity entries as a template.
4. Add localization keys for every name/info field.
5. Run `tools/build-mod.ps1` and test in-game.

## Conventions

- Building IDs prefix with the religion id: `cross_floor_christian`, `altar_christian`...
- Localization keys mirror the IDs: `loc_cross_floor_christian_name`.
- Keep alignment ranges contiguous to avoid gaps that NPCs can't fall into.

A worked example will land here once the Christianity expansion is finished.
