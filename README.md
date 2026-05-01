# SewerMenu

A mod menu for **Schedule I** built with MelonLoader.

## Features

- **Player** - God Mode, Infinite Stamina, Sprint Speed, Jump Height, NoClip, Fly Mode
- **Economy** - Cash/Bank Editor, XP Editor, Unlock Products
- **Items** - Item Spawner with categories and quantity selection
- **World** - Time Control, Police Control, NPC Freeze, Unlock Properties
- **Vehicles** - Vehicle Spawner with color selection
- **Misc** - Freecam, ESP, Debug Overlay
- **Settings** - Customizable keybinds, persistent config

## Installation

1. Install [MelonLoader v0.7.1](https://github.com/LavaGang/MelonLoader/releases)
2. Download `SewerMenu.dll` from [Releases](https://github.com/zampxdev/SewerMenu/releases)
3. Place in `Schedule I/Mods/` folder
4. Launch the game

## Usage

- **F8** - Toggle menu
- **ESC** - Close menu

## Local Verification

After Schedule I updates, launch the game once with MelonLoader installed so `MelonLoader/Il2CppAssemblies` is regenerated. Then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\verify-game-types.ps1
```

This builds the mod and verifies the generated IL2CPP types/members used by the menu without entering a save.

## Requirements

- Schedule I v0.4.2+
- MelonLoader v0.7.1
