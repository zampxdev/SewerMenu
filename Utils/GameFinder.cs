using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SewerMenu.Core.Logging;

namespace SewerMenu.Utils
{
    public static class GameFinder
    {
        #region Cache
        
        private static GameObject _localPlayer;
        private static object _playerHealth;
        private static object _playerMovement;
        private static object _playerInventory;
        private static object _moneyManager;
        private static object _levelManager;
        private static object _timeManager;
        private static object _lawManager;
        private static object _productManager;
        private static object _propertyManager;
        
        private static readonly HashSet<string> _failedLookups = new HashSet<string>();
        private static float _lastFailedLookupClearTime;
        private static readonly float FailedLookupClearInterval = 30f;
        
        private static readonly Dictionary<string, object> _managerCache = new Dictionary<string, object>();
        
        private static readonly Dictionary<string, Type> _discoveredTypes = new Dictionary<string, Type>();
        
        #endregion
        
        #region Player
        
        public static GameObject GetLocalPlayer()
        {
            if (_localPlayer != null)
                return _localPlayer;
            
            if (IsFailedLookup("LocalPlayer"))
                return null;
            
            try
            {
                _localPlayer = GameObject.FindGameObjectWithTag("Player");
                if (_localPlayer != null)
                {
                    SewerLogger.Debug($"Found player by tag: {_localPlayer.name}");
                    return _localPlayer;
                }
                
                string[] playerNames = { "Player", "LocalPlayer", "Player(Clone)", "PlayerController" };
                foreach (var name in playerNames)
                {
                    _localPlayer = GameObject.Find(name);
                    if (_localPlayer != null)
                    {
                        SewerLogger.Debug($"Found player by name: {_localPlayer.name}");
                        return _localPlayer;
                    }
                }
                
                var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var obj in allObjects)
                {
                    var typeName = obj.GetType().Name;
                    var fullName = obj.GetType().FullName ?? "";
                    
                    if (typeName == "Player" || 
                        fullName.Contains("PlayerScripts.Player") ||
                        fullName.Contains("ScheduleOne.Player"))
                    {
                        var isLocalProp = obj.GetType().GetProperty("IsLocal") ?? 
                                         obj.GetType().GetProperty("isLocalPlayer") ??
                                         obj.GetType().GetProperty("IsOwner");
                        
                        if (isLocalProp != null)
                        {
                            try
                            {
                                bool isLocal = (bool)isLocalProp.GetValue(obj);
                                if (isLocal)
                                {
                                    _localPlayer = obj.gameObject;
                                    SewerLogger.Debug($"Found local player: {_localPlayer.name} ({fullName})");
                                    return _localPlayer;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            _localPlayer = obj.gameObject;
                            SewerLogger.Debug($"Found player component: {_localPlayer.name} ({fullName})");
                            return _localPlayer;
                        }
                    }
                }
                
                MarkFailedLookup("LocalPlayer");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding local player", ex);
                MarkFailedLookup("LocalPlayer");
            }
            
            return null;
        }
        
        public static object GetPlayerHealth()
        {
            if (_playerHealth != null)
                return _playerHealth;
            
            if (IsFailedLookup("PlayerHealth"))
                return null;
            
            var player = GetLocalPlayer();
            if (player == null) return null;
            
            try
            {
                var components = player.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in components)
                {
                    var typeName = comp.GetType().Name;
                    if (typeName.Contains("Health"))
                    {
                        _playerHealth = comp;
                        SewerLogger.Debug($"Found health component: {typeName}");
                        return _playerHealth;
                    }
                }
                
                foreach (var comp in components)
                {
                    var healthProp = comp.GetType().GetProperty("Health");
                    if (healthProp != null)
                    {
                        _playerHealth = healthProp.GetValue(comp);
                        if (_playerHealth != null)
                        {
                            SewerLogger.Debug($"Found health via property on {comp.GetType().Name}");
                            return _playerHealth;
                        }
                    }
                }
                
                MarkFailedLookup("PlayerHealth");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding player health", ex);
                MarkFailedLookup("PlayerHealth");
            }
            
            return null;
        }
        
        public static object GetPlayerMovement()
        {
            if (_playerMovement != null)
                return _playerMovement;
            
            if (IsFailedLookup("PlayerMovement"))
                return null;
            
            var player = GetLocalPlayer();
            if (player == null) return null;
            
            try
            {
                var components = player.GetComponentsInChildren<MonoBehaviour>(true);
                
                string[] movementPatterns = { "PlayerMovement", "Movement", "CharacterController", "Motor", "Locomotion" };
                
                foreach (var pattern in movementPatterns)
                {
                    foreach (var comp in components)
                    {
                        var typeName = comp.GetType().Name;
                        if (typeName.Contains(pattern))
                        {
                            _playerMovement = comp;
                            SewerLogger.Debug($"Found movement component: {typeName}");
                            return _playerMovement;
                        }
                    }
                }
                
                var cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    _playerMovement = cc;
                    SewerLogger.Debug("Found CharacterController as movement");
                    return _playerMovement;
                }
                
                MarkFailedLookup("PlayerMovement");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding player movement", ex);
                MarkFailedLookup("PlayerMovement");
            }
            
            return null;
        }
        
        public static object GetPlayerInventory()
        {
            if (_playerInventory != null)
                return _playerInventory;
            
            if (IsFailedLookup("PlayerInventory"))
                return null;
            
            var player = GetLocalPlayer();
            if (player == null) return null;
            
            try
            {
                var components = player.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in components)
                {
                    var typeName = comp.GetType().Name;
                    if (typeName.Contains("Inventory"))
                    {
                        _playerInventory = comp;
                        SewerLogger.Debug($"Found inventory component: {typeName}");
                        return _playerInventory;
                    }
                }
                
                MarkFailedLookup("PlayerInventory");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding player inventory", ex);
                MarkFailedLookup("PlayerInventory");
            }
            
            return null;
        }
        
        #endregion
        
        #region Managers
        
        public static object GetMoneyManager()
        {
            return _moneyManager ??= FindManager("MoneyManager");
        }
        
        public static object GetLevelManager()
        {
            return _levelManager ??= FindManager("LevelManager");
        }
        
        public static object GetTimeManager()
        {
            return _timeManager ??= FindManager("TimeManager");
        }
        
        public static object GetLawManager()
        {
            return _lawManager ??= FindManager("LawManager");
        }
        
        public static object GetProductManager()
        {
            return _productManager ??= FindManager("ProductManager");
        }
        
        public static object GetPropertyManager()
        {
            return _propertyManager ??= FindManager("PropertyManager");
        }
        
        public static object FindManager(params string[] possibleNames)
        {
            string cacheKey = string.Join("|", possibleNames);
            
            if (_managerCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;
            
            if (IsFailedLookup(cacheKey))
                return null;
            
            try
            {
                foreach (var managerName in possibleNames)
                {
                    var go = GameObject.Find(managerName);
                    if (go != null)
                    {
                        var comp = go.GetComponent<MonoBehaviour>();
                        if (comp != null)
                        {
                            _managerCache[cacheKey] = comp;
                            SewerLogger.Debug($"Found manager by GameObject: {managerName}");
                            return comp;
                        }
                    }
                }
                
                var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var managerName in possibleNames)
                {
                    foreach (var obj in allObjects)
                    {
                        var typeName = obj.GetType().Name;
                        if (typeName == managerName || typeName.EndsWith(managerName))
                        {
                            _managerCache[cacheKey] = obj;
                            SewerLogger.Debug($"Found manager by type: {typeName}");
                            return obj;
                        }
                    }
                }
                
                foreach (var managerName in possibleNames)
                {
                    foreach (var obj in allObjects)
                    {
                        var type = obj.GetType();
                        if (type.Name == managerName)
                        {
                            var instanceProp = type.GetProperty("Instance", 
                                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                            if (instanceProp != null)
                            {
                                var instance = instanceProp.GetValue(null);
                                if (instance != null)
                                {
                                    _managerCache[cacheKey] = instance;
                                    SewerLogger.Debug($"Found manager singleton: {managerName}");
                                    return instance;
                                }
                            }
                        }
                    }
                }
                
                MarkFailedLookup(cacheKey);
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Error finding manager: {possibleNames[0]}", ex);
                MarkFailedLookup(cacheKey);
            }
            
            return null;
        }
        
        #endregion
        
        #region Failed Lookup Management
        
        private static bool IsFailedLookup(string key)
        {
            if (Time.time - _lastFailedLookupClearTime > FailedLookupClearInterval)
            {
                _failedLookups.Clear();
                _lastFailedLookupClearTime = Time.time;
                return false;
            }
            
            return _failedLookups.Contains(key);
        }
        
        private static void MarkFailedLookup(string key)
        {
            _failedLookups.Add(key);
        }
        
        #endregion
        
        #region Cache Management
        
        public static void ClearCache()
        {
            _localPlayer = null;
            _playerHealth = null;
            _playerMovement = null;
            _playerInventory = null;
            _moneyManager = null;
            _levelManager = null;
            _timeManager = null;
            _lawManager = null;
            _productManager = null;
            _propertyManager = null;
            _managerCache.Clear();
            _failedLookups.Clear();
            _lastFailedLookupClearTime = 0;
            
            SewerLogger.Debug("GameFinder cache cleared");
        }
        
        public static void OnSceneChanged()
        {
            ClearCache();
        }
        
        #endregion
    }
}
