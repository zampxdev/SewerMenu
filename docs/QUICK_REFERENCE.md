# SewerMenu Quick Reference

## Essential Files
| File | Purpose |
|------|---------|
| `Utils/GameTypes.cs` | **Main game access** - all player/manager references |
| `GAME_TYPES.md` | **Type documentation** - 2908 types, properties, methods |
| `Features/Base/FeatureBase.cs` | Base class for all features |
| `UI/Pages/PageBase.cs` | Base class for UI pages |

---

## GameTypes Cheat Sheet

### Player Access
```csharp
GameTypes.LocalPlayer        // Player instance
GameTypes.Movement           // PlayerMovement (speed, stamina, jump)
GameTypes.Health             // PlayerHealth (HP, damage)
GameTypes.Inventory          // PlayerInventory (items)
GameTypes.PlayerPosition     // Vector3 position
GameTypes.PlayerTransform    // Transform
```

### Managers
```csharp
GameTypes.Money              // MoneyManager
GameTypes.Time               // TimeManager
GameTypes.Level              // LevelManager (XP/rank)
GameTypes.Law                // LawManager
GameTypes.Products           // ProductManager
GameTypes.Properties         // PropertyManager
```

### Collections
```csharp
GameTypes.GetAllNPCs()                    // NPC[]
GameTypes.GetAllPolice()                  // PoliceOfficer[]
GameTypes.GetAllItemDefinitions()         // List<ItemDefinition>
GameTypes.GetItemById("item_id")          // ItemDefinition
```

### Utility
```csharp
GameTypes.IsGameReady                     // bool - player exists?
GameTypes.AddItemToInventory(def, qty)    // Add item to player
GameTypes.LogDiagnostics()                // Debug output
```

---

## Common Properties

### PlayerMovement
```csharp
movement.MoveSpeedMultiplier   // float - speed multiplier
movement.CurrentStaminaReserve // float - current stamina
movement.IsJumping             // bool
movement.IsGrounded            // bool
movement.IsSprinting           // bool
movement.movementY             // float - vertical velocity
movement.CanJump               // bool
movement.CanMove               // bool
```

### MoneyManager
```csharp
money.cashBalance              // float (GET ONLY!)
money.onlineBalance            // float (get/set)
money.ChangeCashBalance(amt, true, true)  // Modify cash
money.GetNetWorth()            // float
money.LifetimeEarnings         // float
```

### TimeManager
```csharp
time.CurrentTime               // int (0-1440 minutes)
time.ElapsedDays               // int
time.CurrentDay                // EDay enum
time.IsNight                   // bool
time.SetTime(minutes, false)   // Set time
time.TimeProgressionMultiplier // float - time speed (0 = frozen)
```

### LevelManager
```csharp
level.Tier                     // int
level.XP                       // int
level.TotalXP                  // int
level.Rank                     // ERank enum
level.AddXP(amount)            // Add XP
level.IncreaseTier()           // Level up
```

### ItemDefinition
```csharp
item.ID                        // string
item.Name                      // string
item.Category                  // EItemCategory
item.StackLimit                // int
item.GetDefaultInstance(qty)   // ItemInstance
```

---

## Feature Template

```csharp
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    public class MyFeature : FeatureBase
    {
        public override string Id => "myfeature";
        public override string Name => "My Feature";
        public override string Description => "Does something";
        public override FeatureCategory Category => FeatureCategory.Player;
        
        public float Value { get; set; } = 1f;
        
        private static float _originalValue = 1f;
        private static bool _stored = false;

        public override void OnEnable()
        {
            SewerLogger.Debug("MyFeature enabled");
        }

        public override void OnDisable()
        {
            // ALWAYS restore original values!
            if (_stored)
            {
                GameTypes.Movement.SomeProperty = _originalValue;
            }
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            
            SafeExecute(() =>
            {
                var movement = GameTypes.Movement;
                if (movement == null) return;
                
                if (!_stored)
                {
                    _originalValue = movement.SomeProperty;
                    _stored = true;
                }
                
                movement.SomeProperty = _originalValue * Value;
            }, "updating feature");
        }
    }
}
```

---

## UI Page Template

```csharp
protected override void DrawContent()
{
    DrawSection("SECTION NAME");
    
    var feature = FeatureManager.Instance.GetFeature<MyFeature>("myfeature");
    if (feature != null)
    {
        // Toggle
        bool enabled = DrawToggle("Feature Name", feature.IsEnabled, "Tooltip");
        if (enabled != feature.IsEnabled) feature.IsEnabled = enabled;
        
        // Slider (when enabled)
        if (feature.IsEnabled)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            feature.Value = DrawSlider("Value", feature.Value, 1f, 10f, "F1");
            GUILayout.EndHorizontal();
        }
        
        // Button
        if (DrawButton("Do Thing", 100))
        {
            feature.DoThing();
        }
        
        // Info display
        DrawInfo("Label", "Value");
    }
}
```

---

## Namespace Reference

| Namespace | Contains |
|-----------|----------|
| `Il2CppScheduleOne.PlayerScripts` | Player, PlayerMovement, PlayerCamera, PlayerInventory |
| `Il2CppScheduleOne.PlayerScripts.Health` | PlayerHealth |
| `Il2CppScheduleOne.Money` | MoneyManager |
| `Il2CppScheduleOne.GameTime` | TimeManager |
| `Il2CppScheduleOne.Levelling` | LevelManager |
| `Il2CppScheduleOne.Law` | LawManager |
| `Il2CppScheduleOne.Police` | PoliceOfficer |
| `Il2CppScheduleOne.NPCs` | NPC |
| `Il2CppScheduleOne.Property` | Property, PropertyManager |
| `Il2CppScheduleOne.Product` | ProductManager |
| `Il2CppScheduleOne.ItemFramework` | ItemDefinition, ItemInstance, Registry |

---

## Build & Test

```bash
# Build
cd "C:\Users\liamt\Desktop\SewerMenu"
dotnet build

# Output location
bin\Debug\SewerMenu.dll

# Auto-copied to
C:\Program Files (x86)\Steam\steamapps\common\Schedule I\Mods\

# Log file
C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader\Latest.log
```

### Verify After Game Updates

Launch Schedule I once with MelonLoader installed so `MelonLoader\Il2CppAssemblies` is regenerated, then rebuild the mod against the regenerated assemblies.

---

## Hotkeys
| Key | Action |
|-----|--------|
| F8 | Toggle menu |
| ESC | Close menu |

---

## Searching GAME_TYPES.md

```bash
# Find a type
grep -n "MoneyManager" GAME_TYPES.md

# Find properties containing "cash"
grep -n "cash" GAME_TYPES.md

# Find all managers
grep -n "Manager$" GAME_TYPES.md

# Find methods
grep -n "AddXP\|AddCash\|SetTime" GAME_TYPES.md
```
