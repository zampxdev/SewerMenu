using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Items;
using SewerMenu.UI.Windows;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for item-related features.
    /// </summary>
    public class ItemsPage : PageBase
    {
        public override string Title => "Items";
        public override FeatureCategory Category => FeatureCategory.Items;
        
        private int _spawnAmount = 1;
        private Vector2 _itemScrollPosition;
        private string[] _categories = new string[] { "All" };
        private int _selectedCategoryIndex = 0;
        private bool _categoriesLoaded = false;

        protected override void DrawContent()
        {
            // Item Spawner Section
            DrawSection("ITEM SPAWNER");
            
            // Button to open the full item spawner window
            GUILayout.BeginHorizontal();
            if (SewerSkin.DrawAccentButton("Open Full Item Spawner", 200))
            {
                ItemSpawnerWindow.Instance.Show();
            }
            GUILayout.Space(10);
            var oldColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUILayout.Label("(Dedicated window with all categories)");
            GUI.contentColor = oldColor;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Quick spawner inline
            var spawner = FeatureManager.Instance.GetFeature<ItemSpawner>("itemspawner");
            if (spawner != null)
            {
                // Load categories once
                if (!_categoriesLoaded)
                {
                    var cats = spawner.GetCategories();
                    _categories = new string[cats.Count + 1];
                    _categories[0] = "All";
                    for (int i = 0; i < cats.Count; i++)
                        _categories[i + 1] = cats[i];
                    _categoriesLoaded = true;
                }

                // Category filter row (replaces text search)
                GUILayout.BeginHorizontal();
                GUILayout.Label("Category:", GUILayout.Width(60));
                
                // Category selector with < > buttons
                if (DrawButton("<", 25))
                {
                    _selectedCategoryIndex--;
                    if (_selectedCategoryIndex < 0) _selectedCategoryIndex = _categories.Length - 1;
                    spawner.SearchFilter = _selectedCategoryIndex == 0 ? "" : _categories[_selectedCategoryIndex];
                }
                
                // Show current category in styled box
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(100), GUILayout.Height(24));
                GUI.backgroundColor = oldBg;
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.AccentColor;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_categories[_selectedCategoryIndex]);
                GUILayout.FlexibleSpace();
                GUI.contentColor = oldColor;
                GUILayout.EndHorizontal();
                
                if (DrawButton(">", 25))
                {
                    _selectedCategoryIndex++;
                    if (_selectedCategoryIndex >= _categories.Length) _selectedCategoryIndex = 0;
                    spawner.SearchFilter = _selectedCategoryIndex == 0 ? "" : _categories[_selectedCategoryIndex];
                }
                
                GUILayout.Space(10);
                if (DrawButton("Refresh", 70))
                {
                    spawner.RefreshItemCache();
                    _categoriesLoaded = false;
                    _selectedCategoryIndex = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Item count info
                var filteredItems = spawner.GetFilteredItems();
                var allItems = spawner.GetAllItems();
                
                GUILayout.BeginHorizontal();
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label($"Showing {filteredItems.Count} of {allItems.Count} items");
                GUI.contentColor = oldColor;
                GUILayout.EndHorizontal();

                GUILayout.Space(3);
                
                // Scrollable item list
                SewerSkin.BeginBox();
                _itemScrollPosition = GUILayout.BeginScrollView(_itemScrollPosition, GUILayout.Height(180));
                
                for (int i = 0; i < filteredItems.Count; i++)
                {
                    var item = filteredItems[i];
                    bool isSelected = spawner.SelectedIndex == i;
                    
                    GUILayout.BeginHorizontal();
                    
                    // Selection styling
                    var prevBg = GUI.backgroundColor;
                    var prevContent = GUI.contentColor;
                    
                    if (isSelected)
                    {
                        GUI.backgroundColor = SewerSkin.AccentDark;
                        GUI.contentColor = Color.white;
                    }
                    
                    // Item button
                    if (GUILayout.Button(item.Name, GUILayout.Height(24)))
                    {
                        spawner.SelectedIndex = i;
                    }
                    
                    // Category label on right
                    GUI.contentColor = SewerSkin.TextMutedColor;
                    GUILayout.Label($"[{item.Category}]", GUILayout.Width(90));
                    
                    GUI.backgroundColor = prevBg;
                    GUI.contentColor = prevContent;
                    GUILayout.EndHorizontal();
                }
                
                if (filteredItems.Count == 0)
                {
                    oldColor = GUI.contentColor;
                    GUI.contentColor = SewerSkin.TextMutedColor;
                    GUILayout.Label("No items found. Click 'Refresh' to load items.");
                    GUI.contentColor = oldColor;
                }
                
                GUILayout.EndScrollView();
                SewerSkin.EndBox();
                
                GUILayout.Space(8);
                
                // Selected item info
                if (filteredItems.Count > 0 && spawner.SelectedIndex >= 0 && spawner.SelectedIndex < filteredItems.Count)
                {
                    var selected = filteredItems[spawner.SelectedIndex];
                    SewerSkin.DrawInfoBadge("Selected", $"{selected.Name} (Stack: {selected.StackLimit})");
                }
                
                GUILayout.Space(5);
                
                // Spawn controls - quantity selector with presets
                _spawnAmount = SewerSkin.DrawQuantitySelector("Amount:", _spawnAmount, new int[] { 1, 10, 50, 100, 999 });
                
                GUILayout.Space(5);
                
                // Main spawn button
                if (SewerSkin.DrawAccentButton("SPAWN ITEM", 200))
                {
                    spawner.SpawnAmount = _spawnAmount;
                    spawner.SpawnSelected();
                }
            }
            else
            {
                SewerSkin.DrawStatus("Item Spawner not available", SewerSkin.StatusType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Item Modifiers Section
            DrawSection("ITEM MODIFIERS");
            
            // Stack Size Modifier
            var stackFeature = FeatureManager.Instance.GetFeature<StackSizeModifier>("stacksizemodifier");
            if (stackFeature != null)
            {
                bool stackEnabled = DrawToggle("Stack Size Modifier", stackFeature.IsEnabled, "Increase max stack sizes");
                if (stackEnabled != stackFeature.IsEnabled) stackFeature.IsEnabled = stackEnabled;
                
                if (stackFeature.IsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Multiplier:", GUILayout.Width(70));
                    
                    // Quick multiplier buttons
                    if (DrawButton("x2", 35)) { stackFeature.StackMultiplier = 2; stackFeature.ApplyStackSizes(); }
                    if (DrawButton("x5", 35)) { stackFeature.StackMultiplier = 5; stackFeature.ApplyStackSizes(); }
                    if (DrawButton("x10", 40)) { stackFeature.StackMultiplier = 10; stackFeature.ApplyStackSizes(); }
                    if (DrawButton("x50", 40)) { stackFeature.StackMultiplier = 50; stackFeature.ApplyStackSizes(); }
                    if (DrawButton("MAX", 45)) { stackFeature.SetAllStackSizes(9999); }
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(5);
            
            // Infinite Items
            var infiniteFeature = FeatureManager.Instance.GetFeature<InfiniteItems>("infiniteitems");
            if (infiniteFeature != null)
            {
                bool enabled = DrawToggle("Infinite Items", infiniteFeature.IsEnabled, "Items not consumed when used");
                if (enabled != infiniteFeature.IsEnabled) infiniteFeature.IsEnabled = enabled;
            }
            
            // Quality Override
            var qualityFeature = FeatureManager.Instance.GetFeature<QualityOverride>("qualityoverride");
            if (qualityFeature != null)
            {
                GUILayout.Space(5);
                
                // Show held item info
                var heldInfo = qualityFeature.GetHeldItemInfo();
                DrawInfo("Held Item", heldInfo);
                
                // Quality selector
                var qualities = qualityFeature.GetAvailableQualities();
                if (qualities.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Quality:", GUILayout.Width(55));
                    
                    if (DrawButton("<", 30))
                    {
                        qualityFeature.SelectedQualityIndex--;
                    }
                    
                    GUILayout.Label(qualityFeature.SelectedQualityName, GUILayout.Width(80));
                    
                    if (DrawButton(">", 30))
                    {
                        qualityFeature.SelectedQualityIndex++;
                    }
                    
                    GUILayout.Space(10);
                    
                    if (SewerSkin.DrawAccentButton("APPLY", 70))
                    {
                        qualityFeature.ApplyQualityToHeldItem();
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(10);
            
            // Growing Section
            DrawSection("GROWING");
            
            var growFeature = FeatureManager.Instance.GetFeature<InstantGrow>("instantgrow");
            if (growFeature != null)
            {
                GUILayout.BeginHorizontal();
                if (SewerSkin.DrawAccentButton("Grow All Plants", 140))
                {
                    growFeature.GrowAll();
                }
                GUILayout.Space(10);
                bool autoGrow = DrawToggle("Auto Grow", growFeature.IsEnabled, "Continuously grow plants");
                if (autoGrow != growFeature.IsEnabled) growFeature.IsEnabled = autoGrow;
                GUILayout.EndHorizontal();
            }
        }
    }
}
