using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    public class JumpHeight : FeatureBase
    {
        public override string Id => "jumpheight";
        public override string Name => "Jump Height";
        public override string Description => "Jump higher than normal";
        public override FeatureCategory Category => FeatureCategory.Player;

        public float Multiplier { get; set; } = 2f;
        
        private bool _wasGrounded = true;
        private bool _jumpBoostApplied = false;
        
        // Tuning constants
        private const float BaseJumpVelocity = 5.5f;
        private const float BoostStrength = 7.5f;  // Multiplier for the boost force
        private const float BoostDuration = 0.25f; // Duration to apply boost

        public override void OnEnable()
        {
            _wasGrounded = true;
            _jumpBoostApplied = false;
        }

        public override void OnDisable()
        {
            _wasGrounded = true;
            _jumpBoostApplied = false;
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                var movement = GameTypes.Movement;
                if (movement == null) return;

                bool isGrounded = movement.IsGrounded;
                
                if (isGrounded)
                {
                    _jumpBoostApplied = false;
                    _wasGrounded = true;
                    return;
                }
                
                // Detect jump: transitioned from grounded to airborne
                if (_wasGrounded && !isGrounded && !_jumpBoostApplied)
                {
                    _jumpBoostApplied = true;
                    
                    // Calculate boost velocity
                    // For Nx height multiplier, velocity needs to be sqrt(N) times original
                    // Extra velocity = baseVel * (sqrt(N) - 1)
                    float extraVelocityFactor = Mathf.Sqrt(Multiplier) - 1f;
                    float boostVelocity = BaseJumpVelocity * extraVelocityFactor * BoostStrength;
                    
                    // Apply boost using game's residual velocity system
                    movement.SetResidualVelocity(Vector3.up, boostVelocity, BoostDuration);
                }
                
                _wasGrounded = isGrounded;
                
            }, "JumpHeight");
        }
    }
}
