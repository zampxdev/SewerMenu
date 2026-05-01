# SewerMenu Project Map

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        SewerMenuMod                              │
│                    (MelonMod Entry Point)                        │
│         OnInitializeMelon → OnUpdate → OnGUI → OnQuit           │
└─────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│ ConfigManager │    │FeatureManager │    │MenuController │
│  (Settings)   │    │  (Features)   │    │    (UI)       │
└───────────────┘    └───────────────┘    └───────────────┘
                              │                     │
                     ┌────────┴────────┐           │
                     ▼                 ▼           ▼
              ┌──────────┐      ┌──────────┐ ┌──────────┐
              │FeatureA  │      │FeatureB  │ │  Pages   │
              │(IFeature)│      │(IFeature)│ │(PageBase)│
              └──────────┘      └──────────┘ └──────────┘
```

## Directory Structure

```
SewerMenu/
│
├── Core/                           # Core Infrastructure
│   ├── SewerMenuMod.cs            # Main MelonMod entry point
│   ├── ModInfo.cs                 # Version, author, metadata
│   ├── Config/
│   │   ├── ConfigManager.cs       # Load/save configuration
│   │   └── SewerConfig.cs         # Config data structures
│   ├── Keybinds/
│   │   └── KeybindManager.cs      # Hotkey capture system
│   └── Logging/
│       └── SewerLogger.cs         # Logging wrapper
│
├── Features/                       # All Cheat Features
│   ├── Base/
│   │   ├── IFeature.cs            # Feature interface
│   │   ├── FeatureBase.cs         # Abstract base class
│   │   ├── FeatureCategory.cs     # Category enum
│   │   └── FeatureManager.cs      # Registration & lifecycle
│   │
│   ├── Player/                    # Player-related features
│   │   ├── GodMode.cs             # Invincibility
│   │   ├── HealthEnergy.cs        # Health/energy manipulation
│   │   ├── InfiniteStamina.cs     # No stamina drain
│   │   ├── SprintSpeed.cs         # Speed multiplier
│   │   ├── JumpHeight.cs          # Jump multiplier
│   │   ├── NoClip.cs              # Fly through walls
│   │   ├── FlyMode.cs             # Free flight
│   │   └── Teleport.cs            # Save/load locations
│   │
│   ├── Economy/                   # Money & progression
│   │   ├── MoneyEditor.cs         # Cash/bank balance
│   │   ├── XPEditor.cs            # Experience points
│   │   ├── UnlockProducts.cs      # Unlock all products
│   │   └── FreePurchases.cs       # Free buying
│   │
│   ├── Items/                     # Item manipulation
│   │   ├── ItemSpawner.cs         # Spawn any item
│   │   ├── StackSizeModifier.cs   # Increase stack sizes
│   │   ├── InfiniteItems.cs       # Items not consumed
│   │   ├── QualityOverride.cs     # Set item quality
│   │   └── InstantGrow.cs         # Instant plant growth
│   │
│   ├── World/                     # World manipulation
│   │   ├── TimeController.cs      # Time of day control
│   │   ├── PoliceDisable.cs       # Disable police spawns
│   │   ├── NeverWanted.cs         # Auto-clear wanted
│   │   ├── NPCFreeze.cs           # Freeze all NPCs
│   │   └── UnlockProperties.cs    # Unlock all properties
│   │
│   ├── Vehicles/                  # Vehicle features
│   │   ├── VehicleSpawner.cs      # Spawn vehicles
│   │   └── VehicleGodMode.cs      # Vehicle invincibility + utilities
│   │
│   └── Misc/                      # Miscellaneous
│       ├── ESP.cs                 # See through walls
│       ├── Freecam.cs             # Detached camera
│       └── DebugOverlay.cs        # FPS, position display
│
├── UI/                            # User Interface (IMGUI)
│   ├── MenuController.cs          # Main window, tabs, lifecycle
│   ├── SewerSkin.cs               # Styling, colors, custom drawing
│   ├── Theme.cs                   # Color palette definitions
│   ├── Styles.cs                  # GUIStyle definitions
│   ├── InputHelper.cs             # Input handling utilities
│   │
│   ├── Pages/                     # Tab pages
│   │   ├── IPage.cs               # Page interface
│   │   ├── PageBase.cs            # Abstract base class
│   │   ├── PlayerPage.cs          # Player features tab
│   │   ├── EconomyPage.cs         # Economy features tab
│   │   ├── ItemsPage.cs           # Items features tab
│   │   ├── WorldPage.cs           # World features tab
│   │   ├── VehiclesPage.cs        # Vehicles features tab
│   │   ├── MiscPage.cs            # Misc features tab
│   │   └── SettingsPage.cs        # Settings & keybinds
│   │
│   ├── Windows/                   # Popup windows
│   │   └── ItemSpawnerWindow.cs   # Item selection popup
│   │
│   └── Components/
│       └── UIComponents.cs        # Reusable UI components
│
├── Utils/                         # Utilities
│   ├── GameTypes.cs               # Game type access (Player, Managers)
│   ├── GameFinder.cs              # GameObject discovery
│   └── TypeDiscovery.cs           # Runtime type inspection
│
├── Properties/
│   └── AssemblyInfo.cs            # Assembly attributes
│
├── SewerMenu.csproj               # Project file
├── SewerMenu.sln                  # Solution file
├── AGENTS.md                      # This development guide
├── GAME_TYPES.md                  # Game type reference
├── KEY_TYPES.md                   # Important types summary
└── DOCUMENTATION.md               # User documentation
```

## Data Flow

### Initialization Flow
```
MelonLoader loads SewerMenu.dll
    │
    ▼
SewerMenuMod.OnInitializeMelon()
    ├── SewerLogger.Initialize()
    ├── ConfigManager.Initialize()
    ├── KeybindManager.Initialize()
    ├── FeatureManager.Initialize()
    │       └── RegisterAllFeatures() → Creates all feature instances
    ├── SewerSkin.Initialize()
    └── MenuController.Initialize()
    
OnLateInitializeMelon()
    ├── ConfigManager.Load()
    └── ApplySavedFeatureStates()
```

### Runtime Flow (Every Frame)
```
SewerMenuMod.OnUpdate()
    ├── MenuController.Update()     # Handle F8 toggle, ESC close
    ├── FeatureManager.Update()     # Update all enabled features
    │       └── foreach feature: feature.OnUpdate()
    └── KeybindManager.Update()     # Process hotkey captures

SewerMenuMod.OnGUI()
    ├── MenuController.OnGUI()      # Draw menu window
    │       └── CurrentPage.Draw()  # Draw active tab content
    └── FeatureManager.OnGUI()      # Draw feature overlays (ESP, etc.)
```

### Feature Lifecycle
```
Feature Registration:
    FeatureManager.RegisterFeature(new GodMode())
        └── feature.OnRegister()

Feature Enable:
    feature.IsEnabled = true
        └── feature.OnEnable()

Feature Update (while enabled):
    FeatureManager.Update()
        └── feature.OnUpdate()

Feature Disable:
    feature.IsEnabled = false
        └── feature.OnDisable()
```

## Key Classes

### FeatureBase
Base class for all features. Provides:
- `IsEnabled` property with auto-logging
- `SafeExecute()` for error handling
- Lifecycle hooks: `OnEnable`, `OnDisable`, `OnUpdate`, `OnGUI`

### GameTypes
Static class for accessing game objects:
- `LocalPlayer` - The player instance
- `Health` - PlayerHealth component
- `Energy` - PlayerEnergy component
- `Movement` - PlayerMovement component
- `Money` - MoneyManager singleton
- `Time` - TimeManager singleton
- `Vehicles` - VehicleManager singleton

### SewerSkin
UI styling system:
- `DrawSection()` - Section headers
- `DrawToggle()` - Feature toggles
- `DrawSlider()` - Value sliders
- `DrawButton()` - Styled buttons
- `DrawNumericInput()` - IL2CPP-safe number input
- Color constants: `AccentColor`, `TextColor`, `SuccessColor`, etc.

### MenuController
Main menu window:
- Tab navigation
- Window dragging/resizing
- Page rendering
- Cursor lock management

## Dependencies

```
MelonLoader v0.7.1
├── MelonLoader.dll
├── 0Harmony.dll
├── Il2CppInterop.Runtime.dll
└── Il2CppInterop.Common.dll

Unity IL2CPP Assemblies
├── Assembly-CSharp.dll (Game code)
├── UnityEngine.CoreModule.dll
├── UnityEngine.IMGUIModule.dll
└── UnityEngine.InputLegacyModule.dll

NuGet
└── Newtonsoft.Json 13.0.3
```
