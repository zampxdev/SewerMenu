using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Player;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for player-related features.
    /// </summary>
    public class PlayerPage : PageBase
    {
        public override string Title => "Player";
        public override FeatureCategory Category => FeatureCategory.Player;
        
        // Location naming - cycle through preset names since we can't use text input
        private static readonly string[] _locationPresets = new string[] 
        { 
            "Home", "Stash", "Dealer", "Supplier", "Safe Spot", 
            "Hideout", "Farm", "Lab", "Shop", "Waypoint"
        };
        private int _selectedLocationIndex = 0;
        private int _selectedPresetIndex = 0;
        private Vector2 _presetScrollPos;
        
        protected override void DrawContent()
        {
            // Health & Energy Section
            DrawSection("HEALTH & ENERGY");
            
            var healthEnergy = FeatureManager.Instance.GetFeature<HealthEnergy>("healthenergy");
            if (healthEnergy != null)
            {
                // Display current values (works regardless of IsEnabled state)
                float currentHealth = healthEnergy.GetCurrentHealth();
                float currentEnergy = healthEnergy.GetCurrentEnergy();
                DrawInfo("Health", $"{currentHealth:F0} / 100");
                DrawInfo("Energy", $"{currentEnergy:F0} / 100");
                
                GUILayout.Space(5);
                
                // Infinite toggles
                bool newInfHealth = DrawToggle("Infinite Health", healthEnergy.InfiniteHealth, "Stay at max health");
                if (newInfHealth != healthEnergy.InfiniteHealth) healthEnergy.InfiniteHealth = newInfHealth;
                
                bool newInfEnergy = DrawToggle("Infinite Energy", healthEnergy.InfiniteEnergy, "Stay at max energy");
                if (newInfEnergy != healthEnergy.InfiniteEnergy) healthEnergy.InfiniteEnergy = newInfEnergy;
                
                GUILayout.Space(5);
                
                // Quick action buttons
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                if (SewerSkin.DrawAccentButton("Heal Full", 80))
                {
                    healthEnergy.HealToFull();
                }
                if (SewerSkin.DrawAccentButton("Restore Energy", 100))
                {
                    healthEnergy.RestoreEnergy();
                }
                if (SewerSkin.DrawAccentButton("Max Both", 70))
                {
                    healthEnergy.HealToFull();
                    healthEnergy.RestoreEnergy();
                }
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(5);
            
            // Combat Section
            DrawSection("COMBAT");
            
            // God Mode
            var godMode = FeatureManager.Instance.GetFeature<GodMode>("godmode");
            if (godMode != null)
            {
                bool newVal = DrawToggle("God Mode", godMode.IsEnabled, "Take no damage");
                if (newVal != godMode.IsEnabled) godMode.IsEnabled = newVal;
            }
            
            // Infinite Stamina
            var stamina = FeatureManager.Instance.GetFeature<InfiniteStamina>("infinitestamina");
            if (stamina != null)
            {
                bool newVal = DrawToggle("Infinite Stamina", stamina.IsEnabled, "Never run out");
                if (newVal != stamina.IsEnabled) stamina.IsEnabled = newVal;
            }
            
            GUILayout.Space(5);
            
            // Movement Section
            DrawSection("MOVEMENT");
            
            // Sprint Speed
            var sprint = FeatureManager.Instance.GetFeature<SprintSpeed>("sprintspeed");
            if (sprint != null)
            {
                bool newEnabled = DrawToggle("Sprint Speed", sprint.IsEnabled, $"Current: {sprint.Multiplier:F1}x");
                if (newEnabled != sprint.IsEnabled) sprint.IsEnabled = newEnabled;
                
                if (sprint.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    sprint.Multiplier = DrawSlider("Speed", sprint.Multiplier, 1f, 10f, "F1");
                    GUILayout.Label("x", GUILayout.Width(15));
                    // Quick buttons
                    if (DrawButton("2x", 35)) sprint.Multiplier = 2f;
                    if (DrawButton("3x", 35)) sprint.Multiplier = 3f;
                    if (DrawButton("5x", 35)) sprint.Multiplier = 5f;
                    GUILayout.EndHorizontal();
                }
            }
            
            // Jump Height
            var jump = FeatureManager.Instance.GetFeature<JumpHeight>("jumpheight");
            if (jump != null)
            {
                bool newEnabled = DrawToggle("Jump Height", jump.IsEnabled, $"Current: {jump.Multiplier:F1}x");
                if (newEnabled != jump.IsEnabled) jump.IsEnabled = newEnabled;
                
                if (jump.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    jump.Multiplier = DrawSlider("Height", jump.Multiplier, 1f, 10f, "F1");
                    GUILayout.Label("x", GUILayout.Width(15));
                    // Quick buttons
                    if (DrawButton("2x", 35)) jump.Multiplier = 2f;
                    if (DrawButton("3x", 35)) jump.Multiplier = 3f;
                    if (DrawButton("5x", 35)) jump.Multiplier = 5f;
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(3);
            
            // NoClip
            var noclip = FeatureManager.Instance.GetFeature<NoClip>("noclip");
            if (noclip != null)
            {
                bool newEnabled = DrawToggle("NoClip", noclip.IsEnabled, "Fly through walls (WASD + Space/Ctrl)");
                if (newEnabled != noclip.IsEnabled) noclip.IsEnabled = newEnabled;
                
                if (noclip.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    noclip.Speed = DrawSlider("Speed", noclip.Speed, 5f, 100f, "F0");
                    GUILayout.EndHorizontal();
                }
            }
            
            // Fly Mode
            var fly = FeatureManager.Instance.GetFeature<FlyMode>("flymode");
            if (fly != null)
            {
                bool newEnabled = DrawToggle("Fly Mode", fly.IsEnabled, "Free flight (Shift = faster)");
                if (newEnabled != fly.IsEnabled) fly.IsEnabled = newEnabled;
                
                if (fly.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    fly.Speed = DrawSlider("Speed", fly.Speed, 5f, 100f, "F0");
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(5);
            
            // Teleport Section
            DrawSection("TELEPORT");
            
            var teleport = FeatureManager.Instance.GetFeature<Teleport>("teleport");
            if (teleport != null)
            {
                // Current position
                var pos = teleport.GetCurrentPosition();
                DrawInfo("Position", $"X: {pos.x:F1}  Y: {pos.y:F1}  Z: {pos.z:F1}");
                
                GUILayout.Space(8);
                
                // Quick teleport to preset locations
                var oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Quick Teleport:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                
                // Preset selector
                if (DrawButton("<", 25))
                {
                    _selectedPresetIndex--;
                    if (_selectedPresetIndex < 0) _selectedPresetIndex = Teleport.PresetLocations.Count - 1;
                }
                
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(160), GUILayout.Height(24));
                GUI.backgroundColor = oldBg;
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.AccentColor;
                GUILayout.FlexibleSpace();
                if (_selectedPresetIndex >= 0 && _selectedPresetIndex < Teleport.PresetLocations.Count)
                    GUILayout.Label(Teleport.PresetLocations[_selectedPresetIndex].Name);
                GUILayout.FlexibleSpace();
                GUI.contentColor = oldColor;
                GUILayout.EndHorizontal();
                
                if (DrawButton(">", 25))
                {
                    _selectedPresetIndex++;
                    if (_selectedPresetIndex >= Teleport.PresetLocations.Count) _selectedPresetIndex = 0;
                }
                
                GUILayout.Space(5);
                
                if (SewerSkin.DrawAccentButton("Go", 50))
                {
                    teleport.TeleportToPreset(_selectedPresetIndex);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                // Save location - use preset names since text input doesn't work in IL2CPP
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Save Current Location:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label("Name:", GUILayout.Width(45));
                
                // Cycle through preset names with < > buttons
                if (DrawButton("<", 25))
                {
                    _selectedLocationIndex--;
                    if (_selectedLocationIndex < 0) _selectedLocationIndex = _locationPresets.Length - 1;
                }
                
                // Show current name in styled box
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(100), GUILayout.Height(24));
                GUI.backgroundColor = oldBg;
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.AccentColor;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_locationPresets[_selectedLocationIndex]);
                GUILayout.FlexibleSpace();
                GUI.contentColor = oldColor;
                GUILayout.EndHorizontal();
                
                if (DrawButton(">", 25))
                {
                    _selectedLocationIndex++;
                    if (_selectedLocationIndex >= _locationPresets.Length) _selectedLocationIndex = 0;
                }
                
                GUILayout.Space(5);
                
                if (SewerSkin.DrawAccentButton("Save", 60))
                {
                    // Generate unique name if this preset is already used
                    var config = Core.Config.ConfigManager.Instance?.Config;
                    var locations = config?.TeleportLocations;
                    string baseName = _locationPresets[_selectedLocationIndex];
                    string finalName = baseName;
                    int suffix = 1;
                    
                    if (locations != null)
                    {
                        while (locations.Exists(l => l.Name == finalName))
                        {
                            suffix++;
                            finalName = $"{baseName} {suffix}";
                        }
                    }
                    
                    teleport.NewLocationName = finalName;
                    teleport.SaveCurrentLocation();
                }
                GUILayout.EndHorizontal();
                
                // Saved locations
                var configRef = Core.Config.ConfigManager.Instance?.Config;
                var savedLocations = configRef?.TeleportLocations;
                if (savedLocations != null && savedLocations.Count > 0)
                {
                    GUILayout.Space(8);
                    oldColor = GUI.contentColor;
                    GUI.contentColor = SewerSkin.TextMutedColor;
                    GUILayout.Label($"Saved Locations ({savedLocations.Count}):");
                    GUI.contentColor = oldColor;
                    
                    foreach (var loc in savedLocations)
                    {
                        if (loc == null) continue;
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.Label("• " + (loc.Name ?? "Unknown"), GUILayout.Width(180));
                        if (DrawButton("Go", 40))
                        {
                            teleport.TeleportTo(new Vector3(loc.X, loc.Y, loc.Z));
                        }
                        if (SewerSkin.DrawDangerButton("X", 25))
                        {
                            savedLocations.Remove(loc);
                            Core.Config.ConfigManager.Instance.QueueSave();
                            break; // Exit loop since collection modified
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
}
