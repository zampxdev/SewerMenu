using UnityEngine;
using SewerMenu.Core.Logging;

namespace SewerMenu.UI
{
    /// <summary>
    /// Modern UI styling system for SewerMenu.
    /// Handles IL2CPP-safe rendering with custom textures and colors.
    /// </summary>
    public static class SewerSkin
    {
        #region Static Cache (Prevents GC)
        
        private static bool _initialized = false;
        private static GUISkin _customSkin;
        
        // Textures - stored statically to prevent GC
        private static Texture2D _windowBackground;
        private static Texture2D _buttonNormal;
        private static Texture2D _buttonHover;
        private static Texture2D _buttonActive;
        private static Texture2D _boxBackground;
        private static Texture2D _toggleOn;
        private static Texture2D _toggleOff;
        private static Texture2D _sliderBackground;
        private static Texture2D _sliderThumb;
        private static Texture2D _textFieldBackground;
        private static Texture2D _solidWhite;
        private static Texture2D _gradientHeader;
        
        // Colors - Modern "Midnight" theme inspired by GitHub Dark
        public static readonly Color AccentColor = new Color(0.345f, 0.651f, 1.0f, 1f);      // #58A6FF - Bright blue
        public static readonly Color AccentDark = new Color(0.22f, 0.545f, 0.992f, 1f);      // #388BFD - Darker blue
        public static readonly Color AccentGlow = new Color(0.475f, 0.753f, 1.0f, 1f);       // #79C0FF - Light blue glow
        public static readonly Color BackgroundColor = new Color(0.051f, 0.067f, 0.09f, 0.98f);  // #0D1117 - Very dark
        public static readonly Color PanelColor = new Color(0.086f, 0.106f, 0.133f, 1f);     // #161B22 - Panel bg
        public static readonly Color PanelLightColor = new Color(0.11f, 0.129f, 0.157f, 1f); // #1C2128 - Lighter panel
        public static readonly Color ButtonColor = new Color(0.129f, 0.157f, 0.188f, 1f);    // #212830 - Button normal
        public static readonly Color ButtonHoverColor = new Color(0.18f, 0.212f, 0.251f, 1f);// #2E3640 - Button hover
        public static readonly Color ButtonActiveColor = new Color(0.22f, 0.545f, 0.992f, 1f);// #388BFD - Button active
        public static readonly Color TextColor = new Color(0.902f, 0.929f, 0.953f, 1f);      // #E6EDF3 - Off-white
        public static readonly Color TextMutedColor = new Color(0.49f, 0.522f, 0.565f, 1f);  // #7D8590 - Gray
        public static readonly Color SuccessColor = new Color(0.247f, 0.725f, 0.314f, 1f);   // #3FB950 - Green
        public static readonly Color WarningColor = new Color(0.824f, 0.6f, 0.133f, 1f);     // #D29922 - Amber
        public static readonly Color ErrorColor = new Color(0.973f, 0.318f, 0.286f, 1f);     // #F85149 - Red
        public static readonly Color BorderColor = new Color(0.188f, 0.212f, 0.239f, 1f);    // #30363D - Subtle border
        public static readonly Color BorderLightColor = new Color(0.282f, 0.31f, 0.345f, 1f);// #484F58 - Light border
        
        // Cached GUIStyles
        private static GUIStyle _headerStyle;
        private static GUIStyle _sectionStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _boxStyle;
        
        #endregion
        
        #region Initialization
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            try
            {
                CreateTextures();
                _initialized = true;
                SewerLogger.Debug("SewerSkin initialized");
            }
            catch (System.Exception ex)
            {
                SewerLogger.Error("Failed to initialize SewerSkin", ex);
            }
        }
        
        private static void CreateTextures()
        {
            // Solid white for drawing colored rectangles
            _solidWhite = MakeTexture(2, 2, Color.white);
            
            // Window background - very dark with slight transparency
            _windowBackground = MakeTexture(2, 2, BackgroundColor);
            
            // Button states - cleaner look with softer borders
            _buttonNormal = MakeRoundedTexture(64, 28, ButtonColor, new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.5f));
            _buttonHover = MakeRoundedTexture(64, 28, ButtonHoverColor, BorderLightColor);
            _buttonActive = MakeRoundedTexture(64, 28, ButtonActiveColor, AccentColor);
            
            // Box background - subtle panel with softer border
            _boxBackground = MakeRoundedTexture(64, 64, PanelColor, new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.4f));
            
            // Toggle textures - modern style
            _toggleOn = MakeToggleTexture(20, 20, true);
            _toggleOff = MakeToggleTexture(20, 20, false);
            
            // Slider - cleaner
            _sliderBackground = MakeTexture(8, 8, new Color(0.1f, 0.12f, 0.15f, 1f));
            _sliderThumb = MakeRoundedTexture(16, 22, AccentColor, AccentDark);
            
            // Text field - darker input
            _textFieldBackground = MakeRoundedTexture(64, 26, new Color(0.04f, 0.05f, 0.065f, 1f), BorderColor);
            
            // Gradient for headers
            _gradientHeader = MakeGradientTexture(1, 32, 
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.15f),
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.0f));
        }
        
        private static Texture2D MakeGradientTexture(int width, int height, Color top, Color bottom)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color color = Color.Lerp(bottom, top, t);
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = color;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private static Texture2D MakeRoundedTexture(int width, int height, Color fillColor, Color borderColor)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            
            int radius = 3;
            var pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    
                    // Simple corner rounding
                    bool isCorner = false;
                    if ((x < radius && y < radius) || (x < radius && y >= height - radius) ||
                        (x >= width - radius && y < radius) || (x >= width - radius && y >= height - radius))
                    {
                        int cx = x < radius ? radius : width - radius - 1;
                        int cy = y < radius ? radius : height - radius - 1;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        isCorner = dist > radius;
                    }
                    
                    if (isCorner)
                        pixels[y * width + x] = Color.clear;
                    else if (isBorder)
                        pixels[y * width + x] = borderColor;
                    else
                        pixels[y * width + x] = fillColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private static Texture2D MakeToggleTexture(int size, int height, bool isOn)
        {
            var texture = new Texture2D(size, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            
            // Modern checkbox style
            Color bgColor = isOn ? AccentColor : new Color(0.12f, 0.14f, 0.17f, 1f);
            Color borderCol = isOn ? AccentDark : BorderColor;
            Color checkColor = isOn ? new Color(1f, 1f, 1f, 1f) : Color.clear;
            
            var pixels = new Color[size * height];
            int radius = 3;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Rounded corners
                    bool isCorner = false;
                    if ((x < radius && y < radius) || (x < radius && y >= height - radius) ||
                        (x >= size - radius && y < radius) || (x >= size - radius && y >= height - radius))
                    {
                        int cx = x < radius ? radius : size - radius - 1;
                        int cy = y < radius ? radius : height - radius - 1;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        isCorner = dist > radius;
                    }
                    
                    bool isBorder = x <= 1 || x >= size - 2 || y <= 1 || y >= height - 2;
                    
                    if (isCorner)
                        pixels[y * size + x] = Color.clear;
                    else if (isBorder)
                        pixels[y * size + x] = borderCol;
                    else if (isOn && x > 4 && x < size - 5 && y > 4 && y < height - 5)
                        pixels[y * size + x] = checkColor;
                    else
                        pixels[y * size + x] = bgColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        #endregion
        
        #region Apply Skin
        
        public static void BeginUI()
        {
            if (!_initialized) Initialize();
            
            // Store original skin settings
            GUI.backgroundColor = Color.white;
            GUI.contentColor = TextColor;
        }
        
        public static void EndUI()
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }
        
        #endregion
        
        #region Custom Drawing Methods
        
        /// <summary>
        /// Draws a styled header label.
        /// </summary>
        public static void DrawHeader(string text)
        {
            var oldColor = GUI.contentColor;
            GUI.contentColor = AccentColor;
            GUILayout.Label(text);
            GUI.contentColor = oldColor;
            GUILayout.Space(5);
        }
        
        /// <summary>
        /// Draws a section title with a subtle background.
        /// </summary>
        public static void DrawSection(string title)
        {
            GUILayout.Space(14);
            
            // Reserve space for the entire section header
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            
            // Draw subtle gradient background
            if (_gradientHeader != null)
            {
                GUI.DrawTexture(headerRect, _gradientHeader, ScaleMode.StretchToFill);
            }
            
            // Draw accent bar on left (3px wide, full height)
            var oldColor = GUI.color;
            GUI.color = AccentColor;
            GUI.DrawTexture(new Rect(headerRect.x, headerRect.y + 3, 3, headerRect.height - 6), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Draw section title
            oldColor = GUI.contentColor;
            GUI.contentColor = TextColor;
            Rect labelRect = new Rect(headerRect.x + 12, headerRect.y + 3, headerRect.width - 12, headerRect.height - 6);
            GUI.Label(labelRect, title.ToUpper());
            GUI.contentColor = oldColor;
            
            // Draw bottom separator line (1px)
            GUI.color = new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.6f);
            GUI.DrawTexture(new Rect(headerRect.x, headerRect.y + headerRect.height - 1, headerRect.width, 1), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUILayout.Space(10);
        }
        
        /// <summary>
        /// Draws a styled toggle with status indicator.
        /// </summary>
        public static bool DrawToggle(string label, bool value, string description = null)
        {
            // Get rect for the entire row
            Rect rowRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            
            // Draw subtle background with left accent when enabled
            var oldColor = GUI.color;
            if (value)
            {
                // Subtle accent tint background
                GUI.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.06f);
                GUI.DrawTexture(new Rect(rowRect.x, rowRect.y + 2, rowRect.width, rowRect.height - 4), _solidWhite ?? Texture2D.whiteTexture);
                
                // Left accent bar
                GUI.color = AccentColor;
                GUI.DrawTexture(new Rect(rowRect.x, rowRect.y + 4, 2, rowRect.height - 8), _solidWhite ?? Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            
            // Toggle checkbox area (invisible button for click detection)
            Rect toggleRect = new Rect(rowRect.x + 8, rowRect.y + 8, 22, 20);
            bool newValue = GUI.Toggle(toggleRect, value, "");
            
            // Draw custom checkbox visual
            oldColor = GUI.color;
            GUI.color = value ? AccentColor : new Color(0.2f, 0.22f, 0.26f, 1f);
            GUI.DrawTexture(new Rect(toggleRect.x, toggleRect.y, 20, 20), _solidWhite ?? Texture2D.whiteTexture);
            
            // Inner fill or checkmark area
            if (value)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(toggleRect.x + 5, toggleRect.y + 5, 10, 10), _solidWhite ?? Texture2D.whiteTexture);
            }
            else
            {
                GUI.color = new Color(0.08f, 0.1f, 0.12f, 1f);
                GUI.DrawTexture(new Rect(toggleRect.x + 2, toggleRect.y + 2, 16, 16), _solidWhite ?? Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            
            // Label
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = value ? TextColor : TextMutedColor;
            Rect labelRect = new Rect(rowRect.x + 38, rowRect.y + 8, rowRect.width - 110, 20);
            GUI.Label(labelRect, label);
            GUI.contentColor = oldContentColor;
            
            // Status indicator on right
            Rect statusRect = new Rect(rowRect.x + rowRect.width - 52, rowRect.y + 8, 44, 20);
            
            // Status background
            oldColor = GUI.color;
            GUI.color = value ? new Color(SuccessColor.r, SuccessColor.g, SuccessColor.b, 0.15f) : new Color(0.12f, 0.14f, 0.16f, 1f);
            GUI.DrawTexture(statusRect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Status text
            oldContentColor = GUI.contentColor;
            GUI.contentColor = value ? SuccessColor : TextMutedColor;
            var statusStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
            GUI.Label(statusRect, value ? "ON" : "OFF", statusStyle);
            GUI.contentColor = oldContentColor;
            
            // Description on next line if provided
            if (!string.IsNullOrEmpty(description))
            {
                Rect descRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                oldContentColor = GUI.contentColor;
                GUI.contentColor = TextMutedColor;
                GUI.Label(new Rect(descRect.x + 38, descRect.y - 2, descRect.width - 38, 18), description);
                GUI.contentColor = oldContentColor;
            }
            
            return newValue;
        }
        
        /// <summary>
        /// Draws a styled slider.
        /// </summary>
        public static float DrawSlider(string label, float value, float min, float max, string format = "F1", string suffix = "")
        {
            Rect rowRect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
            float xPos = rowRect.x;
            
            // Label
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = TextMutedColor;
            GUI.Label(new Rect(xPos, rowRect.y + 4, 75, 20), label + ":");
            GUI.contentColor = oldContentColor;
            xPos += 78;
            
            // Value display box
            Rect valueRect = new Rect(xPos, rowRect.y + 2, 52, 24);
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(valueRect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Value text
            oldContentColor = GUI.contentColor;
            GUI.contentColor = AccentGlow;
            var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            GUI.Label(valueRect, value.ToString(format) + suffix, valueStyle);
            GUI.contentColor = oldContentColor;
            xPos += 60;
            
            // Slider track
            Rect sliderRect = new Rect(xPos, rowRect.y + 4, rowRect.width - xPos - 8, 20);
            float newValue = GUI.HorizontalSlider(sliderRect, value, min, max);
            
            return newValue;
        }
        
        /// <summary>
        /// Draws a styled button.
        /// </summary>
        public static bool DrawButton(string text, float width = 0)
        {
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = ButtonColor;
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(28));
            else
                clicked = GUILayout.Button(text, GUILayout.Height(28));
            
            GUI.backgroundColor = oldBg;
            return clicked;
        }
        
        /// <summary>
        /// Draws a styled accent button.
        /// </summary>
        public static bool DrawAccentButton(string text, float width = 0)
        {
            var oldBg = GUI.backgroundColor;
            var oldColor = GUI.contentColor;
            GUI.backgroundColor = AccentColor;
            GUI.contentColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(30));
            else
                clicked = GUILayout.Button(text, GUILayout.Height(30));
            
            GUI.backgroundColor = oldBg;
            GUI.contentColor = oldColor;
            return clicked;
        }
        
        /// <summary>
        /// Draws a danger/warning button (red).
        /// </summary>
        public static bool DrawDangerButton(string text, float width = 0)
        {
            var oldBg = GUI.backgroundColor;
            var oldColor = GUI.contentColor;
            GUI.backgroundColor = new Color(ErrorColor.r, ErrorColor.g, ErrorColor.b, 0.85f);
            GUI.contentColor = new Color(1f, 1f, 1f, 0.95f);
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(28));
            else
                clicked = GUILayout.Button(text, GUILayout.Height(28));
            
            GUI.backgroundColor = oldBg;
            GUI.contentColor = oldColor;
            return clicked;
        }
        
        /// <summary>
        /// Draws a key-value info line.
        /// </summary>
        public static void DrawInfo(string key, string value)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
            
            // Key label
            var oldColor = GUI.contentColor;
            GUI.contentColor = TextMutedColor;
            GUI.Label(new Rect(rect.x + 8, rect.y + 1, 100, 20), key);
            
            // Value with accent color
            GUI.contentColor = AccentGlow;
            GUI.Label(new Rect(rect.x + 110, rect.y + 1, rect.width - 118, 20), value);
            GUI.contentColor = oldColor;
        }
        
        /// <summary>
        /// Draws a compact info badge.
        /// </summary>
        public static void DrawInfoBadge(string label, string value)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            
            // Background
            var oldColor = GUI.color;
            GUI.color = new Color(0.08f, 0.1f, 0.12f, 0.8f);
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 2, rect.width - 8, rect.height - 4), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Label
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = TextMutedColor;
            GUI.Label(new Rect(rect.x + 12, rect.y + 4, 80, 18), label + ":");
            
            // Value
            GUI.contentColor = AccentGlow;
            GUI.Label(new Rect(rect.x + 94, rect.y + 4, rect.width - 106, 18), value);
            GUI.contentColor = oldContentColor;
        }
        
        /// <summary>
        /// Draws a horizontal separator.
        /// </summary>
        public static void DrawSeparator()
        {
            GUILayout.Space(6);
            Rect rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.5f);
            GUI.DrawTexture(rect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUILayout.Space(6);
        }
        
        /// <summary>
        /// Draws a status message with color.
        /// </summary>
        public static void DrawStatus(string text, StatusType type)
        {
            var oldColor = GUI.contentColor;
            switch (type)
            {
                case StatusType.Success: GUI.contentColor = SuccessColor; break;
                case StatusType.Warning: GUI.contentColor = WarningColor; break;
                case StatusType.Error: GUI.contentColor = ErrorColor; break;
                default: GUI.contentColor = TextColor; break;
            }
            GUILayout.Label(text);
            GUI.contentColor = oldColor;
        }
        
        /// <summary>
        /// DEPRECATED: Text fields don't work in IL2CPP. Use DrawNumericInput or DrawQuantitySelector instead.
        /// This method is kept for backwards compatibility but will just show a label.
        /// </summary>
        [System.Obsolete("Text fields don't work in IL2CPP. Use DrawNumericInput or DrawQuantitySelector instead.")]
        public static string DrawTextField(string value, float width = 100)
        {
            // Text fields don't work in IL2CPP - just show a styled label
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(22));
            GUI.backgroundColor = oldBg;
            
            var oldColor = GUI.contentColor;
            GUI.contentColor = TextMutedColor;
            GUILayout.Label(value ?? "");
            GUI.contentColor = oldColor;
            
            GUILayout.EndHorizontal();
            return value ?? "";
        }
        
        /// <summary>
        /// DEPRECATED: Text fields don't work in IL2CPP. Use DrawNumericInput instead.
        /// </summary>
        [System.Obsolete("Text fields don't work in IL2CPP. Use DrawNumericInput instead.")]
        public static string DrawLabeledTextField(string label, string value, float labelWidth = 70, float fieldWidth = 100)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(labelWidth));
            string result = DrawTextField(value, fieldWidth);
            GUILayout.EndHorizontal();
            return result;
        }
        
        /// <summary>
        /// IL2CPP-safe numeric input with +/- buttons and preset values.
        /// Returns the new value after user interaction.
        /// </summary>
        public static float DrawNumericInput(string label, float value, float step, float[] presets = null, string format = "N0", float labelWidth = 70)
        {
            float result = value;
            
            Rect rowRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            float xPos = rowRect.x;
            
            // Label
            if (!string.IsNullOrEmpty(label))
            {
                var oldContentColor = GUI.contentColor;
                GUI.contentColor = TextMutedColor;
                GUI.Label(new Rect(xPos, rowRect.y + 6, labelWidth, 20), label);
                GUI.contentColor = oldContentColor;
                xPos += labelWidth + 4;
            }
            
            // Value display box
            Rect valueRect = new Rect(xPos, rowRect.y + 2, 85, 28);
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(valueRect, _solidWhite ?? Texture2D.whiteTexture);
            
            // Border
            GUI.color = new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.5f);
            GUI.DrawTexture(new Rect(valueRect.x, valueRect.y, valueRect.width, 1), _solidWhite ?? Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(valueRect.x, valueRect.y + valueRect.height - 1, valueRect.width, 1), _solidWhite ?? Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(valueRect.x, valueRect.y, 1, valueRect.height), _solidWhite ?? Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(valueRect.x + valueRect.width - 1, valueRect.y, 1, valueRect.height), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Value text
            var oldContentColor2 = GUI.contentColor;
            GUI.contentColor = AccentGlow;
            var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(valueRect, value.ToString(format), valueStyle);
            GUI.contentColor = oldContentColor2;
            xPos += 90;
            
            // +/- buttons
            if (GUI.Button(new Rect(xPos, rowRect.y + 2, 28, 28), "-")) result = value - step;
            xPos += 30;
            if (GUI.Button(new Rect(xPos, rowRect.y + 2, 28, 28), "+")) result = value + step;
            xPos += 34;
            
            // Preset buttons
            if (presets != null)
            {
                foreach (var preset in presets)
                {
                    string presetLabel = preset >= 1000000 ? $"{preset / 1000000f:F0}M" :
                                         preset >= 1000 ? $"{preset / 1000f:F0}K" :
                                         preset.ToString("F0");
                    if (GUI.Button(new Rect(xPos, rowRect.y + 2, 48, 28), presetLabel)) result = preset;
                    xPos += 52;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// IL2CPP-safe numeric input for integers.
        /// </summary>
        public static int DrawIntInput(string label, int value, int step, int[] presets = null, float labelWidth = 70)
        {
            float[] floatPresets = null;
            if (presets != null)
            {
                floatPresets = new float[presets.Length];
                for (int i = 0; i < presets.Length; i++)
                    floatPresets[i] = presets[i];
            }
            return (int)DrawNumericInput(label, value, step, floatPresets, "N0", labelWidth);
        }
        
        /// <summary>
        /// Draws a row of preset buttons that return the selected value, or -1 if none clicked.
        /// </summary>
        public static float DrawPresetButtons(float[] presets, string prefix = "", string suffix = "")
        {
            GUILayout.BeginHorizontal();
            float result = -1;
            
            foreach (var preset in presets)
            {
                string label = preset >= 1000000 ? $"{prefix}{preset / 1000000f:F0}M{suffix}" :
                               preset >= 1000 ? $"{prefix}{preset / 1000f:F0}K{suffix}" :
                               $"{prefix}{preset:F0}{suffix}";
                if (DrawButton(label, 55))
                {
                    result = preset;
                }
            }
            
            GUILayout.EndHorizontal();
            return result;
        }
        
        /// <summary>
        /// Draws a quantity selector with preset buttons (no text input).
        /// </summary>
        public static int DrawQuantitySelector(string label, int currentValue, int[] presets, float labelWidth = 70)
        {
            int result = currentValue;
            
            Rect rowRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            float xPos = rowRect.x;
            
            // Label
            if (!string.IsNullOrEmpty(label))
            {
                var oldContentColor = GUI.contentColor;
                GUI.contentColor = TextMutedColor;
                GUI.Label(new Rect(xPos, rowRect.y + 6, labelWidth, 20), label);
                GUI.contentColor = oldContentColor;
                xPos += labelWidth + 4;
            }
            
            // Value display box
            Rect valueRect = new Rect(xPos, rowRect.y + 2, 55, 28);
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(valueRect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Value text
            var oldContentColor2 = GUI.contentColor;
            GUI.contentColor = AccentGlow;
            var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(valueRect, currentValue.ToString(), valueStyle);
            GUI.contentColor = oldContentColor2;
            xPos += 60;
            
            // Preset buttons with selection highlight
            foreach (var preset in presets)
            {
                bool isSelected = currentValue == preset;
                Rect btnRect = new Rect(xPos, rowRect.y + 2, 42, 28);
                
                // Selection highlight
                if (isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = AccentColor;
                    GUI.DrawTexture(btnRect, _solidWhite ?? Texture2D.whiteTexture);
                    GUI.color = oldColor;
                    
                    // Dark text on accent
                    var style = new GUIStyle(GUI.skin.button);
                    oldContentColor2 = GUI.contentColor;
                    GUI.contentColor = new Color(0.02f, 0.04f, 0.06f, 1f);
                    if (GUI.Button(btnRect, preset.ToString(), style)) result = preset;
                    GUI.contentColor = oldContentColor2;
                }
                else
                {
                    if (GUI.Button(btnRect, preset.ToString())) result = preset;
                }
                
                xPos += 46;
            }
            
            return result;
        }
        
        /// <summary>
        /// Begins a styled box/panel.
        /// </summary>
        public static void BeginBox()
        {
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = PanelColor;
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = oldBg;
        }
        
        /// <summary>
        /// Ends a styled box/panel.
        /// </summary>
        public static void EndBox()
        {
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws a progress bar.
        /// </summary>
        public static void DrawProgressBar(float value, float max, string label = null)
        {
            float percent = max > 0 ? Mathf.Clamp01(value / max) : 0;
            
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                var oldContentColor = GUI.contentColor;
                GUI.contentColor = TextMutedColor;
                GUILayout.Label(label, GUILayout.Width(80));
                GUI.contentColor = oldContentColor;
            }
            
            // Get rect for progress bar
            Rect rect = GUILayoutUtility.GetRect(100, 20, GUILayout.ExpandWidth(true));
            
            // Background track
            var oldColor = GUI.color;
            GUI.color = new Color(0.08f, 0.1f, 0.12f, 1f);
            GUI.DrawTexture(rect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Fill bar with gradient effect
            if (percent > 0)
            {
                Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * percent, rect.height - 2);
                oldColor = GUI.color;
                GUI.color = AccentColor;
                GUI.DrawTexture(fillRect, _solidWhite ?? Texture2D.whiteTexture);
                
                // Highlight on top of fill
                GUI.color = new Color(1f, 1f, 1f, 0.15f);
                GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width, fillRect.height * 0.4f), _solidWhite ?? Texture2D.whiteTexture);
                GUI.color = oldColor;
            }
            
            // Text overlay
            var oldContentColor2 = GUI.contentColor;
            GUI.contentColor = TextColor;
            var textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
            GUI.Label(rect, $"{value:F0} / {max:F0}", textStyle);
            GUI.contentColor = oldContentColor2;
            
            GUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Enums
        
        public enum StatusType
        {
            Normal,
            Success,
            Warning,
            Error
        }
        
        #endregion
    }
}
