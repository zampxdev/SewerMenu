using System;
using System.Collections.Generic;
using UnityEngine;
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
        
        // Cache for player camera component
        private static PlayerCamera _playerCamera;
        private static bool _cameraWasEnabled;
        
        #endregion
        
        #region Fields
        
        private bool _initialized = false;
        private bool _isVisible = false;
        private int _currentTab = 0;
        private Vector2 _scrollPosition;
        private Vector2 _targetScrollPosition;
        private Vector2 _displayScrollPosition;
        private Rect _windowRect;
        private float _openAnim = 1f;
        private float _tabAnim = 1f;
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        
        // Resize state
        private bool _isResizing = false;
        private Vector2 _resizeStartMousePos;
        private Vector2 _resizeStartSize;
        
        // Size constraints
        private const float MinWidth = 760f;
        private const float MaxWidth = 1400f;
        private const float MinHeight = 560f;
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
            _openAnim = 0f;
            _tabAnim = 0f;
            _targetScrollPosition = _scrollPosition;
            _displayScrollPosition = _scrollPosition;
            SewerSkin.ResetTransientAnimations();
            
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
            _openAnim = Mathf.MoveTowards(_openAnim, 1f, Time.unscaledDeltaTime * 8f);
            _tabAnim = Mathf.MoveTowards(_tabAnim, 1f, Time.unscaledDeltaTime * 10f);
            
            // Handle resize BEFORE drawing window
            HandleResize();
            
            // Clamp window to screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
            _windowRect.width = Mathf.Clamp(_windowRect.width, MinWidth, MaxWidth);
            _windowRect.height = Mathf.Clamp(_windowRect.height, MinHeight, MaxHeight);
            
            Rect drawRect = GetAnimatedWindowRect();
            HandleWindowDrag(drawRect);
            HandleSmoothScrollInput(drawRect);
            drawRect = GetAnimatedWindowRect();

            var oldColor = GUI.color;
            GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, Mathf.Lerp(0.78f, 1f, _openAnim));
            SewerSkin.DrawWindowPanel(drawRect);

            Rect contentRect = new Rect(drawRect.x + 10f, drawRect.y + 8f, drawRect.width - 20f, drawRect.height - 16f);
            GUILayout.BeginArea(contentRect);
            try
            {
                DrawWindowContent();
            }
            catch (Exception ex)
            {
                SewerSkin.DrawStatus("Error: " + ex.Message, SewerSkin.StatusType.Error);
            }
            GUILayout.EndArea();
            GUI.color = oldColor;
            
            // Draw resize grip on top of window
            DrawResizeGrip();
            
            // Draw popup windows (ItemSpawner, etc.)
            ItemSpawnerWindow.Instance.OnGUI();
            
            SewerSkin.EndUI();
        }
        
        private bool _isHoveringResize = false;

        private Rect GetAnimatedWindowRect()
        {
            return new Rect(
                _windowRect.x,
                _windowRect.y - (1f - _openAnim) * 14f,
                _windowRect.width,
                _windowRect.height
            );
        }

        private void HandleSmoothScrollInput(Rect drawRect)
        {
            Event e = Event.current;
            if (e.type != EventType.ScrollWheel || !drawRect.Contains(e.mousePosition)) return;

            _targetScrollPosition.y = Mathf.Max(0f, _targetScrollPosition.y + e.delta.y * 34f);
            e.Use();
        }
        
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

        private void HandleWindowDrag(Rect drawRect)
        {
            if (_isResizing) return;

            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;
            Rect dragRect = new Rect(drawRect.x, drawRect.y, drawRect.width, 58f);

            if (e.type == EventType.MouseDown && e.button == 0 && dragRect.Contains(mousePos))
            {
                _isDragging = true;
                _dragOffset = new Vector2(mousePos.x - _windowRect.x, mousePos.y - _windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                _windowRect.x = mousePos.x - _dragOffset.x;
                _windowRect.y = mousePos.y - _dragOffset.y;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _isDragging)
            {
                _isDragging = false;
                SaveWindowState();
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
            
            var oldColor = GUI.color;
            
            // Draw a subtle background
            GUI.color = new Color(0.03f, 0.04f, 0.04f, 0.5f);
            GUI.DrawTexture(gripRect, SewerSkin.WhiteTexture);
            
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
                    GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), SewerSkin.WhiteTexture);
                }
            }
            
            // Draw corner accent line
            if (_isHoveringResize || _isResizing)
            {
                GUI.color = SewerSkin.AccentColor;
                // Bottom edge highlight
                GUI.DrawTexture(new Rect(gripRect.x, gripRect.y + gripRect.height - 2f, gripRect.width, 2f), SewerSkin.WhiteTexture);
                // Right edge highlight
                GUI.DrawTexture(new Rect(gripRect.x + gripRect.width - 2f, gripRect.y, 2f, gripRect.height), SewerSkin.WhiteTexture);
            }
            
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
            Rect headerRect = GUILayoutUtility.GetRect(0, 58, GUILayout.ExpandWidth(true));
            
            // Header background with subtle depth.
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.035f, 0.046f, 0.041f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.58f), 6, 1);
            SewerSkin.DrawRoundedRect(new Rect(headerRect.x + 1f, headerRect.y + 1f, headerRect.width - 2f, 12f), new Color(1f, 1f, 1f, 0.045f), Color.clear, 5, 0);
            
            Rect logoRect = new Rect(headerRect.x + 12, headerRect.y + 10, 34, 34);
            SewerSkin.DrawSoftGlow(logoRect, SewerSkin.AccentColor, 0.2f, 7);
            SewerSkin.DrawRoundedRect(logoRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.13f), SewerSkin.AccentColor, 6, 1);

            var logoStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };
            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            var subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, clipping = TextClipping.Clip };
            
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUI.Label(logoRect, "S", logoStyle);

            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(headerRect.x + 58, headerRect.y + 10, 74, 24), "SEWER", titleStyle);
            
            GUI.contentColor = SewerSkin.AccentGlow;
            GUI.Label(new Rect(headerRect.x + 128, headerRect.y + 10, 64, 24), "MENU", titleStyle);

            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(headerRect.x + 60, headerRect.y + 34, 250, 18), "2.0 UI beta - Schedule I mod menu", subtitleStyle);
            
            // Version badge on right
            string versionText = "BETA";
            
            // Version badge background
            float badgeWidth = 56;
            Rect badgeRect = new Rect(headerRect.x + headerRect.width - badgeWidth - 12, headerRect.y + 18, badgeWidth, 22);
            SewerSkin.DrawRoundedRect(badgeRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.16f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.38f), 5, 1);
            
            GUI.contentColor = SewerSkin.AccentGlow;
            GUI.Label(badgeRect, versionText, new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUI.contentColor = oldContentColor;
            
            // Bottom border
            SewerSkin.DrawSolid(new Rect(headerRect.x + 12f, headerRect.y + headerRect.height - 1, headerRect.width - 24f, 1), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.48f));
            
            // ═══════════════════════════════════════════════════════════
            // TAB BAR - Modern style with underline indicator
            // ═══════════════════════════════════════════════════════════
            GUILayout.Space(4);
            
            // Tab bar background
            Rect tabBarRect = GUILayoutUtility.GetRect(0, 38, GUILayout.ExpandWidth(true));
            SewerSkin.DrawRoundedRect(tabBarRect, new Color(0.032f, 0.042f, 0.039f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.45f), 6, 1);
            SewerSkin.DrawRoundedRect(new Rect(tabBarRect.x + 1f, tabBarRect.y + 1f, tabBarRect.width - 2f, 8f), new Color(1f, 1f, 1f, 0.035f), Color.clear, 5, 0);
            
            // Calculate tab widths
            float tabWidth = tabBarRect.width / _tabNames.Length;
            float tabHeight = 36f;
            
            // Draw tabs
            for (int i = 0; i < _tabNames.Length; i++)
            {
                bool isSelected = (_currentTab == i);
                Rect tabRect = new Rect(tabBarRect.x + i * tabWidth, tabBarRect.y, tabWidth, tabHeight);
                
                // Hover detection
                bool isHovered = tabRect.Contains(Event.current.mousePosition);
                
                Rect visualRect = new Rect(tabRect.x + 4f, tabRect.y + 4f, tabRect.width - 8f, tabRect.height - 8f);

                if (isSelected)
                {
                    SewerSkin.DrawSoftGlow(visualRect, SewerSkin.AccentColor, 0.12f, 6);
                    SewerSkin.DrawRoundedRect(visualRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.13f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.42f), 6, 1);
                }
                else if (isHovered)
                {
                    SewerSkin.DrawRoundedRect(visualRect, new Color(0.085f, 0.105f, 0.096f, 0.96f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.45f), 6, 1);
                }

                if (i > 0)
                {
                    SewerSkin.DrawSolid(new Rect(tabRect.x, tabRect.y + 9f, 1f, tabRect.height - 18f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.34f));
                }
                
                // Tab text
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? SewerSkin.TextColor : (isHovered ? SewerSkin.TextColor : SewerSkin.TextMutedColor);
                var tabStyle = new GUIStyle(GUI.skin.label) 
                { 
                    alignment = TextAnchor.MiddleCenter, 
                    fontSize = 12,
                    fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal,
                    clipping = TextClipping.Clip
                };
                GUI.Label(tabRect, _tabNames[i], tabStyle);
                GUI.contentColor = oldContentColor;
                
                // Underline indicator for selected tab
                if (isSelected)
                {
                    float underlineWidth = tabWidth * 0.6f;
                    float underlineX = tabRect.x + (tabWidth - underlineWidth) / 2f;
                    SewerSkin.DrawRoundedRect(new Rect(underlineX, tabRect.y + tabHeight - 4, underlineWidth, 3), SewerSkin.AccentColor, Color.clear, 2, 0);
                }
                
                // Invisible button for click detection
                if (GUI.Button(tabRect, "", GUIStyle.none))
                {
                    if (_currentTab != i)
                    {
                        _currentTab = i;
                        _tabAnim = 0f;
                        _scrollPosition = Vector2.zero;
                        _targetScrollPosition = Vector2.zero;
                        _displayScrollPosition = Vector2.zero;
                        SewerSkin.ResetTransientAnimations();
                    }
                }
            }
            
            // Bottom border line
            SewerSkin.DrawSolid(new Rect(tabBarRect.x, tabBarRect.y + tabHeight, tabBarRect.width, 1), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f));
            
            GUILayout.Space(8);
            
            // ═══════════════════════════════════════════════════════════
            // CONTENT AREA
            // ═══════════════════════════════════════════════════════════
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.055f, 0.07f, 0.085f, 1f);
            float scrollLerp = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f);
            _displayScrollPosition = Vector2.Lerp(_displayScrollPosition, _targetScrollPosition, scrollLerp);
            _scrollPosition = GUILayout.BeginScrollView(_displayScrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = oldBg;

            if (_scrollPosition != _displayScrollPosition)
            {
                _targetScrollPosition = _scrollPosition;
                _displayScrollPosition = _scrollPosition;
            }

            if (_tabAnim < 1f)
            {
                GUILayout.Space((1f - _tabAnim) * 12f);
            }
            
            try
            {
                var category = _tabCategories[_currentTab];
                if (_pages.TryGetValue(category, out var page))
                {
                    var oldGuiColor = GUI.color;
                    var oldContentColor2 = GUI.contentColor;
                    float pageAlpha = Mathf.Lerp(0.68f, 1f, 1f - Mathf.Pow(1f - _tabAnim, 3f));
                    GUI.color = new Color(oldGuiColor.r, oldGuiColor.g, oldGuiColor.b, oldGuiColor.a * pageAlpha);
                    GUI.contentColor = new Color(oldContentColor2.r, oldContentColor2.g, oldContentColor2.b, oldContentColor2.a * pageAlpha);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space((1f - _tabAnim) * 10f);
                    GUILayout.BeginVertical();
                    page.Draw();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUI.color = oldGuiColor;
                    GUI.contentColor = oldContentColor2;
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
            SewerSkin.DrawRoundedRect(statusRect, new Color(0.032f, 0.041f, 0.042f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.44f), 6, 1);
            
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
