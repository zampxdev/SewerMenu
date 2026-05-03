using System;
using UnityEngine;
using SewerMenu.Core.Config;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne;
using Il2CppScheduleOne.PlayerScripts;

namespace SewerMenu.UI
{
    /// <summary>
    /// Keeps gameplay input from leaking through while the IMGUI menu is open.
    /// </summary>
    public class MenuInputBlocker
    {
        private static MenuInputBlocker _instance;
        public static MenuInputBlocker Instance => _instance ??= new MenuInputBlocker();

        private GameInput _gameInput;
        private PlayerInventory _inventory;

        private bool _isLocked;
        private bool _restorePlayerInputEnabled = true;
        private bool _restoreHotbarEnabled = true;
        private bool _restoreEquippingEnabled = true;
        private bool _restoreHolsterEnabled = true;
        private bool _restoreManagementSlotEnabled = true;

        private MenuInputBlocker() { }

        public bool IsLocked => _isLocked;

        public void Update()
        {
            bool shouldLock = MenuController.Instance.IsModalInputSurfaceVisible ||
                              (MenuController.Instance.IsVisible &&
                               (ConfigManager.Instance.Config?.UI?.LockGameInputWhenMenuOpen ?? true));

            if (!shouldLock)
            {
                if (_isLocked || _gameInput != null || _inventory != null)
                {
                    Release();
                }
                return;
            }

            if (!_isLocked)
            {
                Acquire();
            }

            MaintainLock();
        }

        public void Release()
        {
            if (!_isLocked && _gameInput == null && _inventory == null) return;

            try
            {
                if (_gameInput != null && _gameInput.PlayerInput != null)
                {
                    _gameInput.PlayerInput.enabled = _restorePlayerInputEnabled;
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not restore PlayerInput: " + ex.Message);
            }

            try
            {
                if (_inventory != null)
                {
                    _inventory.SetInventoryEnabled(_restoreHotbarEnabled);
                    _inventory.SetEquippingEnabled(_restoreEquippingEnabled);
                    _inventory.SetManagementClipboardEnabled(_restoreManagementSlotEnabled);
                    _inventory.HolsterEnabled = _restoreHolsterEnabled;
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not restore inventory input: " + ex.Message);
            }

            _gameInput = null;
            _inventory = null;
            _isLocked = false;
        }

        public void ConsumeCurrentGuiEvent()
        {
            Event e = Event.current;
            if (e == null || e.type == EventType.Layout || e.type == EventType.Repaint || e.type == EventType.Used)
            {
                return;
            }

            if (e.isMouse || e.isKey || e.type == EventType.ScrollWheel)
            {
                e.Use();
            }
        }

        private void Acquire()
        {
            try
            {
                if (GameInput.InstanceExists)
                {
                    _gameInput = GameInput.Instance;
                    if (_gameInput != null && _gameInput.PlayerInput != null)
                    {
                        _restorePlayerInputEnabled = _gameInput.PlayerInput.enabled;
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not acquire GameInput for menu lock: " + ex.Message);
            }

            try
            {
                _inventory = GameTypes.Inventory;
                if (_inventory != null)
                {
                    _restoreHotbarEnabled = _inventory.HotbarEnabled;
                    _restoreEquippingEnabled = _inventory.EquippingEnabled;
                    _restoreHolsterEnabled = _inventory.HolsterEnabled;
                    _restoreManagementSlotEnabled = _inventory.ManagementSlotEnabled;
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not acquire inventory for menu lock: " + ex.Message);
            }

            _isLocked = true;
        }

        private void MaintainLock()
        {
            try
            {
                if (_gameInput == null && GameInput.InstanceExists)
                {
                    _gameInput = GameInput.Instance;
                    if (_gameInput != null && _gameInput.PlayerInput != null)
                    {
                        _restorePlayerInputEnabled = _gameInput.PlayerInput.enabled;
                    }
                }

                if (_gameInput != null)
                {
                    if (_gameInput.PlayerInput != null && _gameInput.PlayerInput.enabled)
                    {
                        _gameInput.PlayerInput.enabled = false;
                    }

                    GameInput.MouseWheelAxis = 0f;
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not maintain GameInput lock: " + ex.Message);
            }

            try
            {
                if (_inventory == null)
                {
                    _inventory = GameTypes.Inventory;
                    if (_inventory != null)
                    {
                        _restoreHotbarEnabled = _inventory.HotbarEnabled;
                        _restoreEquippingEnabled = _inventory.EquippingEnabled;
                        _restoreHolsterEnabled = _inventory.HolsterEnabled;
                        _restoreManagementSlotEnabled = _inventory.ManagementSlotEnabled;
                    }
                }

                if (_inventory != null)
                {
                    if (_inventory.HotbarEnabled || _inventory.EquippingEnabled || _inventory.ManagementSlotEnabled)
                    {
                        _inventory.SetInventoryEnabled(false);
                    }

                    if (_inventory.EquippingEnabled)
                    {
                        _inventory.SetEquippingEnabled(false);
                    }

                    if (_inventory.ManagementSlotEnabled)
                    {
                        _inventory.SetManagementClipboardEnabled(false);
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Debug("Could not maintain inventory input lock: " + ex.Message);
            }
        }
    }
}
