"""Religions Expanded — build & deploy script.

What it does:
  1. Validates every JSON file under ModBuild/ReligionsExpanded/.
  2. Expands nameKey / infoKey / descKey into the inline locKeys array Going
     Medieval expects, using strings from Localization/<lang>.json.
  3. Merges modular files in Data/Models/<category>/*.json into the single
     repository file the game expects (e.g. BaseBuildingRepository.json).
  4. Copies ModInfo, Preview, AddressableAssets verbatim.
  5. Deploys the result to the user's mods folder.

Usage:
  python tools/build_mod.py
  python tools/build_mod.py --skip-deploy
  python tools/build_mod.py --no-locale-expand   # debug: keep *Key fields raw
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import sys
import tempfile
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
MOD_NAME = "ReligionsExpanded"
MOD_SOURCE = REPO_ROOT / "ModBuild" / MOD_NAME
DEPLOY_ROOT = Path(os.path.expanduser("~/Documents/Foxy Voxel/Going Medieval/Mods"))
DEPLOY_DEST = DEPLOY_ROOT / MOD_NAME

# Maps modular subfolder -> final repository filename the game loads.
REPOSITORY_MAP = {
    "buildings": "BaseBuildingRepository.json",
    "items":     "Resources.json",
    "rooms":     "RoomTypes.json",
    "research":  "Research.json",
    "events":    "GameEventSettingsRepository.json",
    "religions": "ReligionConfig.json",
    "stats":     "StatsRepository.json",
}

# Source field -> target locKeys field
LOC_FIELD_MAP = {
    "nameKey": "name",
    "infoKey": "info",
    "descKey": "description",
}


class BuildError(RuntimeError):
    pass


def step(msg: str) -> None:
    print(f"==> {msg}")


def warn(msg: str) -> None:
    print(f"WARN: {msg}", file=sys.stderr)


# ---------------------------------------------------------------------------
# JSON validation
# ---------------------------------------------------------------------------

def validate_jsons(root: Path) -> list[Path]:
    files = sorted(root.rglob("*.json"))
    failed = False
    for f in files:
        try:
            with f.open(encoding="utf-8") as fh:
                json.load(fh)
        except json.JSONDecodeError as e:
            print(f"  JSON error in {f}: {e}", file=sys.stderr)
            failed = True
    if failed:
        raise BuildError("JSON validation failed.")
    return files


# ---------------------------------------------------------------------------
# Localization
# ---------------------------------------------------------------------------

def load_locales(locale_dir: Path) -> dict[str, dict[str, str]]:
    """language -> { key -> translated string }"""
    locales: dict[str, dict[str, str]] = {}
    for f in sorted(locale_dir.glob("*.json")):
        with f.open(encoding="utf-8") as fh:
            data = json.load(fh)
        lang = data.get("language")
        if not lang:
            warn(f"{f.name} has no 'language' field — skipping.")
            continue
        strings = {k: v for k, v in data.get("strings", {}).items() if not k.startswith("_")}
        locales[lang] = strings
    if not locales:
        raise BuildError("No localization files found.")
    return locales


def expand_loc_keys(node, locales: dict[str, dict[str, str]], missing: set[str]):
    """Walk node; replace nameKey/infoKey/descKey with locKeys array."""
    if isinstance(node, list):
        return [expand_loc_keys(item, locales, missing) for item in node]
    if not isinstance(node, dict):
        return node

    out: dict = {}
    loc_values: dict[str, str] = {}
    for k, v in node.items():
        if k in LOC_FIELD_MAP:
            loc_values[LOC_FIELD_MAP[k]] = str(v)
            continue
        out[k] = expand_loc_keys(v, locales, missing)

    if loc_values:
        loc_array = []
        for lang, strings in locales.items():
            entry = {"languageName": lang}
            for target_field, key in loc_values.items():
                resolved = strings.get(key)
                if resolved is None:
                    missing.add(f"{lang} : {key}")
                    resolved = key
                entry[target_field] = resolved
            loc_array.append(entry)
        out["locKeys"] = loc_array

    return out


# ---------------------------------------------------------------------------
# Merge modular files
# ---------------------------------------------------------------------------

def merge_category(cat_dir: Path, locales: dict, missing: set[str], expand: bool) -> list:
    entries: list = []
    for f in sorted(cat_dir.glob("*.json")):
        with f.open(encoding="utf-8") as fh:
            data = json.load(fh)
        if isinstance(data, dict) and "repository" in data:
            items = data["repository"]
        elif isinstance(data, list):
            items = data
        else:
            items = [data]
        for item in items:
            if isinstance(item, dict) and item.get("_comment"):
                # Drop top-level _comment markers.
                item = {k: v for k, v in item.items() if k != "_comment"}
            if expand:
                item = expand_loc_keys(item, locales, missing)
            entries.append(item)
    return entries


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(description="Build Religions Expanded mod.")
    parser.add_argument("--skip-deploy", action="store_true", help="Build only; don't copy to Documents/.../Mods.")
    parser.add_argument("--no-locale-expand", action="store_true", help="Skip nameKey->locKeys expansion (debug).")
    args = parser.parse_args()

    if not MOD_SOURCE.exists():
        raise BuildError(f"Mod source not found: {MOD_SOURCE}")

    step("Validating JSON files")
    files = validate_jsons(MOD_SOURCE)
    print(f"  {len(files)} files OK")

    step("Loading localization")
    locales = load_locales(MOD_SOURCE / "Localization")
    print(f"  Languages: {', '.join(locales.keys())}")

    step("Merging modular definitions")
    build_out = Path(tempfile.mkdtemp(prefix="religions_expanded_build_"))
    (build_out / "Data" / "Models").mkdir(parents=True, exist_ok=True)

    missing: set[str] = set()
    models_dir = MOD_SOURCE / "Data" / "Models"
    expand = not args.no_locale_expand
    for category, repo_file in REPOSITORY_MAP.items():
        cat_dir = models_dir / category
        if not cat_dir.exists():
            continue
        entries = merge_category(cat_dir, locales, missing, expand)
        if not entries:
            continue
        out_path = build_out / "Data" / "Models" / repo_file
        with out_path.open("w", encoding="utf-8") as fh:
            json.dump({"repository": entries}, fh, indent=4, ensure_ascii=False)
        print(f"  {category} -> {repo_file} ({len(entries)} entries)")

    if missing:
        print()
        warn("missing localization keys (used vanilla key as fallback):")
        for k in sorted(missing):
            warn(f"  - {k}")
        print()

    # Copy ModInfo, Preview, AddressableAssets verbatim
    shutil.copy2(MOD_SOURCE / "ModInfo.json", build_out / "ModInfo.json")
    preview = MOD_SOURCE / "Preview.png"
    if preview.exists():
        shutil.copy2(preview, build_out / "Preview.png")
    addressable_src = MOD_SOURCE / "Data" / "AddressableAssets"
    if addressable_src.exists():
        shutil.copytree(addressable_src, build_out / "Data" / "AddressableAssets")
        print("  Copied AddressableAssets/")

    if args.skip_deploy:
        step("Skipping deploy")
        print(f"Build artefacts at: {build_out}")
        return 0

    step(f"Deploying to {DEPLOY_DEST}")
    if DEPLOY_DEST.exists():
        shutil.rmtree(DEPLOY_DEST)
    shutil.copytree(build_out, DEPLOY_DEST)
    shutil.rmtree(build_out)

    step("Done")
    print("Mod deployed. Launch Going Medieval and enable 'Religions Expanded'.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except BuildError as e:
        print(f"ERROR: {e}", file=sys.stderr)
        sys.exit(1)
