using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.Vehicles;

namespace SewerMenu.Features.Vehicles
{
    public class VehicleUtilities : FeatureBase
    {
        public override string Id => "vehicleutilities";
        public override string Name => "Vehicle Utilities";
        public override string Description => "Flip, repair, and teleport to vehicles";
        public override FeatureCategory Category => FeatureCategory.Vehicles;
        public override bool IsToggleable => false;
        
        public LandVehicle GetCurrentVehicle()
        {
            try
            {
                var movement = GameTypes.Movement;
                if (movement != null)
                    return movement.CurrentVehicle;
            }
            catch { }
            return null;
        }
        
        public bool IsInVehicle()
        {
            return GetCurrentVehicle() != null;
        }
        
        public List<LandVehicle> GetOwnedVehicles()
        {
            var result = new List<LandVehicle>();
            try
            {
                var vehicles = Object.FindObjectsOfType<LandVehicle>();
                if (vehicles == null) return result;
                
                foreach (var vehicle in vehicles)
                {
                    if (vehicle == null) continue;
                    try
                    {
                        if (vehicle.IsPlayerOwned)
                            result.Add(vehicle);
                    }
                    catch { }
                }
            }
            catch { }
            return result;
        }
        
        public void TeleportToVehicle(LandVehicle vehicle)
        {
            if (vehicle == null) return;
            
            try
            {
                var playerTransform = GameTypes.PlayerTransform;
                if (playerTransform == null) return;
                
                var cc = playerTransform.GetComponent<CharacterController>();
                bool wasEnabled = false;
                if (cc != null)
                {
                    wasEnabled = cc.enabled;
                    cc.enabled = false;
                }
                
                Vector3 targetPos = vehicle.transform.position + vehicle.transform.right * 2f + Vector3.up * 0.5f;
                playerTransform.position = targetPos;
                
                if (cc != null)
                    cc.enabled = wasEnabled;
            }
            catch { }
        }
        
        public void FlipVehicle(LandVehicle vehicle)
        {
            if (vehicle == null) return;
            
            try
            {
                var rb = vehicle.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                var currentRot = vehicle.transform.rotation.eulerAngles;
                vehicle.transform.rotation = Quaternion.Euler(0f, currentRot.y, 0f);
                vehicle.transform.position += Vector3.up * 0.5f;
            }
            catch { }
        }
        
        public void FlipCurrentVehicle()
        {
            var vehicle = GetCurrentVehicle();
            if (vehicle != null)
            {
                FlipVehicle(vehicle);
            }
        }
        
        public override void Execute()
        {
            FlipCurrentVehicle();
        }
    }
}
