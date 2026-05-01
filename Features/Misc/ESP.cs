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
        public float RefreshInterval { get; set; } = 0.12f;
        public int MaxLabelsPerFrame { get; set; } = 90;

        private readonly List<EspTarget> _policeTargets = new List<EspTarget>(32);
        private readonly List<EspTarget> _dealerTargets = new List<EspTarget>(32);
        private readonly List<EspTarget> _customerTargets = new List<EspTarget>(64);
        private readonly List<EspTarget> _npcTargets = new List<EspTarget>(96);
        private readonly List<EspTarget> _vehicleTargets = new List<EspTarget>(64);
        private readonly List<EspTarget> _itemTargets = new List<EspTarget>(64);
        private readonly List<EspTarget> _scratchTargets = new List<EspTarget>(128);
        private GUIStyle _labelStyle;
        private Camera _camera;
        private Transform _playerTransform;
        private float _nextRefreshTime;
        private int _refreshCursor;

        public override void OnEnable()
        {
            _nextRefreshTime = 0f;
            _refreshCursor = 0;
            ClearAllTargets();
        }

        public override void OnDisable()
        {
            ClearAllTargets();
            _scratchTargets.Clear();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            if (Time.unscaledTime >= _nextRefreshTime)
            {
                SafeExecute(RefreshNextTargetGroup, "refreshing ESP targets");
                _nextRefreshTime = Time.unscaledTime + Mathf.Clamp(RefreshInterval, 0.05f, 0.75f);
            }
        }

        public override void OnGUI()
        {
            if (!IsEnabled) return;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (_camera == null) _camera = Camera.main;
            if (_playerTransform == null) _playerTransform = GameTypes.PlayerTransform;
            if (_camera == null || _playerTransform == null) return;

            var drawn = 0;
            if (ShowPolice) drawn = DrawTargetList(_policeTargets, drawn);
            if (ShowDealers) drawn = DrawTargetList(_dealerTargets, drawn);
            if (ShowCustomers) drawn = DrawTargetList(_customerTargets, drawn);
            if (ShowNPCs) drawn = DrawTargetList(_npcTargets, drawn);
            if (ShowVehicles) drawn = DrawTargetList(_vehicleTargets, drawn);
            if (ShowItems) DrawTargetList(_itemTargets, drawn);
        }

        private void RefreshNextTargetGroup()
        {
            _camera = Camera.main;
            _playerTransform = GameTypes.PlayerTransform;
            if (_camera == null || _playerTransform == null) return;

            for (var attempt = 0; attempt < 6; attempt++)
            {
                var group = _refreshCursor;
                _refreshCursor = (_refreshCursor + 1) % 6;

                var targetList = GetTargetList(group);
                if (!IsGroupEnabled(group))
                {
                    targetList.Clear();
                    continue;
                }

                _scratchTargets.Clear();

                switch (group)
                {
                    case 0: AddPoliceTargets(); break;
                    case 1: AddDealerTargets(); break;
                    case 2: AddCustomerTargets(); break;
                    case 3: AddNpcTargets(); break;
                    case 4: AddVehicleTargets(); break;
                    case 5: AddItemTargets(); break;
                }

                targetList.Clear();
                targetList.AddRange(_scratchTargets);
                break;
            }
        }

        private bool IsGroupEnabled(int group)
        {
            return group switch
            {
                0 => ShowPolice,
                1 => ShowDealers,
                2 => ShowCustomers,
                3 => ShowNPCs,
                4 => ShowVehicles,
                5 => ShowItems,
                _ => false
            };
        }

        private List<EspTarget> GetTargetList(int group)
        {
            return group switch
            {
                0 => _policeTargets,
                1 => _dealerTargets,
                2 => _customerTargets,
                3 => _npcTargets,
                4 => _vehicleTargets,
                5 => _itemTargets,
                _ => _scratchTargets
            };
        }

        private void ClearAllTargets()
        {
            _policeTargets.Clear();
            _dealerTargets.Clear();
            _customerTargets.Clear();
            _npcTargets.Clear();
            _vehicleTargets.Clear();
            _itemTargets.Clear();
        }

        private int DrawTargetList(List<EspTarget> targets, int drawn)
        {
            for (var i = 0; i < targets.Count && drawn < MaxLabelsPerFrame; i++)
            {
                if (DrawEntityLabel(targets[i]))
                {
                    drawn++;
                }
            }

            return drawn;
        }

        private void AddPoliceTargets()
        {
            try
            {
                var police = UnityEngine.Object.FindObjectsOfType<PoliceOfficer>();
                if (police == null) return;

                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    var label = "POLICE";
                    try
                    {
                        var npc = officer.GetComponent<NPC>();
                        if (npc != null && !npc.IsConscious)
                        {
                            label = "POLICE (KO)";
                        }
                    }
                    catch { }

                    AddTarget(officer.transform, label, new Color(1f, 0.2f, 0.2f), 1.8f);
                }
            }
            catch { }
        }

        private void AddDealerTargets()
        {
            try
            {
                var dealers = UnityEngine.Object.FindObjectsOfType<Dealer>();
                if (dealers == null) return;

                foreach (var dealer in dealers)
                {
                    if (dealer == null) continue;

                    var label = "Dealer";
                    try { label = dealer.FirstName ?? "Dealer"; } catch { }

                    try
                    {
                        if (dealer.IsRecruited)
                        {
                            label += " [RECRUITED]";
                        }
                    }
                    catch { }

                    AddTarget(dealer.transform, label, new Color(0.2f, 1f, 0.5f), 1.8f);
                }
            }
            catch { }
        }

        private void AddCustomerTargets()
        {
            try
            {
                var customers = UnityEngine.Object.FindObjectsOfType<Customer>();
                if (customers == null) return;

                foreach (var customer in customers)
                {
                    if (customer == null) continue;

                    var label = "Customer";
                    try
                    {
                        var npc = customer.NPC;
                        if (npc != null)
                        {
                            label = npc.FirstName ?? "Customer";
                        }
                    }
                    catch { }

                    try
                    {
                        if (customer.IsAwaitingDelivery)
                        {
                            label += " [WAITING]";
                        }
                    }
                    catch { }

                    AddTarget(customer.transform, label, new Color(1f, 0.8f, 0.2f), 1.8f);
                }
            }
            catch { }
        }

        private void AddNpcTargets()
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
                        if (npc.GetComponent<PoliceOfficer>() != null) continue;
                        if (npc.GetComponent<Dealer>() != null) continue;
                        if (npc.GetComponent<Customer>() != null) continue;
                    }
                    catch { }

                    var label = "NPC";
                    try { label = npc.FirstName ?? "NPC"; } catch { }

                    try
                    {
                        if (!npc.IsConscious)
                        {
                            label += " (KO)";
                        }
                    }
                    catch { }

                    AddTarget(npc.transform, label, Theme.Text, 1.8f);
                }
            }
            catch { }
        }

        private void AddVehicleTargets()
        {
            try
            {
                var vehicles = UnityEngine.Object.FindObjectsOfType<LandVehicle>();
                if (vehicles == null) return;

                foreach (var vehicle in vehicles)
                {
                    if (vehicle == null) continue;

                    var label = "Vehicle";
                    try
                    {
                        var data = vehicle.GetVehicleData();
                        if (data != null && !string.IsNullOrEmpty(data.VehicleCode))
                        {
                            label = data.VehicleCode;
                        }
                        else
                        {
                            label = vehicle.gameObject.name;
                        }
                    }
                    catch
                    {
                        try { label = vehicle.gameObject.name; } catch { }
                    }

                    try
                    {
                        if (vehicle.IsPlayerOwned)
                        {
                            label += " [OWNED]";
                        }
                    }
                    catch { }

                    AddTarget(vehicle.transform, label, new Color(0.5f, 0.8f, 1f), 2f);
                }
            }
            catch { }
        }

        private void AddItemTargets()
        {
            try
            {
                var cashPickups = UnityEngine.Object.FindObjectsOfType<CashPickup>();
                if (cashPickups != null)
                {
                    foreach (var cash in cashPickups)
                    {
                        if (cash == null) continue;

                        var label = "$";
                        try { label = $"${cash.Value:F0}"; } catch { }
                        AddTarget(cash.transform, label, new Color(0.2f, 1f, 0.2f), 1f);
                    }
                }

                var itemPickups = UnityEngine.Object.FindObjectsOfType<ItemPickup>();
                if (itemPickups != null)
                {
                    foreach (var item in itemPickups)
                    {
                        if (item == null) continue;

                        var label = "Item";
                        try
                        {
                            if (item.ItemToGive != null)
                            {
                                label = item.ItemToGive.Name ?? "Item";
                            }
                        }
                        catch { }

                        AddTarget(item.transform, label, new Color(0.8f, 0.8f, 0.2f), 1f);
                    }
                }
            }
            catch { }
        }

        private void AddTarget(Transform target, string label, Color color, float heightOffset)
        {
            if (target == null || _playerTransform == null) return;

            try
            {
                var distance = Vector3.Distance(_playerTransform.position, target.position);
                if (distance > MaxDistance || distance < 2f) return;

                _scratchTargets.Add(new EspTarget(target, label, color, heightOffset));
            }
            catch { }
        }

        private bool DrawEntityLabel(EspTarget target)
        {
            if (target.Transform == null || _playerTransform == null || _camera == null) return false;

            float distance;
            Vector3 screenPos;

            try
            {
                distance = Vector3.Distance(_playerTransform.position, target.Transform.position);
                if (distance > MaxDistance || distance < 2f) return false;

                screenPos = _camera.WorldToScreenPoint(target.Transform.position + Vector3.up * target.HeightOffset);
                if (screenPos.z <= 0f) return false;
            }
            catch
            {
                return false;
            }

            screenPos.y = Screen.height - screenPos.y;
            if (screenPos.x < -80f || screenPos.x > Screen.width + 80f ||
                screenPos.y < -60f || screenPos.y > Screen.height + 60f)
            {
                return false;
            }

            var text = ShowDistance
                ? $"{target.Label}\n[{distance:F0}m]"
                : target.Label;

            var scale = Mathf.Clamp(1f - distance / Mathf.Max(MaxDistance, 1f) * 0.5f, 0.55f, 1f);
            _labelStyle.fontSize = Mathf.RoundToInt(12f * scale);

            var width = 120f * scale;
            var height = ShowDistance ? 40f * scale : 24f * scale;
            var rect = new Rect(screenPos.x - width / 2f, screenPos.y - height / 2f, width, height);

            if (ShowBoxes)
            {
                var boxColor = target.Color;
                boxColor.a = 0.3f;
                GUI.color = boxColor;
                GUI.Box(rect, string.Empty);
                GUI.color = Color.white;
            }

            _labelStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, _labelStyle);

            _labelStyle.normal.textColor = target.Color;
            GUI.Label(rect, text, _labelStyle);
            return true;
        }

        private readonly struct EspTarget
        {
            public readonly Transform Transform;
            public readonly string Label;
            public readonly Color Color;
            public readonly float HeightOffset;

            public EspTarget(Transform transform, string label, Color color, float heightOffset)
            {
                Transform = transform;
                Label = label;
                Color = color;
                HeightOffset = heightOffset;
            }
        }
    }
}
