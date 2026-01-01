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
            // HEADER - Clean modern style
            // ═══════════════════════════════════════════════════════════
            Rect headerRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            
            // Header background with subtle gradient feel
            var oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.07f, 1f);
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Title: "SEWER" in accent, "MENU" in white
            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUI.Label(new Rect(headerRect.x + 12, headerRect.y + 6, 60, 20), "SEWER", titleStyle);
            
            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(headerRect.x + 70, headerRect.y + 6, 50, 20), "MENU", titleStyle);
            
            // Version badge on right
            string versionText = "v" + ModInfo.Version;
            var versionStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            
            // Version badge background
            float badgeWidth = 42;
            Rect badgeRect = new Rect(headerRect.x + headerRect.width - badgeWidth - 10, headerRect.y + 8, badgeWidth, 16);
            GUI.color = new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.15f);
            GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            GUI.contentColor = SewerSkin.AccentGlow;
            GUI.Label(badgeRect, versionText, new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter });
            GUI.contentColor = oldContentColor;
            
            // Bottom border
            GUI.color = new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.4f);
            GUI.DrawTexture(new Rect(headerRect.x, headerRect.y + headerRect.height - 1, headerRect.width, 1), Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // ═══════════════════════════════════════════════════════════
            // TAB BAR - Modern style with underline indicator
            // ═══════════════════════════════════════════════════════════
            GUILayout.Space(4);
            
            // Tab bar background
            Rect tabBarRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(tabBarRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Calculate tab widths
            float tabWidth = tabBarRect.width / _tabNames.Length;
            float tabHeight = 34f;
            
            // Draw tabs
            for (int i = 0; i < _tabNames.Length; i++)
            {
                bool isSelected = (_currentTab == i);
                Rect tabRect = new Rect(tabBarRect.x + i * tabWidth, tabBarRect.y, tabWidth, tabHeight);
                
                // Hover detection
                bool isHovered = tabRect.Contains(Event.current.mousePosition);
                
                // Tab background on hover (subtle)
                if (isHovered && !isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = new Color(0.1f, 0.12f, 0.15f, 1f);
                    GUI.DrawTexture(new Rect(tabRect.x + 2, tabRect.y + 2, tabRect.width - 4, tabRect.height - 4), Texture2D.whiteTexture);
                    GUI.color = oldColor;
                }
                
                // Tab text
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? SewerSkin.TextColor : (isHovered ? SewerSkin.TextColor : SewerSkin.TextMutedColor);
                var tabStyle = new GUIStyle(GUI.skin.label) 
                { 
                    alignment = TextAnchor.MiddleCenter, 
                    fontSize = 12,
                    fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal
                };
                GUI.Label(tabRect, _tabNames[i], tabStyle);
                GUI.contentColor = oldContentColor;
                
                // Underline indicator for selected tab
                if (isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = SewerSkin.AccentColor;
                    float underlineWidth = tabWidth * 0.6f;
                    float underlineX = tabRect.x + (tabWidth - underlineWidth) / 2f;
                    GUI.DrawTexture(new Rect(underlineX, tabRect.y + tabHeight - 3, underlineWidth, 3), Texture2D.whiteTexture);
                    GUI.color = oldColor;
                }
                
                // Invisible button for click detection
                if (GUI.Button(tabRect, "", GUIStyle.none))
                {
                    if (_currentTab != i)
                    {
                        _currentTab = i;
                        _scrollPosition = Vector2.zero;
                    }
                }
            }
            
            // Bottom border line
            oldColor = GUI.color;
            GUI.color = new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f);
            GUI.DrawTexture(new Rect(tabBarRect.x, tabBarRect.y + tabHeight, tabBarRect.width, 1), Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            GUILayout.Space(8);
            
            // ═══════════════════════════════════════════════════════════
            // CONTENT AREA
            // ═══════════════════════════════════════════════════════════
            var oldBg = GUI.backgroundColor;
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
            
            GUILayout.Space(2);
            
            // ═══════════════════════════════════════════════════════════
            // STATUS BAR - Clean modern style
            // ═══════════════════════════════════════════════════════════
            Rect statusRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            
            // Status bar background
            oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.065f, 1f);
            GUI.DrawTexture(statusRect, Texture2D.whiteTexture);
            
            // Top border
            GUI.color = new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.3f);
            GUI.DrawTexture(new Rect(statusRect.x, statusRect.y, statusRect.width, 1), Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Active features indicator (left side)
            int activeCount = FeatureManager.Instance.EnabledCount;
            var statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            
            // Green dot
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.SuccessColor;
            GUI.Label(new Rect(statusRect.x + 10, statusRect.y + 5, 14, 16), "●", statusStyle);
            
            // Active count
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(statusRect.x + 24, statusRect.y + 5, 80, 16), activeCount + " active", statusStyle);
            
            // Keyboard shortcuts (right side)
            GUI.contentColor = new Color(0.35f, 0.38f, 0.42f, 1f);
            var shortcutStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(statusRect.x + statusRect.width - 140, statusRect.y + 6, 130, 14), "F8 toggle  ·  ESC close", shortcutStyle);
            GUI.contentColor = oldContentColor;
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
