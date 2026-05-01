using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core;
using SewerMenu.Core.Config;
using SewerMenu.Core.Keybinds;
using SewerMenu.Core.Logging;

namespace SewerMenu.UI.Pages
{
    public class SettingsPage : PageBase
    {
        public override string Title => "Settings";
        public override FeatureCategory Category => FeatureCategory.Settings;

        private Vector2 _keybindScrollPosition;

        protected override void DrawContent()
        {
            DrawSection("GENERAL");

            var config = ConfigManager.Instance.Config;

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

            DrawAnimationQuality(config);

            GUILayout.Space(8);

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

            DrawSection("INTERFACE");

            int favoriteCount = config.UI.FavoriteFeatureIds != null ? config.UI.FavoriteFeatureIds.Count : 0;
            DrawInfo("Favorites", favoriteCount.ToString());
            DrawInfo("Animations", string.IsNullOrEmpty(config.UI.AnimationQuality) ? "Balanced" : config.UI.AnimationQuality);

            bool lockInput = SewerSkin.DrawToggle(
                "Lock Game Input",
                config.UI.LockGameInputWhenMenuOpen,
                "Stops hotbar scrolling and gameplay controls while the menu is open");
            if (lockInput != config.UI.LockGameInputWhenMenuOpen)
            {
                config.UI.LockGameInputWhenMenuOpen = lockInput;
                ConfigManager.Instance.QueueSave();
                ToastManager.Show(lockInput ? "Menu input lock enabled" : "Menu input lock disabled", SewerSkin.StatusType.Success);
            }

            GUILayout.BeginHorizontal();
            if (DrawButton("Reset Window", 105))
            {
                config.UI.WindowX = 100f;
                config.UI.WindowY = 100f;
                config.UI.WindowWidth = 880f;
                config.UI.WindowHeight = 650f;
                ConfigManager.Instance.QueueSave();
                ToastManager.Show("Window layout reset", SewerSkin.StatusType.Success);
            }

            if (DrawButton("Default Favorites", 130))
            {
                config.UI.FavoriteFeatureIds = new System.Collections.Generic.List<string>
                {
                    "godmode",
                    "infinitestamina",
                    "infiniteammo",
                    "esp",
                    "fpsoptimizer",
                    "itemspawner"
                };
                ConfigManager.Instance.QueueSave();
                ToastManager.Show("Favorites restored", SewerSkin.StatusType.Success);
            }
            GUILayout.EndHorizontal();

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

            DrawSection("ABOUT");

            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label("SEWER MENU v" + ModInfo.Version);
            GUI.contentColor = oldColor;

            GUILayout.Label("A mod menu for Schedule I");
            DrawInfo("Total Features", FeatureManager.Instance.FeatureCount.ToString());
            DrawInfo("Enabled", FeatureManager.Instance.EnabledCount.ToString());
        }

        private void DrawAnimationQuality(SewerConfig config)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Animation Quality:", GUILayout.Width(120));

            string current = config.UI.AnimationQuality;
            if (string.IsNullOrEmpty(current)) current = "Balanced";

            DrawQualityButton(config, "Full", current);
            DrawQualityButton(config, "Balanced", current);
            DrawQualityButton(config, "Low", current);
            DrawQualityButton(config, "Auto", current);

            GUILayout.EndHorizontal();
        }

        private void DrawQualityButton(SewerConfig config, string quality, string current)
        {
            bool selected = current == quality;
            Color baseColor = selected ? SewerSkin.ButtonActiveColor : SewerSkin.ButtonColor;
            Color hoverColor = selected ? SewerSkin.AccentGlow : SewerSkin.ButtonHoverColor;
            Color textColor = selected ? new Color(0.02f, 0.04f, 0.03f, 1f) : SewerSkin.TextColor;

            Rect rect = GUILayoutUtility.GetRect(76, 28, GUILayout.Width(76), GUILayout.Height(28));
            if (SewerSkin.DrawButtonRect(rect, quality, baseColor, hoverColor, textColor, 7, selected, 10))
            {
                if (config.UI.AnimationQuality != quality)
                {
                    config.UI.AnimationQuality = quality;
                    SewerSkin.SetAnimationQuality(quality);
                    ConfigManager.Instance.QueueSave();
                }
            }
        }
    }
}
