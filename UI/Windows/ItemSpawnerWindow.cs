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
    // Uses direct drawing (not GUI.Window) to avoid IL2CPP delegate issues
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
        
        private List<string> _categories = new List<string>();
        private int _selectedCategoryIndex = 0;
        private Dictionary<string, List<ItemSpawner.ItemInfo>> _itemsByCategory = new Dictionary<string, List<ItemSpawner.ItemInfo>>();
        
        private Vector2 _categoryScrollPos;
        private Vector2 _itemScrollPos;
        private Vector2 _targetCategoryScrollPos;
        private Vector2 _displayCategoryScrollPos;
        private Vector2 _targetItemScrollPos;
        private Vector2 _displayItemScrollPos;
        private int _selectedItemIndex = -1;
        private ItemSpawner.ItemInfo _selectedItem = null;
        
        private string _searchFilter = "";
        private int _spawnQuantity = 1;
        private string _statusMessage = "";
        private bool _statusSuccess = true;
        
        private bool _dataLoaded = false;
        private List<ItemSpawner.ItemInfo> _currentItems = new List<ItemSpawner.ItemInfo>();
        
        private readonly int[] _quantityPresets = { 1, 5, 10, 25, 50, 100, 999 };
        
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        private float _openAnim = 1f;
        
        private static Texture2D _solidTex;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        
        #endregion
        
        #region Show/Hide
        
        public void Show()
        {
            _isVisible = true;
            _openAnim = 0f;
            _targetCategoryScrollPos = _categoryScrollPos;
            _displayCategoryScrollPos = _categoryScrollPos;
            _targetItemScrollPos = _itemScrollPos;
            _displayItemScrollPos = _itemScrollPos;
            
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
            _selectedItem = null;
            _selectedItemIndex = -1;
            
            try
            {
                var spawner = FeatureManager.Instance?.GetFeature<ItemSpawner>("itemspawner");
                if (spawner == null)
                {
                    SewerLogger.Warning("ItemSpawner feature not found");
                    return;
                }
                
                // Force a fresh registry scan. The first scan can happen before a save is fully loaded.
                spawner.RefreshItemCache();
                var allItems = spawner.GetAllItems();
                
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
                _selectedItemIndex = _itemsByCategory["All"].Count > 0 ? 0 : -1;
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
            
            if (_selectedItemIndex >= _currentItems.Count)
            {
                _selectedItemIndex = _currentItems.Count > 0 ? 0 : -1;
            }
            
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
        
        #region OnGUI
        
        public void OnGUI()
        {
            if (!_isVisible) return;
            
            try
            {
                SewerSkin.BeginUI();
                
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
                _openAnim = Mathf.MoveTowards(_openAnim, 1f, Time.unscaledDeltaTime * 9f);
                
                HandleDragging();
                Rect drawRect = new Rect(_windowRect.x, _windowRect.y - (1f - _openAnim) * 12f, _windowRect.width, _windowRect.height);
                DrawWindowBackground(drawRect);
                
                GUILayout.BeginArea(new Rect(drawRect.x + 10, drawRect.y + 8, drawRect.width - 20, drawRect.height - 16));
                DrawWindowContent();
                GUILayout.EndArea();
                
                SewerSkin.EndUI();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("ItemSpawnerWindow.OnGUI error", ex);
            }
        }
        
        private void DrawWindowBackground(Rect rect)
        {
            SewerSkin.DrawWindowPanel(rect);
        }
        
        private void HandleDragging()
        {
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
            Rect headerRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.035f, 0.046f, 0.041f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.58f), 6, 1);
            
            // Title
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 6, 150, 20), "ITEM SPAWNER", titleStyle);
            GUI.contentColor = oldContentColor;
            
            Rect closeRect = new Rect(headerRect.x + headerRect.width - 32, headerRect.y + 4, 28, 24);
            if (DrawStyledButton(closeRect, "X", true))
            {
                _isVisible = false;
                _isDragging = false;
            }
            
            GUILayout.Space(6);
            DrawTopBar();
            GUILayout.Space(6);
            
            GUILayout.BeginHorizontal();
            DrawCategorySidebar();
            GUILayout.Space(6);
            DrawItemList();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(6);
            DrawBottomBar();
        }
        
        private void DrawTopBar()
        {
            Rect barRect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(barRect, new Color(0.052f, 0.067f, 0.061f, 0.96f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.45f), 6, 1);
            
            float xPos = barRect.x + 8;
            float yPos = barRect.y + 6;
            
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
            SewerSkin.DrawRoundedRect(qtyRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.55f), 6, 1);
            
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentGlow;
            var qtyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            GUI.Label(qtyRect, _spawnQuantity.ToString(), qtyStyle);
            GUI.contentColor = oldContentColor;
            xPos += 59;
            
            if (DrawStyledButton(new Rect(xPos, yPos, 28, 24), "+", false))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 1);
            }
            xPos += 32;
            
            if (DrawStyledButton(new Rect(xPos, yPos, 38, 24), "+10", false))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 10);
            }
            xPos += 50;
            
            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(xPos, yPos, 45, 24), "Quick:");
            GUI.contentColor = oldContentColor;
            xPos += 48;
            
            foreach (int preset in _quantityPresets)
            {
                bool isSelected = _spawnQuantity == preset;
                if (DrawStyledButton(new Rect(xPos, yPos, 42, 24), preset.ToString(), false, isSelected))
                {
                    _spawnQuantity = preset;
                }
                xPos += 46;
            }
            
            float refreshX = barRect.x + barRect.width - 90;
            if (DrawStyledButton(new Rect(refreshX, yPos, 82, 24), "Refresh", false))
            {
                RefreshData();
            }
        }
        
        private void DrawCategorySidebar()
        {
            GUILayout.BeginVertical(GUILayout.Width(140));
            
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.048f, 0.062f, 0.057f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 5, 120, 16), "CATEGORIES", headerStyle);
            GUI.contentColor = oldContentColor;
            
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(135), GUILayout.ExpandHeight(true));
            SewerSkin.DrawRoundedRect(scrollAreaRect, new Color(0.035f, 0.047f, 0.043f, 0.92f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.38f), 6, 1);
            
            SmoothScroll(ref _targetCategoryScrollPos, ref _displayCategoryScrollPos, ref _categoryScrollPos, scrollAreaRect);
            _categoryScrollPos = GUILayout.BeginScrollView(_displayCategoryScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(135), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < _categories.Count; i++)
            {
                string category = _categories[i];
                bool isSelected = i == _selectedCategoryIndex;
                
                int count = _itemsByCategory.TryGetValue(category, out var items) ? items.Count : 0;
                string label = $"{category} ({count})";
                
                Rect btnRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
                
                oldColor = GUI.color;
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(btnRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.88f), new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.75f), 6, 1);
                }
                else
                {
                    bool isHovered = btnRect.Contains(Event.current.mousePosition);
                    SewerSkin.DrawRoundedRect(btnRect, isHovered ? SewerSkin.ButtonHoverColor : SewerSkin.ButtonColor, new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, isHovered ? 0.62f : 0.35f), 6, 1);
                }
                GUI.color = oldColor;
                
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? new Color(0.02f, 0.04f, 0.06f, 1f) : SewerSkin.TextColor;
                var btnStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(8, 4, 0, 0) };
                GUI.Label(btnRect, label, btnStyle);
                GUI.contentColor = oldContentColor;
                
                if (GUI.Button(btnRect, "", GUIStyle.none))
                {
                    _selectedCategoryIndex = i;
                    _selectedItemIndex = 0;
                    _itemScrollPos = Vector2.zero;
                    _targetItemScrollPos = Vector2.zero;
                    _displayItemScrollPos = Vector2.zero;
                    UpdateCurrentItems();
                }
                
                GUILayout.Space(2);
            }
            
            GUILayout.EndScrollView();
            if (_categoryScrollPos != _displayCategoryScrollPos)
            {
                _targetCategoryScrollPos = _categoryScrollPos;
                _displayCategoryScrollPos = _categoryScrollPos;
            }
            GUILayout.EndVertical();
        }
        
        private void DrawItemList()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.048f, 0.062f, 0.057f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            string categoryName = _selectedCategoryIndex < _categories.Count ? _categories[_selectedCategoryIndex] : "Items";
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 5, 200, 16), categoryName.ToUpper(), headerStyle);
            
            GUI.contentColor = SewerSkin.TextMutedColor;
            var countStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(headerRect.x + headerRect.width - 80, headerRect.y + 5, 70, 16), $"{_currentItems.Count} items", countStyle);
            GUI.contentColor = oldContentColor;
            
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            SewerSkin.DrawRoundedRect(scrollAreaRect, new Color(0.035f, 0.047f, 0.043f, 0.92f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.38f), 6, 1);
            
            SmoothScroll(ref _targetItemScrollPos, ref _displayItemScrollPos, ref _itemScrollPos, scrollAreaRect);
            _itemScrollPos = GUILayout.BeginScrollView(_displayItemScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < _currentItems.Count; i++)
            {
                var item = _currentItems[i];
                bool isSelected = i == _selectedItemIndex;
                
                Rect rowRect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
                
                oldColor = GUI.color;
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(rowRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.14f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.55f), 6, 1);
                }
                else
                {
                    bool isHovered = rowRect.Contains(Event.current.mousePosition);
                    SewerSkin.DrawRoundedRect(rowRect, isHovered ? new Color(0.09f, 0.11f, 0.105f, 0.96f) : new Color(0.055f, 0.069f, 0.065f, 0.74f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, isHovered ? 0.55f : 0.28f), 6, 1);
                }
                
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(new Rect(rowRect.x + 4, rowRect.y + 6, 3, rowRect.height - 12), SewerSkin.AccentColor, Color.clear, 2, 0);
                }
                GUI.color = oldColor;
                
                float xPos = rowRect.x + 10;
                
                Rect nameRect = new Rect(xPos, rowRect.y + 4, rowRect.width - 220, 20);
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? SewerSkin.AccentGlow : SewerSkin.TextColor;
                var nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
                GUI.Label(nameRect, item.Name ?? "???", nameStyle);
                GUI.contentColor = oldContentColor;
                
                if (GUI.Button(new Rect(rowRect.x, rowRect.y, rowRect.width - 70, rowRect.height), "", GUIStyle.none))
                {
                    _selectedItemIndex = i;
                    _selectedItem = item;
                }
                
                float tagX = rowRect.x + rowRect.width - 200;
                oldContentColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                var tagStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
                GUI.Label(new Rect(tagX, rowRect.y + 6, 80, 16), $"[{item.Category ?? "?"}]", tagStyle);
                
                GUI.contentColor = new Color(0.5f, 0.55f, 0.6f, 1f);
                GUI.Label(new Rect(tagX + 82, rowRect.y + 6, 40, 16), $"x{item.StackLimit}", tagStyle);
                GUI.contentColor = oldContentColor;
                
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
            if (_itemScrollPos != _displayItemScrollPos)
            {
                _targetItemScrollPos = _itemScrollPos;
                _displayItemScrollPos = _itemScrollPos;
            }
            GUILayout.EndVertical();
        }

        private void SmoothScroll(ref Vector2 target, ref Vector2 display, ref Vector2 current, Rect hitRect)
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel && hitRect.Contains(e.mousePosition))
            {
                target.y = Mathf.Max(0f, target.y + e.delta.y * 30f);
                e.Use();
            }

            float lerp = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f);
            display = Vector2.Lerp(display, target, lerp);
            current = display;
        }
        
        private void DrawBottomBar()
        {
            Rect lineRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = SewerSkin.AccentColor;
            GUI.DrawTexture(lineRect, _solidTex);
            GUI.color = oldColor;
            
            GUILayout.Space(4);
            
            Rect footerRect = GUILayoutUtility.GetRect(0, 54, GUILayout.ExpandWidth(true));
            oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(footerRect, new Color(0.035f, 0.046f, 0.043f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            float xPos = footerRect.x + 10;
            float yPos = footerRect.y + 10;
            
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

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUI.contentColor = _statusSuccess ? SewerSkin.SuccessColor : SewerSkin.WarningColor;
                var statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
                GUI.Label(new Rect(footerRect.x + 10, footerRect.y + 31, footerRect.width - 170, 18), _statusMessage, statusStyle);
                GUI.contentColor = oldContentColor;
            }
            
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
                SewerSkin.DrawRoundedRect(spawnRect, new Color(0.10f, 0.115f, 0.11f, 0.78f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.35f), 7, 1);
                
                oldContentColor = GUI.contentColor;
                GUI.contentColor = new Color(0.4f, 0.42f, 0.45f, 1f);
                var disabledStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                GUI.Label(spawnRect, "SPAWN ITEM", disabledStyle);
                GUI.contentColor = oldContentColor;
            }
        }
        
        private bool DrawStyledButton(Rect rect, string text, bool isDanger = false, bool isAccent = false, bool isSuccess = false)
        {
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

            return SewerSkin.DrawButtonRect(rect, text, bgColor, hoverColor, textColor, 7, isAccent || isSuccess, 11);
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
                    _statusSuccess = true;
                    _statusMessage = $"Spawned {quantity}x {item.Name}";
                    SewerLogger.Success($"Spawned {quantity}x {item.Name} ({item.Id})");
                }
                else
                {
                    _statusSuccess = false;
                    _statusMessage = GameTypes.LastInventoryError ?? $"Failed to spawn {item.Name}";
                    SewerLogger.Warning($"Failed to spawn {item.Name} ({item.Id})");
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
