# TooMuchScrap — Lethal Company Mod

A small mod for Lethal Company that reduces onboard item clutter and lag by letting players merge items of the same type with a single command. Use the `/merge` command to combine nearby identical items into a single instance, summing their values up to a configurable cap.

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
- "MergeItems": comma separated list of internal item names that should be considered for merging.

You do not need to restart the game after editing the config.

## Credits
Heavily based on work by MeGaGiGaGon (ScrapMerging) — credit to MeGaGiGaGon for inspiration and reference implementation ideas.