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
        private static readonly List<ComponentEnabledState> _fallbackCameraComponents = new List<ComponentEnabledState>(8);

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
        private bool _commandPaletteVisible = false;
        private string _commandQuery = "";
        private int _commandSelectedIndex = 0;
        private string _selectedFeatureId = "";
        private int _pendingTabIndex = -1;
        private readonly List<IFeature> _commandResults = new List<IFeature>();

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
            MenuInputBlocker.Instance.Update();

            SewerLogger.Info("Menu opened");
        }

        public void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;

            // Re-enable player camera
            MenuInputBlocker.Instance.Release();
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
                _fallbackCameraComponents.Clear();

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
                                _fallbackCameraComponents.Add(new ComponentEnabledState(comp, comp.enabled));
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

                for (int i = 0; i < _fallbackCameraComponents.Count; i++)
                {
                    var state = _fallbackCameraComponents[i];
                    if (state.Component != null)
                    {
                        state.Component.enabled = state.WasEnabled;
                    }
                }

                _fallbackCameraComponents.Clear();
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

                MenuInputBlocker.Instance.Update();

                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.K))
                {
                    ToggleCommandPalette();
                    return;
                }

                if (_commandPaletteVisible && Input.GetKeyDown(KeyCode.Escape))
                {
                    _commandPaletteVisible = false;
                    return;
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
            SewerSkin.SetAnimationQuality(ConfigManager.Instance.Config?.UI?.AnimationQuality);
            ApplyDeferredGuiChanges();
            float uiDelta = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            float animStrength = SewerSkin.AnimationStrength;
            _openAnim = animStrength < 0.25f ? 1f : Mathf.MoveTowards(_openAnim, 1f, uiDelta * Mathf.Lerp(14f, 8f, animStrength));
            _tabAnim = animStrength < 0.25f ? 1f : Mathf.MoveTowards(_tabAnim, 1f, uiDelta * Mathf.Lerp(16f, 10f, animStrength));

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

            DrawCommandPalette(drawRect);
            ToastManager.Draw();
            MenuInputBlocker.Instance.ConsumeCurrentGuiEvent();

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

        private void ApplyDeferredGuiChanges()
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.Layout)
            {
                return;
            }

            if (_pendingTabIndex >= 0)
            {
                ApplyTabChange(_pendingTabIndex);
                _pendingTabIndex = -1;
            }
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
                _windowRect.width = Mathf.Clamp(Mathf.Round((_resizeStartSize.x + delta.x) / 8f) * 8f, MinWidth, MaxWidth);
                _windowRect.height = Mathf.Clamp(Mathf.Round((_resizeStartSize.y + delta.y) / 8f) * 8f, MinHeight, MaxHeight);
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

            var logoStyle = SewerSkin.GetLabelStyle(19, FontStyle.Bold, TextAnchor.MiddleCenter);
            var titleStyle = SewerSkin.GetLabelStyle(18, FontStyle.Bold);
            var subtitleStyle = SewerSkin.GetLabelStyle(10);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUI.Label(logoRect, "S", logoStyle);

            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(headerRect.x + 58, headerRect.y + 10, 74, 24), "SEWER", titleStyle);

            GUI.contentColor = SewerSkin.AccentGlow;
            GUI.Label(new Rect(headerRect.x + 128, headerRect.y + 10, 64, 24), "MENU", titleStyle);

            bool isBetaBuild = ModInfo.Version.IndexOf("beta", StringComparison.OrdinalIgnoreCase) >= 0;
            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(headerRect.x + 60, headerRect.y + 34, 250, 18), isBetaBuild ? "2.0 beta - Schedule I mod menu" : "2.0 - Schedule I mod menu", subtitleStyle);

            // Version badge on right
            string versionText = isBetaBuild ? "BETA" : "v" + ModInfo.Version;

            // Version badge background
            float badgeWidth = isBetaBuild ? 56 : 74;
            Rect badgeRect = new Rect(headerRect.x + headerRect.width - badgeWidth - 12, headerRect.y + 18, badgeWidth, 22);
            Rect searchRect = new Rect(badgeRect.x - 108f, headerRect.y + 15f, 96f, 28f);
            if (SewerSkin.DrawButtonRect(searchRect, "Search", SewerSkin.ButtonColor, SewerSkin.ButtonHoverColor, SewerSkin.TextColor, 6, false, 11))
            {
                OpenCommandPalette();
            }
            SewerSkin.DrawRoundedRect(badgeRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.16f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.38f), 5, 1);

            GUI.contentColor = SewerSkin.AccentGlow;
            GUI.Label(badgeRect, versionText, SewerSkin.GetLabelStyle(11, FontStyle.Bold, TextAnchor.MiddleCenter));
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
                var tabStyle = SewerSkin.GetLabelStyle(12, isSelected ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter);
                GUI.Label(tabRect, _tabNames[i], tabStyle);
                GUI.contentColor = oldContentColor;

                // Underline indicator for selected tab
                if (isSelected)
                {
                    float underlineWidth = tabWidth * 0.6f;
                    float underlineX = tabRect.x + (tabWidth - underlineWidth) / 2f;
                    SewerSkin.DrawRoundedRect(new Rect(underlineX, tabRect.y + tabHeight - 4, underlineWidth, 3), SewerSkin.AccentColor, Color.clear, 2, 0);
                }

                if (IsRectClicked(tabRect))
                {
                    RequestTabChange(i);
                }
            }

            // Bottom border line
            SewerSkin.DrawSolid(new Rect(tabBarRect.x, tabBarRect.y + tabHeight, tabBarRect.width, 1), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f));

            GUILayout.Space(8);
            DrawQuickAccessBar();
            GUILayout.Space(6);
            DrawActiveFeatureStrip();
            GUILayout.Space(8);

            // ═══════════════════════════════════════════════════════════
            // CONTENT AREA
            // ═══════════════════════════════════════════════════════════
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawCurrentPagePanel();
            GUILayout.Space(8);
            DrawInspectorPanel();
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // ═══════════════════════════════════════════════════════════
            // STATUS BAR - Clean modern style
            // ═══════════════════════════════════════════════════════════
            Rect statusRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));

            // Status bar background
            SewerSkin.DrawRoundedRect(statusRect, new Color(0.032f, 0.041f, 0.042f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.44f), 6, 1);

            // Active features indicator (left side)
            int activeCount = FeatureManager.Instance.EnabledCount;
            var statusStyle = SewerSkin.GetLabelStyle(11);

            // Green dot
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.SuccessColor;
            GUI.Label(new Rect(statusRect.x + 10, statusRect.y + 5, 14, 16), "●", statusStyle);

            // Active count
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(statusRect.x + 24, statusRect.y + 5, 80, 16), activeCount + " active", statusStyle);

            // Keyboard shortcuts (right side)
            GUI.contentColor = new Color(0.35f, 0.38f, 0.42f, 1f);
            var shortcutStyle = SewerSkin.GetLabelStyle(10, FontStyle.Normal, TextAnchor.MiddleRight);
            GUI.Label(new Rect(statusRect.x + statusRect.width - 140, statusRect.y + 6, 130, 14), "F8 toggle  ·  ESC close", shortcutStyle);
            GUI.contentColor = oldContentColor;
        }

        private void DrawCurrentPagePanel()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.055f, 0.07f, 0.085f, 1f);
            float scrollSpeed = Mathf.Lerp(28f, 16f, SewerSkin.AnimationStrength);
            float scrollLerp = 1f - Mathf.Exp(-Mathf.Min(Time.unscaledDeltaTime, 0.033f) * scrollSpeed);
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
                    var oldContentColor = GUI.contentColor;
                    float pageAlpha = Mathf.Lerp(0.68f, 1f, 1f - Mathf.Pow(1f - _tabAnim, 3f));
                    GUI.color = new Color(oldGuiColor.r, oldGuiColor.g, oldGuiColor.b, oldGuiColor.a * pageAlpha);
                    GUI.contentColor = new Color(oldContentColor.r, oldContentColor.g, oldContentColor.b, oldContentColor.a * pageAlpha);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space((1f - _tabAnim) * 10f);
                    GUILayout.BeginVertical();
                    page.Draw();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUI.color = oldGuiColor;
                    GUI.contentColor = oldContentColor;
                }
            }
            catch (System.Exception ex)
            {
                SewerSkin.DrawEmptyState("Tab error", ex.Message, SewerSkin.StatusType.Error);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawQuickAccessBar()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            SewerSkin.DrawRoundedRect(rect, new Color(0.035f, 0.047f, 0.043f, 0.88f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.42f), 7, 1);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 10f, 68f, 18f), "Favorites", SewerSkin.GetLabelStyle(10, FontStyle.Bold));
            GUI.contentColor = oldContentColor;

            float x = rect.x + 82f;
            var favorites = ConfigManager.Instance.Config?.UI?.FavoriteFeatureIds;
            bool drewAny = false;

            if (favorites != null)
            {
                for (int i = 0; i < favorites.Count; i++)
                {
                    var feature = FeatureManager.Instance.GetFeature(favorites[i]);
                    if (feature == null) continue;

                    float chipWidth = Mathf.Clamp(78f + (feature.Name?.Length ?? 0) * 4.2f, 94f, 150f);
                    if (x + chipWidth > rect.x + rect.width - 10f) break;

                    DrawFeatureChip(new Rect(x, rect.y + 7f, chipWidth, 26f), feature, false, true);
                    x += chipWidth + 6f;
                    drewAny = true;
                }
            }

            if (!drewAny)
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(x, rect.y + 10f, rect.width - (x - rect.x) - 10f, 18f), "Pin features from the inspector to build a quick bar.", SewerSkin.GetLabelStyle(10));
                GUI.contentColor = oldContentColor;
            }
        }

        private void DrawActiveFeatureStrip()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 34, GUILayout.ExpandWidth(true));
            SewerSkin.DrawRoundedRect(rect, new Color(0.028f, 0.037f, 0.036f, 0.86f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.34f), 7, 1);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, 64f, 18f), "Active", SewerSkin.GetLabelStyle(10, FontStyle.Bold));
            GUI.contentColor = oldContentColor;

            float x = rect.x + 72f;
            bool drewAny = false;
            foreach (var feature in FeatureManager.Instance.GetEnabledFeatures())
            {
                float chipWidth = Mathf.Clamp(72f + (feature.Name?.Length ?? 0) * 4.0f, 92f, 148f);
                if (x + chipWidth > rect.x + rect.width - 10f) break;

                DrawFeatureChip(new Rect(x, rect.y + 5f, chipWidth, 24f), feature, true, false);
                x += chipWidth + 6f;
                drewAny = true;
            }

            if (!drewAny)
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(x, rect.y + 8f, rect.width - (x - rect.x) - 10f, 18f), "No active features running.", SewerSkin.GetLabelStyle(10));
                GUI.contentColor = oldContentColor;
            }
        }

        private void DrawFeatureChip(Rect rect, IFeature feature, bool canDisable, bool activateOnClick)
        {
            bool selected = _selectedFeatureId == feature.Id;
            bool hovered = rect.Contains(Event.current.mousePosition);
            Color fill = feature.IsEnabled
                ? new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, selected ? 0.28f : 0.18f)
                : new Color(0.065f, 0.082f, 0.076f, hovered ? 0.96f : 0.84f);
            Color border = selected
                ? new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.75f)
                : new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, hovered ? 0.62f : 0.38f);

            SewerSkin.DrawRoundedRect(rect, fill, border, 7, 1);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = feature.IsEnabled ? SewerSkin.AccentGlow : SewerSkin.TextColor;
            float closeWidth = canDisable && feature.IsToggleable ? 20f : 0f;
            GUI.Label(new Rect(rect.x + 9f, rect.y + 3f, rect.width - 16f - closeWidth, rect.height - 6f), feature.Name, SewerSkin.GetLabelStyle(10, feature.IsEnabled ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleLeft));
            GUI.contentColor = oldContentColor;

            Rect clickRect = canDisable && feature.IsToggleable
                ? new Rect(rect.x, rect.y, rect.width - 22f, rect.height)
                : rect;

            if (IsRectClicked(clickRect))
            {
                SelectFeature(feature);
                if (activateOnClick)
                {
                    ActivateFeature(feature);
                }
            }

            if (canDisable && feature.IsToggleable)
            {
                Rect offRect = new Rect(rect.x + rect.width - 21f, rect.y + 2f, 18f, rect.height - 4f);
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(offRect, "x", SewerSkin.GetLabelStyle(10, FontStyle.Bold, TextAnchor.MiddleCenter));
                GUI.contentColor = oldContentColor;
                if (IsRectClicked(offRect))
                {
                    feature.IsEnabled = false;
                    SelectFeature(feature);
                }
            }
        }

        private void DrawInspectorPanel()
        {
            const float inspectorWidth = 252f;
            Rect panelRect = GUILayoutUtility.GetRect(inspectorWidth, 0, GUILayout.Width(inspectorWidth), GUILayout.ExpandHeight(true));
            SewerSkin.DrawRoundedRect(panelRect, new Color(0.034f, 0.045f, 0.042f, 0.95f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.48f), 7, 1);

            Rect inner = new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, panelRect.height - 20f);
            GUILayout.BeginArea(inner);

            IFeature feature = GetSelectedFeature();
            if (feature == null)
            {
                GUI.contentColor = SewerSkin.AccentGlow;
                GUILayout.Label("INSPECTOR", SewerSkin.GetLabelStyle(12, FontStyle.Bold));
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Select a favorite, active feature, or command result to inspect it.", SewerSkin.GetLabelStyle(10));
                GUI.contentColor = Color.white;
                GUILayout.Space(8);
                if (SewerSkin.DrawButton("Open Command Palette"))
                {
                    OpenCommandPalette();
                }
                GUILayout.EndArea();
                return;
            }

            GUI.contentColor = SewerSkin.AccentGlow;
            GUILayout.Label(feature.Name.ToUpper(), SewerSkin.GetLabelStyle(13, FontStyle.Bold));
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label(feature.Category.ToString(), SewerSkin.GetLabelStyle(10));
            GUI.contentColor = Color.white;
            GUILayout.Space(8);

            DrawInspectorStatus(feature);
            GUILayout.Space(8);

            GUI.contentColor = SewerSkin.TextColor;
            GUILayout.Label(feature.Description ?? "No description available.", SewerSkin.GetLabelStyle(10));
            GUI.contentColor = Color.white;
            GUILayout.Space(10);

            if (feature.IsToggleable)
            {
                if (feature.IsEnabled)
                {
                    if (SewerSkin.DrawDangerButton("Disable"))
                    {
                        feature.IsEnabled = false;
                    }
                }
                else if (SewerSkin.DrawAccentButton("Enable"))
                {
                    feature.IsEnabled = true;
                }
            }
            else
            {
                string action = feature.Id == "itemspawner" ? "Open Spawner" : "Run Action";
                if (SewerSkin.DrawAccentButton(action))
                {
                    ActivateFeature(feature);
                }
            }

            if (SewerSkin.DrawButton("Jump To Tab"))
            {
                JumpToFeature(feature);
            }

            bool favorite = IsFavorite(feature.Id);
            if (SewerSkin.DrawButton(favorite ? "Unpin Favorite" : "Pin Favorite"))
            {
                ToggleFavorite(feature.Id);
            }

            GUILayout.Space(8);
            DrawInfoLine("Hotkey", feature.Hotkey.HasValue ? feature.Hotkey.Value.ToString() : "None");
            DrawInfoLine("Host", feature.RequiresHost ? "Required" : "Not required");

            GUILayout.EndArea();
        }

        private void DrawInspectorStatus(IFeature feature)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            Color accent = feature.IsEnabled ? SewerSkin.SuccessColor : SewerSkin.TextMutedColor;
            string status = feature.IsEnabled ? "ACTIVE" : (feature.IsToggleable ? "READY" : "ACTION");
            SewerSkin.DrawRoundedRect(rect, new Color(accent.r, accent.g, accent.b, feature.IsEnabled ? 0.18f : 0.08f), new Color(accent.r, accent.g, accent.b, 0.42f), 7, 1);
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = feature.IsEnabled ? SewerSkin.SuccessColor : SewerSkin.TextMutedColor;
            GUI.Label(rect, status, SewerSkin.GetLabelStyle(11, FontStyle.Bold, TextAnchor.MiddleCenter));
            GUI.contentColor = oldContentColor;
        }

        private void DrawInfoLine(string label, string value)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(rect.x, rect.y, 70f, rect.height), label, SewerSkin.GetLabelStyle(10));
            GUI.contentColor = SewerSkin.TextColor;
            GUI.Label(new Rect(rect.x + 72f, rect.y, rect.width - 72f, rect.height), value, SewerSkin.GetLabelStyle(10, FontStyle.Bold, TextAnchor.MiddleRight));
            GUI.contentColor = oldContentColor;
        }

        #endregion

        #region Helpers

        private void DrawCommandPalette(Rect ownerRect)
        {
            if (!_commandPaletteVisible) return;

            HandleCommandPaletteEvents();
            BuildCommandResults();

            Rect overlay = new Rect(0f, 0f, Screen.width, Screen.height);
            SewerSkin.DrawSolid(overlay, new Color(0f, 0f, 0f, 0.34f));

            float width = Mathf.Min(560f, Screen.width - 80f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, Mathf.Max(70f, ownerRect.y + 48f), width, 384f);
            SewerSkin.DrawWindowPanel(panel);

            Rect inner = new Rect(panel.x + 14f, panel.y + 14f, panel.width - 28f, panel.height - 28f);
            GUILayout.BeginArea(inner);

            GUI.contentColor = SewerSkin.AccentGlow;
            GUILayout.Label("COMMAND PALETTE", SewerSkin.GetLabelStyle(12, FontStyle.Bold));
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Type a feature name, then press Enter or click a result.", SewerSkin.GetLabelStyle(10));
            GUILayout.Space(8);

            Rect queryRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            SewerSkin.DrawRoundedRect(queryRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.46f), 7, 1);
            GUI.contentColor = string.IsNullOrEmpty(_commandQuery) ? SewerSkin.TextMutedColor : SewerSkin.TextColor;
            GUI.Label(new Rect(queryRect.x + 12f, queryRect.y + 8f, queryRect.width - 24f, 20f), string.IsNullOrEmpty(_commandQuery) ? "Search features..." : _commandQuery, SewerSkin.GetLabelStyle(13, FontStyle.Bold));
            GUI.contentColor = Color.white;
            GUILayout.Space(8);

            if (_commandResults.Count == 0)
            {
                SewerSkin.DrawEmptyState("No matches", "Try another feature name or category.", SewerSkin.StatusType.Warning);
            }
            else
            {
                int maxResults = Mathf.Min(8, _commandResults.Count);
                for (int i = 0; i < maxResults; i++)
                {
                    DrawCommandResult(_commandResults[i], i);
                    GUILayout.Space(4);
                }
            }

            GUILayout.FlexibleSpace();
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Ctrl+K open  |  Enter run  |  Esc close", SewerSkin.GetLabelStyle(10, FontStyle.Normal, TextAnchor.MiddleCenter));
            GUI.contentColor = Color.white;

            GUILayout.EndArea();
        }

        private void DrawCommandResult(IFeature feature, int index)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 38, GUILayout.ExpandWidth(true));
            bool selected = index == _commandSelectedIndex;
            bool hovered = rect.Contains(Event.current.mousePosition);
            Color fill = selected
                ? new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.18f)
                : new Color(0.055f, 0.069f, 0.064f, hovered ? 0.94f : 0.72f);
            Color border = selected
                ? new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.62f)
                : new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, hovered ? 0.52f : 0.28f);

            SewerSkin.DrawRoundedRect(rect, fill, border, 7, 1);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = selected ? SewerSkin.AccentGlow : SewerSkin.TextColor;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 120f, 18f), feature.Name, SewerSkin.GetLabelStyle(11, FontStyle.Bold));
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 20f, rect.width - 120f, 14f), feature.Category + " - " + (feature.IsToggleable ? (feature.IsEnabled ? "enabled" : "off") : "action"), SewerSkin.GetLabelStyle(9));
            GUI.contentColor = feature.IsEnabled ? SewerSkin.SuccessColor : SewerSkin.TextMutedColor;
            GUI.Label(new Rect(rect.x + rect.width - 102f, rect.y + 10f, 92f, 18f), feature.IsToggleable ? (feature.IsEnabled ? "ACTIVE" : "TOGGLE") : "OPEN", SewerSkin.GetLabelStyle(10, FontStyle.Bold, TextAnchor.MiddleRight));
            GUI.contentColor = oldContentColor;

            if (IsRectClicked(rect))
            {
                _commandSelectedIndex = index;
                ActivateFeature(feature);
                _commandPaletteVisible = false;
            }
        }

        private void HandleCommandPaletteEvents()
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.KeyDown) return;

            if (e.keyCode == KeyCode.Escape)
            {
                _commandPaletteVisible = false;
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (_commandResults.Count > 0)
                {
                    int index = Mathf.Clamp(_commandSelectedIndex, 0, _commandResults.Count - 1);
                    ActivateFeature(_commandResults[index]);
                    _commandPaletteVisible = false;
                }
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.DownArrow)
            {
                _commandSelectedIndex = Mathf.Min(_commandSelectedIndex + 1, Mathf.Max(0, _commandResults.Count - 1));
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.UpArrow)
            {
                _commandSelectedIndex = Mathf.Max(_commandSelectedIndex - 1, 0);
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Backspace)
            {
                if (_commandQuery.Length > 0)
                {
                    _commandQuery = _commandQuery.Substring(0, _commandQuery.Length - 1);
                    _commandSelectedIndex = 0;
                }
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Delete)
            {
                _commandQuery = "";
                _commandSelectedIndex = 0;
                e.Use();
                return;
            }

            char c = e.character;
            if (!char.IsControl(c) && _commandQuery.Length < 40)
            {
                _commandQuery += c;
                _commandSelectedIndex = 0;
                e.Use();
            }
        }

        private void BuildCommandResults()
        {
            _commandResults.Clear();
            string query = (_commandQuery ?? "").Trim().ToLowerInvariant();

            foreach (var feature in FeatureManager.Instance.AllFeatures)
            {
                if (string.IsNullOrEmpty(query) || MatchesCommand(feature, query))
                {
                    _commandResults.Add(feature);
                }
            }

            if (_commandSelectedIndex >= _commandResults.Count)
            {
                _commandSelectedIndex = Mathf.Max(0, _commandResults.Count - 1);
            }
        }

        private bool MatchesCommand(IFeature feature, string query)
        {
            if (feature == null) return false;
            if ((feature.Name ?? "").ToLowerInvariant().Contains(query)) return true;
            if ((feature.Id ?? "").ToLowerInvariant().Contains(query)) return true;
            if (feature.Category.ToString().ToLowerInvariant().Contains(query)) return true;
            if ((feature.Description ?? "").ToLowerInvariant().Contains(query)) return true;
            return false;
        }

        private void OpenCommandPalette()
        {
            _commandPaletteVisible = true;
            _commandQuery = "";
            _commandSelectedIndex = 0;
            BuildCommandResults();
        }

        private void ToggleCommandPalette()
        {
            if (_commandPaletteVisible)
            {
                _commandPaletteVisible = false;
            }
            else
            {
                OpenCommandPalette();
            }
        }

        private void ActivateFeature(IFeature feature)
        {
            if (feature == null) return;

            SelectFeature(feature);

            if (feature.Id == "itemspawner")
            {
                ItemSpawnerWindow.Instance.Show();
                ToastManager.Show("Item Spawner opened", SewerSkin.StatusType.Normal);
                return;
            }

            if (feature.IsToggleable)
            {
                feature.Toggle();
            }
            else
            {
                feature.Execute();
                ToastManager.Show(feature.Name + " executed", SewerSkin.StatusType.Success);
            }
        }

        private void SelectFeature(IFeature feature)
        {
            if (feature == null) return;
            _selectedFeatureId = feature.Id;
        }

        public void InspectFeature(IFeature feature)
        {
            SelectFeature(feature);
        }

        private IFeature GetSelectedFeature()
        {
            if (!string.IsNullOrEmpty(_selectedFeatureId))
            {
                var selected = FeatureManager.Instance.GetFeature(_selectedFeatureId);
                if (selected != null) return selected;
            }

            var favorites = ConfigManager.Instance.Config?.UI?.FavoriteFeatureIds;
            if (favorites != null)
            {
                for (int i = 0; i < favorites.Count; i++)
                {
                    var feature = FeatureManager.Instance.GetFeature(favorites[i]);
                    if (feature != null) return feature;
                }
            }

            foreach (var feature in FeatureManager.Instance.AllFeatures)
            {
                if (feature != null) return feature;
            }

            return null;
        }

        private bool IsFavorite(string featureId)
        {
            var favorites = ConfigManager.Instance.Config?.UI?.FavoriteFeatureIds;
            return favorites != null && favorites.Contains(featureId);
        }

        private void ToggleFavorite(string featureId)
        {
            var config = ConfigManager.Instance.Config;
            if (config?.UI == null || string.IsNullOrEmpty(featureId)) return;
            config.UI.FavoriteFeatureIds ??= new List<string>();

            if (config.UI.FavoriteFeatureIds.Contains(featureId))
            {
                config.UI.FavoriteFeatureIds.Remove(featureId);
                ToastManager.Show("Favorite removed", SewerSkin.StatusType.Normal);
            }
            else
            {
                config.UI.FavoriteFeatureIds.Add(featureId);
                ToastManager.Show("Favorite pinned", SewerSkin.StatusType.Success);
            }

            ConfigManager.Instance.QueueSave();
        }

        private void JumpToFeature(IFeature feature)
        {
            if (feature == null) return;

            for (int i = 0; i < _tabCategories.Length; i++)
            {
                if (_tabCategories[i] == feature.Category)
                {
                    RequestTabChange(i);
                    return;
                }
            }
        }

        private void RequestTabChange(int tabIndex)
        {
            tabIndex = Mathf.Clamp(tabIndex, 0, _tabNames.Length - 1);
            Event e = Event.current;
            if (e != null && e.type != EventType.Layout)
            {
                _pendingTabIndex = tabIndex;
                return;
            }

            ApplyTabChange(tabIndex);
        }

        private void ApplyTabChange(int tabIndex)
        {
            tabIndex = Mathf.Clamp(tabIndex, 0, _tabNames.Length - 1);
            if (_currentTab == tabIndex) return;

            _currentTab = tabIndex;
            _tabAnim = 0f;
            _scrollPosition = Vector2.zero;
            _targetScrollPosition = Vector2.zero;
            _displayScrollPosition = Vector2.zero;
            SewerSkin.ResetTransientAnimations();
            SaveWindowState();
        }

        private static bool IsRectClicked(Rect rect)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(e.mousePosition))
            {
                return false;
            }

            e.Use();
            return true;
        }

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

        private readonly struct ComponentEnabledState
        {
            public readonly MonoBehaviour Component;
            public readonly bool WasEnabled;

            public ComponentEnabledState(MonoBehaviour component, bool wasEnabled)
            {
                Component = component;
                WasEnabled = wasEnabled;
            }
        }
    }
}
