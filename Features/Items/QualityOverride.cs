using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.Features.Items
{
    /// <summary>
    /// Changes the quality of the held product item.
    /// </summary>
    public class QualityOverride : FeatureBase
    {
        public override string Id => "qualityoverride";
        public override string Name => "Quality Override";
        public override string Description => "Change quality of held product";
        public override FeatureCategory Category => FeatureCategory.Items;
        public override bool IsToggleable => false;

        private List<QualityInfo> _availableQualities = new List<QualityInfo>();
        private bool _qualitiesLoaded = false;
        private int _selectedQualityIndex = 0;

        public class QualityInfo
        {
            public EQuality Quality { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Gets the list of available qualities.
        /// </summary>
        public List<QualityInfo> GetAvailableQualities()
        {
            if (!_qualitiesLoaded)
            {
                LoadQualities();
            }
            return _availableQualities;
        }

        /// <summary>
        /// Loads available qualities from the game's EQuality enum.
        /// </summary>
        private void LoadQualities()
        {
            _availableQualities.Clear();
            _qualitiesLoaded = true;

            try
            {
                var enumValues = Enum.GetValues(typeof(EQuality));

                foreach (var val in enumValues)
                {
                    try
                    {
                        EQuality quality = (EQuality)val;
                        string qualityName = quality.ToString();

                        _availableQualities.Add(new QualityInfo
                        {
                            Quality = quality,
                            Name = qualityName
                        });
                    }
                    catch { }
                }
            }
            catch
            {
                // Fallback
                _availableQualities.Add(new QualityInfo { Quality = (EQuality)0, Name = "Standard" });
            }
        }

        /// <summary>
        /// Gets or sets the selected quality index.
        /// </summary>
        public int SelectedQualityIndex
        {
            get => _selectedQualityIndex;
            set
            {
                var qualities = GetAvailableQualities();
                _selectedQualityIndex = Mathf.Clamp(value, 0, Math.Max(0, qualities.Count - 1));
            }
        }

        /// <summary>
        /// Gets the currently selected quality name.
        /// </summary>
        public string SelectedQualityName
        {
            get
            {
                var qualities = GetAvailableQualities();
                if (qualities.Count == 0 || _selectedQualityIndex >= qualities.Count)
                    return "None";
                return qualities[_selectedQualityIndex].Name;
            }
        }

        /// <summary>
        /// Gets the currently selected EQuality.
        /// </summary>
        public EQuality SelectedQuality
        {
            get
            {
                var qualities = GetAvailableQualities();
                if (qualities.Count == 0 || _selectedQualityIndex >= qualities.Count)
                    return (EQuality)0;
                return qualities[_selectedQualityIndex].Quality;
            }
        }

        /// <summary>
        /// Applies the selected quality to the currently held item.
        /// </summary>
        public bool ApplyQualityToHeldItem()
        {
            try
            {
                var player = GameTypes.LocalPlayer;
                if (player == null) return false;

                // Get the equipped item
                ItemInstance equippedItem = null;
                try
                {
                    equippedItem = player.GetEquippedItem();
                }
                catch { return false; }

                if (equippedItem == null) return false;

                // Check if it's a QualityItemInstance
                var qualityItem = equippedItem.TryCast<QualityItemInstance>();
                if (qualityItem == null) return false;

                // Apply the quality
                var qualities = GetAvailableQualities();
                if (_selectedQualityIndex >= qualities.Count) return false;

                EQuality targetQuality = qualities[_selectedQualityIndex].Quality;
                qualityItem.SetQuality(targetQuality);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets info about the currently held item.
        /// </summary>
        public string GetHeldItemInfo()
        {
            try
            {
                var player = GameTypes.LocalPlayer;
                if (player == null) return "No player";

                ItemInstance equippedItem = null;
                try
                {
                    equippedItem = player.GetEquippedItem();
                }
                catch { return "Error getting item"; }

                if (equippedItem == null) return "No item held";

                // Check if it's a QualityItemInstance
                var qualityItem = equippedItem.TryCast<QualityItemInstance>();
                if (qualityItem == null) return equippedItem.Name + " (no quality)";

                // Get current quality
                try
                {
                    var currentQuality = qualityItem.Quality;
                    return equippedItem.Name + " [" + currentQuality.ToString() + "]";
                }
                catch
                {
                    return equippedItem.Name;
                }
            }
            catch
            {
                return "Error";
            }
        }

        public override void Execute()
        {
            ApplyQualityToHeldItem();
        }
    }
}
