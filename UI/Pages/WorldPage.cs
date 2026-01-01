using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.World;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for world-related features.
    /// </summary>
    public class WorldPage : PageBase
    {
        public override string Title => "World";
        public override FeatureCategory Category => FeatureCategory.World;
        
        // Cache expensive values
        private float _displayTime = 0;
        private float _sliderTime = 12f; // Separate slider value that user controls
        private bool _sliderActive = false;
        private int _cachedDay = 0;
        private int _cachedPoliceCount = 0;
        private int _cachedNPCCount = 0;
        private float _lastCacheTime = 0;
        private const float CacheInterval = 1f;

        protected override void DrawContent()
        {
            RefreshCachedValues();
            
            // Time Section
            DrawSection("TIME CONTROL");
            
            var timeFeature = FeatureManager.Instance.GetFeature<TimeController>("timecontroller");
            if (timeFeature != null)
            {
                DrawInfo("Current Time", TimeController.FormatTime(_displayTime));
                DrawInfo("Day", _cachedDay.ToString());
                
                GUILayout.Space(8);
                
                // Time slider - uses separate _sliderTime so it doesn't get overwritten
                GUILayout.BeginHorizontal();
                float newSliderTime = DrawSlider("Set Time", _sliderTime, 0f, 24f, "F1");
                GUILayout.Label("h", GUILayout.Width(15));
                GUILayout.EndHorizontal();
                
                // Track if slider changed
                if (newSliderTime != _sliderTime)
                {
                    _sliderTime = newSliderTime;
                    _sliderActive = true;
                }
                
                // Apply time button
                if (DrawButton("Apply Time", 100))
                {
                    timeFeature.SetTime(_sliderTime);
                    _sliderActive = false;
                }
                
                GUILayout.Space(5);
                
                // Quick time buttons
                GUILayout.BeginHorizontal();
                if (DrawButton("Morning", 70)) timeFeature.SetMorning();
                if (DrawButton("Noon", 55)) timeFeature.SetNoon();
                if (DrawButton("Evening", 70)) timeFeature.SetEvening();
                if (DrawButton("Midnight", 70)) timeFeature.SetMidnight();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                if (DrawButton("+1 Hour", 70)) timeFeature.AdvanceTime(1f);
                if (DrawButton("+6 Hours", 75)) timeFeature.AdvanceTime(6f);
                if (DrawButton("Skip Day", 70)) timeFeature.SkipDay();
                GUILayout.EndHorizontal();
            }
            else
            {
                SewerSkin.DrawStatus("Time Controller not available", SewerSkin.StatusType.Warning);
            }
            
            // Police Section
            DrawSection("POLICE & WANTED");
            
            var policeFeature = FeatureManager.Instance.GetFeature<PoliceDisable>("policedisable");
            var neverWanted = FeatureManager.Instance.GetFeature<NeverWanted>("neverwanted");
            
            if (policeFeature != null)
            {
                DrawInfo("Police Count", _cachedPoliceCount.ToString());
                
                GUILayout.Space(5);
                
                bool newValue = DrawToggle("Disable Police", policeFeature.IsEnabled);
                if (newValue != policeFeature.IsEnabled) policeFeature.IsEnabled = newValue;
                
                if (neverWanted != null)
                {
                    bool neverWantedValue = DrawToggle("Never Wanted", neverWanted.IsEnabled, "Prevents wanted level from increasing");
                    if (neverWantedValue != neverWanted.IsEnabled) neverWanted.IsEnabled = neverWantedValue;
                }
                
                GUILayout.BeginHorizontal();
                if (DrawButton("Clear Wanted", 100))
                {
                    policeFeature.ClearWantedLevel();
                }
                if (DrawButton("Remove All Police", 130))
                {
                    policeFeature.RemoveAllPolice();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                SewerSkin.DrawStatus("Police system not available", SewerSkin.StatusType.Warning);
            }
            
            // Properties Section
            DrawSection("PROPERTIES");
            
            var propFeature = FeatureManager.Instance.GetFeature<UnlockProperties>("unlockproperties");
            if (propFeature != null)
            {
                if (SewerSkin.DrawAccentButton("Unlock All Properties", 160))
                {
                    propFeature.UnlockAll();
                }
            }
            
            // NPC Section
            DrawSection("NPCs");
            
            var npcFeature = FeatureManager.Instance.GetFeature<NPCFreeze>("npcfreeze");
            if (npcFeature != null)
            {
                DrawInfo("NPCs in Scene", _cachedNPCCount.ToString());
                
                bool newValue = DrawToggle("Freeze NPCs", npcFeature.IsEnabled);
                if (newValue != npcFeature.IsEnabled) npcFeature.IsEnabled = newValue;
            }
        }
        
        private void RefreshCachedValues()
        {
            if (Time.time - _lastCacheTime < CacheInterval) return;
            _lastCacheTime = Time.time;
            
            var time = FeatureManager.Instance.GetFeature<TimeController>("timecontroller");
            if (time != null)
            {
                _displayTime = time.GetCurrentTime();
                _cachedDay = time.GetCurrentDay();
                
                // Only sync slider to current time if user hasn't touched it
                if (!_sliderActive)
                {
                    _sliderTime = _displayTime;
                }
            }
            
            var police = FeatureManager.Instance.GetFeature<PoliceDisable>("policedisable");
            if (police != null)
            {
                _cachedPoliceCount = police.GetPoliceCount();
            }
            
            var npc = FeatureManager.Instance.GetFeature<NPCFreeze>("npcfreeze");
            if (npc != null)
            {
                _cachedNPCCount = npc.GetNPCCount();
            }
        }
    }
}
