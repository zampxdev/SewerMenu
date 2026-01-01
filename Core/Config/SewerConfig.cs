using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SewerMenu.Core.Config
{
    /// <summary>
    /// Root configuration object for SewerMenu.
    /// </summary>
    [Serializable]
    public class SewerConfig
    {
        /// <summary>
        /// Configuration version for migration support.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; } = ModInfo.Version;
        
        /// <summary>
        /// UI-related settings.
        /// </summary>
        [JsonProperty("ui")]
        public UIConfig UI { get; set; } = new UIConfig();
        
        /// <summary>
        /// Feature-specific configurations.
        /// </summary>
        [JsonProperty("features")]
        public Dictionary<string, FeatureConfig> Features { get; set; } = new Dictionary<string, FeatureConfig>();
        
        /// <summary>
        /// Saved teleport locations.
        /// </summary>
        [JsonProperty("teleportLocations")]
        public List<TeleportLocation> TeleportLocations { get; set; } = new List<TeleportLocation>();
        
        /// <summary>
        /// Named presets for quick configuration switching.
        /// </summary>
        [JsonProperty("presets")]
        public Dictionary<string, PresetConfig> Presets { get; set; } = new Dictionary<string, PresetConfig>();
    }
    
    /// <summary>
    /// UI-related configuration.
    /// </summary>
    [Serializable]
    public class UIConfig
    {
        /// <summary>
        /// Key to toggle the menu.
        /// </summary>
        [JsonProperty("menuKey")]
        public string MenuKey { get; set; } = "F8";
        
        /// <summary>
        /// Current theme name.
        /// </summary>
        [JsonProperty("theme")]
        public string Theme { get; set; } = "sewer";
        
        /// <summary>
        /// Window position X.
        /// </summary>
        [JsonProperty("windowX")]
        public float WindowX { get; set; } = 100f;
        
        /// <summary>
        /// Window position Y.
        /// </summary>
        [JsonProperty("windowY")]
        public float WindowY { get; set; } = 100f;
        
        /// <summary>
        /// Window width.
        /// </summary>
        [JsonProperty("windowWidth")]
        public float WindowWidth { get; set; } = 550f;
        
        /// <summary>
        /// Window height.
        /// </summary>
        [JsonProperty("windowHeight")]
        public float WindowHeight { get; set; } = 650f;
        
        /// <summary>
        /// Window opacity (0-1).
        /// </summary>
        [JsonProperty("opacity")]
        public float Opacity { get; set; } = 0.95f;
        
        /// <summary>
        /// Font size for UI elements.
        /// </summary>
        [JsonProperty("fontSize")]
        public int FontSize { get; set; } = 14;
        
        /// <summary>
        /// Whether to show tooltips.
        /// </summary>
        [JsonProperty("showTooltips")]
        public bool ShowTooltips { get; set; } = true;
        
        /// <summary>
        /// Last active tab index.
        /// </summary>
        [JsonProperty("lastTab")]
        public int LastTab { get; set; } = 0;
    }
    
    /// <summary>
    /// Configuration for a single feature.
    /// </summary>
    [Serializable]
    public class FeatureConfig
    {
        /// <summary>
        /// Whether the feature is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// Hotkey for this feature.
        /// </summary>
        [JsonProperty("hotkey")]
        public string Hotkey { get; set; } = null;
        
        /// <summary>
        /// Feature-specific settings.
        /// </summary>
        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// A saved teleport location.
    /// </summary>
    [Serializable]
    public class TeleportLocation
    {
        /// <summary>
        /// Display name for this location.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// X coordinate.
        /// </summary>
        [JsonProperty("x")]
        public float X { get; set; }
        
        /// <summary>
        /// Y coordinate.
        /// </summary>
        [JsonProperty("y")]
        public float Y { get; set; }
        
        /// <summary>
        /// Z coordinate.
        /// </summary>
        [JsonProperty("z")]
        public float Z { get; set; }
        
        /// <summary>
        /// Scene this location is in.
        /// </summary>
        [JsonProperty("scene")]
        public string Scene { get; set; }
        
        /// <summary>
        /// Creates a new teleport location.
        /// </summary>
        public TeleportLocation() { }
        
        /// <summary>
        /// Creates a new teleport location with the specified values.
        /// </summary>
        public TeleportLocation(string name, Vector3 position, string scene = null)
        {
            Name = name;
            X = position.x;
            Y = position.y;
            Z = position.z;
            Scene = scene;
        }
        
        /// <summary>
        /// Gets the position as a Vector3.
        /// </summary>
        [JsonIgnore]
        public Vector3 Position => new Vector3(X, Y, Z);
    }
    
    /// <summary>
    /// A named preset configuration.
    /// </summary>
    [Serializable]
    public class PresetConfig
    {
        /// <summary>
        /// Display name for this preset.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Description of what this preset does.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
        
        /// <summary>
        /// Feature states for this preset.
        /// </summary>
        [JsonProperty("features")]
        public Dictionary<string, FeatureConfig> Features { get; set; } = new Dictionary<string, FeatureConfig>();
    }
}
