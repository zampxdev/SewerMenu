using System;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.Features.Misc
{
    public class InfiniteAmmo : FeatureBase
    {
        public override string Id => "infiniteammo";
        public override string Name => "Infinite Ammo";
        public override string Description => "Keeps ranged weapon ammo full";
        public override FeatureCategory Category => FeatureCategory.Misc;

        public int MinimumAmmo { get; set; } = 999;

        private Equippable_RangedWeapon[] _cachedWeapons = Array.Empty<Equippable_RangedWeapon>();
        private float _nextWeaponRefresh;
        private float _nextVisibleWeaponRefill;

        private const float WeaponCacheRefreshInterval = 1.25f;
        private const float VisibleWeaponRefillInterval = 0.45f;

        public override void OnEnable()
        {
            _nextWeaponRefresh = 0f;
            _nextVisibleWeaponRefill = 0f;
            RefillEquippedItem();
            RefillVisibleWeapons();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                float now = Time.unscaledTime;

                RefillEquippedItem();

                if (now >= _nextWeaponRefresh)
                {
                    RefreshWeaponCache();
                    _nextWeaponRefresh = now + WeaponCacheRefreshInterval;
                }

                if (now >= _nextVisibleWeaponRefill)
                {
                    RefillVisibleWeapons();
                    _nextVisibleWeaponRefill = now + VisibleWeaponRefillInterval;
                }
            }, "refilling weapon ammo");
        }

        private void RefreshWeaponCache()
        {
            try
            {
                _cachedWeapons = UnityEngine.Object.FindObjectsOfType<Equippable_RangedWeapon>()
                    ?? Array.Empty<Equippable_RangedWeapon>();
            }
            catch
            {
                _cachedWeapons = Array.Empty<Equippable_RangedWeapon>();
            }
        }

        private void RefillVisibleWeapons()
        {
            if (_cachedWeapons == null || _cachedWeapons.Length == 0) return;

            foreach (var weapon in _cachedWeapons)
            {
                RefillWeapon(weapon, MinimumAmmo);
            }
        }

        private void RefillEquippedItem()
        {
            try
            {
                var player = GameTypes.LocalPlayer;
                if (player == null) return;

                var equipped = player.GetEquippedItem();
                if (equipped == null) return;

                var equippable = equipped.Equippable;
                if (equippable == null) return;

                var weapon = equippable.TryCast<Equippable_RangedWeapon>();
                RefillWeapon(weapon, MinimumAmmo);
            }
            catch { }
        }

        private static void RefillWeapon(Equippable_RangedWeapon weapon, int minimumAmmo)
        {
            if (weapon == null) return;

            try
            {
                var targetAmmo = Mathf.Max(minimumAmmo, weapon.MagazineSize);
                var weaponItem = weapon.weaponItem;
                RefillIntegerItem(weaponItem, targetAmmo);
            }
            catch { }
        }

        private static void RefillIntegerItem(IntegerItemInstance item, int targetAmmo)
        {
            if (item == null) return;

            try
            {
                if (item.Value < targetAmmo)
                {
                    item.SetValue(targetAmmo);
                }
            }
            catch
            {
                try { item.Value = targetAmmo; } catch { }
            }

            try
            {
                if (item.Quantity < 1)
                {
                    item.SetQuantity(1);
                }
            }
            catch { }
        }
    }
}
