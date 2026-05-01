using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Vehicles;
using Il2CppScheduleOne.Vehicles;

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
        private Vector2 _quickSpawnScroll;
        private float _lastVehicleRefreshTime = -999f;
        private bool _cachedInVehicle;
        private LandVehicle _cachedCurrentVehicle;
        private string _cachedCurrentVehicleName = "Unknown";
        private readonly List<LandVehicle> _cachedOwnedVehicles = new List<LandVehicle>();
        private readonly Dictionary<int, string> _ownedVehicleNames = new Dictionary<int, string>();

        private const float VehicleRefreshInterval = 0.85f;
        private const int VehiclesPerRow = 3;
        private const float QuickSpawnHeight = 132f;
        private const float QuickSpawnRowHeight = 34f;

        protected override void DrawContent()
        {
            DrawSection("VEHICLE SPAWNER");

            var spawner = FeatureManager.Instance.GetFeature<VehicleSpawner>("vehiclespawner");
            if (spawner == null)
            {
                SewerSkin.DrawEmptyState("Vehicle spawner unavailable", "The vehicle spawner feature did not register.", SewerSkin.StatusType.Error);
                return;
            }

            var vehicles = spawner.GetVehicleList();

            if (vehicles.Count == 0)
            {
                SewerSkin.DrawEmptyState("Loading vehicles", "Refresh after loading into a save if the vehicle list is empty.", SewerSkin.StatusType.Warning);
                if (DrawButton("Refresh Vehicle List", 150))
                {
                    spawner.LoadVehicles();
                }
                return;
            }

            DrawInfo("Available Vehicles", vehicles.Count.ToString());

            _selectedIndex = spawner.SelectedIndex;

            GUILayout.Space(5);
            var selected = spawner.GetSelectedVehicle();
            string selectedName = selected != null ? selected.Name : "None";
            string colorInfo = (selected != null && selected.SupportsColor) ? " [Color]" : "";
            DrawInfo("Selected", selectedName + colorInfo + " (" + (_selectedIndex + 1) + "/" + vehicles.Count + ")");

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
            DrawQuickSpawnGrid(spawner, vehicles);
            GUILayout.Space(15);

            DrawSection("VEHICLE UTILITIES");

            var utilities = FeatureManager.Instance.GetFeature<VehicleUtilities>("vehicleutilities");
            if (utilities == null)
            {
                SewerSkin.DrawEmptyState("Vehicle utilities unavailable", "The vehicle utility feature did not register.", SewerSkin.StatusType.Warning);
                return;
            }

            RefreshVehicleUtilityCache(utilities);
            if (_cachedInVehicle)
            {
                DrawInfo("Current Vehicle", _cachedCurrentVehicleName);

                GUILayout.BeginHorizontal();
                if (DrawButton("Flip Upright", 100))
                {
                    utilities.FlipCurrentVehicle();
                    RefreshVehicleUtilityCache(utilities, true);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                SewerSkin.DrawEmptyState("Not in a vehicle", "Vehicle actions appear here when you enter a vehicle.", SewerSkin.StatusType.Normal);
            }

            GUILayout.Space(10);
            DrawSection("YOUR VEHICLES");

            if (_cachedOwnedVehicles.Count == 0)
            {
                SewerSkin.DrawEmptyState("No owned vehicles", "Owned vehicles will appear here once the periodic scan finds them.", SewerSkin.StatusType.Normal);
            }
            else
            {
                DrawInfo("Owned Vehicles", _cachedOwnedVehicles.Count.ToString());

                foreach (var vehicle in _cachedOwnedVehicles)
                {
                    if (vehicle == null) continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("- " + GetCachedVehicleName(vehicle), GUILayout.Width(150));
                    if (DrawButton("Teleport", 70))
                    {
                        utilities.TeleportToVehicle(vehicle);
                    }
                    if (DrawButton("Flip", 50))
                    {
                        utilities.FlipVehicle(vehicle);
                        RefreshVehicleUtilityCache(utilities, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DrawQuickSpawnGrid(VehicleSpawner spawner, List<VehicleSpawner.VehicleInfo> vehicles)
        {
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Quick Spawn (* = supports color):");
            GUI.contentColor = oldColor;

            GUILayout.Space(3);

            if (vehicles == null || vehicles.Count == 0)
            {
                SewerSkin.DrawEmptyState("No vehicles loaded", "Refresh the vehicle list when you are loaded into a save.", SewerSkin.StatusType.Warning);
                return;
            }

            int totalRows = Mathf.CeilToInt(vehicles.Count / (float)VehiclesPerRow);
            int firstRow = Mathf.Clamp(Mathf.FloorToInt(_quickSpawnScroll.y / QuickSpawnRowHeight) - 1, 0, totalRows);
            int visibleRows = Mathf.CeilToInt(QuickSpawnHeight / QuickSpawnRowHeight) + 3;
            int lastRow = Mathf.Clamp(firstRow + visibleRows, 0, totalRows);

            SewerSkin.BeginBox();
            _quickSpawnScroll = GUILayout.BeginScrollView(_quickSpawnScroll, GUILayout.Height(QuickSpawnHeight));

            if (firstRow > 0)
            {
                GUILayout.Space(firstRow * QuickSpawnRowHeight);
            }

            for (int row = firstRow; row < lastRow; row++)
            {
                GUILayout.BeginHorizontal();
                int startIndex = row * VehiclesPerRow;
                int endIndex = Mathf.Min(startIndex + VehiclesPerRow, vehicles.Count);

                for (int j = startIndex; j < endIndex; j++)
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
                GUILayout.Space(4);
            }

            if (lastRow < totalRows)
            {
                GUILayout.Space((totalRows - lastRow) * QuickSpawnRowHeight);
            }

            GUILayout.EndScrollView();
            SewerSkin.EndBox();
        }

        private void RefreshVehicleUtilityCache(VehicleUtilities utilities, bool force = false)
        {
            if (utilities == null) return;

            float now = Time.unscaledTime;
            if (!force && now - _lastVehicleRefreshTime < VehicleRefreshInterval)
            {
                return;
            }

            _lastVehicleRefreshTime = now;

            try
            {
                _cachedCurrentVehicle = utilities.GetCurrentVehicle();
                _cachedInVehicle = _cachedCurrentVehicle != null;
                _cachedCurrentVehicleName = _cachedInVehicle ? ResolveVehicleName(_cachedCurrentVehicle) : "None";
            }
            catch
            {
                _cachedCurrentVehicle = null;
                _cachedInVehicle = false;
                _cachedCurrentVehicleName = "Unknown";
            }

            _cachedOwnedVehicles.Clear();
            _ownedVehicleNames.Clear();

            try
            {
                var owned = utilities.GetOwnedVehicles();
                for (int i = 0; i < owned.Count; i++)
                {
                    var vehicle = owned[i];
                    if (vehicle == null) continue;

                    _cachedOwnedVehicles.Add(vehicle);
                    try
                    {
                        _ownedVehicleNames[vehicle.GetInstanceID()] = ResolveVehicleName(vehicle);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private string GetCachedVehicleName(LandVehicle vehicle)
        {
            if (vehicle == null) return "Vehicle";

            try
            {
                int id = vehicle.GetInstanceID();
                if (_ownedVehicleNames.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            catch { }

            return ResolveVehicleName(vehicle);
        }

        private string ResolveVehicleName(LandVehicle vehicle)
        {
            if (vehicle == null) return "Vehicle";

            try
            {
                var data = vehicle.GetVehicleData();
                if (data != null && !string.IsNullOrEmpty(data.VehicleCode))
                {
                    return data.VehicleCode;
                }
            }
            catch { }

            try
            {
                if (vehicle.gameObject != null && !string.IsNullOrEmpty(vehicle.gameObject.name))
                {
                    return vehicle.gameObject.name;
                }
            }
            catch { }

            return "Vehicle";
        }
    }
}
