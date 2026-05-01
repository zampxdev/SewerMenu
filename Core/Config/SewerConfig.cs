using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SewerMenu.Core.Config
{
    [Serializable]
    public class SewerConfig
    {
        [JsonProperty("version")]
        public string Version { get; set; } = ModInfo.Version;
        
        [JsonProperty("ui")]
        public UIConfig UI { get; set; } = new UIConfig();
        
        [JsonProperty("features")]
        public Dictionary<string, FeatureConfig> Features { get; set; } = new Dictionary<string, FeatureConfig>();
        
        [JsonProperty("teleportLocations")]
        public List<TeleportLocation> TeleportLocations { get; set; } = new List<TeleportLocation>();
        
        [JsonProperty("presets")]
        public Dictionary<string, PresetConfig> Presets { get; set; } = new Dictionary<string, PresetConfig>();
    }
    
    [Serializable]
    public class UIConfig
    {
        [JsonProperty("menuKey")]
        public string MenuKey { get; set; } = "F8";
        
        [JsonProperty("theme")]
        public string Theme { get; set; } = "sewer";
        
        [JsonProperty("windowX")]
        public float WindowX { get; set; } = 100f;
        
        [JsonProperty("windowY")]
        public float WindowY { get; set; } = 100f;
        
        [JsonProperty("windowWidth")]
        public float WindowWidth { get; set; } = 550f;
        
        [JsonProperty("windowHeight")]
        public float WindowHeight { get; set; } = 650f;
        
        [JsonProperty("opacity")]
        public float Opacity { get; set; } = 0.95f;
        
        [JsonProperty("fontSize")]
        public int FontSize { get; set; } = 14;
        
        [JsonProperty("showTooltips")]
        public bool ShowTooltips { get; set; } = true;

        [JsonProperty("lockGameInputWhenMenuOpen")]
        public bool LockGameInputWhenMenuOpen { get; set; } = true;

        [JsonProperty("animationQuality")]
        public string AnimationQuality { get; set; } = "Balanced";

        [JsonProperty("favoriteFeatureIds")]
        public List<string> FavoriteFeatureIds { get; set; } = new List<string>
        {
            "godmode",
            "infinitestamina",
            "infiniteammo",
            "esp",
            "fpsoptimizer",
            "itemspawner"
        };
        
        [JsonProperty("lastTab")]
        public int LastTab { get; set; } = 0;
    }
    
    [Serializable]
    public class FeatureConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;
        
        [JsonProperty("hotkey")]
        public string Hotkey { get; set; } = null;
        
        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class TeleportLocation
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("x")]
        public float X { get; set; }
        
        [JsonProperty("y")]
        public float Y { get; set; }
        
        [JsonProperty("z")]
        public float Z { get; set; }
        
        [JsonProperty("scene")]
        public string Scene { get; set; }
        
        public TeleportLocation() { }
        
        public TeleportLocation(string name, Vector3 position, string scene = null)
        {
            Name = name;
            X = position.x;
            Y = position.y;
            Z = position.z;
            Scene = scene;
        }
        
        [JsonIgnore]
        public Vector3 Position => new Vector3(X, Y, Z);
    }
    
    [Serializable]
    public class PresetConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("features")]
        public Dictionary<string, FeatureConfig> Features { get; set; } = new Dictionary<string, FeatureConfig>();
    }
}
