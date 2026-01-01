using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.Property;

namespace SewerMenu.Features.World
{
    public class UnlockProperties : FeatureBase
    {
        public override string Id => "unlockproperties";
        public override string Name => "Unlock Properties";
        public override string Description => "Unlock all properties and businesses";
        public override FeatureCategory Category => FeatureCategory.World;
        public override bool IsToggleable => false;

        public class PropertyInfo
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public bool IsOwned { get; set; }
            public float Price { get; set; }
        }

        public List<PropertyInfo> GetAllProperties()
        {
            var properties = new List<PropertyInfo>();

            SafeExecute(() =>
            {
                // Find all Property objects in the scene
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                if (allProperties == null) return;

                foreach (var prop in allProperties)
                {
                    if (prop == null) continue;

                    try
                    {
                        properties.Add(new PropertyInfo
                        {
                            Name = prop.PropertyName ?? "Unknown",
                            Code = prop.PropertyCode ?? "",
                            IsOwned = prop.IsOwned,
                            Price = prop.Price
                        });
                    }
                    catch { }
                }
            }, "getting all properties");

            return properties;
        }

        public int GetOwnedCount()
        {
            int count = 0;
            try
            {
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                if (allProperties == null) return 0;

                foreach (var prop in allProperties)
                {
                    if (prop != null && prop.IsOwned)
                        count++;
                }
            }
            catch { }
            return count;
        }

        public int GetTotalCount()
        {
            try
            {
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                return allProperties?.Length ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public void UnlockAll()
        {
            SafeExecute(() =>
            {
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                if (allProperties == null || allProperties.Length == 0)
                {
                    SewerLogger.Warning("No properties found");
                    return;
                }

                int unlockedCount = 0;
                foreach (var prop in allProperties)
                {
                    if (prop == null) continue;

                    try
                    {
                        if (!prop.IsOwned)
                        {
                            prop.IsOwned = true;
                            unlockedCount++;
                        }
                    }
                    catch { }
                }

                if (unlockedCount > 0)
                    SewerLogger.Success($"Unlocked {unlockedCount} properties!");
                else
                    SewerLogger.Info("All properties already owned");
            }, "unlocking all properties");
        }

        public void UnlockProperty(string propertyName)
        {
            SafeExecute(() =>
            {
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                if (allProperties == null) return;

                foreach (var prop in allProperties)
                {
                    if (prop == null) continue;

                    try
                    {
                        if (prop.PropertyName == propertyName || prop.PropertyCode == propertyName)
                        {
                            prop.IsOwned = true;
                            SewerLogger.Success($"Unlocked property: {prop.PropertyName}");
                            return;
                        }
                    }
                    catch { }
                }

                SewerLogger.Warning($"Property not found: {propertyName}");
            }, "unlocking property");
        }

        public void UnlockPropertyByCode(string code)
        {
            SafeExecute(() =>
            {
                var propertyManager = GameTypes.Properties;
                if (propertyManager == null)
                {
                    SewerLogger.Warning("PropertyManager not found");
                    return;
                }

                try
                {
                    var prop = propertyManager.GetProperty(code);
                    if (prop != null)
                    {
                        prop.IsOwned = true;
                        SewerLogger.Success($"Unlocked property: {prop.PropertyName}");
                    }
                    else
                    {
                        SewerLogger.Warning($"Property not found with code: {code}");
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error($"Failed to unlock property: {code}", ex);
                }
            }, "unlocking property by code");
        }

        public void SetAllPricesToZero()
        {
            SafeExecute(() =>
            {
                var allProperties = UnityEngine.Object.FindObjectsOfType<Property>();
                if (allProperties == null) return;

                int modifiedCount = 0;
                foreach (var prop in allProperties)
                {
                    if (prop == null) continue;

                    try
                    {
                        if (prop.Price > 0)
                        {
                            prop.Price = 0f;
                            modifiedCount++;
                        }
                    }
                    catch { }
                }

                if (modifiedCount > 0)
                    SewerLogger.Success($"Set {modifiedCount} property prices to $0");
            }, "setting property prices to zero");
        }

        public override void Execute()
        {
            UnlockAll();
        }
    }
}
