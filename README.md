# TooMuchScrap — Lethal Company Mod

A small mod for Lethal Company that reduces onboard item clutter and lag by letting players merge items of the same type with a single command. Use the `/merge` command to combine nearby identical items into a single instance, summing their values up to a configurable cap.

## Warning (Host-only command)

Only the game host can run the `/merge` command — clients cannot execute it. Clients do not need the mod installed to join; only the host/server needs the mod for merging to work.

## Features
- Merge items of the same internal type when they are close to each other.
- Reduce entity count and server/client lag.
- Configurable merge distance, maximum merged value, and which item types are eligible.

## Usage
- In-game or server console, run:
    /merge
- The command scans for items listed in `MergeItems` and merges items of the same internal name when they are within `MergeDistance`. Merged item values are summed but never exceed `MaxMergeValue`.

## Configuration

- "MergeDistance": max distance (units) between items to consider them for merging.
- "MaxMergeValue": maximum numeric value an item can have after merging (merged totals are clamped to this value).
- "MergeItems": comma separated list of internal item names that should be considered for merging. Default: `HeartContainer,SeveredHandLOD0,SeveredFootLOD0,SeveredThighLOD0,Bone,RibcageBone,Ear,Tongue` (Dine items). Edit this comma-separated list to add or remove items.
- "PrefixChar": Command prefix (Requires game restart after changes)
- "CompanyOnly": true/false — Whether to merge items only while at the Company building.
- "AutoMerge": true/false — Whether to automatically merge items after leaving a moon. Note: if "CompanyOnly" is True, auto-merge will only run when at the Company building.

You do not need to restart the game after editing the config.

## Credits

Heavily based on work by MeGaGiGaGon (ScrapMerging) — credit to MeGaGiGaGon for inspiration and reference implementation ideas.