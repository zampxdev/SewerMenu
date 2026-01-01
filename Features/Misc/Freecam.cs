using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.PlayerScripts;

namespace SewerMenu.Features.Misc
{
    // Disables PlayerCamera component and controls Camera.main directly
    public class Freecam : FeatureBase
    {
        public override string Id => "freecam";
        public override string Name => "Freecam";
        public override string Description => "Free camera movement (detached from player)";
        public override FeatureCategory Category => FeatureCategory.Misc;

        public float MoveSpeed { get; set; } = 20f;
        public float LookSensitivity { get; set; } = 3f;
        public float FastMultiplier { get; set; } = 3f;

        private Camera _camera;
        private PlayerCamera _playerCamera;
        private bool _playerCameraWasEnabled;
        private Vector3 _freecamPosition;
        private float _yaw;
        private float _pitch;
        private bool _initialized;

        public override void OnEnable()
        {
            SafeExecute(() =>
            {
                // Get the main camera
                _camera = Camera.main;
                if (_camera == null)
                {
                    SewerLogger.Warning("Main camera not found");
                    IsEnabled = false;
                    return;
                }

                // Disable the PlayerCamera component so it stops controlling the camera
                _playerCamera = GameTypes.Camera;
                if (_playerCamera != null)
                {
                    _playerCameraWasEnabled = _playerCamera.enabled;
                    _playerCamera.enabled = false;
                    SewerLogger.Debug("Disabled PlayerCamera");
                }

                // Initialize freecam position and rotation from current camera
                _freecamPosition = _camera.transform.position;
                var euler = _camera.transform.eulerAngles;
                _yaw = euler.y;
                _pitch = euler.x;
                if (_pitch > 180f) _pitch -= 360f;

                _initialized = true;
                SewerLogger.Success("Freecam enabled - use WASD to move, RMB+mouse to look");
            }, "enabling freecam");
        }

        public override void OnDisable()
        {
            SafeExecute(() =>
            {
                // Re-enable PlayerCamera
                if (_playerCamera != null && _playerCameraWasEnabled)
                {
                    _playerCamera.enabled = true;
                    SewerLogger.Debug("Re-enabled PlayerCamera");
                }

                _initialized = false;
                SewerLogger.Debug("Freecam disabled");
            }, "disabling freecam");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled || !_initialized || _camera == null) return;

            // Don't process freecam when menu is open (menu handles its own camera disable)
            if (UI.MenuController.Instance.IsVisible) return;

            SafeExecute(() =>
            {
                HandleRotation();
                HandleMovement();
                
                // Apply position and rotation to camera
                _camera.transform.position = _freecamPosition;
                _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }, "updating freecam");
        }

        private void HandleRotation()
        {
            // Only rotate when right mouse button is held
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * LookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * LookSensitivity;

                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            }
        }

        private void HandleMovement()
        {
            Vector3 direction = Vector3.zero;
            
            // Calculate forward/right based on current rotation
            var rotation = Quaternion.Euler(0f, _yaw, 0f);
            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;

            // Forward/Back (W/S)
            if (Input.GetKey(KeyCode.W))
                direction += forward;
            if (Input.GetKey(KeyCode.S))
                direction -= forward;

            // Left/Right (A/D)
            if (Input.GetKey(KeyCode.A))
                direction -= right;
            if (Input.GetKey(KeyCode.D))
                direction += right;

            // Up/Down (E/Q or Space/Ctrl)
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
                direction += Vector3.up;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
                direction -= Vector3.up;

            // Apply movement
            if (direction != Vector3.zero)
            {
                float speed = MoveSpeed;
                
                // Fast mode with Shift
                if (Input.GetKey(KeyCode.LeftShift))
                    speed *= FastMultiplier;
                
                // Slow mode with Alt
                if (Input.GetKey(KeyCode.LeftAlt))
                    speed *= 0.25f;

                _freecamPosition += direction.normalized * speed * Time.deltaTime;
            }
        }

        public void TeleportToPlayer()
        {
            SafeExecute(() =>
            {
                var playerPos = GameTypes.PlayerPosition;
                if (playerPos != Vector3.zero)
                {
                    _freecamPosition = playerPos + Vector3.up * 2f;
                    SewerLogger.Success("Freecam teleported to player");
                }
            }, "teleporting freecam to player");
        }
    }
}
