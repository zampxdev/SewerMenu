using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime;
using SewerMenu.Core.Logging;

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
    public static class GameTypes
    {
        #region Cached References
        
        private static Player _localPlayer;
        private static PlayerMovement _playerMovement;
        private static PlayerHealth _playerHealth;
        private static PlayerEnergy _playerEnergy;
        private static PlayerCamera _playerCamera;
        private static PlayerInventory _playerInventory;
        
        private static MoneyManager _moneyManager;
        private static LawManager _lawManager;
        private static TimeManager _timeManager;
        private static LevelManager _levelManager;
        private static ProductManager _productManager;
        private static PropertyManager _propertyManager;
        private static VehicleManager _vehicleManager;
        
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
        
        public static Player LocalPlayer
        {
            get
            {
                if (_localPlayer == null || !_localPlayer)
                    FindLocalPlayer();
                return _localPlayer;
            }
        }
        
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
        
        public static PlayerHealth Health
        {
            get
            {
                if (_playerHealth == null || !_playerHealth)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
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
        
        public static PlayerEnergy Energy
        {
            get
            {
                if (_playerEnergy == null || !_playerEnergy)
                {
                    var player = LocalPlayer;
                    if (player != null)
                    {
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
        
        public static Transform PlayerTransform => LocalPlayer?.transform;
        
        public static Vector3 PlayerPosition
        {
            get => PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
            set
            {
                if (PlayerTransform != null)
                    PlayerTransform.position = value;
            }
        }
        
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
                        if (player.IsLocalPlayer)
                        {
                            _localPlayer = player;
                            SewerLogger.Debug($"Found local player via IsLocalPlayer: {player.name}");
                            return;
                        }
                    }
                    catch { }
                }
                
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
                
                if (players.Length == 1)
                {
                    _localPlayer = players[0];
                    SewerLogger.Debug($"Found single player: {_localPlayer.name}");
                    return;
                }
                
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
        
        public static System.Collections.Generic.List<ItemDefinition> GetAllItemDefinitions()
        {
            var result = new System.Collections.Generic.List<ItemDefinition>();
            try
            {
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
        
        public static bool IsGameReady => LocalPlayer != null;
        
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
