# SewerMenu

SewerMenu is a MelonLoader mod menu for **Schedule I**. The `beta` branch is the active 2.0 development branch and is currently targeting Schedule I `0.4.5f2`.

## Highlights

- Modern IMGUI menu with animated tabs, toggles, favorites, inspector panel, and Shift+K command palette
- Full item spawner with fuzzy search, category filters, quantity presets, and search commands
- Input lock so scrolling the menu does not scroll the in-game hotbar
- Performance-focused ESP refresh/caching and vehicle tab caching
- Infinite ammo, stack size tools, item quality tools, and inventory error feedback
- Animation quality setting with Auto/Balanced/Low modes for weaker systems
- FPS optimizer controls and safer config save timing

## Requirements

- Schedule I `0.4.5f2` or compatible
- MelonLoader `0.7.1`
- .NET SDK `6.0` or newer for local builds

## Installation

1. Install MelonLoader `0.7.1` for Schedule I.
2. Build or download `SewerMenu.dll`.
3. Place `SewerMenu.dll` in `Schedule I/Mods/`.
4. Launch the game.

## Controls

| Key | Action |
| --- | --- |
| `F8` | Toggle menu |
| `Esc` | Close menu |
| `Shift+K` | Open or close command palette from the menu or gameplay |

## Item Spawner Search Commands

The full item spawner accepts lightweight quantity commands in the search field:

| Example | Behavior |
| --- | --- |
| `weed 5` | Search `weed` and set quantity to `5` |
| `weed x5` | Search `weed` and set quantity to `5` |
| `weed *5` | Search `weed` and set quantity to `5` |
| `weed qty:5` | Search `weed` and set quantity to `5` |
| `weed max` | Search `weed` and set quantity to the menu max |
| `weed stack` | Search `weed` and use the selected/top item's stack size |

Press `Enter` in the search field to spawn the selected/top item with the current quantity.

In the command palette, type `close` and press `Enter` to dismiss the panel without using Escape.

## Build

```powershell
dotnet build --configuration Release
```

By default, successful builds copy `bin/Release/SewerMenu.dll` to:

```text
C:\Program Files (x86)\Steam\steamapps\common\Schedule I\Mods\
```

To build without copying into the game folder:

```powershell
dotnet build --configuration Release /p:SkipCopyToMods=true
```

## Local Verification

After Schedule I updates, launch the game once with MelonLoader installed so `MelonLoader/Il2CppAssemblies` is regenerated. Then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\verify-game-types.ps1
```

This builds without copying a Debug DLL into Mods and verifies the generated IL2CPP types/members used by the menu without entering a save.

## Release Notes

Do not publish a GitHub release or Nexus Mods upload until the 2.0 launch has been explicitly approved.
