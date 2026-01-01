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
    /// <summary>
    /// Main entry point for SewerMenu mod.
    /// Handles initialization, lifecycle, and coordination of all systems.
    /// </summary>
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
        
        /// <summary>
        /// Called when the mod is first loaded.
        /// </summary>
        public override void OnInitializeMelon()
        {
            Instance = this;
            
            // Initialize logging first
            SewerLogger.Initialize(LoggerInstance);
            
            SewerLogger.Info("========================================");
            SewerLogger.Info(ModInfo.GetFullInfo());
            SewerLogger.Info("========================================");
            
            try
            {
                // Initialize configuration
                ConfigManager.Instance.Initialize();
                
                // Initialize keybind system
                KeybindManager.Instance.Initialize();
                
                // Initialize feature manager (registers all features)
                FeatureManager.Instance.Initialize();
                
                // Initialize UI skin
                SewerSkin.Initialize();
                
                // Initialize UI
                MenuController.Instance.Initialize();
                
                _initialized = true;
                SewerLogger.Success("SewerMenu initialized successfully!");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to initialize SewerMenu", ex);
            }
        }
        
        /// <summary>
        /// Called after the first Unity 'Start' messages.
        /// Game is now fully loaded.
        /// </summary>
        public override void OnLateInitializeMelon()
        {
            if (!_initialized) return;
            
            SewerLogger.Info("Late initialization - Game is ready");
            _gameReady = true;
            
            // Load saved configuration
            ConfigManager.Instance.Load();
            
            // Apply saved feature states
            ApplySavedFeatureStates();
        }
        
        /// <summary>
        /// Called when a scene is loaded.
        /// </summary>
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _currentScene = sceneName;
            SewerLogger.Debug($"Scene loaded: {sceneName} (index: {buildIndex})");
            
            // Clear caches on scene change
            GameFinder.OnSceneChanged();
            GameTypes.ClearCache();
            
            // Initialize game types when entering main scene
            if (sceneName == "Main" || sceneName == "Game")
            {
                // Delay initialization slightly to let the scene fully load
                MelonLoader.MelonCoroutines.Start(InitializeGameTypesDelayed());
            }
        }
        
        private System.Collections.IEnumerator InitializeGameTypesDelayed()
        {
            // Wait a few frames for the scene to fully initialize
            yield return null;
            yield return null;
            yield return null;
            
            SewerLogger.Info("Initializing game types...");
            GameTypes.Initialize();
            GameTypes.LogDiagnostics();
        }
        
        /// <summary>
        /// Called when a scene is fully initialized.
        /// </summary>
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            SewerLogger.Debug($"Scene initialized: {sceneName}");
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        public override void OnUpdate()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                // Process menu toggle
                MenuController.Instance.Update();
                
                // Update all features
                FeatureManager.Instance.Update();
                
                // Process keybinds
                KeybindManager.Instance.Update();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnUpdate", ex);
            }
        }
        
        /// <summary>
        /// Called every fixed update (physics).
        /// </summary>
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
        
        /// <summary>
        /// Called during Unity's OnGUI.
        /// </summary>
        public override void OnGUI()
        {
            if (!_initialized || !_gameReady) return;
            
            try
            {
                // Render menu
                MenuController.Instance.OnGUI();
                
                // Render feature-specific UI (ESP, debug overlay, etc.)
                FeatureManager.Instance.OnGUI();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in OnGUI", ex);
            }
        }
        
        /// <summary>
        /// Called when the application is quitting.
        /// </summary>
        public override void OnApplicationQuit()
        {
            SewerLogger.Info("SewerMenu shutting down...");
            
            try
            {
                // Save configuration
                ConfigManager.Instance.Save();
                
                // Shutdown features
                FeatureManager.Instance.Shutdown();
                
                // Shutdown UI
                MenuController.Instance.Shutdown();
                
                SewerLogger.Success("SewerMenu shutdown complete");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error during shutdown", ex);
            }
        }
        
        /// <summary>
        /// Called when the mod is being unloaded.
        /// </summary>
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
        
        /// <summary>
        /// Applies saved feature states from configuration.
        /// </summary>
        private void ApplySavedFeatureStates()
        {
            var config = ConfigManager.Instance.Config;
            if (config?.Features == null) return;
            
            foreach (var featureConfig in config.Features)
            {
                var feature = FeatureManager.Instance.GetFeature(featureConfig.Key);
                if (feature != null)
                {
                    feature.IsEnabled = featureConfig.Value.Enabled;
                    
                    if (!string.IsNullOrEmpty(featureConfig.Value.Hotkey))
                    {
                        if (Enum.TryParse<KeyCode>(featureConfig.Value.Hotkey, out var keyCode))
                        {
                            feature.Hotkey = keyCode;
                        }
                    }
                }
            }
            
            SewerLogger.Debug("Applied saved feature states");
        }
        
        /// <summary>
        /// Gets whether the game is in a playable state.
        /// </summary>
        public bool IsGameReady => _gameReady && !string.IsNullOrEmpty(_currentScene);
        
        /// <summary>
        /// Gets the current scene name.
        /// </summary>
        public string CurrentScene => _currentScene;
        
        #endregion
    }
}
