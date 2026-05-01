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
                    GUILayout.Label("   WASD move, Mouse look, Shift fast, Space/Ctrl up/down");
                    GUI.contentColor = oldColor;
                }
            }

            // Combat Section
            DrawSection("COMBAT");

            var infiniteAmmo = FeatureManager.Instance.GetFeature<InfiniteAmmo>("infiniteammo");
            if (infiniteAmmo != null)
            {
                bool newValue = DrawToggle("Infinite Ammo", infiniteAmmo.IsEnabled, "Keeps gun ammo full");
                if (newValue != infiniteAmmo.IsEnabled) infiniteAmmo.IsEnabled = newValue;

                if (infiniteAmmo.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    infiniteAmmo.MinimumAmmo = Mathf.RoundToInt(DrawSlider("Ammo Buffer", infiniteAmmo.MinimumAmmo, 30f, 999f, "F0"));
                    GUILayout.EndHorizontal();
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

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.RefreshInterval = DrawSlider("Scan Delay", esp.RefreshInterval, 0.05f, 0.75f, "F2");
                    GUILayout.Label("s", GUILayout.Width(15));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    esp.MaxLabelsPerFrame = Mathf.RoundToInt(DrawSlider("Label Limit", esp.MaxLabelsPerFrame, 25f, 150f, "F0"));
                    GUILayout.EndHorizontal();
                }
            }

            // Performance Section
            DrawSection("PERFORMANCE");

            var fpsOptimizer = FeatureManager.Instance.GetFeature<FPSOptimizer>("fpsoptimizer");
            if (fpsOptimizer != null)
            {
                bool newValue = DrawToggle("FPS Optimizer", fpsOptimizer.IsEnabled, "Clean performance profile with minimal visual loss");
                if (newValue != fpsOptimizer.IsEnabled) fpsOptimizer.IsEnabled = newValue;

                if (fpsOptimizer.IsEnabled)
                {
                    fpsOptimizer.UseOcclusionCulling = DrawToggle("Camera Occlusion", fpsOptimizer.UseOcclusionCulling, "Lets Unity skip hidden objects when available");
                    fpsOptimizer.ReduceExpensiveQuality = DrawToggle("Clean Quality Trim", fpsOptimizer.ReduceExpensiveQuality, "Limits costly lights, AA spikes, and DPI overscale");
                    fpsOptimizer.OptimizeShadows = DrawToggle("Shadow Budget", fpsOptimizer.OptimizeShadows, "Caps expensive far shadow rendering");
                    fpsOptimizer.DisableRealtimeReflections = DrawToggle("Static Reflections", fpsOptimizer.DisableRealtimeReflections, "Disables realtime probe updates");
                    fpsOptimizer.ReduceParticleRaycasts = DrawToggle("Particle Budget", fpsOptimizer.ReduceParticleRaycasts, "Reduces particle collision raycast cost");
                    fpsOptimizer.UseDistanceCulling = DrawToggle("View Distance Cap", fpsOptimizer.UseDistanceCulling, "Optional stronger boost, can affect far scenery");
                    fpsOptimizer.OptimizeLod = DrawToggle("LOD Bias", fpsOptimizer.OptimizeLod, "Optional model detail reduction");
                    fpsOptimizer.UseFrameCap = DrawToggle("Frame Cap", fpsOptimizer.UseFrameCap, "Caps FPS to reduce CPU/GPU spikes");

                    if (fpsOptimizer.UseDistanceCulling)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        fpsOptimizer.MaxViewDistance = DrawSlider("View Distance", fpsOptimizer.MaxViewDistance, 500f, 1500f, "F0");
                        GUILayout.Label("m", GUILayout.Width(15));
                        GUILayout.EndHorizontal();
                    }

                    if (fpsOptimizer.OptimizeLod)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        fpsOptimizer.LodBias = DrawSlider("LOD Bias", fpsOptimizer.LodBias, 0.75f, 1.15f, "F2");
                        GUILayout.EndHorizontal();
                    }

                    if (fpsOptimizer.UseFrameCap)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        fpsOptimizer.TargetFrameRate = DrawSlider("Target FPS", fpsOptimizer.TargetFrameRate, 30f, 240f, "F0");
                        GUILayout.EndHorizontal();
                    }
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
