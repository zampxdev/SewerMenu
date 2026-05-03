using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Player
{
    public class SprintSpeed : FeatureBase
    {
        public override string Id => "sprintspeed";
        public override string Name => "Sprint Speed";
        public override string Description => "Modify your movement speed";
        public override FeatureCategory Category => FeatureCategory.Player;

        private float _multiplier = 2f;
        public float Multiplier
        {
            get => _multiplier;
            set
            {
                float clamped = Mathf.Clamp(value, 1f, 10f);
                if (Mathf.Abs(_multiplier - clamped) < 0.001f) return;

                _multiplier = clamped;
                _needsApply = true;
            }
        }
        
        // Static to persist across enable/disable cycles
        private static float _originalSpeed = 1f;
        private static bool _hasStoredOriginal = false;
        private bool _needsApply = true;
        private float _nextApplyTime;
        private const float ReapplyInterval = 0.5f;

        public override void OnEnable()
        {
            _needsApply = true;
            _nextApplyTime = 0f;
            ApplySpeed(true);
            SewerLogger.Debug($"SprintSpeed enabled - multiplier: {Multiplier}x");
        }

        public override void OnDisable()
        {
            // Restore original speed immediately
            SafeExecute(() =>
            {
                if (_hasStoredOriginal)
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

            float now = Time.unscaledTime;
            if (!_needsApply && now < _nextApplyTime)
            {
                return;
            }

            SafeExecute(() =>
            {
                ApplySpeed(false);
                _nextApplyTime = now + ReapplyInterval;
            }, "updating sprint speed");
        }

        private void ApplySpeed(bool force)
        {
            try
            {
                if (!_hasStoredOriginal)
                {
                    _originalSpeed = Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier;
                    _hasStoredOriginal = true;
                    SewerLogger.Debug($"SprintSpeed: Stored original speed = {_originalSpeed}");
                }

                float targetSpeed = _originalSpeed * Multiplier;
                float currentSpeed = Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier;
                if (force || _needsApply || Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
                {
                    Il2CppScheduleOne.PlayerScripts.PlayerMovement.StaticMoveSpeedMultiplier = targetSpeed;
                }

                _needsApply = false;
            }
            catch (System.Exception ex)
            {
                SewerLogger.Error("SprintSpeed: Failed to set speed", ex);
            }
        }

        public static void ResetOriginalSpeed()
        {
            _hasStoredOriginal = false;
            _originalSpeed = 1f;
        }
    }
}
