using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime;
using SewerMenu.Core;
using SewerMenu.Core.Logging;
using SewerMenu.Core.Config;
using SewerMenu.Features.Base;
using SewerMenu.UI.Pages;
using SewerMenu.UI.Windows;
using SewerMenu.Utils;
using Il2CppScheduleOne.PlayerScripts;

namespace SewerMenu.UI
{
    /// <summary>
    /// Main controller for the SewerMenu UI.
    /// Designed to be IL2CPP GC-safe.
    /// </summary>
    public class MenuController
    {
        #region Singleton
        
        private static MenuController _instance;
        public static MenuController Instance => _instance ??= new MenuController();
        
        private MenuController() { }
        
        #endregion
        
        #region Static Fields (prevent GC)
        
        // CRITICAL: Store delegate as static to prevent IL2CPP GC
        // For IL2CPP, we use System.Action<int> and convert it
        private static System.Action<int> _managedWindowFunc;
        private static GUI.WindowFunction _cachedWindowFunc;
        
        // Cache for player camera component
        private static PlayerCamera _playerCamera;
        private static bool _cameraWasEnabled;
        
        #endregion
        
        #region Fields
        
        private bool _initialized = false;
        private bool _isVisible = false;
        private int _currentTab = 0;
        private Vector2 _scrollPosition;
        private Rect _windowRect;
        private readonly int _windowId = 12345;
        
        // Resize state
        private bool _isResizing = false;
        private Vector2 _resizeStartMousePos;
        private Vector2 _resizeStartSize;
        
        // Size constraints
        private const float MinWidth = 500f;
        private const float MaxWidth = 1400f;
        private const float MinHeight = 400f;
        private const float MaxHeight = 1000f;
        private const float ResizeHandleSize = 20f;
        
        private readonly string[] _tabNames = { "Player", "Economy", "Items", "World", "Vehicles", "Misc", "Settings" };
        private readonly FeatureCategory[] _tabCategories = 
        { 
            FeatureCategory.Player, 
            FeatureCategory.Economy, 
            FeatureCategory.Items, 
            FeatureCategory.World, 
            FeatureCategory.Vehicles,
            FeatureCategory.Misc,
            FeatureCategory.Settings
        };
        
        private Dictionary<FeatureCategory, IPage> _pages = new Dictionary<FeatureCategory, IPage>();
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        public bool IsCapturingInput => _isVisible;
        public Rect WindowRect => _windowRect;
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            if (_initialized) return;
            
            // Create the delegate ONCE and cache it statically
            // For IL2CPP, we need to create the delegate properly
            _managedWindowFunc = DrawWindowInternal;
            _cachedWindowFunc = DelegateSupport.ConvertDelegate<GUI.WindowFunction>(_managedWindowFunc);
            
            var config = ConfigManager.Instance.Config;
            _windowRect = new Rect(config.UI.WindowX, config.UI.WindowY, config.UI.WindowWidth, config.UI.WindowHeight);
            _currentTab = Mathf.Clamp(config.UI.LastTab, 0, _tabNames.Length - 1);
            
            InitializePages();
            
            _initialized = true;
            SewerLogger.Info("MenuController initialized");
        }
        
        private void InitializePages()
        {
            _pages[FeatureCategory.Player] = new PlayerPage();
            _pages[FeatureCategory.Economy] = new EconomyPage();
            _pages[FeatureCategory.Items] = new ItemsPage();
            _pages[FeatureCategory.World] = new WorldPage();
            _pages[FeatureCategory.Vehicles] = new VehiclesPage();
            _pages[FeatureCategory.Misc] = new MiscPage();
            _pages[FeatureCategory.Settings] = new SettingsPage();
            
            foreach (var page in _pages.Values)
            {
                page.Initialize();
            }
        }
        
        #endregion
        
        #region Show/Hide
        
        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }
        
        public void Show()
        {
            if (_isVisible) return;
            _isVisible = true;
            
            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Disable player camera to stop mouse look
            DisablePlayerCamera();
            
            SewerLogger.Info("Menu opened");
        }
        
        public void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;
            
            // Re-enable player camera
            EnablePlayerCamera();
            
            // Restore cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            SaveWindowState();
            SewerLogger.Info("Menu closed");
        }
        
        private void DisablePlayerCamera()
        {
            try
            {
                // Use GameTypes to get the PlayerCamera
                _playerCamera = GameTypes.Camera;
                
                if (_playerCamera != null)
                {
                    _cameraWasEnabled = _playerCamera.enabled;
                    _playerCamera.enabled = false;
                    SewerLogger.Debug("Disabled PlayerCamera");
                }
                else
                {
                    SewerLogger.Debug("PlayerCamera not found, trying fallback...");
                    // Fallback: try to find any camera-related component
                    var player = GameTypes.LocalPlayer;
                    if (player != null)
                    {
                        var components = player.GetComponentsInChildren<MonoBehaviour>(true);
                        foreach (var comp in components)
                        {
                            if (comp == null) continue;
                            var name = comp.GetType().Name.ToLower();
                            if (name.Contains("camera") || name.Contains("look") || name.Contains("mouse"))
                            {
                                comp.enabled = false;
                                SewerLogger.Debug($"Disabled fallback component: {comp.GetType().Name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"Could not disable player camera: {ex.Message}");
            }
        }
        
        private void EnablePlayerCamera()
        {
            try
            {
                if (_playerCamera != null && _cameraWasEnabled)
                {
                    _playerCamera.enabled = true;
                    SewerLogger.Debug("Re-enabled PlayerCamera");
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"Could not re-enable player camera: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Update
        
        public void Update()
        {
            if (!_isVisible) return;
            
            try
            {
                // Force cursor state every frame when menu is open
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Keep camera disabled while menu is open
                if (_playerCamera != null && _playerCamera.enabled)
                {
                    _playerCamera.enabled = false;
                }
                
                // Escape to close
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Hide();
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Error in MenuController.Update", ex);
            }
        }
        
        #endregion
        
        #region OnGUI
        
        public void OnGUI()
        {
            if (!_initialized || !_isVisible) return;
            
            // Initialize skin
            SewerSkin.BeginUI();
            
            // Handle resize BEFORE drawing window
            HandleResize();
            
            // Clamp window to screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
            _windowRect.width = Mathf.Clamp(_windowRect.width, MinWidth, MaxWidth);
            _windowRect.height = Mathf.Clamp(_windowRect.height, MinHeight, MaxHeight);
            
            // Draw window using cached delegate
            _windowRect = GUI.Window(_windowId, _windowRect, _cachedWindowFunc, "");
            
            // Draw resize grip on top of window
            DrawResizeGrip();
            
            // Draw popup windows (ItemSpawner, etc.)
            ItemSpawnerWindow.Instance.OnGUI();
            
            SewerSkin.EndUI();
        }
        
        private bool _isHoveringResize = false;
        
        private void HandleResize()
        {
            // Get resize handle rect (bottom-right corner)
            Rect resizeRect = new Rect(
                _windowRect.x + _windowRect.width - ResizeHandleSize,
                _windowRect.y + _windowRect.height - ResizeHandleSize,
                ResizeHandleSize,
                ResizeHandleSize
            );
            
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;
            
            // Check if mouse is in resize area
            _isHoveringResize = resizeRect.Contains(mousePos);
            
            // Handle mouse events
            if (e.type == EventType.MouseDown && e.button == 0 && _isHoveringResize)
            {
                _isResizing = true;
                _resizeStartMousePos = mousePos;
                _resizeStartSize = new Vector2(_windowRect.width, _windowRect.height);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _isResizing)
            {
                _isResizing = false;
                SaveWindowState();
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isResizing)
            {
                Vector2 delta = mousePos - _resizeStartMousePos;
                _windowRect.width = Mathf.Clamp(_resizeStartSize.x + delta.x, MinWidth, MaxWidth);
                _windowRect.height = Mathf.Clamp(_resizeStartSize.y + delta.y, MinHeight, MaxHeight);
                e.Use();
            }
        }
        
        private void DrawResizeGrip()
        {
            float gripSize = ResizeHandleSize + 4f;
            
            // Draw resize grip indicator in bottom-right corner
            Rect gripRect = new Rect(
                _windowRect.x + _windowRect.width - gripSize,
                _windowRect.y + _windowRect.height - gripSize,
                gripSize,
                gripSize
            );
            
            // Determine color based on state
            Color gripColor;
            if (_isResizing)
                gripColor = SewerSkin.AccentColor;
            else if (_isHoveringResize)
                gripColor = new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.7f);
            else
                gripColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            
            // Draw background triangle
            var oldBg = GUI.backgroundColor;
            var oldColor = GUI.color;
            
            // Draw a subtle background
            GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            GUI.DrawTexture(gripRect, Texture2D.whiteTexture);
            
            // Draw the grip dots pattern (3x3 dots in corner)
            GUI.color = gripColor;
            float dotSize = 3f;
            float dotSpacing = 5f;
            float startX = gripRect.x + gripRect.width - 18f;
            float startY = gripRect.y + gripRect.height - 18f;
            
            // Draw dots in a triangular pattern
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    float x = startX + (2 - row + col) * dotSpacing;
                    float y = startY + row * dotSpacing;
                    GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), Texture2D.whiteTexture);
                }
            }
            
            // Draw corner accent line
            if (_isHoveringResize || _isResizing)
            {
                GUI.color = SewerSkin.AccentColor;
                // Bottom edge highlight
                GUI.DrawTexture(new Rect(gripRect.x, gripRect.y + gripRect.height - 2f, gripRect.width, 2f), Texture2D.whiteTexture);
                // Right edge highlight
                GUI.DrawTexture(new Rect(gripRect.x + gripRect.width - 2f, gripRect.y, 2f, gripRect.height), Texture2D.whiteTexture);
            }
            
            GUI.backgroundColor = oldBg;
            GUI.color = oldColor;
        }
        
        // This method is called by the cached delegate
        private void DrawWindowInternal(int id)
        {
            try
            {
                DrawWindowContent();
            }
            catch (Exception ex)
            {
                SewerSkin.DrawStatus("Error: " + ex.Message, SewerSkin.StatusType.Error);
            }
            
            // Make window draggable from title bar
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 25));
        }
        
        private void DrawWindowContent()
        {
            // ═══════════════════════════════════════════════════════════
            // HEADER
            // ═══════════════════════════════════════════════════════════
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.04f, 0.05f, 0.065f, 1f);
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = oldBg;
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label("SEWER", GUILayout.Height(24));
            GUI.contentColor = SewerSkin.TextColor;
            GUILayout.Label("MENU", GUILayout.Height(24));
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.FlexibleSpace();
            GUILayout.Label("v" + ModInfo.Version, GUILayout.Height(24));
            GUILayout.Space(4);
            GUI.contentColor = oldColor;
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUILayout.Space(4);
            
            // ═══════════════════════════════════════════════════════════
            // TAB BAR
            // ═══════════════════════════════════════════════════════════
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabNames.Length; i++)
            {
                bool isSelected = (_currentTab == i);
                
                oldBg = GUI.backgroundColor;
                oldColor = GUI.contentColor;
                
                if (isSelected)
                {
                    GUI.backgroundColor = SewerSkin.AccentColor;
                    GUI.contentColor = new Color(0.02f, 0.04f, 0.06f, 1f);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.1f, 0.12f, 0.14f, 1f);
                    GUI.contentColor = SewerSkin.TextMutedColor;
                }
                
                if (GUILayout.Button(_tabNames[i], GUILayout.Height(28)))
                {
                    if (_currentTab != i)
                    {
                        _currentTab = i;
                        _scrollPosition = Vector2.zero;
                    }
                }
                
                GUI.backgroundColor = oldBg;
                GUI.contentColor = oldColor;
            }
            GUILayout.EndHorizontal();
            
            // Accent line under tabs - using DrawTexture for clean rendering
            Rect accentLineRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
            oldColor = GUI.color;
            GUI.color = SewerSkin.AccentColor;
            GUI.DrawTexture(accentLineRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            GUILayout.Space(6);
            
            // ═══════════════════════════════════════════════════════════
            // CONTENT AREA
            // ═══════════════════════════════════════════════════════════
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.055f, 0.07f, 0.085f, 1f);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = oldBg;
            
            try
            {
                var category = _tabCategories[_currentTab];
                if (_pages.TryGetValue(category, out var page))
                {
                    page.Draw();
                }
            }
            catch (System.Exception ex)
            {
                SewerSkin.DrawStatus("Tab Error: " + ex.Message, SewerSkin.StatusType.Error);
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.Space(4);
            
            // ═══════════════════════════════════════════════════════════
            // STATUS BAR
            // ═══════════════════════════════════════════════════════════
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.04f, 0.05f, 0.065f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(24));
            GUI.backgroundColor = oldBg;
            
            GUILayout.Space(6);
            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.SuccessColor;
            GUILayout.Label("●", GUILayout.Width(12));
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label(FeatureManager.Instance.EnabledCount + " active");
            GUILayout.FlexibleSpace();
            GUI.contentColor = new Color(0.4f, 0.43f, 0.47f, 1f);
            GUILayout.Label("F8 toggle  •  ESC close");
            GUILayout.Space(6);
            GUI.contentColor = oldColor;
            
            GUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Helpers
        
        private void SaveWindowState()
        {
            try
            {
                var config = ConfigManager.Instance.Config;
                config.UI.WindowX = _windowRect.x;
                config.UI.WindowY = _windowRect.y;
                config.UI.WindowWidth = _windowRect.width;
                config.UI.WindowHeight = _windowRect.height;
                config.UI.LastTab = _currentTab;
                ConfigManager.Instance.QueueSave();
            }
            catch { }
        }
        
        public void Shutdown()
        {
            if (_isVisible) Hide();
            _pages.Clear();
            _initialized = false;
        }
        
        #endregion
    }
}
