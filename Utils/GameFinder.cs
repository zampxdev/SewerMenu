using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Il2CppInterop.Runtime;
using SewerMenu.Core.Logging;

namespace SewerMenu.Utils
{
    /// <summary>
    /// Utility class for finding game objects and managers.
    /// Optimized with aggressive caching to prevent performance issues.
    /// </summary>
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
        
        // Track failed lookups to avoid repeated expensive searches
        private static readonly HashSet<string> _failedLookups = new HashSet<string>();
        private static float _lastFailedLookupClearTime;
        private static readonly float FailedLookupClearInterval = 30f; // Only retry failed lookups every 30 seconds
        
        private static readonly Dictionary<string, object> _managerCache = new Dictionary<string, object>();
        
        // Flag to track if we've done initial discovery
        private static readonly Dictionary<string, Type> _discoveredTypes = new Dictionary<string, Type>();
        
        #endregion
        
        #region Type Discovery
        
        /// <summary>
        /// Discovers and caches game types. Can be called multiple times.
        /// Uses IL2CPP-specific methods to get actual type names.
        /// </summary>
        public static void DiscoverGameTypes()
        {
            SewerLogger.Info("=== DISCOVERING GAME TYPES ===");
            
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                SewerLogger.Info($"Found {allObjects.Length} MonoBehaviours in scene");
                
                var typeNames = new HashSet<string>();
                int relevantCount = 0;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;
                    
                    // In IL2CPP, we need to get the Il2CppType to get the real type name
                    string fullName;
                    try
                    {
                        // Try to get the actual IL2CPP type name
                        var il2cppType = Il2CppType.From(obj.GetType());
                        fullName = il2cppType?.FullName ?? obj.GetType().FullName ?? obj.GetType().Name;
                    }
                    catch
                    {
                        // Fallback - try getting name from the object's class
                        try
                        {
                            // Use GetIl2CppType() which is available on IL2CPP objects
                            var objType = obj.GetIl2CppType();
                            fullName = objType?.FullName ?? "Unknown";
                        }
                        catch
                        {
                            fullName = obj.GetType().FullName ?? obj.GetType().Name;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(fullName) || fullName == "UnityEngine.MonoBehaviour")
                        continue;
                    
                    if (!typeNames.Contains(fullName))
                    {
                        typeNames.Add(fullName);
                        
                        // Log interesting types - be more inclusive
                        bool isRelevant = fullName.Contains("Player") || 
                                         fullName.Contains("Money") || 
                                         fullName.Contains("Health") || 
                                         fullName.Contains("Level") ||
                                         fullName.Contains("Movement") || 
                                         fullName.Contains("Manager") ||
                                         fullName.Contains("Controller") ||
                                         fullName.Contains("Character") ||
                                         fullName.Contains("Inventory") ||
                                         fullName.Contains("Time") ||
                                         fullName.Contains("Police") ||
                                         fullName.Contains("Law") ||
                                         fullName.Contains("NPC") ||
                                         fullName.Contains("Drug") ||
                                         fullName.Contains("Product") ||
                                         fullName.Contains("Cash") ||
                                         fullName.Contains("Bank") ||
                                         fullName.Contains("XP") ||
                                         fullName.Contains("Rank") ||
                                         fullName.Contains("Schedule");
                        
                        if (isRelevant)
                        {
                            SewerLogger.Info($"[TYPE] {fullName}");
                            relevantCount++;
                        }
                    }
                }
                
                // Also log ALL unique type names to a summary
                SewerLogger.Info($"=== DISCOVERED {relevantCount} RELEVANT TYPES out of {typeNames.Count} unique types ===");
                
                // Log first 50 type names for debugging
                int count = 0;
                foreach (var name in typeNames.OrderBy(n => n))
                {
                    if (count++ < 100)
                    {
                        SewerLogger.Debug($"[ALL] {name}");
                    }
                }
                
                // Discovery complete
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error discovering game types", ex);
            }
        }
        
        #endregion
        
        #region Player
        
        /// <summary>
        /// Gets the local player GameObject.
        /// </summary>
        public static GameObject GetLocalPlayer()
        {
            // Return cached if valid
            if (_localPlayer != null)
                return _localPlayer;
            
            // Check if we've already failed this lookup recently
            if (IsFailedLookup("LocalPlayer"))
                return null;
            
            try
            {
                // Try to find player by tag first (fastest)
                _localPlayer = GameObject.FindGameObjectWithTag("Player");
                if (_localPlayer != null)
                {
                    SewerLogger.Debug($"Found player by tag: {_localPlayer.name}");
                    return _localPlayer;
                }
                
                // Try common names
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
                
                // Try to find by component - but only once per session
                var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var obj in allObjects)
                {
                    var typeName = obj.GetType().Name;
                    var fullName = obj.GetType().FullName ?? "";
                    
                    // Schedule I specific patterns
                    if (typeName == "Player" || 
                        fullName.Contains("PlayerScripts.Player") ||
                        fullName.Contains("ScheduleOne.Player"))
                    {
                        // Check if this is the local player (not an NPC)
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
                            // No IsLocal property, assume it's the player
                            _localPlayer = obj.gameObject;
                            SewerLogger.Debug($"Found player component: {_localPlayer.name} ({fullName})");
                            return _localPlayer;
                        }
                    }
                }
                
                // Mark as failed lookup
                MarkFailedLookup("LocalPlayer");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error finding local player", ex);
                MarkFailedLookup("LocalPlayer");
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the player's health component.
        /// </summary>
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
                
                // Try to get from Player component properties
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
        
        /// <summary>
        /// Gets the player's movement component.
        /// </summary>
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
                
                // Priority order for movement components
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
                
                // Try CharacterController (Unity built-in)
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
        
        /// <summary>
        /// Gets the player's inventory component.
        /// </summary>
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
        
        /// <summary>
        /// Gets the MoneyManager instance.
        /// </summary>
        public static object GetMoneyManager()
        {
            // Actual game type: Il2CppScheduleOne.Money.MoneyManager
            return _moneyManager ??= FindManager("MoneyManager");
        }
        
        /// <summary>
        /// Gets the LevelManager instance.
        /// </summary>
        public static object GetLevelManager()
        {
            // Actual game type: Il2CppScheduleOne.Levelling.LevelManager
            return _levelManager ??= FindManager("LevelManager");
        }
        
        /// <summary>
        /// Gets the TimeManager instance.
        /// </summary>
        public static object GetTimeManager()
        {
            // Actual game type: Il2CppScheduleOne.GameTime.TimeManager
            return _timeManager ??= FindManager("TimeManager");
        }
        
        /// <summary>
        /// Gets the LawManager instance.
        /// </summary>
        public static object GetLawManager()
        {
            // Actual game type: Il2CppScheduleOne.Law.LawManager
            return _lawManager ??= FindManager("LawManager");
        }
        
        /// <summary>
        /// Gets the ProductManager instance.
        /// </summary>
        public static object GetProductManager()
        {
            // Actual game type: Il2CppScheduleOne.Product.ProductManager
            return _productManager ??= FindManager("ProductManager");
        }
        
        /// <summary>
        /// Gets the PropertyManager instance.
        /// </summary>
        public static object GetPropertyManager()
        {
            // Actual game type: Il2CppScheduleOne.Property.PropertyManager
            return _propertyManager ??= FindManager("PropertyManager");
        }
        
        /// <summary>
        /// Finds a manager by trying multiple possible names.
        /// </summary>
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
                    // Try to find by GameObject name
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
                
                // Try to find by component type name (expensive - only do once)
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
                
                // Try to find singleton instance
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
            // Clear failed lookups periodically to allow retry
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
        
        #region Diagnostics
        
        /// <summary>
        /// Logs all components on the player for debugging.
        /// Uses IL2CPP-specific methods to get actual type names.
        /// </summary>
        public static void LogPlayerComponents()
        {
            var player = GetLocalPlayer();
            if (player == null)
            {
                SewerLogger.Warning("Cannot log player components - player not found");
                return;
            }
            
            SewerLogger.Info($"=== PLAYER COMPONENTS ({player.name}) ===");
            
            var components = player.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                
                // Get IL2CPP type name
                string typeName;
                try
                {
                    var il2cppType = comp.GetIl2CppType();
                    typeName = il2cppType?.FullName ?? comp.GetType().Name;
                }
                catch
                {
                    typeName = comp.GetType().Name;
                }
                
                SewerLogger.Info($"[COMPONENT] {typeName}");
            }
            
            // Also check children
            SewerLogger.Info("=== CHILD COMPONENTS ===");
            var childComps = player.GetComponentsInChildren<MonoBehaviour>(true);
            var logged = new HashSet<string>();
            foreach (var comp in childComps)
            {
                if (comp == null) continue;
                
                string typeName;
                try
                {
                    var il2cppType = comp.GetIl2CppType();
                    typeName = il2cppType?.FullName ?? comp.GetType().Name;
                }
                catch
                {
                    typeName = comp.GetType().Name;
                }
                
                if (!logged.Contains(typeName) && typeName != "UnityEngine.MonoBehaviour")
                {
                    logged.Add(typeName);
                    SewerLogger.Info($"[CHILD] {typeName}");
                }
            }
            
            SewerLogger.Info("=== END PLAYER COMPONENTS ===");
        }
        
        #endregion
        
        #region Cache Management
        
        /// <summary>
        /// Clears all cached references. Call when scene changes.
        /// </summary>
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
        
        /// <summary>
        /// Call this when scene changes to reset lookups.
        /// </summary>
        public static void OnSceneChanged()
        {
            ClearCache();
        }
        
        #endregion
    }
}
