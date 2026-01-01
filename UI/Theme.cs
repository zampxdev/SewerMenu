using UnityEngine;

namespace SewerMenu.UI
{
    /// <summary>
    /// Defines the visual theme for SewerMenu.
    /// Sewer/Underground aesthetic with toxic accents.
    /// </summary>
    public static class Theme
    {
        #region Color Palette
        
        // Primary Colors
        public static readonly Color Background = new Color32(26, 26, 46, 255);       // #1A1A2E - Deep Navy
        public static readonly Color BackgroundDark = new Color32(15, 15, 30, 255);   // Darker variant
        public static readonly Color Panel = new Color32(22, 33, 62, 255);            // #16213E - Dark Blue
        public static readonly Color PanelLight = new Color32(30, 45, 80, 255);       // Lighter panel
        public static readonly Color PanelHover = new Color32(35, 55, 95, 255);       // Hover state
        
        // Accent Colors
        public static readonly Color Accent = new Color32(15, 52, 96, 255);           // #0F3460 - Medium Blue
        public static readonly Color Highlight = new Color32(233, 69, 96, 255);       // #E94560 - Toxic Pink/Red
        public static readonly Color HighlightHover = new Color32(255, 90, 120, 255); // Lighter pink
        
        // State Colors
        public static readonly Color Success = new Color32(74, 222, 128, 255);        // #4ADE80 - Green
        public static readonly Color Warning = new Color32(251, 191, 36, 255);        // #FBBF24 - Amber
        public static readonly Color Error = new Color32(239, 68, 68, 255);           // #EF4444 - Red
        public static readonly Color Info = new Color32(96, 165, 250, 255);           // #60A5FA - Blue
        
        // Text Colors
        public static readonly Color Text = new Color32(229, 229, 229, 255);          // #E5E5E5 - Light Gray
        public static readonly Color TextMuted = new Color32(156, 163, 175, 255);     // #9CA3AF - Gray
        public static readonly Color TextDark = new Color32(75, 85, 99, 255);         // #4B5563 - Dark Gray
        public static readonly Color TextHighlight = Highlight;
        
        // Border Colors
        public static readonly Color Border = new Color32(55, 65, 81, 255);           // #374151 - Dark Gray
        public static readonly Color BorderLight = new Color32(75, 85, 99, 255);      // Lighter border
        public static readonly Color BorderAccent = Highlight;
        
        // Toggle Colors
        public static readonly Color ToggleOn = Success;
        public static readonly Color ToggleOff = new Color32(75, 85, 99, 255);
        public static readonly Color ToggleKnob = Color.white;
        
        // Slider Colors
        public static readonly Color SliderBackground = new Color32(55, 65, 81, 255);
        public static readonly Color SliderFill = Highlight;
        public static readonly Color SliderHandle = Color.white;
        
        // Button Colors
        public static readonly Color ButtonNormal = Panel;
        public static readonly Color ButtonHover = PanelHover;
        public static readonly Color ButtonActive = Highlight;
        public static readonly Color ButtonDisabled = new Color32(40, 40, 60, 255);
        
        // Scrollbar Colors
        public static readonly Color ScrollbarBackground = BackgroundDark;
        public static readonly Color ScrollbarThumb = Border;
        public static readonly Color ScrollbarThumbHover = BorderLight;
        
        #endregion
        
        #region Dimensions
        
        // Window
        public const float WindowMinWidth = 400f;
        public const float WindowMinHeight = 300f;
        public const float WindowMaxWidth = 800f;
        public const float WindowMaxHeight = 900f;
        public const float WindowDefaultWidth = 500f;
        public const float WindowDefaultHeight = 600f;
        
        // Header
        public const float HeaderHeight = 35f;
        public const float HeaderButtonSize = 25f;
        
        // Tabs
        public const float TabHeight = 35f;
        public const float TabMinWidth = 70f;
        public const float TabPadding = 10f;
        
        // Components
        public const float ToggleWidth = 45f;
        public const float ToggleHeight = 22f;
        public const float SliderHeight = 20f;
        public const float ButtonHeight = 30f;
        public const float InputHeight = 25f;
        public const float LabelHeight = 20f;
        public const float SectionHeaderHeight = 30f;
        
        // Spacing
        public const float Padding = 10f;
        public const float SmallPadding = 5f;
        public const float LargePadding = 15f;
        public const float ItemSpacing = 8f;
        public const float SectionSpacing = 15f;
        
        // Borders
        public const float BorderWidth = 1f;
        public const float BorderRadius = 4f;
        
        // Scrollbar
        public const float ScrollbarWidth = 12f;
        
        #endregion
        
        #region Typography
        
        public const int FontSizeSmall = 11;
        public const int FontSizeNormal = 13;
        public const int FontSizeMedium = 14;
        public const int FontSizeLarge = 16;
        public const int FontSizeHeader = 18;
        public const int FontSizeTitle = 22;
        
        #endregion
        
        #region Animation
        
        public const float AnimationSpeed = 8f;
        public const float FadeSpeed = 5f;
        public const float HoverTransitionSpeed = 10f;
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Creates a solid color texture that won't be garbage collected.
        /// </summary>
        public static Texture2D CreateColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            // Prevent garbage collection in IL2CPP
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
        
        /// <summary>
        /// Creates a texture with a border.
        /// </summary>
        public static Texture2D CreateBorderedTexture(Color fill, Color border, int width = 4, int height = 4, int borderWidth = 1)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isBorder = x < borderWidth || x >= width - borderWidth || 
                                   y < borderWidth || y >= height - borderWidth;
                    texture.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
        
        /// <summary>
        /// Creates a gradient texture.
        /// </summary>
        public static Texture2D CreateGradientTexture(Color top, Color bottom, int height = 32)
        {
            var texture = new Texture2D(1, height, TextureFormat.RGBA32, false);
            
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                texture.SetPixel(0, y, Color.Lerp(bottom, top, t));
            }
            
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
        
        /// <summary>
        /// Lerps between two colors with a smooth transition.
        /// </summary>
        public static Color LerpColor(Color from, Color to, float t)
        {
            return Color.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
        }
        
        /// <summary>
        /// Gets a color with modified alpha.
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        /// <summary>
        /// Darkens a color by a percentage.
        /// </summary>
        public static Color Darken(Color color, float amount)
        {
            return new Color(
                color.r * (1 - amount),
                color.g * (1 - amount),
                color.b * (1 - amount),
                color.a
            );
        }
        
        /// <summary>
        /// Lightens a color by a percentage.
        /// </summary>
        public static Color Lighten(Color color, float amount)
        {
            return new Color(
                Mathf.Min(1, color.r + amount),
                Mathf.Min(1, color.g + amount),
                Mathf.Min(1, color.b + amount),
                color.a
            );
        }
        
        #endregion
    }
}
