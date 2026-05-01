using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Core.Logging;

namespace SewerMenu.UI
{
    // UI styling system with IL2CPP-safe rendering (textures stored statically to prevent GC)
    public static class SewerSkin
    {
        #region Static Cache
        
        private static bool _initialized = false;
        private static GUISkin _customSkin;
        private static GUISkin _previousSkin;
        
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
        
        private static Texture2D _togglePillOn;
        private static Texture2D _togglePillOff;
        private static Texture2D _toggleKnob;
        
        private static Texture2D _sliderTrackBg;
        private static Texture2D _sliderTrackFill;
        private static Texture2D _sliderKnob;
        private static readonly Dictionary<string, float> _toggleAnimations = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> _hoverAnimations = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> _sectionAnimations = new Dictionary<string, float>();
        private static readonly Dictionary<string, Texture2D> _roundedTextureCache = new Dictionary<string, Texture2D>();
        
        // Colors - SewerMenu 2.0 beta theme
        public static readonly Color AccentColor = new Color(0.55f, 0.84f, 0.20f, 1f);
        public static readonly Color AccentDark = new Color(0.27f, 0.52f, 0.13f, 1f);
        public static readonly Color AccentGlow = new Color(0.72f, 1.0f, 0.34f, 1f);
        public static readonly Color BackgroundColor = new Color(0.025f, 0.033f, 0.031f, 0.985f);
        public static readonly Color PanelColor = new Color(0.055f, 0.068f, 0.064f, 0.96f);
        public static readonly Color PanelLightColor = new Color(0.085f, 0.102f, 0.096f, 0.96f);
        public static readonly Color ButtonColor = new Color(0.09f, 0.108f, 0.104f, 0.95f);
        public static readonly Color ButtonHoverColor = new Color(0.13f, 0.16f, 0.145f, 0.98f);
        public static readonly Color ButtonActiveColor = new Color(0.31f, 0.59f, 0.16f, 1f);
        public static readonly Color TextColor = new Color(0.93f, 0.95f, 0.92f, 1f);
        public static readonly Color TextMutedColor = new Color(0.57f, 0.62f, 0.57f, 1f);
        public static readonly Color SuccessColor = new Color(0.50f, 0.84f, 0.22f, 1f);
        public static readonly Color WarningColor = new Color(0.86f, 0.65f, 0.18f, 1f);
        public static readonly Color ErrorColor = new Color(0.95f, 0.28f, 0.25f, 1f);
        public static readonly Color BorderColor = new Color(0.16f, 0.22f, 0.18f, 1f);
        public static readonly Color BorderLightColor = new Color(0.36f, 0.58f, 0.22f, 1f);
        
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
            _solidWhite = MakeTexture(2, 2, Color.white);
            _windowBackground = MakeTexture(2, 2, BackgroundColor);
            _buttonNormal = MakeTexture(2, 2, ButtonColor);
            _buttonHover = MakeTexture(2, 2, ButtonHoverColor);
            _buttonActive = MakeTexture(2, 2, ButtonActiveColor);
            _boxBackground = MakeTexture(2, 2, PanelColor);
            _toggleOn = MakeToggleTexture(20, 20, true);
            _toggleOff = MakeToggleTexture(20, 20, false);
            _sliderBackground = MakeTexture(8, 8, new Color(0.1f, 0.12f, 0.15f, 1f));
            _sliderThumb = MakeTexture(2, 2, AccentColor);
            _textFieldBackground = MakeTexture(2, 2, new Color(0.04f, 0.05f, 0.065f, 1f));
            _gradientHeader = MakeGradientTexture(1, 32, 
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.15f),
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.0f));
            _togglePillOn = MakePillTexture(44, 22, AccentColor, AccentDark);
            _togglePillOff = MakePillTexture(44, 22, new Color(0.15f, 0.17f, 0.2f, 1f), new Color(0.22f, 0.24f, 0.28f, 1f));
            _toggleKnob = MakeCircleTexture(18, Color.white);
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
            
            int radius = Mathf.Min(8, Mathf.Max(3, Mathf.Min(width, height) / 3));
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

        private static string ColorKey(Color color)
        {
            int r = Mathf.RoundToInt(Mathf.Clamp01(color.r) * 31f);
            int g = Mathf.RoundToInt(Mathf.Clamp01(color.g) * 31f);
            int b = Mathf.RoundToInt(Mathf.Clamp01(color.b) * 31f);
            int a = Mathf.RoundToInt(Mathf.Clamp01(color.a) * 31f);
            return r + "," + g + "," + b + "," + a;
        }

        private static Texture2D GetRoundedTexture(int width, int height, int radius, Color fillColor, Color borderColor, int borderWidth)
        {
            width = Mathf.Clamp(width, 1, 2048);
            height = Mathf.Clamp(height, 1, 2048);
            radius = Mathf.Clamp(radius, 0, Mathf.Min(width, height) / 2);
            borderWidth = Mathf.Clamp(borderWidth, 0, radius);

            string key = width + "x" + height + "r" + radius + "b" + borderWidth + "|" + ColorKey(fillColor) + "|" + ColorKey(borderColor);
            if (_roundedTextureCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            if (_roundedTextureCache.Count > 700)
            {
                _roundedTextureCache.Clear();
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color[width * height];
            float r = radius;
            float bw = borderWidth;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color;
                    if (radius <= 0)
                    {
                        bool isBorder = borderWidth > 0 && (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                        color = isBorder ? borderColor : fillColor;
                    }
                    else
                    {
                        float px = x + 0.5f;
                        float py = y + 0.5f;
                        float cx = Mathf.Clamp(px, r, width - r);
                        float cy = Mathf.Clamp(py, r, height - r);
                        float dx = px - cx;
                        float dy = py - cy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float edge = r - dist;
                        float alpha = Mathf.Clamp01(edge + 0.85f);

                        if (alpha <= 0f)
                        {
                            color = Color.clear;
                        }
                        else
                        {
                            bool isBorder = bw > 0f && edge <= bw;
                            color = isBorder ? borderColor : fillColor;
                            color.a *= alpha;
                        }
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _roundedTextureCache[key] = texture;
            return texture;
        }
        
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
                    
                    if (x < radius)
                    {
                        float dx = x - radius;
                        float dy = y - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    else if (x >= width - radius)
                    {
                        float dx = x - (width - radius - 1);
                        float dy = y - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    else
                    {
                        dist = Mathf.Abs(y - radius);
                    }
                    
                    if (dist > radius)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else if (dist > radius - 1.5f)
                    {
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
                        float alpha = Mathf.Clamp01(radius - dist);
                        pixels[y * diameter + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                    else
                    {
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
            
            Color bgColor = isOn ? AccentColor : new Color(0.12f, 0.14f, 0.17f, 1f);
            Color borderCol = isOn ? AccentDark : BorderColor;
            Color checkColor = isOn ? new Color(1f, 1f, 1f, 1f) : Color.clear;
            
            var pixels = new Color[size * height];
            int radius = 3;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < size; x++)
                {
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
            
            _previousSkin = GUI.skin;
            if (_customSkin != null)
            {
                GUI.skin = _customSkin;
            }

            GUI.backgroundColor = Color.white;
            GUI.contentColor = TextColor;
        }
        
        private static void CreateStyles()
        {
            _customSkin = UnityEngine.Object.Instantiate(GUI.skin);
            _customSkin.hideFlags = HideFlags.HideAndDontSave;
            _customSkin.window.normal.background = _windowBackground;
            _customSkin.window.normal.textColor = TextColor;
            _customSkin.window.border = new RectOffset(8, 8, 8, 8);
            _customSkin.window.padding = new RectOffset(10, 10, 8, 10);
            _customSkin.box.normal.background = _boxBackground;
            _customSkin.box.normal.textColor = TextColor;
            _customSkin.button.normal.background = _buttonNormal;
            _customSkin.button.hover.background = _buttonHover;
            _customSkin.button.active.background = _buttonActive;
            _customSkin.button.normal.textColor = TextColor;
            _customSkin.button.hover.textColor = TextColor;
            _customSkin.button.active.textColor = new Color(0.02f, 0.04f, 0.03f, 1f);
            _customSkin.label.normal.textColor = TextColor;
            _customSkin.verticalScrollbar.fixedWidth = 12f;
            _customSkin.verticalScrollbarThumb.normal.background = MakeTexture(2, 2, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.62f));
            _customSkin.verticalScrollbarThumb.hover.background = MakeTexture(2, 2, AccentColor);

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
            
            _buttonAccentStyle = new GUIStyle(_buttonStyle);
            _buttonAccentStyle.normal.background = _buttonActive;
            _buttonAccentStyle.normal.textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            _buttonAccentStyle.hover.background = MakeTexture(2, 2, AccentGlow);
            _buttonAccentStyle.hover.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonAccentStyle.hover.textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            _buttonAccentStyle.active.background = MakeTexture(2, 2, AccentDark);
            _buttonAccentStyle.active.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonAccentStyle.active.textColor = TextColor;
            
            _buttonDangerStyle = new GUIStyle(_buttonStyle);
            _buttonDangerStyle.normal.background = MakeTexture(2, 2, new Color(ErrorColor.r * 0.8f, ErrorColor.g * 0.8f, ErrorColor.b * 0.8f, 1f));
            _buttonDangerStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.normal.textColor = TextColor;
            _buttonDangerStyle.hover.background = MakeTexture(2, 2, ErrorColor);
            _buttonDangerStyle.hover.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.hover.textColor = TextColor;
            _buttonDangerStyle.active.background = MakeTexture(2, 2, new Color(ErrorColor.r * 0.6f, ErrorColor.g * 0.6f, ErrorColor.b * 0.6f, 1f));
            _buttonDangerStyle.active.background.hideFlags = HideFlags.HideAndDontSave;
            _buttonDangerStyle.active.textColor = TextColor;
            
            _stylesInitialized = true;
        }
        
        public static void EndUI()
        {
            if (_previousSkin != null)
            {
                GUI.skin = _previousSkin;
                _previousSkin = null;
            }

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }

        public static void ResetTransientAnimations()
        {
            _sectionAnimations.Clear();
        }
        
        #endregion
        
        #region Custom Drawing Methods

        public static Texture2D WhiteTexture
        {
            get
            {
                if (!_initialized) Initialize();
                return _solidWhite ?? Texture2D.whiteTexture;
            }
        }

        public static void DrawSolid(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = oldColor;
        }

        public static void DrawRoundedRect(Rect rect, Color fillColor, Color borderColor, int radius = 7, int borderWidth = 1)
        {
            if (!_initialized) Initialize();
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            var texture = GetRoundedTexture(width, height, radius, fillColor, borderColor, borderWidth);

            var oldColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            GUI.color = oldColor;
        }

        public static void DrawSoftGlow(Rect rect, Color color, float alpha = 0.22f, int radius = 8)
        {
            DrawRoundedRect(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), new Color(color.r, color.g, color.b, alpha * 0.25f), Color.clear, radius + 3, 0);
            DrawRoundedRect(new Rect(rect.x - 1f, rect.y - 1f, rect.width + 2f, rect.height + 2f), new Color(color.r, color.g, color.b, alpha * 0.45f), Color.clear, radius + 1, 0);
        }

        public static bool DrawButtonRect(Rect rect, string text, Color baseColor, Color hoverColor, Color textColor, int radius = 7, bool selected = false, int fontSize = 11)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            string key = "button:" + text + ":" + Mathf.RoundToInt(rect.x) + ":" + Mathf.RoundToInt(rect.y) + ":" + Mathf.RoundToInt(rect.width) + ":" + Mathf.RoundToInt(rect.height);
            if (!_hoverAnimations.TryGetValue(key, out float hoverAnim))
            {
                hoverAnim = hovered ? 1f : 0f;
            }

            if (Event.current.type == EventType.Repaint)
            {
                hoverAnim = Mathf.MoveTowards(hoverAnim, hovered ? 1f : 0f, Time.unscaledDeltaTime * 10f);
                _hoverAnimations[key] = hoverAnim;
            }

            Color fill = selected ? hoverColor : Color.Lerp(baseColor, hoverColor, hoverAnim);
            Color border = selected
                ? new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, 0.92f)
                : Color.Lerp(new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.65f), new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.7f), hoverAnim);

            if (hoverAnim > 0.01f || selected)
            {
                DrawSoftGlow(rect, selected ? AccentColor : hoverColor, selected ? 0.18f : 0.10f, radius);
            }

            DrawRoundedRect(rect, fill, border, radius, 1);
            DrawRoundedRect(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, Mathf.Min(8f, rect.height * 0.35f)), new Color(1f, 1f, 1f, 0.045f + 0.035f * hoverAnim), Color.clear, Mathf.Max(2, radius - 1), 0);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = textColor;
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = selected ? FontStyle.Bold : FontStyle.Normal
            };
            GUI.Label(rect, text, style);
            GUI.contentColor = oldContentColor;

            return GUI.Button(rect, "", GUIStyle.none);
        }

        public static void DrawWindowPanel(Rect rect)
        {
            if (!_initialized) Initialize();

            float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.5f;
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.34f), Color.clear, 8, 0);
            DrawSoftGlow(rect, AccentColor, 0.12f + 0.04f * pulse, 8);
            DrawRoundedRect(rect, BackgroundColor, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.38f), 8, 1);
            DrawRoundedRect(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 46f), new Color(1f, 1f, 1f, 0.035f), Color.clear, 7, 0);
            DrawSolid(new Rect(rect.x + 18f, rect.y + 1f, rect.width - 36f, 1f), new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, 0.34f + 0.10f * pulse));
        }
        
        public static void DrawHeader(string text)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            Rect panelRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 7f);
            DrawRoundedRect(panelRect, new Color(0.038f, 0.052f, 0.046f, 0.82f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 6, 1);
            DrawSolid(new Rect(panelRect.x + 12f, panelRect.y + panelRect.height - 2f, 78f, 2f), AccentColor);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = TextColor;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            GUI.Label(new Rect(panelRect.x + 12, panelRect.y + 4, panelRect.width - 24, 24), text, style);
            GUI.contentColor = oldContentColor;

            GUILayout.Space(8);
        }
        
        public static void DrawSection(string title)
        {
            GUILayout.Space(10);
            
            Rect headerRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            string key = "section:" + title;
            if (!_sectionAnimations.TryGetValue(key, out float anim))
            {
                anim = 0f;
            }

            if (Event.current.type == EventType.Repaint)
            {
                anim = Mathf.MoveTowards(anim, 1f, Time.unscaledDeltaTime * 7.5f);
                _sectionAnimations[key] = anim;
            }

            float eased = 1f - Mathf.Pow(1f - anim, 3f);
            Rect bandRect = new Rect(headerRect.x + 4f + (1f - eased) * 10f, headerRect.y + 3f, headerRect.width - 8f - (1f - eased) * 10f, headerRect.height - 8f);
            DrawRoundedRect(bandRect, new Color(0.06f, 0.083f, 0.067f, 0.70f + 0.16f * eased), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.25f + 0.20f * eased), 5, 1);
            DrawSolid(new Rect(bandRect.x + 8, bandRect.y + 6, 3, bandRect.height - 12), new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.35f + 0.65f * eased));
            DrawSolid(new Rect(bandRect.x + 18, bandRect.y + bandRect.height - 2, 68 * eased, 2), new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, 0.55f * eased));
            
            var oldColor = GUI.contentColor;
            GUI.contentColor = new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, AccentGlow.a * Mathf.Lerp(0.55f, 1f, eased));
            Rect labelRect = new Rect(bandRect.x + 20, bandRect.y + 4, bandRect.width - 28, bandRect.height - 5);
            var sectionStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            GUI.Label(labelRect, title.ToUpper(), sectionStyle);
            GUI.contentColor = oldColor;

            GUILayout.Space(6);
        }
        
        public static bool DrawToggle(string label, bool value, string description = null)
        {
            float rowHeight = string.IsNullOrEmpty(description) ? 42f : 58f;
            Rect rowRect = GUILayoutUtility.GetRect(0, rowHeight, GUILayout.ExpandWidth(true));
            rowRect = new Rect(rowRect.x + 4f, rowRect.y + 2f, rowRect.width - 8f, rowRect.height - 4f);

            string key = label + "|" + (description ?? "");
            if (!_toggleAnimations.TryGetValue(key, out float anim))
            {
                anim = value ? 1f : 0f;
            }

            if (Event.current.type == EventType.Repaint)
            {
                anim = Mathf.MoveTowards(anim, value ? 1f : 0f, Time.unscaledDeltaTime * 9f);
                _toggleAnimations[key] = anim;
            }

            float eased = 1f - Mathf.Pow(1f - anim, 3f);
            bool hovered = rowRect.Contains(Event.current.mousePosition);
            bool newValue = value;
            if (GUI.Button(rowRect, "", GUIStyle.none))
            {
                newValue = !value;
            }

            Color rowColor = hovered
                ? new Color(0.105f, 0.132f, 0.118f, 0.96f)
                : new Color(0.066f, 0.081f, 0.076f, 0.94f);
            DrawRoundedRect(rowRect, rowColor, new Color(BorderColor.r, BorderColor.g, BorderColor.b, hovered ? 0.72f : 0.45f), 7, 1);
            DrawRoundedRect(new Rect(rowRect.x + 1f, rowRect.y + 1f, rowRect.width - 2f, Mathf.Min(10f, rowRect.height * 0.28f)), new Color(1f, 1f, 1f, hovered ? 0.045f : 0.025f), Color.clear, 6, 0);

            if (anim > 0.01f)
            {
                DrawRoundedRect(rowRect, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.035f + 0.065f * anim), Color.clear, 7, 0);
                DrawRoundedRect(new Rect(rowRect.x + 6f, rowRect.y + 8f, 4f, rowRect.height - 16f), new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.55f + 0.35f * anim), Color.clear, 3, 0);
            }

            const float toggleWidth = 48f;
            const float toggleHeight = 24f;
            const float knobSize = 18f;
            const float knobPadding = 3f;
            Rect toggleRect = new Rect(rowRect.x + rowRect.width - toggleWidth - 12f, rowRect.y + (rowRect.height - toggleHeight) / 2f, toggleWidth, toggleHeight);

            Color switchFill = Color.Lerp(new Color(0.14f, 0.16f, 0.19f, 1f), AccentColor, eased);
            Color switchBorder = Color.Lerp(new Color(0.27f, 0.30f, 0.34f, 1f), AccentDark, eased);
            if (anim > 0.01f)
            {
                DrawSoftGlow(toggleRect, AccentColor, 0.22f * anim, 12);
            }
            DrawRoundedRect(toggleRect, switchFill, switchBorder, 12, 1);
            DrawRoundedRect(new Rect(toggleRect.x + 3f, toggleRect.y + 3f, toggleRect.width - 6f, 5f), new Color(1f, 1f, 1f, 0.12f), Color.clear, 5, 0);

            float knobX = Mathf.Lerp(toggleRect.x + knobPadding, toggleRect.x + toggleWidth - knobSize - knobPadding, eased);
            Rect knobShadow = new Rect(knobX + 1f, toggleRect.y + 4f, knobSize, knobSize);
            Rect knobRect = new Rect(knobX, toggleRect.y + 3f, knobSize, knobSize);
            var oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.28f);
            GUI.DrawTexture(knobShadow, _toggleKnob ?? WhiteTexture, ScaleMode.StretchToFill);
            GUI.color = Color.Lerp(new Color(0.84f, 0.87f, 0.86f, 1f), Color.white, eased);
            GUI.DrawTexture(knobRect, _toggleKnob ?? WhiteTexture, ScaleMode.StretchToFill);
            GUI.color = oldColor;

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = Color.Lerp(TextMutedColor, TextColor, Mathf.Max(anim, hovered ? 0.45f : 0f));
            var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = value ? FontStyle.Bold : FontStyle.Normal, clipping = TextClipping.Clip };
            GUI.Label(new Rect(rowRect.x + 14f, rowRect.y + 8f, rowRect.width - 84f, 22f), label, labelStyle);

            if (!string.IsNullOrEmpty(description))
            {
                GUI.contentColor = new Color(TextMutedColor.r, TextMutedColor.g, TextMutedColor.b, hovered ? 0.9f : 0.68f);
                var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, clipping = TextClipping.Clip };
                GUI.Label(new Rect(rowRect.x + 14f, rowRect.y + 31f, rowRect.width - 86f, 18f), description, descStyle);
            }

            GUI.contentColor = oldContentColor;
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
            DrawRoundedRect(valueRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 6, 1);
            
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
            var oldColor = GUI.color;
            DrawRoundedRect(trackRect, new Color(0.085f, 0.098f, 0.095f, 1f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 3, 1);
            
            // Calculate fill percentage
            float percent = Mathf.Clamp01((value - min) / (max - min));
            
            // Draw filled portion (accent color)
            if (percent > 0)
            {
                float fillWidth = (trackWidth - 2) * percent;
                Rect fillRect = new Rect(trackRect.x + 1, trackRect.y + 1, fillWidth, trackHeight - 2);
                DrawRoundedRect(fillRect, AccentColor, Color.clear, 3, 0);
            }
            
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
            
            Rect rect = width > 0
                ? GUILayoutUtility.GetRect(width, 30, GUILayout.Width(width), GUILayout.Height(30))
                : GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true), GUILayout.Height(30));

            return DrawButtonRect(rect, text, ButtonColor, ButtonHoverColor, TextColor, 7, false, 11);
        }
        
        /// <summary>
        /// Draws a styled accent button.
        /// </summary>
        public static bool DrawAccentButton(string text, float width = 0)
        {
            if (!_stylesInitialized) CreateStyles();
            
            Rect rect = width > 0
                ? GUILayoutUtility.GetRect(width, 32, GUILayout.Width(width), GUILayout.Height(32))
                : GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true), GUILayout.Height(32));

            return DrawButtonRect(rect, text, ButtonActiveColor, AccentGlow, new Color(0.02f, 0.04f, 0.03f, 1f), 7, false, 11);
        }
        
        /// <summary>
        /// Draws a danger/warning button (red).
        /// </summary>
        public static bool DrawDangerButton(string text, float width = 0)
        {
            if (!_stylesInitialized) CreateStyles();
            
            Rect rect = width > 0
                ? GUILayoutUtility.GetRect(width, 30, GUILayout.Width(width), GUILayout.Height(30))
                : GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true), GUILayout.Height(30));

            return DrawButtonRect(rect, text, new Color(ErrorColor.r * 0.7f, ErrorColor.g * 0.7f, ErrorColor.b * 0.7f, 1f), ErrorColor, TextColor, 7, false, 11);
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
            Rect badgeRect = new Rect(rect.x + 4, rect.y + 2, rect.width - 8, rect.height - 4);
            
            DrawRoundedRect(badgeRect, new Color(0.055f, 0.071f, 0.064f, 0.9f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.5f), 6, 1);
            
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
            DrawRoundedRect(valueRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 6, 1);
            
            // Value text
            var oldContentColor2 = GUI.contentColor;
            GUI.contentColor = AccentGlow;
            var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(valueRect, value.ToString(format), valueStyle);
            GUI.contentColor = oldContentColor2;
            xPos += 90;
            
            // +/- buttons
            if (DrawButtonRect(new Rect(xPos, rowRect.y + 2, 28, 28), "-", ButtonColor, ButtonHoverColor, TextColor, 6, false, 12)) result = value - step;
            xPos += 30;
            if (DrawButtonRect(new Rect(xPos, rowRect.y + 2, 28, 28), "+", ButtonColor, ButtonHoverColor, TextColor, 6, false, 12)) result = value + step;
            xPos += 34;
            
            // Preset buttons
            if (presets != null)
            {
                foreach (var preset in presets)
                {
                    string presetLabel = preset >= 1000000 ? $"{preset / 1000000f:F0}M" :
                                         preset >= 1000 ? $"{preset / 1000f:F0}K" :
                                         preset.ToString("F0");
                    if (DrawButtonRect(new Rect(xPos, rowRect.y + 2, 48, 28), presetLabel, ButtonColor, ButtonHoverColor, TextColor, 6, false, 10)) result = preset;
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
            DrawRoundedRect(valueRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 6, 1);
            
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
                Color baseColor = isSelected ? ButtonActiveColor : ButtonColor;
                Color hoverColor = isSelected ? AccentGlow : ButtonHoverColor;
                Color textColor = isSelected ? new Color(0.02f, 0.04f, 0.03f, 1f) : TextColor;
                if (DrawButtonRect(btnRect, preset.ToString(), baseColor, hoverColor, textColor, 6, isSelected, 10)) result = preset;
                
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
            DrawRoundedRect(rect, new Color(0.06f, 0.075f, 0.07f, 1f), new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.55f), 8, 1);
            
            // Fill bar
            if (percent > 0)
            {
                float fillWidth = (rect.width - 2) * percent;
                Rect fillRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, rect.height - 2);
                DrawRoundedRect(fillRect, AccentColor, Color.clear, 8, 0);
                
                // Subtle glow at the end of fill
                if (fillWidth > 4)
                {
                    DrawRoundedRect(new Rect(fillRect.x + fillRect.width - 4, fillRect.y, 4, fillRect.height), new Color(AccentGlow.r, AccentGlow.g, AccentGlow.b, 0.4f), Color.clear, 4, 0);
                }
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
