using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using SewerMenu.UI;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.Features.Misc
{
    /// <summary>
    /// Draws labels and boxes for entities through walls using actual game types.
    /// </summary>
    public class ESP : FeatureBase
    {
        public override string Id => "esp";
        public override string Name => "ESP";
        public override string Description => "See entities through walls";
        public override FeatureCategory Category => FeatureCategory.Misc;

        public bool ShowNPCs { get; set; } = true;
        public bool ShowCustomers { get; set; } = true;
        public bool ShowDealers { get; set; } = true;
        public bool ShowVehicles { get; set; } = true;
        public bool ShowPolice { get; set; } = true;
        public bool ShowItems { get; set; } = false;
        public bool ShowDistance { get; set; } = true;
        public bool ShowBoxes { get; set; } = false;
        public float MaxDistance { get; set; } = 100f;

        private GUIStyle _labelStyle;
        private Camera _camera;
        private Transform _playerTransform;
        
        // Cached arrays to reduce GC
        private float _lastUpdateTime;
        private const float UpdateInterval = 0.1f;

        public override void OnGUI()
        {
            if (!IsEnabled) return;

            // Initialize style
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            // Get camera and player
            _camera = Camera.main;
            var player = GameFinder.GetLocalPlayer();
            _playerTransform = player?.transform;

            if (_camera == null || _playerTransform == null) return;

            // Draw ESP for different entity types using actual game types
            if (ShowPolice) DrawPoliceESP();
            if (ShowDealers) DrawDealerESP();
            if (ShowCustomers) DrawCustomerESP();
            if (ShowNPCs) DrawNPCESP();
            if (ShowVehicles) DrawVehicleESP();
            if (ShowItems) DrawItemESP();
        }

        private void DrawPoliceESP()
        {
            try
            {
                var police = UnityEngine.Object.FindObjectsOfType<PoliceOfficer>();
                if (police == null) return;
                
                foreach (var officer in police)
                {
                    if (officer == null) continue;
                    try
                    {
                        string label = "POLICE";
                        // Try to get officer state
                        try
                        {
                            var npc = officer.GetComponent<NPC>();
                            if (npc != null && npc.IsConscious == false)
                                label = "POLICE (KO)";
                        }
                        catch { }
                        
                        DrawEntityLabel(officer.transform, label, new Color(1f, 0.2f, 0.2f), 1.8f);
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        private void DrawDealerESP()
        {
            try
            {
                var dealers = UnityEngine.Object.FindObjectsOfType<Dealer>();
                if (dealers == null) return;
                
                foreach (var dealer in dealers)
                {
                    if (dealer == null) continue;
                    try
                    {
                        string name = "Dealer";
                        try { name = dealer.FirstName ?? "Dealer"; } catch { }
                        
                        string label = name;
                        try
                        {
                            if (dealer.IsRecruited)
                                label += " [RECRUITED]";
                        }
                        catch { }
                        
                        DrawEntityLabel(dealer.transform, label, new Color(0.2f, 1f, 0.5f), 1.8f);
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        private void DrawCustomerESP()
        {
            try
            {
                var customers = UnityEngine.Object.FindObjectsOfType<Customer>();
                if (customers == null) return;
                
                foreach (var customer in customers)
                {
                    if (customer == null) continue;
                    try
                    {
                        string name = "Customer";
                        try 
                        { 
                            var npc = customer.NPC;
                            if (npc != null)
                                name = npc.FirstName ?? "Customer";
                        } 
                        catch { }
                        
                        string label = name;
                        try
                        {
                            if (customer.IsAwaitingDelivery)
                                label += " [WAITING]";
                        }
                        catch { }
                        
                        DrawEntityLabel(customer.transform, label, new Color(1f, 0.8f, 0.2f), 1.8f);
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        private void DrawNPCESP()
        {
            try
            {
                var npcs = UnityEngine.Object.FindObjectsOfType<NPC>();
                if (npcs == null) return;
                
                foreach (var npc in npcs)
                {
                    if (npc == null) continue;
                    try
                    {
                        // Skip if already shown as police, dealer, or customer
                        if (npc.GetComponent<PoliceOfficer>() != null) continue;
                        if (npc.GetComponent<Dealer>() != null) continue;
                        if (npc.GetComponent<Customer>() != null) continue;
                        
                        string name = "NPC";
                        try { name = npc.FirstName ?? "NPC"; } catch { }
                        
                        string label = name;
                        try
                        {
                            if (!npc.IsConscious)
                                label += " (KO)";
                        }
                        catch { }
                        
                        DrawEntityLabel(npc.transform, label, Theme.Text, 1.8f);
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        private void DrawVehicleESP()
        {
            try
            {
                var vehicles = UnityEngine.Object.FindObjectsOfType<LandVehicle>();
                if (vehicles == null) return;
                
                foreach (var vehicle in vehicles)
                {
                    if (vehicle == null) continue;
                    try
                    {
                        string name = "Vehicle";
                        try 
                        { 
                            // Use VehicleCode from VehicleData, or fallback to GameObject name
                            var data = vehicle.GetVehicleData();
                            if (data != null && !string.IsNullOrEmpty(data.VehicleCode))
                                name = data.VehicleCode;
                            else
                                name = vehicle.gameObject.name;
                        } 
                        catch 
                        {
                            try { name = vehicle.gameObject.name; } catch { }
                        }
                        
                        string label = name;
                        try
                        {
                            if (vehicle.IsPlayerOwned)
                                label += " [OWNED]";
                        }
                        catch { }
                        
                        DrawEntityLabel(vehicle.transform, label, new Color(0.5f, 0.8f, 1f), 2f);
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        private void DrawItemESP()
        {
            try
            {
                // Cash pickups
                var cashPickups = UnityEngine.Object.FindObjectsOfType<CashPickup>();
                if (cashPickups != null)
                {
                    foreach (var cash in cashPickups)
                    {
                        if (cash == null) continue;
                        try
                        {
                            string label = $"${cash.Value:F0}";
                            DrawEntityLabel(cash.transform, label, new Color(0.2f, 1f, 0.2f), 1f);
                        }
                        catch { continue; }
                    }
                }
                
                // Item pickups
                var itemPickups = UnityEngine.Object.FindObjectsOfType<ItemPickup>();
                if (itemPickups != null)
                {
                    foreach (var item in itemPickups)
                    {
                        if (item == null) continue;
                        try
                        {
                            string name = "Item";
                            try
                            {
                                if (item.ItemToGive != null)
                                    name = item.ItemToGive.Name ?? "Item";
                            }
                            catch { }
                            
                            DrawEntityLabel(item.transform, name, new Color(0.8f, 0.8f, 0.2f), 1f);
                        }
                        catch { continue; }
                    }
                }
            }
            catch { }
        }

        private void DrawEntityLabel(Transform target, string label, Color color, float heightOffset = 1.5f)
        {
            if (target == null || _playerTransform == null) return;

            float distance = Vector3.Distance(_playerTransform.position, target.position);
            if (distance > MaxDistance || distance < 2f) return; // Skip very close entities

            Vector3 screenPos = _camera.WorldToScreenPoint(target.position + Vector3.up * heightOffset);
            if (screenPos.z <= 0) return; // Behind camera

            // Flip Y for GUI coordinates
            screenPos.y = Screen.height - screenPos.y;

            // Build label text
            string text = label;
            if (ShowDistance)
                text += $"\n[{distance:F0}m]";

            // Scale based on distance
            float scale = Mathf.Clamp(1f - (distance / MaxDistance) * 0.5f, 0.5f, 1f);
            int fontSize = Mathf.RoundToInt(12 * scale);
            _labelStyle.fontSize = fontSize;

            float width = 120 * scale;
            float height = 40 * scale;

            // Draw box if enabled
            if (ShowBoxes)
            {
                var boxColor = color;
                boxColor.a = 0.3f;
                GUI.color = boxColor;
                GUI.Box(new Rect(screenPos.x - width/2, screenPos.y - height/2, width, height), "");
                GUI.color = Color.white;
            }

            // Draw shadow
            _labelStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(screenPos.x - width/2 + 1, screenPos.y - height/2 + 1, width, height), text, _labelStyle);
            
            // Draw label
            _labelStyle.normal.textColor = color;
            GUI.Label(new Rect(screenPos.x - width/2, screenPos.y - height/2, width, height), text, _labelStyle);
        }
    }
}
