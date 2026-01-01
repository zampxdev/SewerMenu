using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.World
{
    /// <summary>
    /// Disables police and clears wanted level.
    /// Uses direct IL2CPP access via GameTypes.
    /// 
    /// PoliceOfficer key methods:
    /// - Deactivate() - Deactivates the officer
    /// - Activate() - Activates the officer
    /// 
    /// PoliceOfficer key properties:
    /// - AutoDeactivate (get/set)
    /// - Suspicion (get/set)
    /// - PursuitTarget (get)
    /// </summary>
    public class PoliceDisable : FeatureBase
    {
        public override string Id => "policedisable";
        public override string Name => "Disable Police";
        public override string Description => "Disable police and clear wanted level";
        public override FeatureCategory Category => FeatureCategory.World;

        // Throttle updates to once per second instead of every frame
        private float _lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 1.0f;

        public override void OnEnable()
        {
            // Run immediately on enable
            ClearWantedLevel();
            DisablePoliceAI();
            _lastUpdateTime = Time.time;
            SewerLogger.Debug("PoliceDisable enabled");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("PoliceDisable disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            // Only run once per second to avoid lag
            if (Time.time - _lastUpdateTime < UPDATE_INTERVAL) return;
            _lastUpdateTime = Time.time;

            // Periodically clear wanted level and disable police (for newly spawned officers)
            SafeExecute(() =>
            {
                ClearWantedLevel();
                DisablePoliceAI();
            }, "maintaining police disable");
        }

        /// <summary>
        /// Gets the count of active police officers.
        /// </summary>
        public int GetPoliceCount()
        {
            try
            {
                var police = GameTypes.GetAllPolice();
                return police?.Length ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Clears the player's wanted level by resetting police suspicion.
        /// </summary>
        public void ClearWantedLevel()
        {
            SafeExecute(() =>
            {
                var police = GameTypes.GetAllPolice();
                if (police == null || police.Length == 0) return;

                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    try
                    {
                        // Reset suspicion
                        officer.Suspicion = 0f;
                    }
                    catch { }
                }
            }, "clearing wanted level");
        }

        /// <summary>
        /// Disables police AI by deactivating officers.
        /// </summary>
        public void DisablePoliceAI()
        {
            SafeExecute(() =>
            {
                var police = GameTypes.GetAllPolice();
                if (police == null || police.Length == 0) return;

                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    try
                    {
                        // Reset suspicion
                        officer.Suspicion = 0f;
                        
                        // Set auto deactivate
                        officer.AutoDeactivate = true;
                        
                        // Deactivate the officer
                        officer.Deactivate();
                    }
                    catch { }
                }
            }, "disabling police AI");
        }

        /// <summary>
        /// Removes all police from the scene.
        /// </summary>
        public void RemoveAllPolice()
        {
            SafeExecute(() =>
            {
                var police = GameTypes.GetAllPolice();
                if (police == null || police.Length == 0)
                {
                    SewerLogger.Info("No police officers found");
                    return;
                }

                int removedCount = 0;
                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    try
                    {
                        GameObject.Destroy(officer.gameObject);
                        removedCount++;
                    }
                    catch { }
                }

                if (removedCount > 0)
                    SewerLogger.Success($"Removed {removedCount} police officers");
            }, "removing police");
        }

        /// <summary>
        /// Teleports all police away from the player.
        /// </summary>
        public void TeleportPoliceAway()
        {
            SafeExecute(() =>
            {
                var police = GameTypes.GetAllPolice();
                var playerPos = GameTypes.PlayerPosition;
                
                if (police == null || police.Length == 0 || playerPos == Vector3.zero) return;

                int movedCount = 0;
                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    try
                    {
                        // Move officer 500 units away
                        var direction = (officer.transform.position - playerPos).normalized;
                        if (direction == Vector3.zero)
                            direction = Vector3.forward;
                        
                        officer.transform.position = playerPos + direction * 500f;
                        movedCount++;
                    }
                    catch { }
                }

                if (movedCount > 0)
                    SewerLogger.Success($"Teleported {movedCount} police officers away");
            }, "teleporting police away");
        }
    }
}
