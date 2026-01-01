using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Core.Config;

namespace SewerMenu.Core.Keybinds
{
    /// <summary>
    /// Manages keybindings for the mod.
    /// </summary>
    public class KeybindManager
    {
        #region Singleton
        
        private static KeybindManager _instance;
        public static KeybindManager Instance => _instance ??= new KeybindManager();
        
        private KeybindManager() { }
        
        #endregion
        
        #region Fields
        
        private readonly Dictionary<string, KeyCode> _keybinds = new Dictionary<string, KeyCode>();
        private readonly Dictionary<string, Action> _keybindActions = new Dictionary<string, Action>();
        private bool _initialized = false;
        private bool _isCapturing = false;
        private string _capturingFor = null;
        private Action<KeyCode> _captureCallback = null;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets whether a keybind capture is in progress.
        /// </summary>
        public bool IsCapturing => _isCapturing;
        
        /// <summary>
        /// Gets the ID of the keybind being captured.
        /// </summary>
        public string CapturingFor => _capturingFor;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the keybind manager.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                SewerLogger.Warning("KeybindManager already initialized");
                return;
            }
            
            // Register default keybinds
            RegisterDefaultKeybinds();
            
            _initialized = true;
            SewerLogger.Debug("KeybindManager initialized");
        }
        
        /// <summary>
        /// Registers default keybinds.
        /// </summary>
        private void RegisterDefaultKeybinds()
        {
            // Menu toggle
            RegisterKeybind("menu_toggle", KeyCode.F8, () =>
            {
                UI.MenuController.Instance.Toggle();
            });
            
            // Quick disable all
            RegisterKeybind("disable_all", KeyCode.F9, () =>
            {
                Features.Base.FeatureManager.Instance.DisableAll();
            });
        }
        
        #endregion
        
        #region Registration
        
        /// <summary>
        /// Registers a keybind with an action.
        /// </summary>
        public void RegisterKeybind(string id, KeyCode defaultKey, Action action)
        {
            // Check if we have a saved keybind
            var config = ConfigManager.Instance.Config;
            if (config?.UI != null)
            {
                // Try to load from config
                // For now, use default
            }
            
            _keybinds[id] = defaultKey;
            _keybindActions[id] = action;
            
            SewerLogger.Debug($"Registered keybind: {id} = {defaultKey}");
        }
        
        /// <summary>
        /// Unregisters a keybind.
        /// </summary>
        public void UnregisterKeybind(string id)
        {
            _keybinds.Remove(id);
            _keybindActions.Remove(id);
        }
        
        /// <summary>
        /// Sets a keybind to a new key.
        /// </summary>
        public void SetKeybind(string id, KeyCode key)
        {
            if (_keybinds.ContainsKey(id))
            {
                _keybinds[id] = key;
                SewerLogger.Debug($"Set keybind: {id} = {key}");
                ConfigManager.Instance.QueueSave();
            }
        }
        
        /// <summary>
        /// Gets the current key for a keybind.
        /// </summary>
        public KeyCode? GetKeybind(string id)
        {
            return _keybinds.TryGetValue(id, out var key) ? key : (KeyCode?)null;
        }
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// Processes keybind inputs. Call from Update.
        /// </summary>
        public void Update()
        {
            // Handle keybind capture mode
            if (_isCapturing)
            {
                ProcessCapture();
                return;
            }
            
            bool menuVisible = UI.MenuController.Instance.IsVisible;
            
            // Process registered keybinds
            foreach (var kvp in _keybinds)
            {
                if (Input.GetKeyDown(kvp.Value))
                {
                    // Always allow menu toggle, skip others if menu is open
                    if (kvp.Key != "menu_toggle" && menuVisible)
                    {
                        continue;
                    }
                    
                    if (_keybindActions.TryGetValue(kvp.Key, out var action))
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            SewerLogger.Error($"Error executing keybind {kvp.Key}", ex);
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region Capture Mode
        
        /// <summary>
        /// Starts capturing a new keybind.
        /// </summary>
        public void StartCapture(string keybindId, Action<KeyCode> callback)
        {
            _isCapturing = true;
            _capturingFor = keybindId;
            _captureCallback = callback;
            
            SewerLogger.Debug($"Started keybind capture for: {keybindId}");
        }
        
        /// <summary>
        /// Cancels keybind capture.
        /// </summary>
        public void CancelCapture()
        {
            _isCapturing = false;
            _capturingFor = null;
            _captureCallback = null;
            
            SewerLogger.Debug("Keybind capture cancelled");
        }
        
        /// <summary>
        /// Processes keybind capture.
        /// </summary>
        private void ProcessCapture()
        {
            // Check for escape to cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelCapture();
                return;
            }
            
            // Check for any key press
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                // Skip mouse buttons and special keys
                if (key == KeyCode.None || 
                    key == KeyCode.Escape ||
                    key.ToString().StartsWith("Mouse") ||
                    key.ToString().StartsWith("Joystick"))
                {
                    continue;
                }
                
                if (Input.GetKeyDown(key))
                {
                    // Found a key
                    var callback = _captureCallback;
                    var keybindId = _capturingFor;
                    
                    _isCapturing = false;
                    _capturingFor = null;
                    _captureCallback = null;
                    
                    // Set the keybind
                    SetKeybind(keybindId, key);
                    
                    // Invoke callback
                    callback?.Invoke(key);
                    
                    SewerLogger.Debug($"Captured keybind: {keybindId} = {key}");
                    return;
                }
            }
        }
        
        #endregion
        
        #region Helpers
        
        /// <summary>
        /// Gets a display string for a keybind.
        /// </summary>
        public string GetKeybindDisplayString(string id)
        {
            if (_keybinds.TryGetValue(id, out var key))
            {
                return FormatKeyCode(key);
            }
            return "None";
        }
        
        /// <summary>
        /// Formats a KeyCode for display.
        /// </summary>
        public static string FormatKeyCode(KeyCode key)
        {
            string name = key.ToString();
            
            // Clean up common names
            if (name.StartsWith("Alpha"))
                return name.Substring(5);
            if (name.StartsWith("Keypad"))
                return "Num" + name.Substring(6);
            if (name == "Return")
                return "Enter";
            if (name == "BackQuote")
                return "`";
                
            return name;
        }
        
        /// <summary>
        /// Checks if a key is currently being used by another keybind.
        /// </summary>
        public bool IsKeyInUse(KeyCode key, string excludeId = null)
        {
            foreach (var kvp in _keybinds)
            {
                if (kvp.Key != excludeId && kvp.Value == key)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Gets all registered keybinds.
        /// </summary>
        public IReadOnlyDictionary<string, KeyCode> GetAllKeybinds()
        {
            return _keybinds;
        }
        
        #endregion
    }
}
