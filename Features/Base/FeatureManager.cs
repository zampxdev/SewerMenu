using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Base
{
    public class FeatureManager
    {
        #region Singleton
        
        private static FeatureManager _instance;
        public static FeatureManager Instance => _instance ??= new FeatureManager();
        
        private FeatureManager() { }
        
        #endregion
        
        #region Fields
        
        private readonly Dictionary<string, IFeature> _features = new Dictionary<string, IFeature>();
        private readonly Dictionary<FeatureCategory, List<IFeature>> _featuresByCategory = new Dictionary<FeatureCategory, List<IFeature>>();
        private bool _initialized = false;
        
        #endregion
        
        #region Properties
        
        public IReadOnlyCollection<IFeature> AllFeatures => _features.Values;
        
        public int FeatureCount => _features.Count;
        
        public int EnabledCount => _features.Values.Count(f => f.IsEnabled);
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            if (_initialized)
            {
                SewerLogger.Warning("FeatureManager already initialized");
                return;
            }
            
            SewerLogger.Info("Initializing FeatureManager...");
            
            foreach (FeatureCategory category in Enum.GetValues(typeof(FeatureCategory)))
            {
                _featuresByCategory[category] = new List<IFeature>();
            }
            
            RegisterAllFeatures();
            
            LoadHotkeysFromConfig();
            
            _initialized = true;
            SewerLogger.Success($"FeatureManager initialized with {FeatureCount} features");
        }
        
        private void RegisterAllFeatures()
        {
            RegisterFeature(new Player.GodMode());
            RegisterFeature(new Player.InfiniteStamina());
            RegisterFeature(new Player.SprintSpeed());
            RegisterFeature(new Player.JumpHeight());
            RegisterFeature(new Player.NoClip());
            RegisterFeature(new Player.FlyMode());
            RegisterFeature(new Player.Teleport());
            RegisterFeature(new Player.HealthEnergy());
            
            RegisterFeature(new Economy.MoneyEditor());
            RegisterFeature(new Economy.XPEditor());
            RegisterFeature(new Economy.UnlockProducts());
            RegisterFeature(new Economy.FreePurchases());
            
            RegisterFeature(new Items.ItemSpawner());
            RegisterFeature(new Items.StackSizeModifier());
            RegisterFeature(new Items.InfiniteItems());
            RegisterFeature(new Items.QualityOverride());
            RegisterFeature(new Items.InstantGrow());
            
            RegisterFeature(new World.TimeController());
            RegisterFeature(new World.PoliceDisable());
            RegisterFeature(new World.NeverWanted());
            RegisterFeature(new World.UnlockProperties());
            RegisterFeature(new World.NPCFreeze());
            
            RegisterFeature(new Vehicles.VehicleSpawner());
            RegisterFeature(new Vehicles.VehicleUtilities());
            
            RegisterFeature(new Misc.Freecam());
            RegisterFeature(new Misc.ESP());
            RegisterFeature(new Misc.DebugOverlay());
        }
        
        #endregion
        
        #region Registration
        
        public void RegisterFeature(IFeature feature)
        {
            if (feature == null)
            {
                SewerLogger.Error("Cannot register null feature");
                return;
            }
            
            if (_features.ContainsKey(feature.Id))
            {
                SewerLogger.Warning($"Feature already registered: {feature.Id}");
                return;
            }
            
            try
            {
                _features[feature.Id] = feature;
                _featuresByCategory[feature.Category].Add(feature);
                feature.OnRegister();
                
                SewerLogger.Debug($"Registered feature: {feature.Name} ({feature.Category})");
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Failed to register feature: {feature.Name}", ex);
            }
        }
        
        public void UnregisterFeature(string featureId)
        {
            if (!_features.TryGetValue(featureId, out var feature))
            {
                SewerLogger.Warning($"Feature not found: {featureId}");
                return;
            }
            
            try
            {
                feature.OnUnregister();
                _features.Remove(featureId);
                _featuresByCategory[feature.Category].Remove(feature);
                
                SewerLogger.Debug($"Unregistered feature: {feature.Name}");
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Failed to unregister feature: {feature.Name}", ex);
            }
        }
        
        #endregion
        
        #region Retrieval
        
        public IFeature GetFeature(string featureId)
        {
            _features.TryGetValue(featureId, out var feature);
            return feature;
        }
        
        public T GetFeature<T>(string featureId) where T : class, IFeature
        {
            return GetFeature(featureId) as T;
        }
        
        public IReadOnlyList<IFeature> GetFeaturesByCategory(FeatureCategory category)
        {
            return _featuresByCategory.TryGetValue(category, out var features) 
                ? features 
                : new List<IFeature>();
        }
        
        public IEnumerable<IFeature> GetEnabledFeatures()
        {
            return _features.Values.Where(f => f.IsEnabled);
        }
        
        #endregion
        
        #region Lifecycle
        
        public void Update()
        {
            ProcessHotkeys();
            
            foreach (var feature in _features.Values)
            {
                if (feature.IsEnabled)
                {
                    try
                    {
                        feature.OnUpdate();
                    }
                    catch (Exception ex)
                    {
                        SewerLogger.Error($"Error in {feature.Name}.OnUpdate", ex);
                    }
                }
            }
        }
        
        public void FixedUpdate()
        {
            foreach (var feature in _features.Values)
            {
                if (feature.IsEnabled)
                {
                    try
                    {
                        feature.OnFixedUpdate();
                    }
                    catch (Exception ex)
                    {
                        SewerLogger.Error($"Error in {feature.Name}.OnFixedUpdate", ex);
                    }
                }
            }
        }
        
        public void OnGUI()
        {
            foreach (var feature in _features.Values)
            {
                if (feature.IsEnabled)
                {
                    try
                    {
                        feature.OnGUI();
                    }
                    catch (Exception ex)
                    {
                        SewerLogger.Error($"Error in {feature.Name}.OnGUI", ex);
                    }
                }
            }
        }
        
        public void Shutdown()
        {
            SewerLogger.Info("Shutting down FeatureManager...");
            
            foreach (var feature in _features.Values.ToList())
            {
                try
                {
                    feature.OnUnregister();
                }
                catch (Exception ex)
                {
                    SewerLogger.Error($"Error unregistering {feature.Name}", ex);
                }
            }
            
            _features.Clear();
            foreach (var list in _featuresByCategory.Values)
            {
                list.Clear();
            }
            
            _initialized = false;
            SewerLogger.Success("FeatureManager shutdown complete");
        }
        
        #endregion
        
        #region Hotkeys
        
        private void LoadHotkeysFromConfig()
        {
            try
            {
                var configManager = Core.Config.ConfigManager.Instance;
                if (configManager?.Config == null) return;
                
                foreach (var feature in _features.Values)
                {
                    var featureConfig = configManager.GetFeatureConfig(feature.Id);
                    if (!string.IsNullOrEmpty(featureConfig.Hotkey))
                    {
                        if (System.Enum.TryParse<KeyCode>(featureConfig.Hotkey, out var keyCode))
                        {
                            feature.Hotkey = keyCode;
                            SewerLogger.Debug($"Loaded hotkey for {feature.Name}: {keyCode}");
                        }
                    }
                }
                
                SetDefaultHotkeys();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to load hotkeys from config", ex);
            }
        }
        
        private void SetDefaultHotkeys()
        {
            var defaults = new System.Collections.Generic.Dictionary<string, KeyCode>
            {
                { "godmode", KeyCode.F1 },
                { "noclip", KeyCode.F2 },
                { "flymode", KeyCode.F3 },
                { "esp", KeyCode.F4 },
                { "freecam", KeyCode.F5 },
                { "policedisable", KeyCode.F6 },
                { "neverwanted", KeyCode.F7 }
            };
            
            foreach (var kvp in defaults)
            {
                var feature = GetFeature(kvp.Key);
                if (feature != null && !feature.Hotkey.HasValue)
                {
                    feature.Hotkey = kvp.Value;
                    SewerLogger.Debug($"Set default hotkey for {feature.Name}: {kvp.Value}");
                }
            }
        }
        
        private void ProcessHotkeys()
        {
            foreach (var feature in _features.Values)
            {
                if (feature.Hotkey.HasValue && Input.GetKeyDown(feature.Hotkey.Value))
                {
                    if (feature.IsToggleable)
                    {
                        feature.Toggle();
                    }
                    else
                    {
                        feature.Execute();
                    }
                }
            }
        }
        
        public void SetHotkey(string featureId, KeyCode? hotkey)
        {
            var feature = GetFeature(featureId);
            if (feature != null)
            {
                feature.Hotkey = hotkey;
                SewerLogger.Debug($"Set hotkey for {feature.Name}: {hotkey}");
            }
        }
        
        #endregion
        
        #region Bulk Operations
        
        public void DisableAll()
        {
            foreach (var feature in _features.Values)
            {
                if (feature.IsEnabled && feature.IsToggleable)
                {
                    feature.IsEnabled = false;
                }
            }
            SewerLogger.Info("All features disabled");
        }
        
        public void DisableCategory(FeatureCategory category)
        {
            foreach (var feature in GetFeaturesByCategory(category))
            {
                if (feature.IsEnabled && feature.IsToggleable)
                {
                    feature.IsEnabled = false;
                }
            }
            SewerLogger.Info($"All {category} features disabled");
        }
        
        #endregion
    }
}
