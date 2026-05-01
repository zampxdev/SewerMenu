using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerScripts.Health;
using Il2CppScheduleOne;

using Il2CppScheduleOne.Money;

using Il2CppScheduleOne.Law;

using Il2CppScheduleOne.GameTime;

using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Equipping;

using Il2CppScheduleOne.NPCs;

using Il2CppScheduleOne.Property;

using Il2CppScheduleOne.Product;

using Il2CppScheduleOne.Levelling;

using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne.Vehicles.Modification;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Economy;
using UnityEngine;

namespace SewerMenu.Utils
{
    public static class TypeDiscovery
    {
        public static void TestTypes()
        {
            Player player = null;
            PlayerMovement movement = null;
            PlayerHealth health = null;
            PlayerCamera camera = null;
            
            MoneyManager money = null;
            LawManager law = null;
            TimeManager time = null;
            
            ItemDefinition itemDef = null;
            ItemInstance itemInstance = null;
            IntegerItemInstance integerItem = null;
            Equippable_RangedWeapon rangedWeapon = null;
            
            NPC npc = null;
            
            PropertyManager propMgr = null;
            
            ProductDefinition productDef = null;
            ProductManager productMgr = null;
            
            LevelManager levelMgr = null;
            
            _ = player; _ = movement; _ = health; _ = camera;
            _ = money; _ = law; _ = time;
            _ = itemDef; _ = itemInstance; _ = integerItem; _ = rangedWeapon;
            _ = npc;
            _ = productDef;
            _ = propMgr;
            _ = productMgr;
            _ = levelMgr;
        }

        public static void TestFeatureMembers()
        {
            Player player = null;
            Player localPlayer = Player.Local;
            bool isOwner = player.IsOwner;
            bool isLocalPlayer = player.IsLocalPlayer;
            PlayerHealth playerHealth = player.Health;
            PlayerEnergy playerEnergy = player.Energy;
            PlayerInventory inventory = null;
            ItemInstance equippedItem = player.GetEquippedItem();

            PlayerMovement movement = null;
            float stamina = movement.CurrentStaminaReserve;
            float speed = PlayerMovement.StaticMoveSpeedMultiplier;
            bool isGrounded = movement.IsGrounded;
            LandVehicle currentVehicle = movement.CurrentVehicle;
            movement.SetStamina(stamina, false);
            movement.SetResidualVelocity(Vector3.up, 1f, 1f);
            PlayerMovement.StaticMoveSpeedMultiplier = speed;

            float currentHealth = playerHealth.CurrentHealth;
            bool isAlive = playerHealth.IsAlive;
            playerHealth.SetHealth(currentHealth);

            float currentEnergy = playerEnergy.CurrentEnergy;
            playerEnergy.SetEnergy(currentEnergy);
            playerEnergy.RestoreEnergy();

            MoneyManager money = null;
            float cash = money.cashBalance;
            float online = money.onlineBalance;
            float lifetime = money.LifetimeEarnings;
            money.ChangeCashBalance(cash, true, true);
            money.CreateOnlineTransaction("Deposit", online, 1f, "SewerMenu");
            money.onlineBalance = online;
            float netWorth = money.GetNetWorth();

            LevelManager level = null;
            int tier = level.Tier;
            int xp = level.XP;
            int totalXp = level.TotalXP;
            float xpToNextTier = level.XPToNextTier;
            object rank = level.Rank;
            level.Tier = tier;
            level.XP = xp;
            level.TotalXP = totalXp;
            level.AddXP(xp);
            level.IncreaseTier();

            TimeManager time = null;
            int currentTime = time.CurrentTime;
            int elapsedDays = time.ElapsedDays;
            object currentDay = time.CurrentDay;
            bool isNight = time.IsNight;
            float timeMultiplier = time.TimeSpeedMultiplier;
            time.SetTime(currentTime);
            time.SetTimeAndSync(currentTime);
            time.ElapsedDays = elapsedDays;
            time.SetTimeSpeedMultiplier(timeMultiplier);

            Registry registry = null;
            Registry registryInstance = Registry.Instance;
            bool registryInstanceExists = Registry.InstanceExists;
            Il2CppSystem.Collections.Generic.List<ItemDefinition> items = registry.GetAllItems();
            object itemRegistry = registry.ItemRegistry;
            object runtimeItems = registry.ItemsAddedAtRuntime;

            ItemDefinition itemDefinition = null;
            string itemId = itemDefinition.ID;
            string itemName = itemDefinition.Name;
            object itemCategory = itemDefinition.Category;
            int stackLimit = itemDefinition.StackLimit;
            itemDefinition.StackLimit = stackLimit;
            ItemInstance itemInstance = itemDefinition.GetDefaultInstance(1);
            object itemEquippable = itemInstance.Equippable;
            bool canFit = inventory.CanItemFitInInventory(itemInstance, 1);
            uint amountOfItem = inventory.GetAmountOfItem(itemId);
            bool inventoryInstanceExists = PlayerInventory.InstanceExists;
            PlayerInventory inventoryInstance = PlayerInventory.Instance;
            inventory.AddItemToInventory(itemInstance);

            IntegerItemInstance integerItem = null;
            int integerValue = integerItem.Value;
            int integerQuantity = integerItem.Quantity;
            integerItem.SetValue(integerValue);
            integerItem.SetQuantity(integerQuantity);

            Equippable_RangedWeapon rangedWeapon = null;
            int weaponAmmo = rangedWeapon.Ammo;
            int magazineSize = rangedWeapon.MagazineSize;
            IntegerItemInstance weaponItem = rangedWeapon.weaponItem;
            bool weaponCanReload = rangedWeapon.CanReload;
            bool weaponCanFire = rangedWeapon.CanFire(false);
            rangedWeapon.MagazineSize = magazineSize;
            rangedWeapon.CanReload = weaponCanReload;
            rangedWeapon.Reload();

            QualityItemInstance qualityItem = null;
            EQuality quality = qualityItem.Quality;
            qualityItem.SetQuality(quality);

            ItemPickup itemPickup = null;
            ItemDefinition itemToGive = itemPickup.ItemToGive;

            NPC npc = null;
            object npcMovement = npc.Movement;
            object npcHealth = npc.Health;
            string firstName = npc.FirstName;
            bool isConscious = npc.IsConscious;

            PoliceOfficer officer = null;
            float suspicion = officer.Suspicion;
            officer.Suspicion = suspicion;
            officer.AutoDeactivate = true;
            officer.Deactivate();

            Property property = null;
            string propertyName = property.PropertyName;
            string propertyCode = property.PropertyCode;
            bool isOwned = property.IsOwned;
            float price = property.Price;
            property.IsOwned = isOwned;
            property.Price = price;

            PropertyManager propertyManager = null;
            Property foundProperty = propertyManager.GetProperty(propertyCode);

            ProductDefinition productDefinition = null;
            string productId = productDefinition.ID;
            string productName = productDefinition.Name;

            ProductManager productManager = null;
            object allProducts = productManager.AllProducts;
            productManager.DiscoverProduct(productId);

            Plant plant = null;
            bool plantGrown = plant.IsFullyGrown;
            plant.SetNormalizedGrowthProgress(1f);

            ShroomColony colony = null;
            bool colonyGrown = colony.IsFullyGrown;
            colony.SetFullyGrown();

            VehicleManager vehicleManager = null;
            Il2CppSystem.Collections.Generic.List<LandVehicle> vehiclePrefabs = vehicleManager.VehiclePrefabs;

            LandVehicle vehicle = null;
            var vehicleData = vehicle.GetVehicleData();
            string vehicleCode = vehicleData.VehicleCode;
            object vehicleColorComponent = vehicle.Color;
            bool isPlayerOwned = vehicle.IsPlayerOwned;
            EVehicleColor vehicleColor = default;
            vehicle.ApplyColor(vehicleColor);
            vehicle.IsPlayerOwned = isPlayerOwned;
            LandVehicle spawnedVehicle = vehicleManager.SpawnAndReturnVehicle(vehicleData.VehicleCode, Vector3.zero, Quaternion.identity, false);

            VehicleColors vehicleColors = VehicleColors.Instance;
            string vehicleColorName = vehicleColors.GetColorName(vehicleColor);

            Dealer dealer = null;
            string dealerName = dealer.FirstName;
            bool recruited = dealer.IsRecruited;

            Customer customer = null;
            NPC customerNpc = customer.NPC;
            bool awaitingDelivery = customer.IsAwaitingDelivery;

            CashPickup cashPickup = null;
            float cashValue = cashPickup.Value;

            _ = localPlayer; _ = isOwner; _ = isLocalPlayer; _ = playerHealth; _ = playerEnergy; _ = equippedItem;
            _ = isGrounded; _ = currentVehicle; _ = isAlive; _ = netWorth; _ = lifetime;
            _ = xpToNextTier; _ = rank; _ = currentDay; _ = isNight; _ = registryInstance; _ = registryInstanceExists;
            _ = items; _ = itemRegistry; _ = runtimeItems;
            _ = itemId; _ = itemName; _ = itemCategory; _ = itemEquippable; _ = itemToGive; _ = npcMovement;
            _ = npcHealth; _ = firstName; _ = isConscious; _ = propertyName; _ = foundProperty;
            _ = productName; _ = allProducts; _ = plantGrown; _ = colonyGrown; _ = vehiclePrefabs;
            _ = vehicleCode; _ = spawnedVehicle; _ = vehicleColorComponent; _ = vehicleColorName; _ = dealerName;
            _ = recruited; _ = customerNpc; _ = awaitingDelivery; _ = cashValue;
            _ = integerValue; _ = weaponAmmo; _ = weaponItem; _ = weaponCanFire;
            _ = canFit; _ = amountOfItem; _ = inventoryInstanceExists; _ = inventoryInstance;
        }
    }
}
