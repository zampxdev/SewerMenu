using System;
using System.IO;
using Newtonsoft.Json;
using SewerMenu.Core.Logging;

namespace SewerMenu.Core.Config
{
    /// <summary>
    /// Manages loading, saving, and accessing configuration.
    /// </summary>
    public class ConfigManager
    {
        #region Singleton
        
        private static ConfigManager _instance;
        public static ConfigManager Instance => _instance ??= new ConfigManager();
        
        private ConfigManager() { }
        
        #endregion
        
        #region Fields
        
        private string _configDirectory;
        private string _configFilePath;
        private bool _initialized = false;
        private float _lastSaveTime = 0f;
        private bool _pendingSave = false;
        private const float SaveDebounceTime = 0.5f; // 500ms debounce
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public SewerConfig Config { get; private set; }
        
        /// <summary>
        /// Gets the configuration directory path.
        /// </summary>
        public string ConfigDirectory => _configDirectory;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the configuration manager.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                SewerLogger.Warning("ConfigManager already initialized");
                return;
            }
            
            // Set up paths
            _configDirectory = Path.Combine(
                MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
                "SewerMenu"
            );
            _configFilePath = Path.Combine(_configDirectory, "config.json");
            
            // Ensure directory exists
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
                SewerLogger.Debug($"Created config directory: {_configDirectory}");
            }
            
            // Initialize with defaults
            Config = new SewerConfig();
            
            _initialized = true;
            SewerLogger.Debug("ConfigManager initialized");
        }
        
        #endregion
        
        #region Load/Save
        
        /// <summary>
        /// Loads configuration from disk.
        /// </summary>
        public void Load()
        {
            if (!_initialized)
            {
                SewerLogger.Error("ConfigManager not initialized");
                return;
            }
            
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    Config = JsonConvert.DeserializeObject<SewerConfig>(json);
                    
                    // Validate and migrate if needed
                    ValidateConfig();
                    
                    SewerLogger.Success("Configuration loaded");
                }
                else
                {
                    // Create default config
                    Config = CreateDefaultConfig();
                    Save();
                    SewerLogger.Info("Created default configuration");
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to load configuration", ex);
                
                // Backup corrupted config and create new
                BackupCorruptedConfig();
                Config = CreateDefaultConfig();
                Save();
            }
        }
        
        /// <summary>
        /// Saves configuration to disk.
        /// </summary>
        public void Save()
        {
            if (!_initialized)
            {
                SewerLogger.Error("ConfigManager not initialized");
                return;
            }
            
            try
            {
                // Backup existing config
                if (File.Exists(_configFilePath))
                {
                    string backupPath = _configFilePath + ".backup";
                    File.Copy(_configFilePath, backupPath, true);
                }
                
                // Serialize and save
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
                
                _pendingSave = false;
                SewerLogger.Debug("Configuration saved");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to save configuration", ex);
            }
        }
        
        /// <summary>
        /// Queues a save operation (debounced).
        /// </summary>
        public void QueueSave()
        {
            _pendingSave = true;
            _lastSaveTime = UnityEngine.Time.time;
        }
        
        /// <summary>
        /// Processes pending save operations.
        /// Call this from Update.
        /// </summary>
        public void ProcessPendingSave()
        {
            if (_pendingSave && UnityEngine.Time.time - _lastSaveTime >= SaveDebounceTime)
            {
                Save();
            }
        }
        
        #endregion
        
        #region Configuration Helpers
        
        /// <summary>
        /// Creates a default configuration with built-in presets.
        /// </summary>
        private SewerConfig CreateDefaultConfig()
        {
            var config = new SewerConfig();
            
            // Add built-in presets
            config.Presets["casual"] = new PresetConfig
            {
                Name = "Casual",
                Description = "Basic cheats for casual play",
                Features = new System.Collections.Generic.Dictionary<string, FeatureConfig>
                {
                    ["godmode"] = new FeatureConfig { Enabled = true },
                    ["infinitestamina"] = new FeatureConfig { Enabled = true },
                    ["sprintspeed"] = new FeatureConfig 
                    { 
                        Enabled = true, 
                        Settings = new System.Collections.Generic.Dictionary<string, object> 
                        { 
                            ["multiplier"] = 2.0f 
                        } 
                    }
                }
            };
            
            config.Presets["grinder"] = new PresetConfig
            {
                Name = "Grinder",
                Description = "Maximize productivity",
                Features = new System.Collections.Generic.Dictionary<string, FeatureConfig>
                {
                    ["stacksize"] = new FeatureConfig 
                    { 
                        Enabled = true,
                        Settings = new System.Collections.Generic.Dictionary<string, object>
                        {
                            ["multiplier"] = 10
                        }
                    },
                    ["instantgrow"] = new FeatureConfig { Enabled = true },
                    ["freepurchases"] = new FeatureConfig { Enabled = true }
                }
            };
            
            config.Presets["godlike"] = new PresetConfig
            {
                Name = "Godlike",
                Description = "Everything enabled at maximum",
                Features = new System.Collections.Generic.Dictionary<string, FeatureConfig>
                {
                    ["godmode"] = new FeatureConfig { Enabled = true },
                    ["infinitestamina"] = new FeatureConfig { Enabled = true },
                    ["sprintspeed"] = new FeatureConfig 
                    { 
                        Enabled = true,
                        Settings = new System.Collections.Generic.Dictionary<string, object>
                        {
                            ["multiplier"] = 5.0f
                        }
                    },
                    ["policedisable"] = new FeatureConfig { Enabled = true },
                    ["freepurchases"] = new FeatureConfig { Enabled = true }
                }
            };
            
            // Add default teleport locations
            config.TeleportLocations.Add(new TeleportLocation
            {
                Name = "Spawn Point",
                X = 0, Y = 0, Z = 0,
                Scene = "Main"
            });
            
            return config;
        }
        
        /// <summary>
        /// Validates and migrates configuration if needed.
        /// </summary>
        private void ValidateConfig()
        {
            if (Config == null)
            {
                Config = CreateDefaultConfig();
                return;
            }
            
            // Ensure all required objects exist
            Config.UI ??= new UIConfig();
            Config.Features ??= new System.Collections.Generic.Dictionary<string, FeatureConfig>();
            Config.TeleportLocations ??= new System.Collections.Generic.List<TeleportLocation>();
            Config.Presets ??= new System.Collections.Generic.Dictionary<string, PresetConfig>();
            
            // Version migration
            if (Config.Version != ModInfo.Version)
            {
                SewerLogger.Info($"Migrating config from {Config.Version} to {ModInfo.Version}");
                Config.Version = ModInfo.Version;
                // Add migration logic here as needed
            }
        }
        
        /// <summary>
        /// Backs up a corrupted configuration file.
        /// </summary>
        private void BackupCorruptedConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string corruptedPath = _configFilePath + ".corrupted." + DateTime.Now.ToString("yyyyMMddHHmmss");
                    File.Move(_configFilePath, corruptedPath);
                    SewerLogger.Warning($"Backed up corrupted config to: {corruptedPath}");
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to backup corrupted config", ex);
            }
        }
        
        #endregion
        
        #region Feature Configuration
        
        /// <summary>
        /// Gets the configuration for a specific feature.
        /// </summary>
        public FeatureConfig GetFeatureConfig(string featureId)
        {
            if (Config.Features.TryGetValue(featureId, out var config))
            {
                return config;
            }
            
            // Create default config for this feature
            config = new FeatureConfig();
            Config.Features[featureId] = config;
            return config;
        }
        
        /// <summary>
        /// Sets a feature's enabled state and queues a save.
        /// </summary>
        public void SetFeatureEnabled(string featureId, bool enabled)
        {
            var config = GetFeatureConfig(featureId);
            config.Enabled = enabled;
            QueueSave();
        }
        
        /// <summary>
        /// Sets a feature setting and queues a save.
        /// </summary>
        public void SetFeatureSetting(string featureId, string key, object value)
        {
            var config = GetFeatureConfig(featureId);
            config.Settings[key] = value;
            QueueSave();
        }
        
        /// <summary>
        /// Gets a feature setting with a default value.
        /// </summary>
        public T GetFeatureSetting<T>(string featureId, string key, T defaultValue = default)
        {
            var config = GetFeatureConfig(featureId);
            
            if (config.Settings.TryGetValue(key, out var value))
            {
                try
                {
                    // Handle JSON deserialization quirks
                    if (value is long longValue && typeof(T) == typeof(int))
                    {
                        return (T)(object)(int)longValue;
                    }
                    if (value is double doubleValue && typeof(T) == typeof(float))
                    {
                        return (T)(object)(float)doubleValue;
                    }
                    
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        
        #endregion
        
        #region Presets
        
        /// <summary>
        /// Applies a preset configuration.
        /// </summary>
        public void ApplyPreset(string presetName)
        {
            if (!Config.Presets.TryGetValue(presetName, out var preset))
            {
                SewerLogger.Warning($"Preset not found: {presetName}");
                return;
            }
            
            SewerLogger.Info($"Applying preset: {preset.Name}");
            
            // First disable all features
            foreach (var feature in Features.Base.FeatureManager.Instance.AllFeatures)
            {
                if (feature.IsEnabled && feature.IsToggleable)
                {
                    feature.IsEnabled = false;
                }
            }
            
            // Then apply preset features
            foreach (var featureConfig in preset.Features)
            {
                var feature = Features.Base.FeatureManager.Instance.GetFeature(featureConfig.Key);
                if (feature != null)
                {
                    feature.IsEnabled = featureConfig.Value.Enabled;
                    
                    // Apply settings
                    foreach (var setting in featureConfig.Value.Settings)
                    {
                        SetFeatureSetting(featureConfig.Key, setting.Key, setting.Value);
                    }
                }
            }
            
            QueueSave();
            SewerLogger.Success($"Preset '{preset.Name}' applied");
        }
        
        #endregion
    }
}
