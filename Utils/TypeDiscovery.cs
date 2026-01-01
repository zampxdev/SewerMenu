// Temporary file to discover available IL2CPP types

// Player-related types
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerScripts.Health;

// Economy types
using Il2CppScheduleOne.Money;

// Law/Police types
using Il2CppScheduleOne.Law;

// Time types
using Il2CppScheduleOne.GameTime;

// Item/Inventory types
using Il2CppScheduleOne.ItemFramework;

// NPC types
using Il2CppScheduleOne.NPCs;

// Property types
using Il2CppScheduleOne.Property;

// Product types
using Il2CppScheduleOne.Product;

// Level/XP types
using Il2CppScheduleOne.Levelling;

// Additional namespaces to test
using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.Storage;

namespace SewerMenu.Utils
{
    public static class TypeDiscovery
    {
        public static void TestTypes()
        {
            // Player types
            Player player = null;
            PlayerMovement movement = null;
            PlayerHealth health = null;
            PlayerCamera camera = null;
            
            // Managers
            MoneyManager money = null;
            LawManager law = null;
            TimeManager time = null;
            
            // Test specific types from each namespace
            // ItemFramework
            ItemDefinition itemDef = null;
            ItemInstance itemInstance = null;
            
            // NPCs
            NPC npc = null;
            
            // Property
            PropertyManager propMgr = null;
            
            // Product
            ProductDefinition productDef = null;
            ProductManager productMgr = null;
            
            // Levelling
            LevelManager levelMgr = null;
            
            // Suppress warnings
            _ = player; _ = movement; _ = health; _ = camera;
            _ = money; _ = law; _ = time;
            _ = itemDef; _ = itemInstance;
            _ = npc;
            _ = productDef;
            _ = propMgr;
            _ = productMgr;
            _ = levelMgr;
        }
    }
}
