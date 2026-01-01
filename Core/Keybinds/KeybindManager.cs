using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Core.Config;

namespace SewerMenu.Core.Keybinds
{
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
        
        public bool IsCapturing => _isCapturing;
        
        public string CapturingFor => _capturingFor;
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            if (_initialized)
            {
                SewerLogger.Warning("KeybindManager already initialized");
                return;
            }
            
            RegisterDefaultKeybinds();
            
            _initialized = true;
            SewerLogger.Debug("KeybindManager initialized");
        }
        
        private void RegisterDefaultKeybinds()
        {
            RegisterKeybind("menu_toggle", KeyCode.F8, () =>
            {
                UI.MenuController.Instance.Toggle();
            });
            
            RegisterKeybind("disable_all", KeyCode.F9, () =>
            {
                Features.Base.FeatureManager.Instance.DisableAll();
            });
        }
        
        #endregion
        
        #region Registration
        
        public void RegisterKeybind(string id, KeyCode defaultKey, Action action)
        {
            var config = ConfigManager.Instance.Config;
            if (config?.UI != null)
            {
            }
            
            _keybinds[id] = defaultKey;
            _keybindActions[id] = action;
            
            SewerLogger.Debug($"Registered keybind: {id} = {defaultKey}");
        }
        
        public void UnregisterKeybind(string id)
        {
            _keybinds.Remove(id);
            _keybindActions.Remove(id);
        }
        
        public void SetKeybind(string id, KeyCode key)
        {
            if (_keybinds.ContainsKey(id))
            {
                _keybinds[id] = key;
                SewerLogger.Debug($"Set keybind: {id} = {key}");
                ConfigManager.Instance.QueueSave();
            }
        }
        
        public KeyCode? GetKeybind(string id)
        {
            return _keybinds.TryGetValue(id, out var key) ? key : (KeyCode?)null;
        }
        
        #endregion
        
        #region Update
        
        public void Update()
        {
            if (_isCapturing)
            {
                ProcessCapture();
                return;
            }
            
            bool menuVisible = UI.MenuController.Instance.IsVisible;
            
            foreach (var kvp in _keybinds)
            {
                if (Input.GetKeyDown(kvp.Value))
                {
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
        
        public void StartCapture(string keybindId, Action<KeyCode> callback)
        {
            _isCapturing = true;
            _capturingFor = keybindId;
            _captureCallback = callback;
            
            SewerLogger.Debug($"Started keybind capture for: {keybindId}");
        }
        
        public void CancelCapture()
        {
            _isCapturing = false;
            _capturingFor = null;
            _captureCallback = null;
            
            SewerLogger.Debug("Keybind capture cancelled");
        }
        
        private void ProcessCapture()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelCapture();
                return;
            }
            
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (key == KeyCode.None || 
                    key == KeyCode.Escape ||
                    key.ToString().StartsWith("Mouse") ||
                    key.ToString().StartsWith("Joystick"))
                {
                    continue;
                }
                
                if (Input.GetKeyDown(key))
                {
                    var callback = _captureCallback;
                    var keybindId = _capturingFor;
                    
                    _isCapturing = false;
                    _capturingFor = null;
                    _captureCallback = null;
                    
                    SetKeybind(keybindId, key);
                    
                    callback?.Invoke(key);
                    
                    SewerLogger.Debug($"Captured keybind: {keybindId} = {key}");
                    return;
                }
            }
        }
        
        #endregion
        
        #region Helpers
        
        public string GetKeybindDisplayString(string id)
        {
            if (_keybinds.TryGetValue(id, out var key))
            {
                return FormatKeyCode(key);
            }
            return "None";
        }
        
        public static string FormatKeyCode(KeyCode key)
        {
            string name = key.ToString();
            
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
        
        public IReadOnlyDictionary<string, KeyCode> GetAllKeybinds()
        {
            return _keybinds;
        }
        
        #endregion
    }
}
