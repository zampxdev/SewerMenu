using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Vehicles;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for vehicle-related features.
    /// </summary>
    public class VehiclesPage : PageBase
    {
        public override string Title => "Vehicles";
        public override FeatureCategory Category => FeatureCategory.Vehicles;
        
        private int _selectedIndex = 0;

        protected override void DrawContent()
        {
            // Vehicle Spawner Section
            DrawSection("VEHICLE SPAWNER");
            
            var spawner = FeatureManager.Instance.GetFeature<VehicleSpawner>("vehiclespawner");
            if (spawner == null)
            {
                SewerSkin.DrawStatus("Vehicle Spawner not available", SewerSkin.StatusType.Error);
                return;
            }
            
            var vehicles = spawner.GetVehicleList();
            
            if (vehicles.Count == 0)
            {
                SewerSkin.DrawStatus("Loading vehicles...", SewerSkin.StatusType.Warning);
                if (DrawButton("Refresh Vehicle List", 150))
                {
                    spawner.LoadVehicles();
                }
                return;
            }
            
            DrawInfo("Available Vehicles", vehicles.Count.ToString());
            
            // Keep selected index in sync
            _selectedIndex = spawner.SelectedIndex;
            
            // Vehicle selector
            GUILayout.Space(5);
            var selected = spawner.GetSelectedVehicle();
            string selectedName = selected != null ? selected.Name : "None";
            string colorInfo = (selected != null && selected.SupportsColor) ? " [Color]" : "";
            DrawInfo("Selected", selectedName + colorInfo + " (" + (_selectedIndex + 1) + "/" + vehicles.Count + ")");
            
            // Navigation buttons
            GUILayout.BeginHorizontal();
            if (DrawButton("< Prev", 70))
            {
                spawner.SelectedIndex--;
            }
            if (DrawButton("Next >", 70))
            {
                spawner.SelectedIndex++;
            }
            GUILayout.FlexibleSpace();
            if (SewerSkin.DrawAccentButton("SPAWN", 80))
            {
                spawner.SpawnSelectedVehicle();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Color selector
            DrawInfo("Color", spawner.SelectedColorName);
            
            GUILayout.BeginHorizontal();
            if (DrawButton("< Color", 70))
            {
                spawner.SelectedColorIndex--;
            }
            if (DrawButton("Color >", 70))
            {
                spawner.SelectedColorIndex++;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Quick spawn buttons
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Quick Spawn (* = supports color):");
            GUI.contentColor = oldColor;
            
            GUILayout.Space(3);
            
            // Display all vehicles in rows of 3
            int vehiclesPerRow = 3;
            for (int i = 0; i < vehicles.Count; i += vehiclesPerRow)
            {
                GUILayout.BeginHorizontal();
                for (int j = i; j < i + vehiclesPerRow && j < vehicles.Count; j++)
                {
                    var vehicle = vehicles[j];
                    string label = vehicle.SupportsColor ? vehicle.Name + "*" : vehicle.Name;
                    
                    if (j == _selectedIndex)
                    {
                        if (SewerSkin.DrawAccentButton(label, 110))
                        {
                            spawner.SpawnVehicle(vehicle.Code, spawner.SelectedColorIndex);
                        }
                    }
                    else
                    {
                        if (DrawButton(label, 110))
                        {
                            spawner.SpawnVehicle(vehicle.Code, spawner.SelectedColorIndex);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(15);
            
            // Vehicle Utilities Section
            DrawSection("VEHICLE UTILITIES");
            
            var utilities = FeatureManager.Instance.GetFeature<VehicleUtilities>("vehicleutilities");
            if (utilities == null)
            {
                SewerSkin.DrawStatus("Vehicle utilities not available", SewerSkin.StatusType.Warning);
                return;
            }
            
            bool inVehicle = utilities.IsInVehicle();
            if (inVehicle)
            {
                var vehicle = utilities.GetCurrentVehicle();
                string vehicleName = "Unknown";
                if (vehicle != null && vehicle.gameObject != null)
                {
                    vehicleName = vehicle.gameObject.name;
                }
                DrawInfo("Current Vehicle", vehicleName);
                
                GUILayout.BeginHorizontal();
                if (DrawButton("Flip Upright", 100))
                {
                    utilities.FlipCurrentVehicle();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                var mutedColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Not in a vehicle");
                GUI.contentColor = mutedColor;
            }
            
            // Owned Vehicles Section
            GUILayout.Space(10);
            DrawSection("YOUR VEHICLES");
            
            var ownedVehicles = utilities.GetOwnedVehicles();
            if (ownedVehicles.Count == 0)
            {
                var mutedColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("No owned vehicles found");
                GUI.contentColor = mutedColor;
            }
            else
            {
                DrawInfo("Owned Vehicles", ownedVehicles.Count.ToString());
                
                foreach (var vehicle in ownedVehicles)
                {
                    if (vehicle == null) continue;
                    
                    string vName = "Vehicle";
                    try
                    {
                        var data = vehicle.GetVehicleData();
                        if (data != null && !string.IsNullOrEmpty(data.VehicleCode))
                            vName = data.VehicleCode;
                        else
                            vName = vehicle.gameObject.name;
                    }
                    catch
                    {
                        try { vName = vehicle.gameObject.name; } catch { }
                    }
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("• " + vName, GUILayout.Width(150));
                    if (DrawButton("Teleport", 70))
                    {
                        utilities.TeleportToVehicle(vehicle);
                    }
                    if (DrawButton("Flip", 50))
                    {
                        utilities.FlipVehicle(vehicle);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
