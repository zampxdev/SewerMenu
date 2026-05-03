# SewerMenu

SewerMenu is a MelonLoader mod menu for **Schedule I**. Version `2.0.0` targets Schedule I `0.4.5f2`.

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

After Schedule I updates, launch the game once with MelonLoader installed so `MelonLoader/Il2CppAssemblies` is regenerated, then run your local verifier script if you use one.

Local PowerShell helper scripts are intentionally ignored and are not part of the public source release.

## Release Upload Automation

GitHub Actions includes a release upload workflow that can publish a GitHub release asset to Nexus Mods.

Required repository settings:

- Secret: `NEXUSMODS_API_KEY`
- Variable: `NEXUSMODS_FILE_GROUP_ID`

Publish a GitHub release with either `SewerMenu.dll` or `SewerMenu-vX.Y.Z.zip` attached. When the release is published, the workflow uploads it to Nexus. It can also be run manually from Actions with a release tag and optional exact asset name.

## Release Notes

See `CHANGELOG.md` for the 2.0 release notes.
