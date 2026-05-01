using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.PlayerScripts;

namespace SewerMenu.Features.Misc
{
    /// <summary>
    /// Freecam - Detaches camera from player for free movement.
    /// Uses OnLateUpdate to set camera position AFTER all game camera scripts run.
    /// This is the proper Unity pattern for camera control.
    /// </summary>
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
        private Transform _originalParent;
        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;
        
        // Store all disabled MonoBehaviours to restore later
        private List<MonoBehaviour> _disabledBehaviours = new List<MonoBehaviour>();
        
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

                // Store original parent and local transform
                _originalParent = _camera.transform.parent;
                _originalLocalPos = _camera.transform.localPosition;
                _originalLocalRot = _camera.transform.localRotation;

                // Initialize freecam position and rotation from current world position
                _freecamPosition = _camera.transform.position;
                var euler = _camera.transform.eulerAngles;
                _yaw = euler.y;
                _pitch = euler.x;
                if (_pitch > 180f) _pitch -= 360f;

                // Disable ALL MonoBehaviours on the camera and its parent hierarchy
                // This ensures no script can override our camera position
                _disabledBehaviours.Clear();
                DisableCameraScripts(_camera.gameObject);
                if (_camera.transform.parent != null)
                {
                    DisableCameraScripts(_camera.transform.parent.gameObject);
                }

                // Unparent the camera so it's no longer affected by player movement
                _camera.transform.SetParent(null, true);

                // Lock cursor for FPS-style camera control
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _initialized = true;
                SewerLogger.Success("Freecam enabled - WASD move, Mouse look, Shift=fast, Alt=slow, Space/Ctrl=up/down");
            }, "enabling freecam");
        }

        private void DisableCameraScripts(GameObject obj)
        {
            if (obj == null) return;
            
            try
            {
                var behaviours = obj.GetComponents<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour == null) continue;
                    // Skip Camera component itself (it's not a MonoBehaviour in the same sense)
                    if (behaviour.GetType().Name.Contains("Camera")) continue;
                    
                    if (behaviour.enabled)
                    {
                        behaviour.enabled = false;
                        _disabledBehaviours.Add(behaviour);
                        SewerLogger.Debug($"Disabled camera script: {behaviour.GetType().Name}");
                    }
                }
            }
            catch { }
        }

        public override void OnDisable()
        {
            SafeExecute(() =>
            {
                // Reparent the camera back to its original parent
                if (_camera != null && _originalParent != null)
                {
                    _camera.transform.SetParent(_originalParent, false);
                    _camera.transform.localPosition = _originalLocalPos;
                    _camera.transform.localRotation = _originalLocalRot;
                }

                // Re-enable all previously disabled behaviours
                foreach (var behaviour in _disabledBehaviours)
                {
                    if (behaviour != null)
                    {
                        behaviour.enabled = true;
                    }
                }
                _disabledBehaviours.Clear();

                // Re-lock cursor for normal gameplay
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _initialized = false;
                SewerLogger.Debug("Freecam disabled - camera restored");
            }, "disabling freecam");
        }

        /// <summary>
        /// OnUpdate handles input processing.
        /// </summary>
        public override void OnUpdate()
        {
            if (!IsEnabled || !_initialized) return;
            if (_camera == null) return;
            
            // Don't process when menu is open
            if (UI.MenuController.Instance.IsVisible) return;

            SafeExecute(() =>
            {
                HandleRotation();
                HandleMovement();
            }, "processing freecam input");
        }

        /// <summary>
        /// OnLateUpdate applies camera position AFTER all game scripts run.
        /// This is the key to making freecam work - we run last and override everything.
        /// </summary>
        public override void OnLateUpdate()
        {
            if (!IsEnabled || !_initialized) return;
            if (_camera == null) return;
            
            // Don't update camera when menu is open
            if (UI.MenuController.Instance.IsVisible) return;

            // Apply position and rotation - this runs AFTER all game camera scripts
            _camera.transform.position = _freecamPosition;
            _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void HandleRotation()
        {
            // FPS-style mouse look (cursor is locked)
            float mouseX = Input.GetAxis("Mouse X") * LookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * LookSensitivity;

            _yaw += mouseX;
            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
        }

        private void HandleMovement()
        {
            Vector3 direction = Vector3.zero;
            
            // Calculate forward/right based on current yaw rotation
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

            // Up/Down (Space/Ctrl or E/Q)
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
                direction += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q))
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

        /// <summary>
        /// Teleport the freecam back to the player's position.
        /// </summary>
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
