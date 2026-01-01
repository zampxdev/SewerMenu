using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.ItemFramework;
using System.Collections.Generic;

namespace SewerMenu.Features.Items
{
    /// <summary>
    /// Modifies the maximum stack size for items.
    /// Uses direct IL2CPP access via GameTypes.GetAllItemDefinitions().
    /// 
    /// ItemDefinition.StackLimit property controls max stack size.
    /// </summary>
    public class StackSizeModifier : FeatureBase
    {
        public override string Id => "stacksizemodifier";
        public override string Name => "Stack Size Modifier";
        public override string Description => "Increase maximum stack sizes";
        public override FeatureCategory Category => FeatureCategory.Items;

        public int StackMultiplier { get; set; } = 10;
        public int MaxStackSize { get; set; } = 9999;

        // Store original values for restoration
        private static Dictionary<string, int> _originalStackSizes = new Dictionary<string, int>();
        private static bool _hasStoredOriginals = false;

        public override void OnEnable()
        {
            ApplyStackSizes();
            SewerLogger.Debug($"StackSizeModifier enabled - multiplier: {StackMultiplier}x, max: {MaxStackSize}");
        }

        public override void OnDisable()
        {
            RestoreStackSizes();
            SewerLogger.Debug("StackSizeModifier disabled - restored original stack sizes");
        }

        /// <summary>
        /// Applies the stack size modifications to all items.
        /// </summary>
        public void ApplyStackSizes()
        {
            SafeExecute(() =>
            {
                var items = GameTypes.GetAllItemDefinitions();
                if (items == null || items.Count == 0)
                {
                    SewerLogger.Warning("No items found to modify stack sizes");
                    return;
                }

                int modifiedCount = 0;
                foreach (var item in items)
                {
                    if (item == null) continue;

                    try
                    {
                        string id = item.ID;
                        if (string.IsNullOrEmpty(id)) continue;

                        int currentSize = item.StackLimit;
                        
                        // Store original if not already stored
                        if (!_originalStackSizes.ContainsKey(id))
                        {
                            _originalStackSizes[id] = currentSize;
                        }

                        // Calculate new size
                        int originalSize = _originalStackSizes[id];
                        int newSize = Mathf.Min(originalSize * StackMultiplier, MaxStackSize);
                        
                        // Apply if different
                        if (item.StackLimit != newSize)
                        {
                            item.StackLimit = newSize;
                            modifiedCount++;
                        }
                    }
                    catch { }
                }

                _hasStoredOriginals = true;

                if (modifiedCount > 0)
                    SewerLogger.Success($"Modified stack sizes for {modifiedCount} items (x{StackMultiplier}, max {MaxStackSize})");
            }, "applying stack sizes");
        }

        /// <summary>
        /// Restores original stack sizes.
        /// </summary>
        public void RestoreStackSizes()
        {
            if (!_hasStoredOriginals || _originalStackSizes.Count == 0)
                return;

            SafeExecute(() =>
            {
                var items = GameTypes.GetAllItemDefinitions();
                if (items == null) return;

                int restoredCount = 0;
                foreach (var item in items)
                {
                    if (item == null) continue;

                    try
                    {
                        string id = item.ID;
                        if (!string.IsNullOrEmpty(id) && _originalStackSizes.TryGetValue(id, out int originalSize))
                        {
                            if (item.StackLimit != originalSize)
                            {
                                item.StackLimit = originalSize;
                                restoredCount++;
                            }
                        }
                    }
                    catch { }
                }

                if (restoredCount > 0)
                    SewerLogger.Debug($"Restored stack sizes for {restoredCount} items");
            }, "restoring stack sizes");
        }

        /// <summary>
        /// Sets all items to a specific stack size.
        /// </summary>
        public void SetAllStackSizes(int size)
        {
            SafeExecute(() =>
            {
                var items = GameTypes.GetAllItemDefinitions();
                if (items == null) return;

                int modifiedCount = 0;
                foreach (var item in items)
                {
                    if (item == null) continue;

                    try
                    {
                        string id = item.ID;
                        if (string.IsNullOrEmpty(id)) continue;

                        // Store original if not already stored
                        if (!_originalStackSizes.ContainsKey(id))
                        {
                            _originalStackSizes[id] = item.StackLimit;
                        }

                        item.StackLimit = size;
                        modifiedCount++;
                    }
                    catch { }
                }

                _hasStoredOriginals = true;
                SewerLogger.Success($"Set stack size to {size} for {modifiedCount} items");
            }, "setting all stack sizes");
        }
    }
}
