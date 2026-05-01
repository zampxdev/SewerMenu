# SewerMenu - Complete Developer Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Technical Stack](#technical-stack)
3. [Project Structure](#project-structure)
4. [How to Find Game Types](#how-to-find-game-types)
5. [GameTypes Utility Class](#gametypes-utility-class)
6. [Creating New Features](#creating-new-features)
7. [UI System](#ui-system)
8. [Common Patterns](#common-patterns)
9. [Debugging](#debugging)
10. [Building & Deployment](#building--deployment)

---

## Project Overview

**SewerMenu** is a MelonLoader mod menu for the Unity IL2CPP game **Schedule I** (a drug dealer simulation game by TVGS). It provides cheats and trainer functionality through an in-game GUI.

### Key Information
- **Game**: Schedule I v0.4.2f9
- **Engine**: Unity 2022.3.62f2 with IL2CPP
- **Mod Loader**: MelonLoader v0.7.1 Open-Beta
- **Runtime**: .NET 6.0
- **Game Location**: `C:\Program Files (x86)\Steam\steamapps\common\Schedule I`
- **Project Location**: `C:\Users\liamt\Desktop\SewerMenu`

---

## Technical Stack

### Dependencies (from SewerMenu.csproj)
```xml
<ItemGroup>
    <!-- MelonLoader Core -->
    <Reference Include="MelonLoader">
        <HintPath>$(GamePath)\MelonLoader\net6\MelonLoader.dll</HintPath>
    </Reference>
    
    <!-- IL2CPP Interop -->
    <Reference Include="Il2CppInterop.Runtime">
        <HintPath>$(GamePath)\MelonLoader\net6\Il2CppInterop.Runtime.dll</HintPath>
    </Reference>
    
    <!-- Unity Assemblies (from MelonLoader) -->
    <Reference Include="UnityEngine">
        <HintPath>$(GamePath)\MelonLoader\Il2CppAssemblies\UnityEngine.dll</HintPath>
    </Reference>
    <!-- ... other Unity assemblies -->
    
    <!-- Game Assembly -->
    <Reference Include="Assembly-CSharp">
        <HintPath>$(GamePath)\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll</HintPath>
    </Reference>
</ItemGroup>
```

### IL2CPP Type Naming Convention
All game types use the `Il2Cpp` prefix:
- `Il2CppScheduleOne.PlayerScripts.Player`
- `Il2CppScheduleOne.Money.MoneyManager`
- `Il2CppScheduleOne.ItemFramework.ItemDefinition`

---

## Project Structure

```
SewerMenu/
├── Core/
│   ├── SewerMenuMod.cs          # Main mod entry point (MelonMod)
│   ├── ModInfo.cs               # Version info
│   ├── Config/
│   │   ├── SewerConfig.cs       # Configuration data classes
│   │   └── ConfigManager.cs     # Config save/load
│   ├── Keybinds/
│   │   └── KeybindManager.cs    # Hotkey handling
│   └── Logging/
│       └── SewerLogger.cs       # Logging utilities
│
├── Features/
│   ├── Base/
│   │   ├── IFeature.cs          # Feature interface
│   │   ├── FeatureBase.cs       # Base class for features
│   │   ├── FeatureManager.cs    # Feature registry & lifecycle
│   │   └── FeatureCategory.cs   # Category enum
│   ├── Player/
│   │   ├── GodMode.cs
│   │   ├── InfiniteStamina.cs
│   │   ├── SprintSpeed.cs
│   │   ├── JumpHeight.cs
│   │   ├── NoClip.cs
│   │   ├── FlyMode.cs
│   │   └── Teleport.cs
│   ├── Economy/
│   │   ├── MoneyEditor.cs
│   │   ├── XPEditor.cs
│   │   ├── UnlockProducts.cs
│   │   └── FreePurchases.cs
│   ├── Items/
│   │   ├── ItemSpawner.cs
│   │   ├── StackSizeModifier.cs
│   │   ├── InfiniteItems.cs
│   │   ├── QualityOverride.cs
│   │   └── InstantGrow.cs
│   ├── World/
│   │   ├── TimeController.cs
│   │   ├── PoliceDisable.cs
│   │   ├── NPCFreeze.cs
│   │   └── UnlockProperties.cs
│   └── Misc/
│       ├── ESP.cs
│       ├── Freecam.cs
│       └── DebugOverlay.cs
│
├── UI/
│   ├── MenuController.cs        # Main menu window
│   ├── SewerSkin.cs            # GUI styling
│   ├── Theme.cs                # Color definitions
│   ├── Styles.cs               # GUIStyle definitions
│   └── Pages/
│       ├── IPage.cs
│       ├── PageBase.cs
│       ├── PlayerPage.cs
│       ├── EconomyPage.cs
│       ├── ItemsPage.cs
│       ├── WorldPage.cs
│       ├── MiscPage.cs
│       └── SettingsPage.cs
│
├── Utils/
│   ├── GameTypes.cs            # ★ MAIN GAME ACCESS CLASS ★
│   ├── GameFinder.cs           # Legacy (deprecated)
│   └── TypeDiscovery.cs        # Type exploration tool
│
├── GAME_TYPES.md               # ★ COMPLETE TYPE REFERENCE ★
├── KEY_TYPES.md                # Quick reference for common types
├── DOCUMENTATION.md            # This file
└── SewerMenu.csproj
```

---

## How to Find Game Types

### Step 1: Check GAME_TYPES.md
The file `GAME_TYPES.md` contains **2908 types across 130 namespaces** extracted from the game. This is your primary reference.

#### Structure of GAME_TYPES.md
```markdown
## Il2CppScheduleOne.Money

```csharp
using Il2CppScheduleOne.Money;
```

### Classes

#### MoneyManager

*Extends: `NetworkSingleton`1`*

**Properties:**

| Name | Type | Access |
|------|------|--------|
| `cashBalance` | `float` | get |
| `onlineBalance` | `float` | get/set |

**Methods:**

| Name | Parameters | Returns |
|------|------------|---------|
| `ChangeCashBalance` | `float change, bool visualizeChange, bool playCashSound` | `void` |
```

### Step 2: Search for Types
Use grep/search to find types:
```bash
# Find a manager
grep -n "Manager" GAME_TYPES.md | head -50

# Find player-related types
grep -n "Player" GAME_TYPES.md

# Find specific property
grep -n "cashBalance" GAME_TYPES.md
```

### Step 3: Check the Namespace
Each section in GAME_TYPES.md shows the using statement:
```csharp
using Il2CppScheduleOne.Money;  // For MoneyManager
using Il2CppScheduleOne.PlayerScripts;  // For Player, PlayerMovement
using Il2CppScheduleOne.ItemFramework;  // For ItemDefinition
```

### Step 4: Add to GameTypes.cs
Once you find a type, add it to `Utils/GameTypes.cs`:

```csharp
// Add the using statement at the top
using Il2CppScheduleOne.NewNamespace;

// Add a cached reference
private static NewManager _newManager;

// Add a property to access it
public static NewManager NewThing
{
    get
    {
        if (_newManager == null || !_newManager)
        {
            _newManager = UnityEngine.Object.FindObjectOfType<NewManager>();
        }
        return _newManager;
    }
}
```

---

## GameTypes Utility Class

`Utils/GameTypes.cs` is the **central access point** for all game objects. Always use this instead of direct FindObjectOfType calls.

### Available Properties

#### Player Components
```csharp
GameTypes.LocalPlayer      // Player - main player instance
GameTypes.Movement         // PlayerMovement - speed, stamina, teleport
GameTypes.Health           // PlayerHealth - health, damage
GameTypes.Camera           // PlayerCamera - camera control
GameTypes.Inventory        // PlayerInventory - items
GameTypes.PlayerTransform  // Transform - position/rotation
GameTypes.PlayerPosition   // Vector3 - current position
GameTypes.PlayerGameObject // GameObject - player's GO
```

#### Managers
```csharp
GameTypes.Money       // MoneyManager - cash, bank balance
GameTypes.Law         // LawManager - police
GameTypes.Time        // TimeManager - time of day
GameTypes.Level       // LevelManager - XP, rank, tier
GameTypes.Products    // ProductManager - unlockable products
GameTypes.Properties  // PropertyManager - properties/businesses
```

#### Collections
```csharp
GameTypes.GetAllNPCs()              // NPC[] - all NPCs
GameTypes.GetAllPolice()            // PoliceOfficer[] - all police
GameTypes.GetAllItemDefinitions()   // List<ItemDefinition> - all items
GameTypes.GetItemById(string id)    // ItemDefinition - specific item
```

#### Utility Methods
```csharp
GameTypes.IsGameReady                           // bool - player exists?
GameTypes.Initialize()                          // Force initialization
GameTypes.Refresh()                             // Clear cache and reinit
GameTypes.CreateItemInstance(def, qty)          // Create item instance
GameTypes.AddItemToInventory(def, qty)          // Add item to player
GameTypes.LogDiagnostics()                      // Log all found objects
```

---

## Creating New Features

### Step 1: Create the Feature Class

```csharp
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    public class MyFeature : FeatureBase
    {
        // Required overrides
        public override string Id => "myfeature";           // Unique ID
        public override string Name => "My Feature";        // Display name
        public override string Description => "Does stuff"; // Tooltip
        public override FeatureCategory Category => FeatureCategory.Player;
        
        // Optional: Set to false for action-only features (buttons)
        public override bool IsToggleable => true;
        
        // Feature properties (shown in UI)
        public float MyValue { get; set; } = 1.0f;
        
        // Called when feature is enabled
        public override void OnEnable()
        {
            SewerLogger.Debug("MyFeature enabled");
        }
        
        // Called when feature is disabled
        public override void OnDisable()
        {
            // Restore original state here!
            SewerLogger.Debug("MyFeature disabled");
        }
        
        // Called every frame while enabled
        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            
            SafeExecute(() =>
            {
                // Your logic here
                var player = GameTypes.LocalPlayer;
                if (player == null) return;
                
                // Do stuff...
            }, "updating my feature");
        }
        
        // For non-toggleable features (buttons)
        public override void Execute()
        {
            // One-time action
        }
    }
}
```

### Step 2: Register the Feature

In `Features/Base/FeatureManager.cs`, add to `RegisterFeatures()`:

```csharp
private void RegisterFeatures()
{
    // ... existing features ...
    
    // Add your feature
    RegisterFeature(new MyFeature());
}
```

### Step 3: Add UI (Optional)

In the appropriate page (e.g., `UI/Pages/PlayerPage.cs`):

```csharp
var myFeature = FeatureManager.Instance.GetFeature<MyFeature>("myfeature");
if (myFeature != null)
{
    // Toggle
    bool enabled = DrawToggle("My Feature", myFeature.IsEnabled, "Description");
    if (enabled != myFeature.IsEnabled) myFeature.IsEnabled = enabled;
    
    // Slider (if enabled)
    if (myFeature.IsEnabled)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        myFeature.MyValue = DrawSlider("Value", myFeature.MyValue, 0f, 10f, "F1");
        GUILayout.EndHorizontal();
    }
}
```

---

## UI System

### MenuController
- Main window management
- Tab navigation
- Window dragging
- Cursor lock/unlock

### SewerSkin
Static class for consistent styling:
```csharp
SewerSkin.BeginUI();           // Call at start of OnGUI
SewerSkin.EndUI();             // Call at end of OnGUI
SewerSkin.DrawSeparator();     // Horizontal line
SewerSkin.BeginBox();          // Start styled box
SewerSkin.EndBox();            // End styled box
SewerSkin.DrawAccentButton();  // Highlighted button
SewerSkin.DrawStatus();        // Status message
```

### PageBase Helper Methods
```csharp
DrawSection("TITLE");                           // Section header
DrawInfo("Label", "Value");                     // Info display
DrawToggle("Name", value, "tooltip");           // Toggle checkbox
DrawSlider("Name", value, min, max, format);    // Slider
DrawTextField(text, width);                     // Text input
DrawButton("Text", width);                      // Button
```

---

## Common Patterns

### Storing Original Values
Always store original values to restore on disable:

```csharp
private static float _originalValue = 1f;
private static bool _hasStoredOriginal = false;

public override void OnUpdate()
{
    if (!_hasStoredOriginal)
    {
        _originalValue = GameTypes.Movement.SomeProperty;
        _hasStoredOriginal = true;
    }
    // Apply modified value...
}

public override void OnDisable()
{
    if (_hasStoredOriginal)
    {
        GameTypes.Movement.SomeProperty = _originalValue;
    }
}
```

### Safe Execution
Always wrap game access in SafeExecute:

```csharp
SafeExecute(() =>
{
    var player = GameTypes.LocalPlayer;
    if (player == null) return;
    
    // Your code...
}, "description of action");
```

### Caching UI Values
For expensive operations, cache values:

```csharp
private float _cachedValue = 0;
private float _lastCacheTime = 0;
private const float CacheInterval = 1f;

private void RefreshCache()
{
    if (Time.time - _lastCacheTime < CacheInterval) return;
    _lastCacheTime = Time.time;
    
    _cachedValue = GameTypes.Money.cashBalance;
}
```

---

## Debugging

### MelonLoader Log
Check: `C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader\Latest.log`

### Logging Methods
```csharp
SewerLogger.Info("Message");
SewerLogger.Debug("Debug message");
SewerLogger.Warning("Warning");
SewerLogger.Error("Error", exception);
SewerLogger.Success("Success!");
```

### Type Discovery
Use `GameTypes.LogDiagnostics()` to see what's found:
```
=== GAME TYPES DIAGNOSTICS ===
LocalPlayer: Player_0
Movement: FOUND
Health: FOUND
...
=== END DIAGNOSTICS ===
```

### Finding New Types
If you need to discover new types at runtime:
```csharp
// Find all MonoBehaviours
var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
foreach (var obj in allObjects)
{
    SewerLogger.Debug($"Found: {obj.GetType().FullName}");
}
```

---

## Building & Deployment

### Build Command
```bash
cd "C:\Users\liamt\Desktop\SewerMenu"
dotnet build
```

### Output
- DLL: `bin\Debug\SewerMenu.dll`
- Auto-copied to: `C:\Program Files (x86)\Steam\steamapps\common\Schedule I\Mods\`

### Post-Build (in .csproj)
```xml
<Target Name="CopyToMods" AfterTargets="Build">
    <Copy SourceFiles="$(TargetPath)" 
          DestinationFolder="$(GamePath)\Mods\" />
</Target>
```

### Testing
1. Build the project
2. Launch Schedule I
3. Press F8 to open menu
4. Check MelonLoader log for errors

---

## Quick Reference: Key Types

### Player
```csharp
// Access
var player = GameTypes.LocalPlayer;
var movement = GameTypes.Movement;
var health = GameTypes.Health;

// Properties
movement.MoveSpeedMultiplier  // float - movement speed
movement.CurrentStaminaReserve // float - stamina
movement.IsJumping            // bool
movement.IsGrounded           // bool
movement.movementY            // float - vertical velocity
health.CurrentHealth          // float
health.IsAlive               // bool
```

### Money
```csharp
var money = GameTypes.Money;
money.cashBalance             // float (GET ONLY!)
money.onlineBalance           // float (get/set)
money.ChangeCashBalance(amount, true, true);  // Modify cash
money.GetNetWorth();          // float
```

### Time
```csharp
var time = GameTypes.Time;
time.CurrentTime              // int (minutes 0-1440)
time.ElapsedDays             // int
time.IsNight                 // bool
time.SetTime(minutes, false); // Set time
time.TimeProgressionMultiplier // float - time speed
```

### Items
```csharp
var items = GameTypes.GetAllItemDefinitions();
var item = GameTypes.GetItemById("weed");
item.ID                       // string
item.Name                     // string
item.StackLimit              // int
item.Category                // EItemCategory
GameTypes.AddItemToInventory(item, 10);
```

---

## Updating GAME_TYPES.md

If the game updates and you need to regenerate the type documentation:

1. Use `Utils/TypeDiscovery.cs` (or create a similar tool)
2. Run it in-game to enumerate all types from Assembly-CSharp
3. Format output as markdown
4. Save to GAME_TYPES.md

The TypeDiscovery tool iterates through all types in the game assembly and extracts:
- Class names and inheritance
- Properties with types and access modifiers
- Methods with parameters and return types
- Enums and their values

---

## Troubleshooting

### "Type not found" errors
- Check the namespace in GAME_TYPES.md
- Ensure using statement is correct with `Il2Cpp` prefix
- Verify the type exists in the game version

### Feature doesn't restore on disable
- Use static variables for original values
- Store original value ONCE (first time only)
- Check that OnDisable actually runs

### UI not showing
- Check MenuController.Initialize() is called
- Verify F8 keybind is registered
- Check for exceptions in log

### Items not spawning
- Ensure Registry is loaded (wait for game to fully load)
- Check item ID is correct
- Verify inventory has space

---

*Last Updated: December 2024*
*Game Version: Schedule I v0.4.2f9*
*Mod Version: SewerMenu v1.0.0*
