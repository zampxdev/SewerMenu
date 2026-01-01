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
                // Clamp to screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
                
                // Handle dragging manually
                HandleDragging();
                
                // Draw window background
                GUI.Box(_windowRect, "");
                
                // Draw content inside the rect
                GUILayout.BeginArea(_windowRect);
                DrawWindowContent();
                GUILayout.EndArea();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("ItemSpawnerWindow.OnGUI error", ex);
            }
        }
        
        private void HandleDragging()
        {
            // Title bar rect for dragging - exclude the close button area (last 40 pixels)
            Rect titleBar = new Rect(_windowRect.x, _windowRect.y, _windowRect.width - 40, 25);
            
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
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(28));
            GUI.backgroundColor = oldBg;
            
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label("◆ ITEM SPAWNER", GUILayout.ExpandWidth(true));
            GUI.contentColor = oldColor;
            
            // Close button
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.6f, 0.15f, 0.15f, 1f);
            if (GUILayout.Button("✕", GUILayout.Width(28), GUILayout.Height(22)))
            {
                _isVisible = false;
                _isDragging = false;
            }
            GUI.backgroundColor = oldBg;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(4);
            
            // ═══════════════════════════════════════════════════════════
            // QUANTITY BAR
            // ═══════════════════════════════════════════════════════════
            DrawTopBar();
            
            GUILayout.Space(4);
            
            // ═══════════════════════════════════════════════════════════
            // MAIN CONTENT
            // ═══════════════════════════════════════════════════════════
            GUILayout.BeginHorizontal();
            
            // Left sidebar - categories
            DrawCategorySidebar();
            
            GUILayout.Space(4);
            
            // Right panel - items
            DrawItemList();
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(4);
            
            // ═══════════════════════════════════════════════════════════
            // FOOTER
            // ═══════════════════════════════════════════════════════════
            DrawBottomBar();
        }
        
        private void DrawTopBar()
        {
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.14f, 0.14f, 0.16f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(32));
            GUI.backgroundColor = oldBg;
            
            // Quantity label
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Quantity:", GUILayout.Width(60));
            GUI.contentColor = oldColor;
            
            // -10 button
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            if (GUILayout.Button("-10", GUILayout.Width(38), GUILayout.Height(24)))
            {
                _spawnQuantity = Mathf.Max(1, _spawnQuantity - 10);
            }
            
            if (GUILayout.Button("-", GUILayout.Width(28), GUILayout.Height(24)))
            {
                _spawnQuantity = Mathf.Max(1, _spawnQuantity - 1);
            }
            GUI.backgroundColor = oldBg;
            
            // Quantity display
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(60), GUILayout.Height(24));
            GUI.backgroundColor = oldBg;
            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.FlexibleSpace();
            GUILayout.Label(_spawnQuantity.ToString());
            GUILayout.FlexibleSpace();
            GUI.contentColor = oldColor;
            GUILayout.EndHorizontal();
            
            // + buttons
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            if (GUILayout.Button("+", GUILayout.Width(28), GUILayout.Height(24)))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 1);
            }
            
            if (GUILayout.Button("+10", GUILayout.Width(38), GUILayout.Height(24)))
            {
                _spawnQuantity = Mathf.Min(9999, _spawnQuantity + 10);
            }
            GUI.backgroundColor = oldBg;
            
            GUILayout.Space(15);
            
            // Quick presets
            oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("Quick:", GUILayout.Width(40));
            GUI.contentColor = oldColor;
            
            foreach (int preset in _quantityPresets)
            {
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = _spawnQuantity == preset 
                    ? SewerSkin.AccentDark 
                    : new Color(0.22f, 0.22f, 0.26f, 1f);
                if (GUILayout.Button(preset.ToString(), GUILayout.Width(42), GUILayout.Height(24)))
                {
                    _spawnQuantity = preset;
                }
                GUI.backgroundColor = oldBg;
            }
            
            GUILayout.FlexibleSpace();
            
            // Refresh button
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.4f, 0.5f, 1f);
            if (GUILayout.Button("↻ Refresh", GUILayout.Width(80), GUILayout.Height(24)))
            {
                RefreshData();
            }
            GUI.backgroundColor = oldBg;
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawCategorySidebar()
        {
            GUILayout.BeginVertical(GUILayout.Width(130));
            
            // Header
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(24));
            GUI.backgroundColor = oldBg;
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("CATEGORIES");
            GUI.contentColor = oldColor;
            GUILayout.EndHorizontal();
            
            // Scrollable list
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            _categoryScrollPos = GUILayout.BeginScrollView(_categoryScrollPos, GUI.skin.box, GUILayout.Width(125), GUILayout.ExpandHeight(true));
            GUI.backgroundColor = oldBg;
            
            for (int i = 0; i < _categories.Count; i++)
            {
                string category = _categories[i];
                bool isSelected = i == _selectedCategoryIndex;
                
                int count = _itemsByCategory.TryGetValue(category, out var items) ? items.Count : 0;
                string label = $"{category} ({count})";
                
                oldBg = GUI.backgroundColor;
                oldColor = GUI.contentColor;
                
                if (isSelected)
                {
                    GUI.backgroundColor = SewerSkin.AccentColor;
                    GUI.contentColor = Color.black;
                }
                else
                {
                    GUI.backgroundColor = new Color(0.18f, 0.18f, 0.22f, 1f);
                }
                
                if (GUILayout.Button(label, GUILayout.Height(24)))
                {
                    _selectedCategoryIndex = i;
                    _selectedItemIndex = 0;
                    _itemScrollPos = Vector2.zero;
                    UpdateCurrentItems();
                }
                
                GUI.backgroundColor = oldBg;
                GUI.contentColor = oldColor;
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }
        
        private void DrawItemList()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            // Header
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(24));
            GUI.backgroundColor = oldBg;
            
            string categoryName = _selectedCategoryIndex < _categories.Count ? _categories[_selectedCategoryIndex] : "Items";
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            GUILayout.Label(categoryName.ToUpper());
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_currentItems.Count} items");
            GUI.contentColor = oldColor;
            GUILayout.EndHorizontal();
            
            // Item list
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            _itemScrollPos = GUILayout.BeginScrollView(_itemScrollPos, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUI.backgroundColor = oldBg;
            
            for (int i = 0; i < _currentItems.Count; i++)
            {
                var item = _currentItems[i];
                bool isSelected = i == _selectedItemIndex;
                
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = isSelected 
                    ? new Color(0.0f, 0.5f, 0.6f, 0.5f) 
                    : new Color(0.16f, 0.16f, 0.18f, 0.5f);
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(26));
                GUI.backgroundColor = oldBg;
                
                // Item name button
                oldBg = GUI.backgroundColor;
                oldColor = GUI.contentColor;
                
                if (isSelected)
                {
                    GUI.backgroundColor = SewerSkin.AccentDark;
                    GUI.contentColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = new Color(0.22f, 0.22f, 0.26f, 1f);
                }
                
                if (GUILayout.Button(item.Name ?? "???", GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                {
                    _selectedItemIndex = i;
                    _selectedItem = item;
                }
                
                GUI.backgroundColor = oldBg;
                GUI.contentColor = oldColor;
                
                // Category tag
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label($"[{item.Category ?? "?"}]", GUILayout.Width(85));
                
                // Stack limit
                GUI.contentColor = new Color(0.6f, 0.6f, 0.65f, 1f);
                GUILayout.Label($"x{item.StackLimit}", GUILayout.Width(40));
                GUI.contentColor = oldColor;
                
                // Quick spawn button
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.2f, 0.5f, 0.3f, 1f);
                if (GUILayout.Button("+ Add", GUILayout.Width(50), GUILayout.Height(22)))
                {
                    SpawnItem(item, _spawnQuantity);
                }
                GUI.backgroundColor = oldBg;
                
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
            }
            
            if (_currentItems.Count == 0)
            {
                GUILayout.Space(20);
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("No items found.\n\nTry:\n• Select a different category\n• Click 'Refresh' to reload items");
                GUI.contentColor = oldColor;
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }
        
        private void DrawBottomBar()
        {
            // Accent line
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = SewerSkin.AccentColor;
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            GUI.backgroundColor = oldBg;
            
            GUILayout.Space(4);
            
            // Footer bar
            oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(36));
            GUI.backgroundColor = oldBg;
            
            // Selected item info
            if (_selectedItem != null)
            {
                var oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Selected:", GUILayout.Width(55));
                GUI.contentColor = SewerSkin.AccentColor;
                GUILayout.Label(_selectedItem.Name, GUILayout.Width(180));
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label($"ID: {_selectedItem.Id}", GUILayout.Width(120));
                GUI.contentColor = oldColor;
            }
            else
            {
                var oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Select an item from the list above");
                GUI.contentColor = oldColor;
            }
            
            GUILayout.FlexibleSpace();
            
            // Total items count
            int totalItems = _itemsByCategory.TryGetValue("All", out var all) ? all.Count : 0;
            var mutedColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label($"{totalItems} items total", GUILayout.Width(90));
            GUI.contentColor = mutedColor;
            
            GUILayout.Space(10);
            
            // Main spawn button
            if (_selectedItem != null)
            {
                oldBg = GUI.backgroundColor;
                var oldColor = GUI.contentColor;
                GUI.backgroundColor = SewerSkin.AccentColor;
                GUI.contentColor = Color.black;
                
                string btnText = _spawnQuantity > 1 
                    ? $"SPAWN {_spawnQuantity}x" 
                    : "SPAWN ITEM";
                    
                if (GUILayout.Button(btnText, GUILayout.Width(140), GUILayout.Height(28)))
                {
                    SpawnItem(_selectedItem, _spawnQuantity);
                }
                GUI.backgroundColor = oldBg;
                GUI.contentColor = oldColor;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("SPAWN ITEM", GUILayout.Width(140), GUILayout.Height(28));
                GUI.enabled = true;
            }
            
            GUILayout.EndHorizontal();
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
