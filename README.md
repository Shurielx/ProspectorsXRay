# ⛏️ Prospector's X-Ray

A **Vintage Story (1.22.*)** mod that adds survey modes under the `F` key and highlights detected ore blocks through walls (X-Ray) while holding a pickaxe.

Based on [Prospector's Instinct](https://mods.vintagestory.at/prospectorsinstinct) by Cornwall Mods.

---

## Features

* **Survey modes on `F` key (Prospecting Pick):**
  * **32x32x32** (requires 3 samples)
  * **64x64x64** (requires 5 samples)
* **X-Ray visibility:**
  * Initially reveals 50% of detected ore blocks while holding a pickaxe or propick in main hand or off-hand.
  * Mining an ore block from the deposit reveals **+5%** more hidden ores.
* **High performance & Zero-Lag (v1.1.0):**
  * **Asynchronous / Debounced GPU mesh updates:** Mining ore produces zero stutters/freezes.
  * **Fast-path O(1) spatial filter:** Ignores unrelated block destruction in < 1 nanosecond.
  * **Zero-GC memory architecture:** Reusable vertex data (`MeshData.Clear()`), pooled vectors (`Vec3f.Set()`), and static reflection caching prevent Garbage Collection spikes during long play sessions.
* **Display modes:**
  * **Wireframe** (draws only block edges/outlines)
  * **Block** (cubes – default size 25% of a block to prevent visual clutter)
* **Ore filtering (`/ore`):**
  * Show/hide individual ore variants and groups.
  * Meta-group `ingotable` (shows only ores for smelting tool metals).

---

## Commands (`/ore`)

| Command | Description |
| :--- | :--- |
| `/ore display wireframe` | Switch to wireframe outlines (3D lines) |
| `/ore display block` | Switch to solid/mini cubes |
| `/ore displaysize [10-100]` | Set cube size in block mode (default: `25` = 25% of block size) |
| `/ore displayopacity [0-100]` | Set highlight opacity (default: `60`) |
| `/ore list` | List all ores and their current visibility status |
| `/ore list group` | List only ore groups |
| `/ore list ore` | List individual ore variants |
| `/ore show [name]` | Show an ore or group (e.g. `/ore show magnetite`) |
| `/ore hide [name]` | Hide an ore or group (e.g. `/ore hide chromium`) |
| `/ore showonly [name]` | Show ONLY the specified ore or group |
| `/ore hideonly [name]` | Hide ONLY the specified ore or group |
| `/ore show ingotable` | Show only tool metal ores (Copper, Tin, Iron, Gold, Silver, Lead, Zinc, Bismuth, Nickel) |
| `/ore show all` / `/ore hide all` | Enable or disable all ores |
| `/ore clear` | Clear the active survey and hide highlights |
| `/ore info` | Show status of the active survey |
| `/ore reload` | Reset all settings to default |
| `/ore help` | Show command list in chat |

---

## Installation

1. Download `ProspectorsXRay.zip` from [Releases](https://github.com/Shurielx/ProspectorsXRay/releases).
2. Drop the `.zip` file into your game mods folder:
   * **Windows:** `%appdata%\VintagestoryData\Mods`
   * **Linux:** `~/.config/VintagestoryData/Mods`

---

## Building from Source

Requires [.NET 10.0 SDK](https://dotnet.microsoft.com/download) and Vintage Story installed.

```bash
dotnet build -c Release
```

The compiled mod package will be created in `releases/ProspectorsXRay.zip` and automatically copied to your Vintage Story `Mods` folder.
