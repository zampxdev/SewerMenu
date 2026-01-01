using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne.Vehicles.Modification;
using Il2CppScheduleOne.Persistence.Datas;

namespace SewerMenu.Features.Vehicles
{
    /// <summary>
    /// Spawns vehicles at the player's location.
    /// </summary>
    public class VehicleSpawner : FeatureBase
    {
        public override string Id => "vehiclespawner";
        public override string Name => "Vehicle Spawner";
        public override string Description => "Spawn any vehicle at your location";
        public override FeatureCategory Category => FeatureCategory.Vehicles;
        public override bool IsToggleable => false;
        
        // Cached vehicle list
        private List<VehicleInfo> _vehicleList = new List<VehicleInfo>();
        private bool _vehiclesLoaded = false;
        private int _selectedVehicleIndex = 0;
        private int _selectedColorIndex = 0;
        
        // Available vehicle colors - loaded dynamically from game
        private List<ColorInfo> _availableColors = new List<ColorInfo>();
        private bool _colorsLoaded = false;
        
        public class VehicleInfo
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public LandVehicle Prefab { get; set; }
            public bool SupportsColor { get; set; }
        }
        
        public class ColorInfo
        {
            public EVehicleColor Color { get; set; }
            public string Name { get; set; }
        }
        
        /// <summary>
        /// Gets the list of available colors.
        /// </summary>
        public List<ColorInfo> GetAvailableColors()
        {
            if (!_colorsLoaded)
            {
                LoadColors();
            }
            return _availableColors;
        }
        
        /// <summary>
        /// Loads available colors from the game's EVehicleColor enum.
        /// </summary>
        private void LoadColors()
        {
            _availableColors.Clear();
            _colorsLoaded = true;
            
            try
            {
                var enumValues = System.Enum.GetValues(typeof(EVehicleColor));
                
                foreach (var val in enumValues)
                {
                    try
                    {
                        EVehicleColor color = (EVehicleColor)val;
                        string colorName = color.ToString();
                        
                        try
                        {
                            var vehicleColors = VehicleColors.Instance;
                            if (vehicleColors != null)
                            {
                                string displayName = vehicleColors.GetColorName(color);
                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    colorName = displayName;
                                }
                            }
                        }
                        catch { }
                        
                        _availableColors.Add(new ColorInfo
                        {
                            Color = color,
                            Name = colorName
                        });
                    }
                    catch { }
                }
            }
            catch
            {
                _availableColors.Add(new ColorInfo { Color = (EVehicleColor)0, Name = "Default" });
                _availableColors.Add(new ColorInfo { Color = (EVehicleColor)1, Name = "Color 1" });
                _availableColors.Add(new ColorInfo { Color = (EVehicleColor)2, Name = "Color 2" });
            }
        }
        
        /// <summary>
        /// Gets or sets the selected color index.
        /// </summary>
        public int SelectedColorIndex
        {
            get => _selectedColorIndex;
            set
            {
                var colors = GetAvailableColors();
                _selectedColorIndex = Mathf.Clamp(value, 0, Math.Max(0, colors.Count - 1));
            }
        }
        
        /// <summary>
        /// Gets the currently selected color name.
        /// </summary>
        public string SelectedColorName
        {
            get
            {
                var colors = GetAvailableColors();
                if (colors.Count == 0 || _selectedColorIndex >= colors.Count)
                    return "None";
                return colors[_selectedColorIndex].Name;
            }
        }
        
        /// <summary>
        /// Gets the currently selected EVehicleColor.
        /// </summary>
        public EVehicleColor SelectedColor
        {
            get
            {
                var colors = GetAvailableColors();
                if (colors.Count == 0 || _selectedColorIndex >= colors.Count)
                    return (EVehicleColor)0;
                return colors[_selectedColorIndex].Color;
            }
        }
        
        /// <summary>
        /// Gets the list of available vehicles.
        /// </summary>
        public List<VehicleInfo> GetVehicleList()
        {
            if (!_vehiclesLoaded)
            {
                LoadVehicles();
            }
            return _vehicleList;
        }
        
        /// <summary>
        /// Gets the currently selected vehicle index.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedVehicleIndex;
            set => _selectedVehicleIndex = Mathf.Clamp(value, 0, Math.Max(0, _vehicleList.Count - 1));
        }
        
        /// <summary>
        /// Loads the vehicle list from VehicleManager.
        /// Uses defensive error handling for Il2Cpp List iteration.
        /// </summary>
        public void LoadVehicles()
        {
            _vehicleList.Clear();
            _vehiclesLoaded = false;
            
            try
            {
                var vehicleManager = GameTypes.Vehicles;
                if (vehicleManager == null) return;
                
                Il2CppSystem.Collections.Generic.List<LandVehicle> prefabs = null;
                try
                {
                    prefabs = vehicleManager.VehiclePrefabs;
                }
                catch { return; }
                
                if (prefabs == null) return;
                
                int prefabCount = 0;
                try
                {
                    prefabCount = prefabs.Count;
                }
                catch { return; }
                
                if (prefabCount == 0) return;
                
                // Iterate using index instead of foreach for safer Il2Cpp access
                for (int i = 0; i < prefabCount; i++)
                {
                    try
                    {
                        LandVehicle prefab = null;
                        try
                        {
                            prefab = prefabs[i];
                        }
                        catch
                        {
                            continue; // Skip this index if access fails
                        }
                        
                        if (prefab == null) continue;
                        
                        // Get GameObject name safely
                        string vehicleName = null;
                        try
                        {
                            var go = prefab.gameObject;
                            if (go != null)
                            {
                                vehicleName = go.name;
                            }
                        }
                        catch
                        {
                            continue; // Skip if we can't get the name
                        }
                        
                        if (string.IsNullOrEmpty(vehicleName)) continue;
                        
                        // Clean up the name - remove "(Clone)" suffix if present
                        if (vehicleName.EndsWith("(Clone)"))
                            vehicleName = vehicleName.Substring(0, vehicleName.Length - 7).Trim();
                        
                        // Get the actual vehicle code from VehicleData
                        string vehicleCode = vehicleName;
                        try
                        {
                            var vehicleData = prefab.GetVehicleData();
                            if (vehicleData != null && !string.IsNullOrEmpty(vehicleData.VehicleCode))
                            {
                                vehicleCode = vehicleData.VehicleCode;
                            }
                        }
                        catch { }
                        
                        // Check if vehicle supports color changes
                        bool supportsColor = false;
                        try
                        {
                            var colorComponent = prefab.Color;
                            supportsColor = colorComponent != null;
                        }
                        catch { }
                        
                        var info = new VehicleInfo
                        {
                            Code = vehicleCode,
                            Name = vehicleName,
                            Prefab = prefab,
                            SupportsColor = supportsColor
                        };
                        _vehicleList.Add(info);
                    }
                    catch { }
                }
                
                _vehiclesLoaded = _vehicleList.Count > 0;
            }
            catch { }
        }
        
        /// <summary>
        /// Spawns the selected vehicle at the player's position with the selected color.
        /// </summary>
        public bool SpawnSelectedVehicle()
        {
            if (_vehicleList.Count == 0)
            {
                LoadVehicles();
                if (_vehicleList.Count == 0) return false;
            }
            
            if (_selectedVehicleIndex < 0 || _selectedVehicleIndex >= _vehicleList.Count)
                return false;
            
            var vehicle = _vehicleList[_selectedVehicleIndex];
            return SpawnVehicle(vehicle.Code, _selectedColorIndex);
        }
        
        /// <summary>
        /// Spawns a vehicle by code at the player's position with optional color.
        /// </summary>
        public bool SpawnVehicle(string vehicleCode, int colorIndex = -1)
        {
            try
            {
                var vehicleManager = GameTypes.Vehicles;
                if (vehicleManager == null) return false;
                
                var playerTransform = GameTypes.PlayerTransform;
                if (playerTransform == null) return false;
                
                // Spawn slightly in front of the player
                Vector3 spawnPos = playerTransform.position + playerTransform.forward * 3f;
                Quaternion spawnRot = playerTransform.rotation;
                
                // Try spawning with playerOwned = false first to avoid VehicleColors bug
                LandVehicle spawnedVehicle = null;
                try
                {
                    spawnedVehicle = vehicleManager.SpawnAndReturnVehicle(
                        vehicleCode, 
                        spawnPos, 
                        spawnRot, 
                        false  // Don't set playerOwned initially to avoid color bug
                    );
                }
                catch { }
                
                if (spawnedVehicle != null)
                {
                    // Try to set player ownership first
                    try
                    {
                        spawnedVehicle.IsPlayerOwned = true;
                    }
                    catch
                    {
                        // Ignore ownership errors - vehicle still spawned
                    }
                    
                    // Apply color AFTER setting ownership (ownership change can reset color)
                    if (colorIndex >= 0)
                    {
                        var colors = GetAvailableColors();
                        if (colorIndex < colors.Count)
                        {
                            try
                            {
                                EVehicleColor color = colors[colorIndex].Color;
                                spawnedVehicle.ApplyColor(color);
                            }
                            catch
                            {
                                try
                                {
                                    var colorComponent = spawnedVehicle.Color;
                                    if (colorComponent != null)
                                    {
                                        EVehicleColor color = colors[colorIndex].Color;
                                        colorComponent.ApplyColor(color);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the currently selected vehicle info.
        /// </summary>
        public VehicleInfo GetSelectedVehicle()
        {
            if (_vehicleList.Count == 0 || _selectedVehicleIndex < 0 || _selectedVehicleIndex >= _vehicleList.Count)
                return null;
            return _vehicleList[_selectedVehicleIndex];
        }
        
        public override void Execute()
        {
            SpawnSelectedVehicle();
        }
    }
}
