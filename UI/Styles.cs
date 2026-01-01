using UnityEngine;

namespace SewerMenu.UI
{
    /// <summary>
    /// Manages GUIStyle instances for consistent UI rendering.
    /// </summary>
    public static class Styles
    {
        #region Fields
        
        private static bool _initialized = false;
        
        // Cached textures
        private static Texture2D _backgroundTexture;
        private static Texture2D _panelTexture;
        private static Texture2D _panelHoverTexture;
        private static Texture2D _buttonTexture;
        private static Texture2D _buttonHoverTexture;
        private static Texture2D _buttonActiveTexture;
        private static Texture2D _toggleOnTexture;
        private static Texture2D _toggleOffTexture;
        private static Texture2D _sliderBackgroundTexture;
        private static Texture2D _sliderFillTexture;
        private static Texture2D _headerTexture;
        private static Texture2D _tabActiveTexture;
        private static Texture2D _tabInactiveTexture;
        private static Texture2D _borderTexture;
        private static Texture2D _transparentTexture;
        
        #endregion
        
        #region Styles
        
        // Window styles
        public static GUIStyle Window { get; private set; }
        public static GUIStyle WindowHeader { get; private set; }
        public static GUIStyle WindowTitle { get; private set; }
        public static GUIStyle WindowCloseButton { get; private set; }
        
        // Tab styles
        public static GUIStyle TabActive { get; private set; }
        public static GUIStyle TabInactive { get; private set; }
        public static GUIStyle TabBar { get; private set; }
        
        // Content styles
        public static GUIStyle ContentArea { get; private set; }
        public static GUIStyle Section { get; private set; }
        public static GUIStyle SectionHeader { get; private set; }
        
        // Text styles
        public static GUIStyle Label { get; private set; }
        public static GUIStyle LabelBold { get; private set; }
        public static GUIStyle LabelMuted { get; private set; }
        public static GUIStyle LabelCentered { get; private set; }
        public static GUIStyle LabelRight { get; private set; }
        public static GUIStyle Title { get; private set; }
        public static GUIStyle Subtitle { get; private set; }
        
        // Button styles
        public static GUIStyle Button { get; private set; }
        public static GUIStyle ButtonSmall { get; private set; }
        public static GUIStyle ButtonPrimary { get; private set; }
        public static GUIStyle ButtonDanger { get; private set; }
        
        // Toggle styles
        public static GUIStyle Toggle { get; private set; }
        public static GUIStyle ToggleLabel { get; private set; }
        
        // Slider styles
        public static GUIStyle SliderBackground { get; private set; }
        public static GUIStyle SliderThumb { get; private set; }
        public static GUIStyle SliderLabel { get; private set; }
        
        // Input styles
        public static GUIStyle TextField { get; private set; }
        public static GUIStyle TextArea { get; private set; }
        
        // Scrollview styles
        public static GUIStyle ScrollView { get; private set; }
        public static GUIStyle ScrollViewVertical { get; private set; }
        public static GUIStyle ScrollViewHorizontal { get; private set; }
        
        // Misc styles
        public static GUIStyle Box { get; private set; }
        public static GUIStyle Tooltip { get; private set; }
        public static GUIStyle StatusBar { get; private set; }
        
        // List styles
        public static GUIStyle ListItem { get; private set; }
        public static GUIStyle ListItemSelected { get; private set; }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes all styles. Must be called from OnGUI.
        /// </summary>
        public static void Initialize()
        {
            // Make sure GUI.skin is available
            if (GUI.skin == null) return;
            
            // Check if textures were garbage collected and reinitialize if needed
            if (_initialized && _backgroundTexture == null)
            {
                _initialized = false;
            }
            
            if (_initialized) return;
            
            try
            {
                CreateTextures();
                CreateStyles();
                _initialized = true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SewerMenu] Failed to initialize styles: {ex.Message}");
                // Create minimal fallback styles
                CreateFallbackStyles();
                _initialized = true;
            }
        }
        
        /// <summary>
        /// Creates minimal fallback styles if main initialization fails.
        /// </summary>
        private static void CreateFallbackStyles()
        {
            Window = new GUIStyle(GUI.skin.box);
            WindowHeader = new GUIStyle(GUI.skin.box);
            WindowTitle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            WindowCloseButton = new GUIStyle(GUI.skin.button);
            TabBar = new GUIStyle(GUI.skin.box);
            TabActive = new GUIStyle(GUI.skin.button);
            TabInactive = new GUIStyle(GUI.skin.button);
            ContentArea = new GUIStyle(GUI.skin.box);
            Section = new GUIStyle(GUI.skin.box);
            SectionHeader = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            Label = new GUIStyle(GUI.skin.label);
            LabelBold = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            LabelMuted = new GUIStyle(GUI.skin.label);
            LabelCentered = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            LabelRight = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            Title = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 18 };
            Subtitle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            Button = new GUIStyle(GUI.skin.button);
            ButtonSmall = new GUIStyle(GUI.skin.button);
            ButtonPrimary = new GUIStyle(GUI.skin.button);
            ButtonDanger = new GUIStyle(GUI.skin.button);
            Toggle = new GUIStyle(GUI.skin.toggle);
            ToggleLabel = new GUIStyle(GUI.skin.label);
            SliderBackground = new GUIStyle(GUI.skin.horizontalSlider);
            SliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            SliderLabel = new GUIStyle(GUI.skin.label);
            TextField = new GUIStyle(GUI.skin.textField);
            TextArea = new GUIStyle(GUI.skin.textArea);
            ScrollView = new GUIStyle();
            ScrollViewVertical = new GUIStyle(GUI.skin.verticalScrollbar);
            ScrollViewHorizontal = new GUIStyle(GUI.skin.horizontalScrollbar);
            Box = new GUIStyle(GUI.skin.box);
            Tooltip = new GUIStyle(GUI.skin.box);
            StatusBar = new GUIStyle(GUI.skin.box);
            ListItem = new GUIStyle(GUI.skin.button);
            ListItemSelected = new GUIStyle(GUI.skin.button);
        }
        
        /// <summary>
        /// Creates all required textures.
        /// </summary>
        private static void CreateTextures()
        {
            _backgroundTexture = Theme.CreateColorTexture(Theme.Background);
            _panelTexture = Theme.CreateColorTexture(Theme.Panel);
            _panelHoverTexture = Theme.CreateColorTexture(Theme.PanelHover);
            _buttonTexture = Theme.CreateColorTexture(Theme.ButtonNormal);
            _buttonHoverTexture = Theme.CreateColorTexture(Theme.ButtonHover);
            _buttonActiveTexture = Theme.CreateColorTexture(Theme.ButtonActive);
            _toggleOnTexture = Theme.CreateColorTexture(Theme.ToggleOn);
            _toggleOffTexture = Theme.CreateColorTexture(Theme.ToggleOff);
            _sliderBackgroundTexture = Theme.CreateColorTexture(Theme.SliderBackground);
            _sliderFillTexture = Theme.CreateColorTexture(Theme.SliderFill);
            _headerTexture = Theme.CreateColorTexture(Theme.BackgroundDark);
            _tabActiveTexture = Theme.CreateColorTexture(Theme.Highlight);
            _tabInactiveTexture = Theme.CreateColorTexture(Theme.Panel);
            _borderTexture = Theme.CreateColorTexture(Theme.Border);
            _transparentTexture = Theme.CreateColorTexture(Color.clear);
        }
        
        /// <summary>
        /// Creates all GUIStyle instances.
        /// </summary>
        private static void CreateStyles()
        {
            // Window
            Window = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _backgroundTexture, textColor = Theme.Text },
                onNormal = { background = _backgroundTexture, textColor = Theme.Text },
                border = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            
            // Window Header
            WindowHeader = new GUIStyle()
            {
                normal = { background = _headerTexture, textColor = Theme.Text },
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, 0, 0),
                fixedHeight = Theme.HeaderHeight,
                alignment = TextAnchor.MiddleLeft
            };
            
            // Window Title
            WindowTitle = new GUIStyle()
            {
                normal = { textColor = Theme.Highlight },
                fontSize = Theme.FontSizeHeader,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            
            // Window Close Button
            WindowCloseButton = new GUIStyle()
            {
                normal = { background = _transparentTexture, textColor = Theme.TextMuted },
                hover = { background = _transparentTexture, textColor = Theme.Highlight },
                active = { background = _transparentTexture, textColor = Theme.Error },
                fontSize = Theme.FontSizeLarge,
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = Theme.HeaderButtonSize,
                fixedHeight = Theme.HeaderButtonSize
            };
            
            // Tab Bar
            TabBar = new GUIStyle()
            {
                normal = { background = _panelTexture },
                padding = new RectOffset((int)Theme.SmallPadding, (int)Theme.SmallPadding, 0, 0),
                fixedHeight = Theme.TabHeight
            };
            
            // Tab Active
            TabActive = new GUIStyle()
            {
                normal = { background = _tabActiveTexture, textColor = Theme.Text },
                hover = { background = _tabActiveTexture, textColor = Theme.Text },
                fontSize = Theme.FontSizeNormal,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset((int)Theme.TabPadding, (int)Theme.TabPadding, 0, 0),
                margin = new RectOffset(2, 2, 5, 5)
            };
            
            // Tab Inactive
            TabInactive = new GUIStyle()
            {
                normal = { background = _tabInactiveTexture, textColor = Theme.TextMuted },
                hover = { background = _panelHoverTexture, textColor = Theme.Text },
                fontSize = Theme.FontSizeNormal,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset((int)Theme.TabPadding, (int)Theme.TabPadding, 0, 0),
                margin = new RectOffset(2, 2, 5, 5)
            };
            
            // Content Area
            ContentArea = new GUIStyle()
            {
                normal = { background = _backgroundTexture },
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding)
            };
            
            // Section
            Section = new GUIStyle()
            {
                normal = { background = _panelTexture },
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding),
                margin = new RectOffset(0, 0, 0, (int)Theme.ItemSpacing)
            };
            
            // Section Header
            SectionHeader = new GUIStyle()
            {
                normal = { textColor = Theme.Highlight },
                fontSize = Theme.FontSizeMedium,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, (int)Theme.SmallPadding)
            };
            
            // Labels
            Label = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Theme.Text },
                fontSize = Theme.FontSizeNormal,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            
            LabelBold = new GUIStyle(Label)
            {
                fontStyle = FontStyle.Bold
            };
            
            LabelMuted = new GUIStyle(Label)
            {
                normal = { textColor = Theme.TextMuted },
                fontSize = Theme.FontSizeSmall
            };
            
            LabelCentered = new GUIStyle(Label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            
            LabelRight = new GUIStyle(Label)
            {
                alignment = TextAnchor.MiddleRight
            };
            
            Title = new GUIStyle(Label)
            {
                fontSize = Theme.FontSizeTitle,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Theme.Highlight }
            };
            
            Subtitle = new GUIStyle(Label)
            {
                fontSize = Theme.FontSizeLarge,
                fontStyle = FontStyle.Bold
            };
            
            // Buttons
            Button = new GUIStyle(GUI.skin.button)
            {
                normal = { background = _buttonTexture, textColor = Theme.Text },
                hover = { background = _buttonHoverTexture, textColor = Theme.Text },
                active = { background = _buttonActiveTexture, textColor = Theme.Text },
                focused = { background = _buttonHoverTexture, textColor = Theme.Text },
                fontSize = Theme.FontSizeNormal,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, (int)Theme.SmallPadding, (int)Theme.SmallPadding),
                margin = new RectOffset(0, 0, 0, (int)Theme.SmallPadding),
                fixedHeight = Theme.ButtonHeight
            };
            
            ButtonSmall = new GUIStyle(Button)
            {
                fontSize = Theme.FontSizeSmall,
                fixedHeight = Theme.ButtonHeight - 6,
                padding = new RectOffset((int)Theme.SmallPadding, (int)Theme.SmallPadding, 2, 2)
            };
            
            ButtonPrimary = new GUIStyle(Button)
            {
                normal = { background = _tabActiveTexture, textColor = Theme.Text },
                hover = { background = Theme.CreateColorTexture(Theme.HighlightHover), textColor = Theme.Text },
                fontStyle = FontStyle.Bold
            };
            
            ButtonDanger = new GUIStyle(Button)
            {
                normal = { background = Theme.CreateColorTexture(Theme.Error), textColor = Theme.Text },
                hover = { background = Theme.CreateColorTexture(Theme.Lighten(Theme.Error, 0.1f)), textColor = Theme.Text }
            };
            
            // Toggle
            Toggle = new GUIStyle()
            {
                normal = { background = _toggleOffTexture },
                onNormal = { background = _toggleOnTexture },
                fixedWidth = Theme.ToggleWidth,
                fixedHeight = Theme.ToggleHeight
            };
            
            ToggleLabel = new GUIStyle(Label)
            {
                alignment = TextAnchor.MiddleLeft
            };
            
            // Slider
            SliderBackground = new GUIStyle()
            {
                normal = { background = _sliderBackgroundTexture },
                fixedHeight = Theme.SliderHeight
            };
            
            SliderThumb = new GUIStyle()
            {
                normal = { background = Theme.CreateColorTexture(Theme.SliderHandle) },
                fixedWidth = 14,
                fixedHeight = 14
            };
            
            SliderLabel = new GUIStyle(Label)
            {
                fontSize = Theme.FontSizeSmall,
                alignment = TextAnchor.MiddleRight
            };
            
            // Text Field
            TextField = new GUIStyle(GUI.skin.textField)
            {
                normal = { background = _panelTexture, textColor = Theme.Text },
                focused = { background = _panelHoverTexture, textColor = Theme.Text },
                hover = { background = _panelHoverTexture, textColor = Theme.Text },
                fontSize = Theme.FontSizeNormal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset((int)Theme.SmallPadding, (int)Theme.SmallPadding, 2, 2),
                fixedHeight = Theme.InputHeight
            };
            
            TextArea = new GUIStyle(TextField)
            {
                wordWrap = true,
                fixedHeight = 0
            };
            
            // Scroll View
            ScrollView = new GUIStyle()
            {
                normal = { background = _transparentTexture }
            };
            
            ScrollViewVertical = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                normal = { background = _sliderBackgroundTexture },
                fixedWidth = Theme.ScrollbarWidth
            };
            
            ScrollViewHorizontal = new GUIStyle(GUI.skin.horizontalScrollbar)
            {
                normal = { background = _sliderBackgroundTexture },
                fixedHeight = Theme.ScrollbarWidth
            };
            
            // Box
            Box = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _panelTexture, textColor = Theme.Text },
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding, (int)Theme.Padding)
            };
            
            // Tooltip
            Tooltip = new GUIStyle()
            {
                normal = { background = Theme.CreateColorTexture(Theme.BackgroundDark), textColor = Theme.Text },
                fontSize = Theme.FontSizeSmall,
                padding = new RectOffset((int)Theme.SmallPadding, (int)Theme.SmallPadding, (int)Theme.SmallPadding, (int)Theme.SmallPadding),
                wordWrap = true
            };
            
            // Status Bar
            StatusBar = new GUIStyle()
            {
                normal = { background = _headerTexture, textColor = Theme.TextMuted },
                fontSize = Theme.FontSizeSmall,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset((int)Theme.Padding, (int)Theme.Padding, 0, 0),
                fixedHeight = 25
            };
            
            // List Item
            ListItem = new GUIStyle(Button)
            {
                normal = { background = _transparentTexture, textColor = Theme.Text },
                hover = { background = _panelHoverTexture, textColor = Theme.Text },
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 22,
                margin = new RectOffset(0, 0, 0, 1)
            };
            
            // List Item Selected
            ListItemSelected = new GUIStyle(ListItem)
            {
                normal = { background = _tabActiveTexture, textColor = Theme.Text },
                hover = { background = _tabActiveTexture, textColor = Theme.Text },
                fontStyle = FontStyle.Bold
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Forces style reinitialization.
        /// </summary>
        public static void Reinitialize()
        {
            _initialized = false;
            Initialize();
        }
        
        /// <summary>
        /// Draws a horizontal line separator.
        /// </summary>
        public static void DrawSeparator()
        {
            GUILayout.Space(Theme.SmallPadding);
            var rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, _borderTexture);
            GUILayout.Space(Theme.SmallPadding);
        }
        
        /// <summary>
        /// Draws a colored box.
        /// </summary>
        public static void DrawColoredBox(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
        
        #endregion
    }
}
