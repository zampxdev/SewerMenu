using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Misc;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for miscellaneous features.
    /// </summary>
    public class MiscPage : PageBase
    {
        public override string Title => "Misc";
        public override FeatureCategory Category => FeatureCategory.Misc;

        protected override void DrawContent()
        {
            // Camera Section
            DrawSection("CAMERA");
            
            var freecam = FeatureManager.Instance.GetFeature<Freecam>("freecam");
            if (freecam != null)
            {
                bool newValue = DrawToggle("Freecam", freecam.IsEnabled);
                if (newValue != freecam.IsEnabled) freecam.IsEnabled = newValue;
                
                if (freecam.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    freecam.MoveSpeed = DrawSlider("Move Speed", freecam.MoveSpeed, 5f, 100f, "F0");
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    freecam.LookSensitivity = DrawSlider("Sensitivity", freecam.LookSensitivity, 0.5f, 10f, "F1");
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (DrawButton("Teleport to Player", 140))
                    {
                        freecam.TeleportToPlayer();
                    }
                    GUILayout.EndHorizontal();
                    
                    var oldColor = GUI.contentColor;
                    GUI.contentColor = SewerSkin.TextMutedColor;
                    GUILayout.Label("   Controls: WASD move, E/Q up/down, RMB look");
                    GUI.contentColor = oldColor;
                }
            }
            
            // Visuals Section
            DrawSection("VISUALS");
            
            var esp = FeatureManager.Instance.GetFeature<ESP>("esp");
            if (esp != null)
            {
                bool newValue = DrawToggle("ESP", esp.IsEnabled, "See through walls");
                if (newValue != esp.IsEnabled) esp.IsEnabled = newValue;
                
                if (esp.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.ShowPolice = GUILayout.Toggle(esp.ShowPolice, " Police", GUILayout.Width(70));
                    esp.ShowDealers = GUILayout.Toggle(esp.ShowDealers, " Dealers", GUILayout.Width(75));
                    esp.ShowCustomers = GUILayout.Toggle(esp.ShowCustomers, " Customers", GUILayout.Width(90));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.ShowNPCs = GUILayout.Toggle(esp.ShowNPCs, " NPCs", GUILayout.Width(60));
                    esp.ShowVehicles = GUILayout.Toggle(esp.ShowVehicles, " Vehicles", GUILayout.Width(80));
                    esp.ShowItems = GUILayout.Toggle(esp.ShowItems, " Items", GUILayout.Width(65));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.ShowDistance = GUILayout.Toggle(esp.ShowDistance, " Distance", GUILayout.Width(80));
                    esp.ShowBoxes = GUILayout.Toggle(esp.ShowBoxes, " Boxes", GUILayout.Width(70));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.MaxDistance = DrawSlider("Max Distance", esp.MaxDistance, 10f, 500f, "F0");
                    GUILayout.Label("m", GUILayout.Width(15));
                    GUILayout.EndHorizontal();
                }
            }
            
            // Debug Section
            DrawSection("DEBUG");
            
            var debug = FeatureManager.Instance.GetFeature<DebugOverlay>("debugoverlay");
            if (debug != null)
            {
                bool newValue = DrawToggle("Debug Overlay", debug.IsEnabled);
                if (newValue != debug.IsEnabled) debug.IsEnabled = newValue;
                
                if (debug.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    debug.ShowFPS = GUILayout.Toggle(debug.ShowFPS, " FPS", GUILayout.Width(60));
                    debug.ShowPosition = GUILayout.Toggle(debug.ShowPosition, " Position", GUILayout.Width(80));
                    debug.ShowRotation = GUILayout.Toggle(debug.ShowRotation, " Rotation", GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    debug.ShowVelocity = GUILayout.Toggle(debug.ShowVelocity, " Velocity", GUILayout.Width(80));
                    debug.ShowGameState = GUILayout.Toggle(debug.ShowGameState, " Game State", GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
