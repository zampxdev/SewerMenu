using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Features.Items;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.UI.Windows
{
    /// <summary>
    /// Dedicated window for item spawning with category organization.
    /// Uses GUILayout.Window with lambda to avoid IL2CPP delegate issues.
    /// </summary>
    public class ItemSpawnerWindow
    {
        #region Singleton
        
        private static ItemSpawnerWindow _instance;
        public static ItemSpawnerWindow Instance => _instance ??= new ItemSpawnerWindow();
        
        private ItemSpawnerWindow() { }
        
        #endregion
        
        #region Fields
        
        private bool _isVisible = false;
        private Rect _windowRect = new Rect(100, 100, 750, 550);
        
        // Category system
        private List<string> _categories = new List<string>();
        private int _selectedCategoryIndex = 0;
        private Dictionary<string, List<ItemSpawner.ItemInfo>> _itemsByCategory = new Dictionary<string, List<ItemSpawner.ItemInfo>>();
        
        // Item list
        private Vector2 _categoryScrollPos;
        private Vector2 _itemScrollPos;
        private int _selectedItemIndex = -1;
        private ItemSpawner.ItemInfo _selectedItem = null;
        
        // Search and quantity
        private string _searchFilter = "";
        private int _spawnQuantity = 1;
        
        // Cached data
        private bool _dataLoaded = false;
        private List<ItemSpawner.ItemInfo> _currentItems = new List<ItemSpawner.ItemInfo>();
        
        // Presets for quantity
        private readonly int[] _quantityPresets = { 1, 5, 10, 25, 50, 100, 999 };
        
        // For dragging
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        
        // Cached textures for consistent styling
        private static Texture2D _solidTex;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        
        #endregion
        
        #region Show/Hide
        
        public void Show()
        {
            _isVisible = true;
            
            // Center window on screen
            _windowRect.x = (Screen.width - _windowRect.width) / 2;
            _windowRect.y = (Screen.height - _windowRect.height) / 2;
            
            // Load data if not loaded
            if (!_dataLoaded)
            {
                RefreshData();
            }
            
            // Ensure texture exists
            if (_solidTex == null)
            {
                _solidTex = new Texture2D(1, 1);
                _solidTex.SetPixel(0, 0, Color.white);
                _solidTex.Apply();
                _solidTex.hideFlags = HideFlags.HideAndDontSave;
            }
            
            SewerLogger.Debug("ItemSpawnerWindow opened");
        }
        
        public void Hide()
        {
            _isVisible = false;
            _isDragging = false;
            SewerLogger.Debug("ItemSpawnerWindow closed");
        }
        
        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }
        
        #endregion
        
        #region Data Loading
        
        public void RefreshData()
        {
            _categories.Clear();
            _itemsByCategory.Clear();
            _currentItems.Clear();
            
            try
            {
                var spawner = FeatureManager.Instance?.GetFeature<ItemSpawner>("itemspawner");
                if (spawner == null)
                {
                    SewerLogger.Warning("ItemSpawner feature not found");
                    return;
                }
                
                // Get all items
                var allItems = spawner.GetAllItems();
                if (allItems == null || allItems.Count == 0)
                {
                    spawner.RefreshItemCache();
                    allItems = spawner.GetAllItems();
                }
                
                if (allItems == null || allItems.Count == 0)
                {
                    SewerLogger.Warning("No items found");
                    return;
                }
                
                // Add "All" category first
                _categories.Add("All");
                _itemsByCategory["All"] = new List<ItemSpawner.ItemInfo>(allItems);
                
                // Group by category
                var grouped = allItems.GroupBy(i => i.Category ?? "Misc").OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    string category = group.Key;
                    if (!_categories.Contains(category))
                    {
                        _categories.Add(category);
                    }
                    _itemsByCategory[category] = group.OrderBy(i => i.Name).ToList();
                }
                
                // Set initial view
                _selectedCategoryIndex = 0;
                UpdateCurrentItems();
                
                _dataLoaded = true;
                SewerLogger.Info($"ItemSpawnerWindow: Loaded {allItems.Count} items in {_categories.Count} categories");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to load item data", ex);
            }
        }
        
        private void UpdateCurrentItems()
        {
            if (_selectedCategoryIndex < 0 || _selectedCategoryIndex >= _categories.Count)
            {
                _currentItems.Clear();
                return;
            }
            
            string category = _categories[_selectedCategoryIndex];
            
            if (!_itemsByCategory.TryGetValue(category, out var items))
            {
                _currentItems.Clear();
                return;
            }
            
            // Apply search filter
            if (string.IsNullOrWhiteSpace(_searchFilter))
            {
                _currentItems = new List<ItemSpawner.ItemInfo>(items);
            }
            else
            {
                string filter = _searchFilter.ToLower();
                _currentItems = items
                    .Where(i => i.Name.ToLower().Contains(filter) || 
                               i.Id.ToLower().Contains(filter))
                    .ToList();
            }
            
            // Reset selection if out of bounds
            if (_selectedItemIndex >= _currentItems.Count)
            {
                _selectedItemIndex = _currentItems.Count > 0 ? 0 : -1;
            }
            
            // Update selected item reference
            if (_selectedItemIndex >= 0 && _selectedItemIndex < _currentItems.Count)
            {
                _selectedItem = _currentItems[_selectedItemIndex];
            }
            else
            {
                _selectedItem = null;
            }
        }
        
        #endregion
        
        #region OnGUI - No GUI.Window, just direct drawing
        
        public void OnGUI()
        {
            if (!_isVisible) return;
            
            try
            {
                SewerSkin.BeginUI();
                
                // Clamp to screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
                
                // Handle dragging manually
                HandleDragging();
                
                // Draw window background with our theme
                DrawWindowBackground();
                
                // Draw content inside the rect
                GUILayout.BeginArea(new Rect(_windowRect.x + 8, _windowRect.y + 4, _windowRect.width - 16, _windowRect.height - 8));
                DrawWindowContent();
                GUILayout.EndArea();
                
                SewerSkin.EndUI();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("ItemSpawnerWindow.OnGUI error", ex);
            }
        }
        
        private void DrawWindowBackground()
        {
            if (_solidTex == null) return;
            
            var oldColor = GUI.color;
            
            // Main background
            GUI.color = SewerSkin.BackgroundColor;
            GUI.DrawTexture(_windowRect, _solidTex);
            
            // Border
            GUI.color = SewerSkin.BorderColor;
            GUI.DrawTexture(new Rect(_windowRect.x, _windowRect.y, _windowRect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(_windowRect.x, _windowRect.y + _windowRect.height - 1, _windowRect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(_windowRect.x, _windowRect.y, 1, _windowRect.height), _solidTex);
            GUI.DrawTexture(new Rect(_windowRect.x + _windowRect.width - 1, _windowRect.y, 1, _windowRect.height), _solidTex);
            
            // Accent line at top
            GUI.color = SewerSkin.AccentColor;
            GUI.DrawTexture(new Rect(_windowRect.x, _windowRect.y, _windowRect.width, 2), _solidTex);
            
            GUI.color = oldColor;
        }
        
        private void HandleDragging()
        {
            // Title bar rect for dragging - exclude the close button area (last 40 pixels)
            Rect titleBar = new Rect(_windowRect.x, _windowRect.y, _windowRect.width - 40, 30);
            
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && titleBar.Contains(e.mousePosition))
            {
                _isDragging = true;
                _dragOffset = e.mousePosition - new Vector2(_windowRect.x, _windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                _windowRect.x = e.mousePosition.x - _dragOffset.x;
                _windowRect.y = e.mousePosition.y - _dragOffset.y;
                e.Use();
            }
        }
        
        private void DrawWindowContent()
        {
            // ═══════════════════════════════════════════════════════════
            // HEADER
            // ═══════════════════════════════════════════════════════════
            Rect headerRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            
            // Header background
            var oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.07f, 1f);
            GUI.DrawTexture(headerRect, _solidTex);
            GUI.color = oldColor;
            
            // Title
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 6, 150, 20), "ITEM SPAWNER", titleStyle);
            GUI.contentColor = oldContentColor;
            
            // Close button (X)
            Rect closeRect = new Rect(headerRect.x + headerRect.width - 32, headerRect.y + 4, 28, 24);
            if (DrawStyledButton(closeRect, "X", true))
            {
                _isVisible = false;
                _isDragging = false;
            }
            
            GUILayout.Space(6);
            
            // ═══════════════════════════════════════════════════════════
            // QUANTITY BAR
            // ═══════════════════════════════════════════════════════════
            DrawTopBar();
            
            GUILayout.Space(6);
            
            // ═══════════════════════════════════════════════════════════
            // MAIN CONTENT
            // ═══════════════════════════════════════════════════════════
            GUILayout.BeginHorizontal();
            
            // Left sidebar - categories
            DrawCategorySidebar();
            
            GUILayout.Space(6);
            
            // Right panel - items
            DrawItemList();
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(6);
            
            // ═══════════════════════════════════════════════════════════
            // FOOTER
            // ═══════════════════════════════════════════════════════════
            DrawBottomBar();
        }
        
        private void DrawTopBar()
        {
            Rect barRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            
            // Background
            var oldColor = GUI.color;
            GUI.color = SewerSkin.PanelColor;
            GUI.DrawTexture(barRect, _solidTex);
            GUI.color = oldColor;
            
            float xPos = barRect.x + 8;
            float yPos = barRect.y + 6;
            
            // Quantity label
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(xPos, yPos, 60, 24), "Quantity:");
            GUI.contentColor = oldContentColor;
            xPos += 65;
            
            // -10 button
            if (DrawStyledButton(new Rect(xPos, yPos, 38, 24), "-10", false))
            {
                _spawnQuantity = Mathf.Max(1, _spawnQuantity - 10);
            }
            xPos += 42;
            
            // - button
            if (DrawStyledButton(new Rect(xPos, yPos, 28, 24), "-", false))
            {
                _spawnQuantity = Mathf.Max(1, _spawnQuantity - 1);
            }
            xPos += 32;
            
            // Quantity display box
            Rect qtyRect = new Rect(xPos, yPos, 55, 24);
            oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(qtyRect, _solidTex);
            GUI.color = new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f);
            GUI.DrawTexture(new Rect(qtyRect.x, qtyRect.y, qtyRect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(qtyRect.x, qtyRect.y + qtyRect.height - 1, qtyRect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(qtyRect.x, qtyRect.y, 1, qtyRect.height), _solidTex);
            GUI.DrawTexture(new Rect(qtyRect.x + qtyRect.width - 1, qtyRect.y, 1, qtyRect.height), _solidTex);
            GUI.color = oldColor;
            
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentGlow;
            var qtyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            GUI.Label(qtyRect, _spawnQuantity.ToString(), qtyStyle);
            GUI.contentColor = oldContentColor;
            xPos += 59;
            
            // + button
            if (DrawStyledButton(new Rect(xPos, yPos, 28, 24), "+", false))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 1);
            }
            xPos += 32;
            
            // +10 button
            if (DrawStyledButton(new Rect(xPos, yPos, 38, 24), "+10", false))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 10);
            }
            xPos += 50;
            
            // Quick presets label
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(xPos, yPos, 45, 24), "Quick:");
            GUI.contentColor = oldContentColor;
            xPos += 48;
            
            // Preset buttons
            foreach (int preset in _quantityPresets)
            {
                bool isSelected = _spawnQuantity == preset;
                if (DrawStyledButton(new Rect(xPos, yPos, 42, 24), preset.ToString(), false, isSelected))
                {
                    _spawnQuantity = preset;
                }
                xPos += 46;
            }
            
            // Refresh button (right side)
            float refreshX = barRect.x + barRect.width - 90;
            if (DrawStyledButton(new Rect(refreshX, yPos, 82, 24), "Refresh", false))
            {
                RefreshData();
            }
        }
        
        private void DrawCategorySidebar()
        {
            GUILayout.BeginVertical(GUILayout.Width(140));
            
            // Header
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(headerRect, _solidTex);
            GUI.color = oldColor;
            
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 5, 120, 16), "CATEGORIES", headerStyle);
            GUI.contentColor = oldContentColor;
            
            // Scrollable list background
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(135), GUILayout.ExpandHeight(true));
            oldColor = GUI.color;
            GUI.color = new Color(0.055f, 0.07f, 0.085f, 1f);
            GUI.DrawTexture(scrollAreaRect, _solidTex);
            GUI.color = oldColor;
            
            // Scroll view
            _categoryScrollPos = GUILayout.BeginScrollView(_categoryScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(135), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < _categories.Count; i++)
            {
                string category = _categories[i];
                bool isSelected = i == _selectedCategoryIndex;
                
                int count = _itemsByCategory.TryGetValue(category, out var items) ? items.Count : 0;
                string label = $"{category} ({count})";
                
                Rect btnRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
                
                // Draw button background
                oldColor = GUI.color;
                if (isSelected)
                {
                    GUI.color = SewerSkin.AccentColor;
                }
                else
                {
                    bool isHovered = btnRect.Contains(Event.current.mousePosition);
                    GUI.color = isHovered ? SewerSkin.ButtonHoverColor : SewerSkin.ButtonColor;
                }
                GUI.DrawTexture(btnRect, _solidTex);
                GUI.color = oldColor;
                
                // Draw text
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? new Color(0.02f, 0.04f, 0.06f, 1f) : SewerSkin.TextColor;
                var btnStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(8, 4, 0, 0) };
                GUI.Label(btnRect, label, btnStyle);
                GUI.contentColor = oldContentColor;
                
                // Click detection
                if (GUI.Button(btnRect, "", GUIStyle.none))
                {
                    _selectedCategoryIndex = i;
                    _selectedItemIndex = 0;
                    _itemScrollPos = Vector2.zero;
                    UpdateCurrentItems();
                }
                
                GUILayout.Space(2);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        
        private void DrawItemList()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            // Header
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.075f, 0.09f, 1f);
            GUI.DrawTexture(headerRect, _solidTex);
            GUI.color = oldColor;
            
            string categoryName = _selectedCategoryIndex < _categories.Count ? _categories[_selectedCategoryIndex] : "Items";
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 5, 200, 16), categoryName.ToUpper(), headerStyle);
            
            GUI.contentColor = SewerSkin.TextMutedColor;
            var countStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(headerRect.x + headerRect.width - 80, headerRect.y + 5, 70, 16), $"{_currentItems.Count} items", countStyle);
            GUI.contentColor = oldContentColor;
            
            // Item list background
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            oldColor = GUI.color;
            GUI.color = new Color(0.055f, 0.07f, 0.085f, 1f);
            GUI.DrawTexture(scrollAreaRect, _solidTex);
            GUI.color = oldColor;
            
            // Scroll view
            _itemScrollPos = GUILayout.BeginScrollView(_itemScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < _currentItems.Count; i++)
            {
                var item = _currentItems[i];
                bool isSelected = i == _selectedItemIndex;
                
                Rect rowRect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
                
                // Row background
                oldColor = GUI.color;
                if (isSelected)
                {
                    GUI.color = new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.25f);
                }
                else
                {
                    bool isHovered = rowRect.Contains(Event.current.mousePosition);
                    GUI.color = isHovered ? new Color(0.1f, 0.12f, 0.14f, 1f) : new Color(0.07f, 0.085f, 0.1f, 0.5f);
                }
                GUI.DrawTexture(rowRect, _solidTex);
                
                // Left accent bar for selected
                if (isSelected)
                {
                    GUI.color = SewerSkin.AccentColor;
                    GUI.DrawTexture(new Rect(rowRect.x, rowRect.y + 2, 3, rowRect.height - 4), _solidTex);
                }
                GUI.color = oldColor;
                
                float xPos = rowRect.x + 10;
                
                // Item name (clickable area)
                Rect nameRect = new Rect(xPos, rowRect.y + 4, rowRect.width - 220, 20);
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? SewerSkin.AccentGlow : SewerSkin.TextColor;
                var nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
                GUI.Label(nameRect, item.Name ?? "???", nameStyle);
                GUI.contentColor = oldContentColor;
                
                // Click to select
                if (GUI.Button(new Rect(rowRect.x, rowRect.y, rowRect.width - 70, rowRect.height), "", GUIStyle.none))
                {
                    _selectedItemIndex = i;
                    _selectedItem = item;
                }
                
                // Category tag
                float tagX = rowRect.x + rowRect.width - 200;
                oldContentColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                var tagStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
                GUI.Label(new Rect(tagX, rowRect.y + 6, 80, 16), $"[{item.Category ?? "?"}]", tagStyle);
                
                // Stack limit
                GUI.contentColor = new Color(0.5f, 0.55f, 0.6f, 1f);
                GUI.Label(new Rect(tagX + 82, rowRect.y + 6, 40, 16), $"x{item.StackLimit}", tagStyle);
                GUI.contentColor = oldContentColor;
                
                // Quick add button
                Rect addRect = new Rect(rowRect.x + rowRect.width - 58, rowRect.y + 3, 52, 22);
                if (DrawStyledButton(addRect, "+ Add", false, false, true))
                {
                    SpawnItem(item, _spawnQuantity);
                }
                
                GUILayout.Space(2);
            }
            
            if (_currentItems.Count == 0)
            {
                GUILayout.Space(30);
                oldContentColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                var emptyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                GUILayout.Label("No items found.\n\nTry selecting a different category\nor click 'Refresh' to reload items.", emptyStyle);
                GUI.contentColor = oldContentColor;
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        
        private void DrawBottomBar()
        {
            // Accent line
            Rect lineRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = SewerSkin.AccentColor;
            GUI.DrawTexture(lineRect, _solidTex);
            GUI.color = oldColor;
            
            GUILayout.Space(4);
            
            // Footer bar
            Rect footerRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.065f, 1f);
            GUI.DrawTexture(footerRect, _solidTex);
            GUI.color = oldColor;
            
            float xPos = footerRect.x + 10;
            float yPos = footerRect.y + 10;
            
            // Selected item info
            var oldContentColor = GUI.contentColor;
            if (_selectedItem != null)
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(xPos, yPos, 55, 20), "Selected:");
                xPos += 58;
                
                GUI.contentColor = SewerSkin.AccentGlow;
                var nameStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUI.Label(new Rect(xPos, yPos, 180, 20), _selectedItem.Name, nameStyle);
                xPos += 185;
                
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(xPos, yPos, 150, 20), $"ID: {_selectedItem.Id}");
            }
            else
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(xPos, yPos, 250, 20), "Select an item from the list above");
            }
            
            // Total items count (right side)
            int totalItems = _itemsByCategory.TryGetValue("All", out var all) ? all.Count : 0;
            GUI.contentColor = SewerSkin.TextMutedColor;
            var countStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(footerRect.x + footerRect.width - 250, yPos, 90, 20), $"{totalItems} items total", countStyle);
            GUI.contentColor = oldContentColor;
            
            // Main spawn button
            Rect spawnRect = new Rect(footerRect.x + footerRect.width - 150, footerRect.y + 6, 140, 28);
            if (_selectedItem != null)
            {
                string btnText = _spawnQuantity > 1 ? $"SPAWN {_spawnQuantity}x" : "SPAWN ITEM";
                if (DrawStyledButton(spawnRect, btnText, false, true))
                {
                    SpawnItem(_selectedItem, _spawnQuantity);
                }
            }
            else
            {
                // Disabled state
                oldColor = GUI.color;
                GUI.color = new Color(0.15f, 0.17f, 0.2f, 1f);
                GUI.DrawTexture(spawnRect, _solidTex);
                GUI.color = oldColor;
                
                oldContentColor = GUI.contentColor;
                GUI.contentColor = new Color(0.4f, 0.42f, 0.45f, 1f);
                var disabledStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                GUI.Label(spawnRect, "SPAWN ITEM", disabledStyle);
                GUI.contentColor = oldContentColor;
            }
        }
        
        /// <summary>
        /// Draws a styled button using our theme. Returns true if clicked.
        /// </summary>
        private bool DrawStyledButton(Rect rect, string text, bool isDanger = false, bool isAccent = false, bool isSuccess = false)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);
            
            var oldColor = GUI.color;
            
            // Determine colors based on type
            Color bgColor, hoverColor, textColor;
            if (isDanger)
            {
                bgColor = new Color(0.6f, 0.15f, 0.15f, 1f);
                hoverColor = new Color(0.7f, 0.2f, 0.2f, 1f);
                textColor = SewerSkin.TextColor;
            }
            else if (isAccent)
            {
                bgColor = SewerSkin.AccentColor;
                hoverColor = SewerSkin.AccentGlow;
                textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            }
            else if (isSuccess)
            {
                bgColor = new Color(0.15f, 0.4f, 0.2f, 1f);
                hoverColor = new Color(0.2f, 0.5f, 0.25f, 1f);
                textColor = SewerSkin.TextColor;
            }
            else
            {
                bgColor = SewerSkin.ButtonColor;
                hoverColor = SewerSkin.ButtonHoverColor;
                textColor = SewerSkin.TextColor;
            }
            
            // Draw background
            GUI.color = isHovered ? hoverColor : bgColor;
            GUI.DrawTexture(rect, _solidTex);
            
            // Draw subtle border
            GUI.color = new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), _solidTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), _solidTex);
            GUI.DrawTexture(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), _solidTex);
            GUI.color = oldColor;
            
            // Draw text
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = textColor;
            var btnStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
            GUI.Label(rect, text, btnStyle);
            GUI.contentColor = oldContentColor;
            
            // Click detection
            return GUI.Button(rect, "", GUIStyle.none);
        }
        
        #endregion
        
        #region Spawning
        
        private void SpawnItem(ItemSpawner.ItemInfo item, int quantity)
        {
            if (item == null || item.Definition == null)
            {
                SewerLogger.Warning("Cannot spawn - invalid item");
                return;
            }
            
            try
            {
                bool success = GameTypes.AddItemToInventory(item.Definition, quantity);
                if (success)
                {
                    SewerLogger.Success($"Spawned {quantity}x {item.Name}");
                }
                else
                {
                    SewerLogger.Warning($"Failed to spawn {item.Name}");
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Error($"Error spawning {item.Name}", ex);
            }
        }
        
        #endregion
    }
}
