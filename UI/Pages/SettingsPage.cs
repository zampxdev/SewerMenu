using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core;
using SewerMenu.Core.Config;
using SewerMenu.Core.Keybinds;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for mod settings.
    /// </summary>
    public class SettingsPage : PageBase
    {
        public override string Title => "Settings";
        public override FeatureCategory Category => FeatureCategory.Settings;
        
        private Vector2 _keybindScrollPosition;
        private Vector2 _debugScrollPosition;

        protected override void DrawContent()
        {
            // General Section
            DrawSection("GENERAL");
            
            var config = ConfigManager.Instance.Config;
            
            // Menu toggle key
            GUILayout.BeginHorizontal();
            GUILayout.Label("Menu Toggle Key:", GUILayout.Width(120));
            
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label(config.UI.MenuKey, GUILayout.Width(60));
            GUI.contentColor = oldColor;
            
            if (KeybindManager.Instance.IsCapturing && KeybindManager.Instance.CapturingFor == "menu_toggle")
            {
                GUI.contentColor = SewerSkin.WarningColor;
                GUILayout.Label("Press any key...");
                GUI.contentColor = oldColor;
            }
            else
            {
                if (DrawButton("Change", 65))
                {
                    KeybindManager.Instance.StartCapture("menu_toggle", (key) =>
                    {
                        config.UI.MenuKey = key.ToString();
                        ConfigManager.Instance.QueueSave();
                        SewerLogger.Success("Menu toggle key set to " + key);
                    });
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(8);
            
            // Config actions
            GUILayout.BeginHorizontal();
            if (DrawButton("Save Config", 100))
            {
                ConfigManager.Instance.Save();
                SewerLogger.Success("Configuration saved");
            }
            if (DrawButton("Reload Config", 105))
            {
                ConfigManager.Instance.Load();
                SewerLogger.Success("Configuration reloaded");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            if (SewerSkin.DrawAccentButton("Disable All Features", 160))
            {
                FeatureManager.Instance.DisableAll();
                SewerLogger.Info("All features disabled");
            }
            
            // Keybinds Section
            DrawSection("KEYBINDS");
            
            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Click 'Set' to assign a hotkey to toggle features");
            GUI.contentColor = oldColor;
            
            SewerSkin.BeginBox();
            _keybindScrollPosition = GUILayout.BeginScrollView(_keybindScrollPosition, GUILayout.Height(140));
            
            foreach (var feature in FeatureManager.Instance.AllFeatures)
            {
                if (!feature.IsToggleable) continue;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(feature.Name, GUILayout.Width(130));
                
                string hotkeyText = feature.Hotkey.HasValue ? feature.Hotkey.Value.ToString() : "None";
                
                oldColor = GUI.contentColor;
                GUI.contentColor = feature.Hotkey.HasValue ? SewerSkin.AccentColor : SewerSkin.TextMutedColor;
                GUILayout.Label(hotkeyText, GUILayout.Width(65));
                GUI.contentColor = oldColor;
                
                if (KeybindManager.Instance.IsCapturing && KeybindManager.Instance.CapturingFor == feature.Id)
                {
                    GUI.contentColor = SewerSkin.WarningColor;
                    GUILayout.Label("Press...", GUILayout.Width(60));
                    GUI.contentColor = Color.white;
                }
                else
                {
                    if (DrawButton("Set", 40))
                    {
                        string featureId = feature.Id;
                        KeybindManager.Instance.StartCapture(featureId, (key) =>
                        {
                            FeatureManager.Instance.SetHotkey(featureId, key);
                            ConfigManager.Instance.SetFeatureSetting(featureId, "hotkey", key.ToString());
                        });
                    }
                    
                    if (feature.Hotkey.HasValue)
                    {
                        if (DrawButton("X", 22))
                        {
                            feature.Hotkey = null;
                            ConfigManager.Instance.SetFeatureSetting(feature.Id, "hotkey", null);
                        }
                    }
                }
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
            SewerSkin.EndBox();
            
            // About Section
            DrawSection("ABOUT");
            
            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label("SEWER MENU v" + ModInfo.Version);
            GUI.contentColor = oldColor;
            
            GUILayout.Label("A mod menu for Schedule I");
            DrawInfo("Total Features", FeatureManager.Instance.FeatureCount.ToString());
            DrawInfo("Enabled", FeatureManager.Instance.EnabledCount.ToString());
            
            // Debug Tools Section
            DrawSection("DEBUG TOOLS");
            
            GUILayout.BeginHorizontal();
            if (DrawButton("Discover Types", 110))
            {
                GameFinder.DiscoverGameTypes();
            }
            if (DrawButton("Log Player", 90))
            {
                GameFinder.LogPlayerComponents();
            }
            if (DrawButton("Clear Cache", 90))
            {
                GameFinder.ClearCache();
                GameTypes.ClearCache();
                SewerLogger.Info("Cache cleared");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Show current player status
            var player = GameFinder.GetLocalPlayer();
            DrawInfo("Player Found", player != null ? "Yes - " + player.name : "No");
            
            if (player != null)
            {
                var components = player.GetComponents<MonoBehaviour>();
                
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label($"Components ({components.Length}):");
                GUI.contentColor = oldColor;
                
                SewerSkin.BeginBox();
                _debugScrollPosition = GUILayout.BeginScrollView(_debugScrollPosition, GUILayout.Height(70));
                foreach (var comp in components)
                {
                    GUILayout.Label("  • " + comp.GetType().Name);
                }
                GUILayout.EndScrollView();
                SewerSkin.EndBox();
            }
        }
    }
}
