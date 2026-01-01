using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime;
using SewerMenu.Core.Logging;

// Import game types from Assembly-CSharp.dll
// These are the IL2CPP-generated types with Il2Cpp prefix
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerScripts.Health;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Law;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne;
using Il2CppSystem.Collections.Generic;

namespace SewerMenu.Utils
{
    /// <summary>
    /// Provides direct access to game types using IL2CPP interop.
    /// All game object access should go through this class.
    /// 
    /// Key Types Reference:
    /// - Player: Main player class (IsOwner for local player detection)
    /// - PlayerMovement: MoveSpeedMultiplier, CurrentStaminaReserve, Teleport()
    /// - PlayerHealth: CurrentHealth, SetHealth(), RecoverHealth(), IsAlive
    /// - MoneyManager: cashBalance (get), onlineBalance (get/set), ChangeCashBalance()
    /// - TimeManager: CurrentTime, SetTime(), ElapsedDays, CurrentDay
    /// - LawManager: PoliceCalled()
    /// </summary>
    public static class GameTypes
    {
        #region Cached References
        
        // Player components
        private static Player _localPlayer;
        private static PlayerMovement _playerMovement;
        private static PlayerHealth _playerHealth;
        private static PlayerEnergy _playerEnergy;
        private static PlayerCamera _playerCamera;
        private static PlayerInventory _playerInventory;
        
        // Managers
        private static MoneyManager _moneyManager;
        private static LawManager _lawManager;
        private static TimeManager _timeManager;
        private static LevelManager _levelManager;
        private static ProductManager _productManager;
        private static PropertyManager _propertyManager;
        private static VehicleManager _vehicleManager;
        
        // State
        private static bool _initialized = false;
        private static float _lastRefreshTime = 0f;
        private const float RefreshInterval = 2f;
        
        #endregion
        
        #region Initialization
        
        public static void Initialize()
        {
            if (_initialized && UnityEngine.Time.time - _lastRefreshTime < RefreshInterval)
                return;
            
            try
            {
                FindLocalPlayer();
                FindManagers();
                _initialized = true;
                _lastRefreshTime = UnityEngine.Time.time;
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to initialize GameTypes", ex);
            }
        }
        
        public static void Refresh()
        {
            ClearCache();
            Initialize();
        }
        
        public static void ClearCache()
        {
            _localPlayer = null;
            _playerMovement = null;
            _playerHealth = null;
            _playerEnergy = null;
            _playerCamera = null;
            _playerInventory = null;
            _moneyManager = null;
            _lawManager = null;
            _timeManager = null;
            _levelManager = null;
            _productManager = null;
            _propertyManager = null;
            _vehicleManager = null;
            _initialized = false;
            SewerLogger.Debug("GameTypes cache cleared");
        }
        
        #endregion
        
        #region Player Access
        
        /// <summary>
        /// Gets the local player instance.
        /// </summary>
        public static Player LocalPlayer
        {
            get
            {
                if (_localPlayer == null || !_localPlayer)
                    FindLocalPlayer();
                return _localPlayer;
            }
        }
        
        /// <summary>
        /// Gets the player's movement component.
        /// Key properties: MoveSpeedMultiplier, CurrentStaminaReserve, IsSprinting, CanJump
        /// Key methods: Teleport(Vector3, bool), SetCrouched(bool)
        /// </summary>
        public static PlayerMovement Movement
        {
            get
            {
                if (_playerMovement == null || !_playerMovement)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
                        _playerMovement = player.GetComponent<PlayerMovement>();
                        if (_playerMovement == null)
                            _playerMovement = player.GetComponentInChildren<PlayerMovement>();
                    }
                }
                return _playerMovement;
            }
        }
        
        /// <summary>
        /// Gets the player's health component.
        /// Key properties: CurrentHealth, IsAlive, CanTakeDamage
        /// Key methods: SetHealth(float), RecoverHealth(float), TakeDamage(float, bool, bool)
        /// </summary>
        public static PlayerHealth Health
        {
            get
            {
                if (_playerHealth == null || !_playerHealth)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
                        // PlayerHealth is accessed via Player.Health property
                        _playerHealth = player.Health;
                        if (_playerHealth == null)
                        {
                            _playerHealth = player.GetComponent<PlayerHealth>();
                            if (_playerHealth == null)
                                _playerHealth = player.GetComponentInChildren<PlayerHealth>();
                        }
                    }
                }
                return _playerHealth;
            }
        }
        
        /// <summary>
        /// Gets the player's energy component.
        /// Key properties: CurrentEnergy, DEBUG_DISABLE_ENERGY
        /// Key methods: SetEnergy(float), RestoreEnergy(), ChangeEnergy(float)
        /// </summary>
        public static PlayerEnergy Energy
        {
            get
            {
                if (_playerEnergy == null || !_playerEnergy)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
                        // PlayerEnergy is accessed via Player.Energy property
                        _playerEnergy = player.Energy;
                        if (_playerEnergy == null)
                        {
                            _playerEnergy = player.GetComponent<PlayerEnergy>();
                            if (_playerEnergy == null)
                                _playerEnergy = player.GetComponentInChildren<PlayerEnergy>();
                        }
                    }
                }
                return _playerEnergy;
            }
        }
        
        /// <summary>
        /// Gets the player's camera component.
        /// Key properties: FreeCamEnabled, canLook, Camera
        /// </summary>
        public static PlayerCamera Camera
        {
            get
            {
                if (_playerCamera == null || !_playerCamera)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
                        _playerCamera = player.GetComponent<PlayerCamera>();
                        if (_playerCamera == null)
                            _playerCamera = player.GetComponentInChildren<PlayerCamera>();
                        if (_playerCamera == null)
                            _playerCamera = UnityEngine.Object.FindObjectOfType<PlayerCamera>();
                    }
                }
                return _playerCamera;
            }
        }
        
        /// <summary>
        /// Gets the player's inventory.
        /// </summary>
        public static PlayerInventory Inventory
        {
            get
            {
                if (_playerInventory == null || !_playerInventory)
                {
                    _playerInventory = UnityEngine.Object.FindObjectOfType<PlayerInventory>();
                }
                return _playerInventory;
            }
        }
        
        /// <summary>
        /// Gets the player's Transform for position/teleport operations.
        /// </summary>
        public static Transform PlayerTransform => LocalPlayer?.transform;
        
        /// <summary>
        /// Gets or sets the player's current position.
        /// </summary>
        public static Vector3 PlayerPosition
        {
            get => PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
            set
            {
                if (PlayerTransform != null)
                    PlayerTransform.position = value;
            }
        }
        
        /// <summary>
        /// Gets the player's GameObject.
        /// </summary>
        public static GameObject PlayerGameObject => LocalPlayer?.gameObject;
        
        private static void FindLocalPlayer()
        {
            try
            {
                var players = UnityEngine.Object.FindObjectsOfType<Player>();
                foreach (var player in players)
                {
                    if (player == null) continue;
                    
                    try
                    {
                        // IsOwner is the FishNet networking property for local player
                        if (player.IsOwner)
                        {
                            _localPlayer = player;
                            SewerLogger.Debug($"Found local player via IsOwner: {player.name}");
                            return;
                        }
                    }
                    catch { }
                    
                    try
                    {
                        // Alternative: IsLocalPlayer property
                        if (player.IsLocalPlayer)
                        {
                            _localPlayer = player;
                            SewerLogger.Debug($"Found local player via IsLocalPlayer: {player.name}");
                            return;
                        }
                    }
                    catch { }
                }
                
                // Fallback: Find by name
                var playerGO = GameObject.Find("Player");
                if (playerGO != null)
                {
                    _localPlayer = playerGO.GetComponent<Player>();
                    if (_localPlayer != null)
                    {
                        SewerLogger.Debug($"Found player by name: {playerGO.name}");
                        return;
                    }
                }
                
                // Fallback: Single player
                if (players.Length == 1)
                {
                    _localPlayer = players[0];
                    SewerLogger.Debug($"Found single player: {_localPlayer.name}");
                    return;
                }
                
                // Fallback: Tag
                playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null)
                {
                    _localPlayer = playerGO.GetComponent<Player>();
                    if (_localPlayer != null)
                    {
                        SewerLogger.Debug($"Found player by tag: {playerGO.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding local player", ex);
            }
        }
        
        #endregion
        
        #region Manager Access
        
        /// <summary>
        /// Gets the MoneyManager singleton.
        /// Key properties: cashBalance (get only), onlineBalance (get/set)
        /// Key methods: ChangeCashBalance(float, bool, bool), CreateOnlineTransaction(...)
        /// </summary>
        public static MoneyManager Money
        {
            get
            {
                if (_moneyManager == null || !_moneyManager)
                {
                    _moneyManager = UnityEngine.Object.FindObjectOfType<MoneyManager>();
                    if (_moneyManager != null)
                        SewerLogger.Debug("Found MoneyManager");
                }
                return _moneyManager;
            }
        }
        
        /// <summary>
        /// Gets the LawManager singleton.
        /// Key methods: PoliceCalled(Player, Crime)
        /// </summary>
        public static LawManager Law
        {
            get
            {
                if (_lawManager == null || !_lawManager)
                {
                    _lawManager = UnityEngine.Object.FindObjectOfType<LawManager>();
                    if (_lawManager != null)
                        SewerLogger.Debug("Found LawManager");
                }
                return _lawManager;
            }
        }
        
        /// <summary>
        /// Gets the TimeManager singleton.
        /// Key properties: CurrentTime, ElapsedDays, CurrentDay, IsNight
        /// Key methods: SetTime(int, bool)
        /// </summary>
        public static TimeManager Time
        {
            get
            {
                if (_timeManager == null || !_timeManager)
                {
                    _timeManager = UnityEngine.Object.FindObjectOfType<TimeManager>();
                    if (_timeManager != null)
                        SewerLogger.Debug("Found TimeManager");
                }
                return _timeManager;
            }
        }
        
        /// <summary>
        /// Gets the LevelManager singleton.
        /// </summary>
        public static LevelManager Level
        {
            get
            {
                if (_levelManager == null || !_levelManager)
                {
                    _levelManager = UnityEngine.Object.FindObjectOfType<LevelManager>();
                    if (_levelManager != null)
                        SewerLogger.Debug("Found LevelManager");
                }
                return _levelManager;
            }
        }
        
        /// <summary>
        /// Gets the ProductManager singleton.
        /// </summary>
        public static ProductManager Products
        {
            get
            {
                if (_productManager == null || !_productManager)
                {
                    _productManager = UnityEngine.Object.FindObjectOfType<ProductManager>();
                    if (_productManager != null)
                        SewerLogger.Debug("Found ProductManager");
                }
                return _productManager;
            }
        }
        
        /// <summary>
        /// Gets the PropertyManager singleton.
        /// </summary>
        public static PropertyManager Properties
        {
            get
            {
                if (_propertyManager == null || !_propertyManager)
                {
                    _propertyManager = UnityEngine.Object.FindObjectOfType<PropertyManager>();
                    if (_propertyManager != null)
                        SewerLogger.Debug("Found PropertyManager");
                }
                return _propertyManager;
            }
        }
        
        /// <summary>
        /// Gets the VehicleManager singleton.
        /// Key properties: VehiclePrefabs, AllVehicles, PlayerOwnedVehicles
        /// Key methods: GetVehiclePrefab(string), SpawnAndReturnVehicle(...)
        /// </summary>
        public static VehicleManager Vehicles
        {
            get
            {
                if (_vehicleManager == null || !_vehicleManager)
                {
                    _vehicleManager = UnityEngine.Object.FindObjectOfType<VehicleManager>();
                    if (_vehicleManager != null)
                        SewerLogger.Debug("Found VehicleManager");
                }
                return _vehicleManager;
            }
        }
        
        private static void FindManagers()
        {
            try
            {
                _ = Money;
                _ = Law;
                _ = Time;
                _ = Level;
                _ = Products;
                _ = Properties;
                _ = Vehicles;
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding managers", ex);
            }
        }
        
        #endregion
        
        #region NPC Access
        
        /// <summary>
        /// Gets all NPCs in the scene.
        /// </summary>
        public static NPC[] GetAllNPCs()
        {
            try
            {
                return UnityEngine.Object.FindObjectsOfType<NPC>();
            }
            catch
            {
                return new NPC[0];
            }
        }
        
        #endregion
        
        #region Police Access
        
        /// <summary>
        /// Gets all police officers in the scene.
        /// </summary>
        public static PoliceOfficer[] GetAllPolice()
        {
            try
            {
                return UnityEngine.Object.FindObjectsOfType<PoliceOfficer>();
            }
            catch
            {
                return new PoliceOfficer[0];
            }
        }
        
        #endregion
        
        #region Item Access
        
        /// <summary>
        /// Gets all item definitions from the Registry.
        /// </summary>
        public static System.Collections.Generic.List<ItemDefinition> GetAllItemDefinitions()
        {
            var result = new System.Collections.Generic.List<ItemDefinition>();
            try
            {
                // Try to get Registry singleton
                var registry = UnityEngine.Object.FindObjectOfType<Registry>();
                if (registry != null)
                {
                    var items = registry.GetAllItems();
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            if (item != null)
                                result.Add(item);
                        }
                    }
                }
                
                // Fallback: find all ItemDefinition ScriptableObjects
                if (result.Count == 0)
                {
                    var allDefs = Resources.FindObjectsOfTypeAll<ItemDefinition>();
                    if (allDefs != null)
                    {
                        foreach (var def in allDefs)
                        {
                            if (def != null && !string.IsNullOrEmpty(def.ID))
                                result.Add(def);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to get item definitions", ex);
            }
            return result;
        }
        
        /// <summary>
        /// Gets an item definition by ID.
        /// </summary>
        public static ItemDefinition GetItemById(string id)
        {
            try
            {
                var items = GetAllItemDefinitions();
                foreach (var item in items)
                {
                    if (item != null && item.ID == id)
                        return item;
                }
            }
            catch { }
            return null;
        }
        
        /// <summary>
        /// Creates an item instance from a definition.
        /// </summary>
        public static ItemInstance CreateItemInstance(ItemDefinition definition, int quantity = 1)
        {
            try
            {
                if (definition != null)
                {
                    return definition.GetDefaultInstance(quantity);
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Failed to create item instance for {definition?.ID}", ex);
            }
            return null;
        }
        
        /// <summary>
        /// Adds an item to the player's inventory.
        /// </summary>
        public static bool AddItemToInventory(ItemDefinition definition, int quantity = 1)
        {
            try
            {
                var inventory = Inventory;
                if (inventory == null)
                {
                    SewerLogger.Warning("Player inventory not found");
                    return false;
                }
                
                var instance = CreateItemInstance(definition, quantity);
                if (instance != null)
                {
                    inventory.AddItemToInventory(instance);
                    return true;
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Failed to add item to inventory", ex);
            }
            return false;
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Checks if the game is in a playable state (player exists).
        /// </summary>
        public static bool IsGameReady => LocalPlayer != null;
        
        /// <summary>
        /// Log diagnostic information about found game objects.
        /// </summary>
        public static void LogDiagnostics()
        {
            SewerLogger.Info("=== GAME TYPES DIAGNOSTICS ===");
            SewerLogger.Info($"LocalPlayer: {(LocalPlayer != null ? LocalPlayer.name : "NOT FOUND")}");
            SewerLogger.Info($"Movement: {(Movement != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"Health: {(Health != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"Energy: {(Energy != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"Camera: {(Camera != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"Inventory: {(Inventory != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"MoneyManager: {(Money != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"LawManager: {(Law != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"TimeManager: {(Time != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"LevelManager: {(Level != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"ProductManager: {(Products != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"PropertyManager: {(Properties != null ? "FOUND" : "NOT FOUND")}");
            SewerLogger.Info($"VehicleManager: {(Vehicles != null ? "FOUND" : "NOT FOUND")}");
            
            if (LocalPlayer != null)
                SewerLogger.Info($"Player Position: {PlayerPosition}");
            
            SewerLogger.Info("=== END DIAGNOSTICS ===");
        }
        
        #endregion
    }
}
