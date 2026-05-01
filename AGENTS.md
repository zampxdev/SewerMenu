# AGENTS.md - SewerMenu Development Guide

## Project Overview
SewerMenu is a MelonLoader mod menu for **Schedule I** (Unity IL2CPP game). It provides cheat features like god mode, teleportation, item spawning, etc.

**Tech Stack:** C# / .NET 6.0 / MelonLoader v0.7.1 / Unity 2022.3 IL2CPP

## Build Commands

```bash
# Build Release (auto-copies to game Mods folder)
dotnet build --configuration Release

# Build Debug
dotnet build --configuration Debug

# Clean and rebuild
dotnet clean && dotnet build --configuration Release
```

**Output:** `bin/Release/SewerMenu.dll` -> Auto-copied to `C:\Program Files (x86)\Steam\steamapps\common\Schedule I\Mods\`

**No tests exist** - This is a game mod without a test framework.

## Project Structure

```
SewerMenu/
├── Core/                    # Core mod infrastructure
│   ├── SewerMenuMod.cs      # Main entry point (MelonMod)
│   ├── ModInfo.cs           # Version and metadata
│   ├── Config/              # Configuration system
│   └── Logging/             # SewerLogger wrapper
├── Features/                # All cheat features
│   ├── Base/                # IFeature, FeatureBase, FeatureManager
│   ├── Player/              # GodMode, Teleport, NoClip, etc.
│   ├── Economy/             # MoneyEditor, XPEditor
│   ├── Items/               # ItemSpawner, StackSize
│   ├── World/               # TimeController, PoliceDisable
│   ├── Vehicles/            # VehicleSpawner, VehicleGodMode
│   └── Misc/                # ESP, Freecam, DebugOverlay
├── UI/                      # IMGUI-based menu system
│   ├── MenuController.cs    # Main window controller
│   ├── SewerSkin.cs         # Styling and custom drawing
│   ├── Pages/               # Tab pages (PlayerPage, etc.)
│   └── Windows/             # Popup windows
└── Utils/                   # Utilities
    ├── GameTypes.cs         # Game type access (Player, Managers)
    └── GameFinder.cs        # GameObject discovery
```

## Code Style Guidelines

### Namespaces & Imports
```csharp
using System;
using MelonLoader;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Features.Base;
// Game types use Il2Cpp prefix
using Il2CppScheduleOne.PlayerScripts;
```

### Naming Conventions
- **Classes:** PascalCase (`GodMode`, `FeatureManager`)
- **Methods:** PascalCase (`OnUpdate`, `GetLocalPlayer`)
- **Properties:** PascalCase (`IsEnabled`, `CurrentHealth`)
- **Private fields:** `_camelCase` (`_initialized`, `_playerHealth`)
- **Constants:** PascalCase (`MaxHealth`, `RefreshInterval`)
- **Feature IDs:** lowercase (`"godmode"`, `"infinitestamina"`)

### Creating a New Feature
1. Create class in appropriate `Features/` subfolder
2. Extend `FeatureBase`
3. Implement required properties: `Id`, `Name`, `Description`, `Category`
4. Override lifecycle methods as needed: `OnEnable`, `OnDisable`, `OnUpdate`
5. Register in `FeatureManager.RegisterAllFeatures()`

```csharp
public class MyFeature : FeatureBase
{
    public override string Id => "myfeature";
    public override string Name => "My Feature";
    public override string Description => "Does something cool";
    public override FeatureCategory Category => FeatureCategory.Player;

    public override void OnUpdate()
    {
        if (!IsEnabled) return;
        SafeExecute(() => {
            // Feature logic here
        }, "updating my feature");
    }
}
```

### IL2CPP Safety Patterns (CRITICAL)

**Always wrap IL2CPP calls in try/catch:**
```csharp
try { value = someIl2CppObject.Property; } catch { }
```

**Use SafeExecute for feature logic:**
```csharp
SafeExecute(() => {
    var health = GameTypes.Health;
    if (health != null) health.SetHealth(100f);
}, "setting health");
```

**Null check everything:**
```csharp
var player = GameTypes.LocalPlayer;
if (player == null) return;
```

### IMGUI Rules (CRITICAL - Prevents Crashes)

1. **Keep layouts flat** - Avoid deep nesting
2. **Always match Begin/End pairs** - Every `BeginHorizontal` needs `EndHorizontal`
3. **Use GUI.DrawTexture for lines** - NOT `GUILayout.Box` (causes artifacts)
4. **Wrap page content in try/catch**
5. **Use SewerSkin methods** - They handle styling safely

```csharp
// GOOD - Using SewerSkin
SewerSkin.DrawSection("MY SECTION");
bool newVal = SewerSkin.DrawToggle("Feature", isEnabled);

// BAD - Raw GUILayout.Box for lines (causes visual artifacts)
GUILayout.Box("", GUILayout.Height(1));

// GOOD - GUI.DrawTexture for lines
Rect rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
GUI.DrawTexture(rect, Texture2D.whiteTexture);
```

### Error Handling
- Use `SewerLogger` for all logging
- Wrap risky operations in `SafeExecute()`
- Never let exceptions propagate from `OnUpdate`/`OnGUI`

```csharp
SewerLogger.Info("Message");
SewerLogger.Warning("Warning");
SewerLogger.Error("Error", exception);
SewerLogger.Debug("Debug info");  // Only logs if EnableDebugLogging=true
```

### Game Type Access
Use `GameTypes` static class for all game object access:
```csharp
var player = GameTypes.LocalPlayer;
var health = GameTypes.Health;
var energy = GameTypes.Energy;
var money = GameTypes.Money;
var movement = GameTypes.Movement;
```

### UI Page Pattern
```csharp
public class MyPage : PageBase
{
    public override string Title => "My Page";
    public override FeatureCategory Category => FeatureCategory.Player;

    protected override void DrawContent()
    {
        DrawSection("SECTION NAME");
        
        var feature = FeatureManager.Instance.GetFeature<MyFeature>("myfeature");
        if (feature != null)
        {
            bool newVal = DrawToggle("Feature Name", feature.IsEnabled, "Description");
            if (newVal != feature.IsEnabled) feature.IsEnabled = newVal;
        }
    }
}
```

## Key Files Reference

| File | Purpose |
|------|---------|
| `Core/SewerMenuMod.cs` | Main entry point, lifecycle |
| `Core/ModInfo.cs` | Version number (update for releases) |
| `Features/Base/FeatureManager.cs` | Feature registration |
| `UI/SewerSkin.cs` | All UI styling/drawing |
| `UI/MenuController.cs` | Main menu window |
| `Utils/GameTypes.cs` | Game object access |

## Common Pitfalls

1. **Text fields don't work in IL2CPP** - Use `DrawNumericInput` or `DrawQuantitySelector`
2. **GUILayout.Box causes artifacts** - Use `GUI.DrawTexture` for lines/separators
3. **Exceptions in OnGUI corrupt IMGUI state** - Always wrap in try/catch
4. **IL2CPP objects can be garbage collected** - Cache references, check for null
5. **Don't use `GUILayout.TextField`** - It doesn't work in IL2CPP

## Version Updates
Update version in `Core/ModInfo.cs`:
```csharp
public const string Version = "1.0.4";
```
