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
        
        // Modern toggle textures (pill-style switch)
        private static Texture2D _togglePillOn;
        private static Texture2D _togglePillOff;
        private static Texture2D _toggleKnob;
        
        // Enhanced slider textures
        private static Texture2D _sliderTrackBg;
        private static Texture2D _sliderTrackFill;
        private static Texture2D _sliderKnob;
        
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
        private static GUIStyle _buttonAccentStyle;
        private static GUIStyle _buttonDangerStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _boxStyle;
        private static bool _stylesInitialized = false;
        
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
            
            // Modern toggle (pill-style switch) - 44x22 pixels
            _togglePillOn = MakePillTexture(44, 22, AccentColor, AccentDark);
            _togglePillOff = MakePillTexture(44, 22, new Color(0.15f, 0.17f, 0.2f, 1f), new Color(0.22f, 0.24f, 0.28f, 1f));
            _toggleKnob = MakeCircleTexture(18, Color.white);
            
            // Enhanced slider
            _sliderTrackBg = MakePillTexture(100, 8, new Color(0.1f, 0.12f, 0.15f, 1f), new Color(0.15f, 0.17f, 0.2f, 1f));
            _sliderTrackFill = MakePillTexture(100, 8, AccentColor, AccentDark);
            _sliderKnob = MakeCircleTexture(16, Color.white);
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
        
        /// <summary>
        /// Creates a pill/capsule shaped texture (rounded ends).
        /// </summary>
        private static Texture2D MakePillTexture(int width, int height, Color fillColor, Color borderColor)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            
            int radius = height / 2;
            var pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = float.MaxValue;
                    
                    // Left cap
                    if (x < radius)
                    {
                        float dx = x - radius;
                        float dy = y - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    // Right cap
                    else if (x >= width - radius)
                    {
                        float dx = x - (width - radius - 1);
                        float dy = y - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    // Middle section
                    else
                    {
                        dist = Mathf.Abs(y - radius);
                    }
                    
                    // Anti-aliased edge
                    if (dist > radius)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else if (dist > radius - 1.5f)
                    {
                        // Border with anti-aliasing
                        float alpha = Mathf.Clamp01(radius - dist);
                        pixels[y * width + x] = new Color(borderColor.r, borderColor.g, borderColor.b, alpha);
                    }
                    else if (dist > radius - 2.5f)
                    {
                        pixels[y * width + x] = borderColor;
                    }
                    else
                    {
                        pixels[y * width + x] = fillColor;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        /// <summary>
        /// Creates a circular texture with anti-aliasing.
        /// </summary>
        private static Texture2D MakeCircleTexture(int diameter, Color color)
        {
            var texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            
            float radius = diameter / 2f;
            float center = (diameter - 1) / 2f;
            var pixels = new Color[diameter * diameter];
            
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist > radius)
                    {
                        pixels[y * diameter + x] = Color.clear;
                    }
                    else if (dist > radius - 1.2f)
                    {
                        // Anti-aliased edge
                        float alpha = Mathf.Clamp01(radius - dist);
                        pixels[y * diameter + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                    else
                    {
                        // Add subtle gradient for depth
                        float gradientFactor = 1f - (dist / radius) * 0.15f;
                        pixels[y * diameter + x] = new Color(
                            Mathf.Min(1f, color.r * gradientFactor + 0.05f),
                            Mathf.Min(1f, color.g * gradientFactor + 0.05f),
                            Mathf.Min(1f, color.b * gradientFactor + 0.05f),
                            color.a
                        );
                    }
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
            if (!_stylesInitialized) CreateStyles();
            
            // Store original skin settings
            GUI.backgroundColor = Color.white;
            GUI.contentColor = TextColor;
        }
        
        private static void CreateStyles()
        {
            // Button style - modern dark with subtle border
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(2, 2, 2, 2)
            };
            _buttonStyle.normal.background = _buttonNormal;
            _buttonStyle.normal.textColor = TextColor;
            _buttonStyle.hover.background = _buttonHover;
            _buttonStyle.hover.textColor = TextColor;
            _buttonStyle.active.background = _buttonActive;
            _buttonStyle.active.textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            _buttonStyle.focused.background = _buttonHover;
            _buttonStyle.focused.textColor = TextColor;
            
            // Accent button style - blue background
            _buttonAccentStyle = new GUIStyle(_buttonStyle);
            _buttonAccentStyle.normal.background = _buttonActive;
            _buttonAccentStyle.normal.textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            _buttonAccentStyle.hover.background = MakeRoundedTexture(64, 28, AccentGlow, AccentColor);
            _buttonAccentStyle.hover.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonAccentStyle.hover.textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            _buttonAccentStyle.active.background = MakeRoundedTexture(64, 28, AccentDark, AccentColor);
            _buttonAccentStyle.active.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonAccentStyle.active.textColor = TextColor;
            
            // Danger button style - red background
            _buttonDangerStyle = new GUIStyle(_buttonStyle);
            _buttonDangerStyle.normal.background = MakeRoundedTexture(64, 28, new Color(ErrorColor.r * 0.8f, ErrorColor.g * 0.8f, ErrorColor.b * 0.8f, 1f), ErrorColor);
            _buttonDangerStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.normal.textColor = TextColor;
            _buttonDangerStyle.hover.background = MakeRoundedTexture(64, 28, ErrorColor, new Color(1f, 0.4f, 0.4f, 1f));
            _buttonDangerStyle.hover.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.hover.textColor = TextColor;
            _buttonDangerStyle.active.background = MakeRoundedTexture(64, 28, new Color(ErrorColor.r * 0.6f, ErrorColor.g * 0.6f, ErrorColor.b * 0.6f, 1f), ErrorColor);
            _buttonDangerStyle.active.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.active.textColor = TextColor;
            
            _stylesInitialized = true;
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
        /// Draws a modern pill-style toggle switch.
        /// </summary>
        public static bool DrawToggle(string label, bool value, string description = null)
        {
            // Get rect for the entire row
            Rect rowRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            
            // Draw subtle background with left accent when enabled
            var oldColor = GUI.color;
            if (value)
            {
                // Subtle accent tint background
                GUI.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.04f);
                GUI.DrawTexture(new Rect(rowRect.x, rowRect.y + 1, rowRect.width, rowRect.height - 2), _solidWhite ?? Texture2D.whiteTexture);
                
                // Left accent bar
                GUI.color = AccentColor;
                GUI.DrawTexture(new Rect(rowRect.x, rowRect.y + 4, 3, rowRect.height - 8), _solidWhite ?? Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            
            // Toggle switch area - pill style (44x22)
            const float toggleWidth = 44f;
            const float toggleHeight = 22f;
            const float knobSize = 18f;
            const float knobPadding = 2f;
            
            Rect toggleRect = new Rect(rowRect.x + 12, rowRect.y + (rowRect.height - toggleHeight) / 2f, toggleWidth, toggleHeight);
            
            // Invisible toggle for click detection (covers the whole toggle) - use GUIStyle.none to hide default checkbox
            bool newValue = GUI.Toggle(new Rect(toggleRect.x - 2, toggleRect.y - 2, toggleWidth + 4, toggleHeight + 4), value, "", GUIStyle.none);
            
            // Draw pill background
            oldColor = GUI.color;
            GUI.color = Color.white;
            if (value)
            {
                if (_togglePillOn != null)
                    GUI.DrawTexture(toggleRect, _togglePillOn, ScaleMode.StretchToFill);
                else
                {
                    GUI.color = AccentColor;
                    GUI.DrawTexture(toggleRect, _solidWhite ?? Texture2D.whiteTexture);
                }
            }
            else
            {
                if (_togglePillOff != null)
                    GUI.DrawTexture(toggleRect, _togglePillOff, ScaleMode.StretchToFill);
                else
                {
                    GUI.color = new Color(0.15f, 0.17f, 0.2f, 1f);
                    GUI.DrawTexture(toggleRect, _solidWhite ?? Texture2D.whiteTexture);
                }
            }
            
            // Draw knob (circle) - slides left/right based on state
            float knobX = value 
                ? toggleRect.x + toggleWidth - knobSize - knobPadding  // Right position (ON)
                : toggleRect.x + knobPadding;                           // Left position (OFF)
            float knobY = toggleRect.y + (toggleHeight - knobSize) / 2f;
            
            Rect knobRect = new Rect(knobX, knobY, knobSize, knobSize);
            GUI.color = Color.white;
            if (_toggleKnob != null)
            {
                GUI.DrawTexture(knobRect, _toggleKnob, ScaleMode.StretchToFill);
            }
            else
            {
                GUI.DrawTexture(knobRect, _solidWhite ?? Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            
            // Label - positioned after toggle
            float labelX = toggleRect.x + toggleWidth + 12f;
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = value ? TextColor : TextMutedColor;
            Rect labelRect = new Rect(labelX, rowRect.y + 6, rowRect.width - labelX - 60, 20);
            GUI.Label(labelRect, label);
            GUI.contentColor = oldContentColor;
            
            // Status text on right (smaller, subtle)
            Rect statusRect = new Rect(rowRect.x + rowRect.width - 40, rowRect.y + 7, 32, 18);
            oldContentColor = GUI.contentColor;
            GUI.contentColor = value ? SuccessColor : new Color(TextMutedColor.r, TextMutedColor.g, TextMutedColor.b, 0.6f);
            var statusStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, fontSize = 10 };
            GUI.Label(statusRect, value ? "ON" : "OFF", statusStyle);
            GUI.contentColor = oldContentColor;
            
            // Description on next line if provided
            if (!string.IsNullOrEmpty(description))
            {
                Rect descRect = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
                oldContentColor = GUI.contentColor;
                GUI.contentColor = new Color(TextMutedColor.r, TextMutedColor.g, TextMutedColor.b, 0.7f);
                var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
                GUI.Label(new Rect(labelX, descRect.y - 4, descRect.width - labelX - 10, 16), description, descStyle);
                GUI.contentColor = oldContentColor;
            }
            
            return newValue;
        }
        
        /// <summary>
        /// Draws a styled slider with filled track.
        /// </summary>
        public static float DrawSlider(string label, float value, float min, float max, string format = "F1", string suffix = "")
        {
            Rect rowRect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
            float xPos = rowRect.x;
            
            // Label
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = TextMutedColor;
            GUI.Label(new Rect(xPos, rowRect.y + 4, 70, 20), label + ":");
            GUI.contentColor = oldContentColor;
            xPos += 72;
            
            // Value display box with rounded appearance
            Rect valueRect = new Rect(xPos, rowRect.y + 2, 50, 24);
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.08f, 0.1f, 1f);
            GUI.DrawTexture(valueRect, _solidWhite ?? Texture2D.whiteTexture);
            // Subtle border
            GUI.color = new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.4f);
            GUI.DrawTexture(new Rect(valueRect.x, valueRect.y, valueRect.width, 1), _solidWhite ?? Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(valueRect.x, valueRect.y + valueRect.height - 1, valueRect.width, 1), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Value text
            oldContentColor = GUI.contentColor;
            GUI.contentColor = AccentGlow;
            var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
            GUI.Label(valueRect, value.ToString(format) + suffix, valueStyle);
            GUI.contentColor = oldContentColor;
            xPos += 56;
            
            // Custom slider track
            float trackHeight = 6f;
            float trackY = rowRect.y + (rowRect.height - trackHeight) / 2f;
            float trackWidth = rowRect.width - xPos - 12;
            Rect trackRect = new Rect(xPos, trackY, trackWidth, trackHeight);
            
            // Draw background track (dark)
            oldColor = GUI.color;
            GUI.color = new Color(0.1f, 0.12f, 0.15f, 1f);
            GUI.DrawTexture(trackRect, _solidWhite ?? Texture2D.whiteTexture);
            
            // Calculate fill percentage
            float percent = Mathf.Clamp01((value - min) / (max - min));
            
            // Draw filled portion (accent color)
            if (percent > 0)
            {
                float fillWidth = (trackWidth - 2) * percent;
                Rect fillRect = new Rect(trackRect.x + 1, trackRect.y + 1, fillWidth, trackHeight - 2);
                GUI.color = AccentColor;
                GUI.DrawTexture(fillRect, _solidWhite ?? Texture2D.whiteTexture);
                
                // Subtle highlight on top of fill
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width, 2), _solidWhite ?? Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            
            // Draw knob/thumb
            float knobSize = 14f;
            float knobX = trackRect.x + (trackWidth - knobSize) * percent;
            float knobY = rowRect.y + (rowRect.height - knobSize) / 2f;
            Rect knobRect = new Rect(knobX, knobY, knobSize, knobSize);
            
            // Knob shadow
            GUI.color = new Color(0f, 0f, 0f, 0.3f);
            GUI.DrawTexture(new Rect(knobRect.x + 1, knobRect.y + 1, knobSize, knobSize), _sliderKnob ?? _solidWhite ?? Texture2D.whiteTexture);
            
            // Knob
            GUI.color = Color.white;
            if (_sliderKnob != null)
                GUI.DrawTexture(knobRect, _sliderKnob, ScaleMode.StretchToFill);
            else
                GUI.DrawTexture(knobRect, _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Invisible slider for interaction (covers the track area)
            Rect interactionRect = new Rect(trackRect.x, rowRect.y, trackWidth, rowRect.height);
            float newValue = GUI.HorizontalSlider(interactionRect, value, min, max, GUIStyle.none, GUIStyle.none);
            
            return newValue;
        }
        
        /// <summary>
        /// Draws a styled button.
        /// </summary>
        public static bool DrawButton(string text, float width = 0)
        {
            if (!_stylesInitialized) CreateStyles();
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, _buttonStyle, GUILayout.Width(width), GUILayout.Height(28));
            else
                clicked = GUILayout.Button(text, _buttonStyle, GUILayout.Height(28));
            
            return clicked;
        }
        
        /// <summary>
        /// Draws a styled accent button.
        /// </summary>
        public static bool DrawAccentButton(string text, float width = 0)
        {
            if (!_stylesInitialized) CreateStyles();
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, _buttonAccentStyle, GUILayout.Width(width), GUILayout.Height(30));
            else
                clicked = GUILayout.Button(text, _buttonAccentStyle, GUILayout.Height(30));
            
            return clicked;
        }
        
        /// <summary>
        /// Draws a danger/warning button (red).
        /// </summary>
        public static bool DrawDangerButton(string text, float width = 0)
        {
            if (!_stylesInitialized) CreateStyles();
            
            bool clicked;
            if (width > 0)
                clicked = GUILayout.Button(text, _buttonDangerStyle, GUILayout.Width(width), GUILayout.Height(28));
            else
                clicked = GUILayout.Button(text, _buttonDangerStyle, GUILayout.Height(28));
            
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
        /// Draws a modern progress bar with rounded ends.
        /// </summary>
        public static void DrawProgressBar(float value, float max, string label = null)
        {
            float percent = max > 0 ? Mathf.Clamp01(value / max) : 0;
            
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                var oldContentColor = GUI.contentColor;
                GUI.contentColor = TextMutedColor;
                GUILayout.Label(label, GUILayout.Width(70));
                GUI.contentColor = oldContentColor;
            }
            
            // Get rect for progress bar
            Rect rect = GUILayoutUtility.GetRect(100, 18, GUILayout.ExpandWidth(true));
            
            // Background track (dark, rounded feel)
            var oldColor = GUI.color;
            GUI.color = new Color(0.08f, 0.1f, 0.12f, 1f);
            GUI.DrawTexture(rect, _solidWhite ?? Texture2D.whiteTexture);
            
            // Subtle inner shadow at top
            GUI.color = new Color(0f, 0f, 0f, 0.2f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), _solidWhite ?? Texture2D.whiteTexture);
            GUI.color = oldColor;
            
            // Fill bar
            if (percent > 0)
            {
                float fillWidth = (rect.width - 2) * percent;
                Rect fillRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, rect.height - 2);
                
                // Main fill
                oldColor = GUI.color;
                GUI.color = AccentColor;
                GUI.DrawTexture(fillRect, _solidWhite ?? Texture2D.whiteTexture);
                
                // Top highlight for depth
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width, 3), _solidWhite ?? Texture2D.whiteTexture);
                
                // Subtle glow at the end of fill
                if (fillWidth > 4)
                {
                    GUI.color = new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, 0.4f);
                    GUI.DrawTexture(new Rect(fillRect.x + fillRect.width - 3, fillRect.y, 3, fillRect.height), _solidWhite ?? Texture2D.whiteTexture);
                }
                GUI.color = oldColor;
            }
            
            // Text overlay
            var oldContentColor2 = GUI.contentColor;
            GUI.contentColor = TextColor;
            var textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };
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
