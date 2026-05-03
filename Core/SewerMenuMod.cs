using System;
using MelonLoader;
using UnityEngine;
using SewerMenu.Core;
using SewerMenu.Core.Logging;
using SewerMenu.Core.Config;
using SewerMenu.Core.Keybinds;
using SewerMenu.Features.Base;
using SewerMenu.UI;
using SewerMenu.Utils;

namespace SewerMenu
{
    public class SewerMenuMod : MelonMod
    {
        #region Singleton
        
        public static SewerMenuMod Instance { get; private set; }
        
        #endregion
        
        #region State
        
        private bool _initialized = false;
        private bool _gameReady = false;
        private string _currentScene = "";
        
        #endregion
        
        #region MelonLoader Lifecycle
        
        public override void OnInitializeMelon()
        {
            Instance = this;
            
            SewerLogger.Initialize(LoggerInstance);
            
            SewerLogger.Info("========================================");
            SewerLogger.Info(ModInfo.GetFullInfo());
            SewerLogger.Info("========================================");
            
            try
            {
                ConfigManager.Instance.Initialize();
                
                KeybindManager.Instance.Initialize();
                
                FeatureManager.Instance.Initialize();
                
                SewerSkin.Initialize();
                
                MenuController.Instance.Initialize();
                
                _initialized = true;
                SewerLogger.Success("SewerMenu initialized successfully!");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to initialize SewerMenu", ex);
            }
        }
        
        public override void OnLateInitializeMelon()
        {
            if (!_initialized) return;
            
            SewerLogger.Info("Late initialization - Game is ready");
            _gameReady = true;
            
            ConfigManager.Instance.Load();
            
            ApplySavedFeatureStates();
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _currentScene = sceneName;
            SewerLogger.Debug($"Scene loaded: {sceneName} (index: {buildIndex})");
            
            GameFinder.OnSceneChanged();
            GameTypes.ClearCache();
            
            if (sceneName == "Main" || sceneName == "Game")
            {
                MelonLoader.MelonCoroutines.Start(InitializeGameTypesDelayed());
            }
        }
        
        private System.Collections.IEnumerator InitializeGameTypesDelayed()
        {
            yield return null;
            yield return null;
            yield return null;
            
            SewerLogger.Info("Initializing game types...");
            GameTypes.Initialize();
            GameTypes.LogDiagnostics();
        }
        
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            SewerLogger.Debug($"Scene initialized: {sceneName}");
        }
        
        public override void OnUpdate()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                MenuController.Instance.Update();
                
                FeatureManager.Instance.Update();
                
                KeybindManager.Instance.Update();
                
                // Process any pending config saves (debounced)
                ConfigManager.Instance.ProcessPendingSave();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnUpdate", ex);
            }
        }
        
        public override void OnFixedUpdate()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                FeatureManager.Instance.FixedUpdate();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnFixedUpdate", ex);
            }
        }
        
        public override void OnLateUpdate()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                FeatureManager.Instance.LateUpdate();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnLateUpdate", ex);
            }
        }
        
        public override void OnGUI()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                MenuController.Instance.OnGUI();
                
                FeatureManager.Instance.OnGUI();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnGUI", ex);
            }
        }
        
        public override void OnApplicationQuit()
        {
            SewerLogger.Info("SewerMenu shutting down...");
            
            try
            {
                ConfigManager.Instance.Save();
                
                FeatureManager.Instance.Shutdown();
                
                MenuController.Instance.Shutdown();
                
                SewerLogger.Success("SewerMenu shutdown complete");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error during shutdown", ex);
            }
        }
        
        public override void OnDeinitializeMelon()
        {
            if (_initialized)
            {
                OnApplicationQuit();
            }
            
            Instance = null;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void ApplySavedFeatureStates()
        {
            var config = ConfigManager.Instance.Config;
            if (config?.Features == null) return;

            var enabledFeatureNames = new System.Collections.Generic.List<string>();
            ConfigManager.Instance.BeginFeatureStatePersistenceBlock();
            
            try
            {
                foreach (var featureConfig in config.Features)
                {
                    var feature = FeatureManager.Instance.GetFeature(featureConfig.Key);
                    if (feature != null && featureConfig.Value != null)
                    {
                        feature.IsEnabled = featureConfig.Value.Enabled;

                        if (feature.IsEnabled)
                        {
                            enabledFeatureNames.Add(feature.Name);
                        }

                        if (!string.IsNullOrEmpty(featureConfig.Value.Hotkey))
                        {
                            if (Enum.TryParse<KeyCode>(featureConfig.Value.Hotkey, out var keyCode))
                            {
                                feature.Hotkey = keyCode;
                            }
                        }
                    }
                }
            }
            finally
            {
                ConfigManager.Instance.EndFeatureStatePersistenceBlock();
            }

            FeatureManager.Instance.RefreshHotkeyCache();
            
            if (enabledFeatureNames.Count > 0)
            {
                SewerLogger.Info("Restored enabled features from config: " + string.Join(", ", enabledFeatureNames));
            }
            else
            {
                SewerLogger.Debug("No saved enabled features restored");
            }
        }
        
        public bool IsGameReady => _gameReady && !string.IsNullOrEmpty(_currentScene);
        
        public string CurrentScene => _currentScene;
        
        #endregion
    }
}
