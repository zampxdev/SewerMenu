using System;
using System.IO;
using Newtonsoft.Json;
using SewerMenu.Core.Logging;

namespace SewerMenu.Core.Config
{
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
        private const float SaveDebounceTime = 0.5f;
        
        #endregion
        
        #region Properties
        
        public SewerConfig Config { get; private set; }
        
        public string ConfigDirectory => _configDirectory;
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            if (_initialized)
            {
                SewerLogger.Warning("ConfigManager already initialized");
                return;
            }
            
            _configDirectory = Path.Combine(
                MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
                "SewerMenu"
            );
            _configFilePath = Path.Combine(_configDirectory, "config.json");
            
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
                SewerLogger.Debug($"Created config directory: {_configDirectory}");
            }
            
            Config = new SewerConfig();
            
            _initialized = true;
            SewerLogger.Debug("ConfigManager initialized");
        }
        
        #endregion
        
        #region Load/Save
        
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
                    
                    ValidateConfig();
                    
                    SewerLogger.Success("Configuration loaded");
                }
                else
                {
                    Config = CreateDefaultConfig();
                    Save();
                    SewerLogger.Info("Created default configuration");
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to load configuration", ex);
                
                BackupCorruptedConfig();
                Config = CreateDefaultConfig();
                Save();
            }
        }
        
        public void Save()
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
                    string backupPath = _configFilePath + ".backup";
                    File.Copy(_configFilePath, backupPath, true);
                }
                
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
        
        public void QueueSave()
        {
            _pendingSave = true;
            _lastSaveTime = UnityEngine.Time.unscaledTime;
        }
        
        public void ProcessPendingSave()
        {
            if (_pendingSave && UnityEngine.Time.unscaledTime - _lastSaveTime >= SaveDebounceTime)
            {
                Save();
            }
        }
        
        #endregion
        
        #region Configuration Helpers
        
        private SewerConfig CreateDefaultConfig()
        {
            var config = new SewerConfig();
            
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
            
            config.TeleportLocations.Add(new TeleportLocation
            {
                Name = "Spawn Point",
                X = 0, Y = 0, Z = 0,
                Scene = "Main"
            });
            
            return config;
        }
        
        private void ValidateConfig()
        {
            if (Config == null)
            {
                Config = CreateDefaultConfig();
                return;
            }
            
            Config.UI ??= new UIConfig();
            Config.Features ??= new System.Collections.Generic.Dictionary<string, FeatureConfig>();
            Config.TeleportLocations ??= new System.Collections.Generic.List<TeleportLocation>();
            Config.Presets ??= new System.Collections.Generic.Dictionary<string, PresetConfig>();
            Config.UI.FavoriteFeatureIds = NormalizeFavoriteFeatureIds(Config.UI.FavoriteFeatureIds);

            string animationQuality = Config.UI.AnimationQuality;
            if (animationQuality != "Full" && animationQuality != "Balanced" && animationQuality != "Low" && animationQuality != "Auto")
            {
                Config.UI.AnimationQuality = "Balanced";
            }
            
            if (Config.Version != ModInfo.Version)
            {
                SewerLogger.Info($"Migrating config from {Config.Version} to {ModInfo.Version}");
                Config.Version = ModInfo.Version;
            }
        }
        
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

        private static System.Collections.Generic.List<string> NormalizeFavoriteFeatureIds(System.Collections.Generic.List<string> favorites)
        {
            var defaults = new[]
            {
                "godmode",
                "infinitestamina",
                "infiniteammo",
                "esp",
                "fpsoptimizer",
                "itemspawner"
            };

            if (favorites == null || favorites.Count == 0)
            {
                return new System.Collections.Generic.List<string>(defaults);
            }

            var normalized = new System.Collections.Generic.List<string>(favorites.Count);
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < favorites.Count; i++)
            {
                string id = favorites[i];
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                {
                    continue;
                }

                normalized.Add(id);
            }

            return normalized.Count > 0 ? normalized : new System.Collections.Generic.List<string>(defaults);
        }
        
        #endregion
        
        #region Feature Configuration
        
        public FeatureConfig GetFeatureConfig(string featureId)
        {
            if (Config.Features.TryGetValue(featureId, out var config))
            {
                return config;
            }
            
            config = new FeatureConfig();
            Config.Features[featureId] = config;
            return config;
        }
        
        public void SetFeatureEnabled(string featureId, bool enabled)
        {
            var config = GetFeatureConfig(featureId);
            config.Enabled = enabled;
            QueueSave();
        }
        
        public void SetFeatureSetting(string featureId, string key, object value)
        {
            var config = GetFeatureConfig(featureId);
            config.Settings[key] = value;
            QueueSave();
        }
        
        public T GetFeatureSetting<T>(string featureId, string key, T defaultValue = default)
        {
            var config = GetFeatureConfig(featureId);
            
            if (config.Settings.TryGetValue(key, out var value))
            {
                try
                {
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
        
        public void ApplyPreset(string presetName)
        {
            if (!Config.Presets.TryGetValue(presetName, out var preset))
            {
                SewerLogger.Warning($"Preset not found: {presetName}");
                return;
            }
            
            SewerLogger.Info($"Applying preset: {preset.Name}");
            
            foreach (var feature in Features.Base.FeatureManager.Instance.AllFeatures)
            {
                if (feature.IsEnabled && feature.IsToggleable)
                {
                    feature.IsEnabled = false;
                }
            }
            
            foreach (var featureConfig in preset.Features)
            {
                var feature = Features.Base.FeatureManager.Instance.GetFeature(featureConfig.Key);
                if (feature != null)
                {
                    feature.IsEnabled = featureConfig.Value.Enabled;
                    
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
