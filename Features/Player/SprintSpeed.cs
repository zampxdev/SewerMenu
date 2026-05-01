using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    public class SprintSpeed : FeatureBase
    {
        public override string Id => "sprintspeed";
        public override string Name => "Sprint Speed";
        public override string Description => "Modify your movement speed";
        public override FeatureCategory Category => FeatureCategory.Player;

        public float Multiplier { get; set; } = 2f;
        
        // Static to persist across enable/disable cycles
        private static float _originalSpeed = 1f;
        private static bool _hasStoredOriginal = false;

        public override void OnEnable()
        {
            SewerLogger.Debug($"SprintSpeed enabled - multiplier: {Multiplier}x");
        }

        public override void OnDisable()
        {
            // Restore original speed immediately
            SafeExecute(() =>
            {
                var movement = GameTypes.Movement;
                if (movement != null && _hasStoredOriginal)
                {
                    try
                    {
                        Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier = _originalSpeed;
                        SewerLogger.Debug($"SprintSpeed disabled - restored speed to {_originalSpeed}");
                    }
                    catch { }
                }
            }, "restoring speed");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                var movement = GameTypes.Movement;
                if (movement == null) return;

                try
                {
                    // Store original speed ONCE ever (first time we see the movement component)
                    if (!_hasStoredOriginal)
                    {
                        _originalSpeed = Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier;
                        _hasStoredOriginal = true;
                        SewerLogger.Debug($"SprintSpeed: Stored original speed = {_originalSpeed}");
                    }

                    // Apply speed multiplier
                    float targetSpeed = _originalSpeed * Multiplier;
                    if (System.Math.Abs(Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier - targetSpeed) > 0.01f)
                    {
                        Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier = targetSpeed;
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("SprintSpeed: Failed to set speed", ex);
                }
            }, "updating sprint speed");
        }

        public static void ResetOriginalSpeed()
        {
            _hasStoredOriginal = false;
            _originalSpeed = 1f;
        }
    }
}
