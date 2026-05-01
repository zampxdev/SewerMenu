using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Enables free flight movement for the player.
    /// </summary>
    public class FlyMode : FeatureBase
    {
        public override string Id => "flymode";
        public override string Name => "Fly Mode";
        public override string Description => "Free flight movement";
        public override FeatureCategory Category => FeatureCategory.Player;

        public float Speed { get; set; } = 20f;
        
        private CharacterController _characterController;
        private Rigidbody _rigidbody;
        private bool _wasKinematic;
        private bool _wasGravity;
        private bool _wasCharacterControllerEnabled;

        public override void OnEnable()
        {
            // Disable NoClip if active (mutual exclusion)
            var noClip = FeatureManager.Instance.GetFeature<NoClip>("noclip");
            if (noClip != null && noClip.IsEnabled)
            {
                noClip.IsEnabled = false;
                SewerLogger.Debug("Disabled NoClip (mutual exclusion with FlyMode)");
            }
            
            SafeExecute(() =>
            {
                var playerGO = GameTypes.PlayerGameObject;
                if (playerGO == null) return;

                // Handle CharacterController
                _characterController = playerGO.GetComponent<CharacterController>();
                if (_characterController != null)
                {
                    _wasCharacterControllerEnabled = _characterController.enabled;
                }

                // Handle Rigidbody
                _rigidbody = playerGO.GetComponent<Rigidbody>();
                if (_rigidbody != null)
                {
                    _wasKinematic = _rigidbody.isKinematic;
                    _wasGravity = _rigidbody.useGravity;
                    _rigidbody.useGravity = false;
                    _rigidbody.velocity = Vector3.zero;
                }

                SewerLogger.Debug("FlyMode enabled");
            }, "enabling fly mode");
        }

        public override void OnDisable()
        {
            SafeExecute(() =>
            {
                // Restore Rigidbody state
                if (_rigidbody != null)
                {
                    _rigidbody.isKinematic = _wasKinematic;
                    _rigidbody.useGravity = _wasGravity;
                }

                SewerLogger.Debug("FlyMode disabled");
            }, "disabling fly mode");
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

                    // Use rigidbody if available, otherwise direct transform
                    if (_rigidbody != null && !_rigidbody.isKinematic)
                    {
                        _rigidbody.velocity = direction.normalized * currentSpeed;
                    }
                    else
                    {
                        transform.position += direction.normalized * currentSpeed * Time.deltaTime;
                    }
                }
                else if (_rigidbody != null && !_rigidbody.isKinematic)
                {
                    _rigidbody.velocity = Vector3.zero;
                }
            }, "updating fly mode movement");
        }
    }
}
