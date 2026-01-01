using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.Features.Items
{
    public class ItemSpawner : FeatureBase
    {
        public override string Id => "itemspawner";
        public override string Name => "Item Spawner";
        public override string Description => "Spawn any item";
        public override FeatureCategory Category => FeatureCategory.Items;
        public override bool IsToggleable => false;

        public int SelectedIndex { get; set; } = 0;
        public int SpawnAmount { get; set; } = 1;
        public string SearchFilter { get; set; } = "";
        
        private List<ItemInfo> _allItems = new List<ItemInfo>();
        private List<ItemInfo> _filteredItems = new List<ItemInfo>();
        private bool _itemsCached = false;
        private string _lastFilter = "";

        public class ItemInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public int StackLimit { get; set; }
            public ItemDefinition Definition { get; set; }
        }

        public List<ItemInfo> GetAllItems()
        {
            if (_itemsCached && _allItems.Count > 0)
                return _allItems;

            RefreshItemCache();
            return _allItems;
        }

        public List<ItemInfo> GetFilteredItems()
        {
            if (!_itemsCached)
                RefreshItemCache();

            if (_lastFilter != SearchFilter)
            {
                _lastFilter = SearchFilter;
                if (string.IsNullOrWhiteSpace(SearchFilter))
                {
                    _filteredItems = new List<ItemInfo>(_allItems);
                }
                else
                {
                    string filter = SearchFilter.ToLower();
                    _filteredItems = _allItems
                        .Where(i => i.Name.ToLower().Contains(filter) || 
                                   i.Id.ToLower().Contains(filter) ||
                                   i.Category.ToLower().Contains(filter))
                        .ToList();
                }
                
                if (SelectedIndex >= _filteredItems.Count)
                    SelectedIndex = 0;
            }

            return _filteredItems;
        }

        public void RefreshItemCache()
        {
            _allItems.Clear();
            _filteredItems.Clear();
            _itemsCached = false;
            _lastFilter = "";

            try
            {
                var definitions = GameTypes.GetAllItemDefinitions();
                
                foreach (var def in definitions)
                {
                    if (def == null) continue;
                    
                    try
                    {
                        string id = def.ID ?? "";
                        string name = def.Name ?? id;
                        string category = "Unknown";
                        int stackLimit = 1;
                        
                        try { category = def.Category.ToString(); } catch { }
                        try { stackLimit = def.StackLimit; } catch { }
                        
                        // Skip empty IDs
                        if (string.IsNullOrEmpty(id)) continue;
                        
                        _allItems.Add(new ItemInfo
                        {
                            Id = id,
                            Name = name,
                            Category = category,
                            StackLimit = stackLimit,
                            Definition = def
                        });
                    }
                    catch { }
                }

                // Sort by category then name
                _allItems = _allItems
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Name)
                    .ToList();

                _filteredItems = new List<ItemInfo>(_allItems);
                _itemsCached = true;
                
                SewerLogger.Info($"ItemSpawner: Cached {_allItems.Count} items");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("ItemSpawner: Failed to cache items", ex);
            }
        }

        public void SpawnSelected()
        {
            var items = GetFilteredItems();
            if (items.Count == 0 || SelectedIndex < 0 || SelectedIndex >= items.Count)
            {
                SewerLogger.Warning("No item selected");
                return;
            }

            var item = items[SelectedIndex];
            SpawnItem(item, SpawnAmount);
        }

        public void SpawnItem(ItemInfo item, int amount = 1)
        {
            if (item == null || item.Definition == null)
            {
                SewerLogger.Warning("Invalid item");
                return;
            }

            SafeExecute(() =>
            {
                int spawnAmount = Mathf.Clamp(amount, 1, 9999);
                
                bool success = GameTypes.AddItemToInventory(item.Definition, spawnAmount);
                
                if (success)
                {
                    SewerLogger.Success($"Spawned {spawnAmount}x {item.Name}");
                }
                else
                {
                    SewerLogger.Warning($"Failed to spawn {item.Name}");
                }
            }, "spawning item");
        }

        public void SpawnItemById(string itemId, int amount = 1)
        {
            var def = GameTypes.GetItemById(itemId);
            if (def == null)
            {
                SewerLogger.Warning($"Item not found: {itemId}");
                return;
            }

            SafeExecute(() =>
            {
                bool success = GameTypes.AddItemToInventory(def, amount);
                if (success)
                {
                    SewerLogger.Success($"Spawned {amount}x {def.Name}");
                }
                else
                {
                    SewerLogger.Warning($"Failed to spawn {def.Name}");
                }
            }, "spawning item by ID");
        }

        public List<string> GetCategories()
        {
            if (!_itemsCached)
                RefreshItemCache();

            return _allItems
                .Select(i => i.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public override void Execute()
        {
            SpawnSelected();
        }
    }
}
