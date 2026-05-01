using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Allows the player to pass through walls and objects.
    /// Uses WASD for movement, Space/Ctrl for up/down.
    /// Uses GameTypes for player access.
    /// </summary>
    public class NoClip : FeatureBase
    {
        public override string Id => "noclip";
        public override string Name => "No Clip";
        public override string Description => "Pass through walls and objects (WASD + Space/Ctrl)";
        public override FeatureCategory Category => FeatureCategory.Player;

        public float Speed { get; set; } = 15f;
        
        private Collider[] _playerColliders;
        private CharacterController _characterController;
        private Rigidbody _rigidbody;
        private bool _wasKinematic;
        private bool _wasGravity;

        public override void OnEnable()
        {
            // Disable FlyMode if active (mutual exclusion)
            var flyMode = FeatureManager.Instance.GetFeature<FlyMode>("flymode");
            if (flyMode != null && flyMode.IsEnabled)
            {
                flyMode.IsEnabled = false;
                SewerLogger.Debug("Disabled FlyMode (mutual exclusion with NoClip)");
            }
            
            SafeExecute(() =>
            {
                var playerGO = GameTypes.PlayerGameObject;
                if (playerGO == null) return;

                // Disable all colliders on player
                _playerColliders = playerGO.GetComponentsInChildren<Collider>();
                foreach (var col in _playerColliders)
                {
                    if (col != null)
                        col.enabled = false;
                }

                // Handle CharacterController
                _characterController = playerGO.GetComponent<CharacterController>();
                if (_characterController != null)
                    _characterController.enabled = false;

                // Handle Rigidbody
                _rigidbody = playerGO.GetComponent<Rigidbody>();
                if (_rigidbody != null)
                {
                    _wasKinematic = _rigidbody.isKinematic;
                    _wasGravity = _rigidbody.useGravity;
                    _rigidbody.isKinematic = true;
                    _rigidbody.useGravity = false;
                }

                SewerLogger.Debug("NoClip enabled - colliders disabled");
            }, "enabling noclip");
        }

        public override void OnDisable()
        {
            SafeExecute(() =>
            {
                // Re-enable colliders
                if (_playerColliders != null)
                {
                    foreach (var col in _playerColliders)
                    {
                        if (col != null)
                            col.enabled = true;
                    }
                }

                // Re-enable CharacterController
                if (_characterController != null)
                    _characterController.enabled = true;

                // Restore Rigidbody state
                if (_rigidbody != null)
                {
                    _rigidbody.isKinematic = _wasKinematic;
                    _rigidbody.useGravity = _wasGravity;
                }

                SewerLogger.Debug("NoClip disabled - colliders restored");
            }, "disabling noclip");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                var transform = GameTypes.PlayerTransform;
                if (transform == null) return;

                var camera = Camera.main;
                if (camera == null) return;

                Vector3 direction = Vector3.zero;

                // Forward/Back (W/S)
                if (Input.GetKey(KeyCode.W))
                    direction += camera.transform.forward;
                if (Input.GetKey(KeyCode.S))
                    direction -= camera.transform.forward;

                // Left/Right (A/D)
                if (Input.GetKey(KeyCode.A))
                    direction -= camera.transform.right;
                if (Input.GetKey(KeyCode.D))
                    direction += camera.transform.right;

                // Up/Down (Space/Ctrl)
                if (Input.GetKey(KeyCode.Space))
                    direction += Vector3.up;
                if (Input.GetKey(KeyCode.LeftControl))
                    direction -= Vector3.up;

                // Apply movement
                if (direction != Vector3.zero)
                {
                    float currentSpeed = Speed;
                    
                    // Sprint modifier
                    if (Input.GetKey(KeyCode.LeftShift))
                        currentSpeed *= 2f;

                    transform.position += direction.normalized * currentSpeed * Time.deltaTime;
                }
            }, "updating noclip movement");
        }
    }
}
