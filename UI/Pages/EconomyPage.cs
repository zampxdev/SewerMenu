using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Features.Economy;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Page for economy-related features.
    /// </summary>
    public class EconomyPage : PageBase
    {
        public override string Title => "Economy";
        public override FeatureCategory Category => FeatureCategory.Economy;
        
        // Cache values to avoid calling expensive methods every frame
        private float _cachedCash = 0;
        private float _cachedBank = 0;
        private float _cachedNetWorth = 0;
        private int _cachedTier = 0;
        private int _cachedXP = 0;
        private string _cachedRank = "";
        private float _lastCacheTime = 0;
        private const float CacheInterval = 1f;

        protected override void DrawContent()
        {
            RefreshCachedValues();
            
            // Money Section
            DrawSection("MONEY");
            
            var money = FeatureManager.Instance.GetFeature<MoneyEditor>("moneyeditor");
            if (money != null)
            {
                // Display current values with badges
                GUILayout.BeginHorizontal();
                SewerSkin.DrawInfoBadge("Cash", "$" + _cachedCash.ToString("N0"));
                GUILayout.Space(10);
                SewerSkin.DrawInfoBadge("Bank", "$" + _cachedBank.ToString("N0"));
                GUILayout.Space(10);
                SewerSkin.DrawInfoBadge("Net Worth", "$" + _cachedNetWorth.ToString("N0"));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                // SET CASH - button presets only (no text input)
                var oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Set Cash:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                if (DrawButton("$1K", 50)) money.SetCash(1000);
                if (DrawButton("$10K", 55)) money.SetCash(10000);
                if (DrawButton("$50K", 55)) money.SetCash(50000);
                if (DrawButton("$100K", 60)) money.SetCash(100000);
                if (DrawButton("$500K", 60)) money.SetCash(500000);
                if (DrawButton("$1M", 50)) money.SetCash(1000000);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // ADD CASH
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Add Cash:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                if (DrawButton("+$1K", 55)) money.AddCash(1000);
                if (DrawButton("+$5K", 55)) money.AddCash(5000);
                if (DrawButton("+$10K", 60)) money.AddCash(10000);
                if (DrawButton("+$50K", 60)) money.AddCash(50000);
                if (DrawButton("+$100K", 65)) money.AddCash(100000);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                // SET BANK
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Set Bank:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                if (DrawButton("$10K", 55)) money.SetOnlineBalance(10000);
                if (DrawButton("$100K", 60)) money.SetOnlineBalance(100000);
                if (DrawButton("$500K", 60)) money.SetOnlineBalance(500000);
                if (DrawButton("$1M", 50)) money.SetOnlineBalance(1000000);
                if (DrawButton("$5M", 50)) money.SetOnlineBalance(5000000);
                if (DrawButton("$10M", 55)) money.SetOnlineBalance(10000000);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // ADD BANK
                oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Add Bank:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                if (DrawButton("+$10K", 60)) money.AddOnlineBalance(10000);
                if (DrawButton("+$50K", 60)) money.AddOnlineBalance(50000);
                if (DrawButton("+$100K", 65)) money.AddOnlineBalance(100000);
                if (DrawButton("+$500K", 65)) money.AddOnlineBalance(500000);
                if (DrawButton("+$1M", 55)) money.AddOnlineBalance(1000000);
                GUILayout.EndHorizontal();
            }
            else
            {
                SewerSkin.DrawStatus("Money system not found", SewerSkin.StatusType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Level Section
            DrawSection("RANK & XP");
            
            var xp = FeatureManager.Instance.GetFeature<XPEditor>("xpeditor");
            if (xp != null)
            {
                GUILayout.BeginHorizontal();
                SewerSkin.DrawInfoBadge("Rank", _cachedRank);
                GUILayout.Space(10);
                SewerSkin.DrawInfoBadge("Tier", _cachedTier.ToString());
                GUILayout.Space(10);
                SewerSkin.DrawInfoBadge("XP", _cachedXP.ToString("N0"));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(8);
                
                // ADD XP
                var oldColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUILayout.Label("Add XP:");
                GUI.contentColor = oldColor;
                
                GUILayout.BeginHorizontal();
                if (DrawButton("+100", 50)) xp.AddXP(100);
                if (DrawButton("+1K", 50)) xp.AddXP(1000);
                if (DrawButton("+5K", 50)) xp.AddXP(5000);
                if (DrawButton("+10K", 55)) xp.AddXP(10000);
                if (DrawButton("+50K", 55)) xp.AddXP(50000);
                if (DrawButton("+100K", 60)) xp.AddXP(100000);
                GUILayout.EndHorizontal();
            }
            else
            {
                SewerSkin.DrawStatus("Level system not found", SewerSkin.StatusType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Products Section
            DrawSection("PRODUCTS & PURCHASES");
            
            GUILayout.BeginHorizontal();
            var unlock = FeatureManager.Instance.GetFeature<UnlockProducts>("unlockproducts");
            if (unlock != null)
            {
                if (SewerSkin.DrawAccentButton("Unlock All Products", 160))
                {
                    unlock.UnlockAll();
                }
            }
            
            GUILayout.Space(10);
            
            var free = FeatureManager.Instance.GetFeature<FreePurchases>("freepurchases");
            if (free != null)
            {
                bool newVal = DrawToggle("Free Purchases", free.IsEnabled, "All purchases cost $0");
                if (newVal != free.IsEnabled) free.IsEnabled = newVal;
            }
            GUILayout.EndHorizontal();
        }
        
        private void RefreshCachedValues()
        {
            if (Time.time - _lastCacheTime < CacheInterval) return;
            _lastCacheTime = Time.time;
            
            var money = FeatureManager.Instance.GetFeature<MoneyEditor>("moneyeditor");
            if (money != null)
            {
                _cachedCash = money.GetCurrentCash();
                _cachedBank = money.GetOnlineBalance();
                _cachedNetWorth = money.GetNetWorth();
            }
            
            var xp = FeatureManager.Instance.GetFeature<XPEditor>("xpeditor");
            if (xp != null)
            {
                _cachedTier = xp.GetCurrentTier();
                _cachedXP = xp.GetCurrentXP();
                _cachedRank = xp.GetRankName();
            }
        }
    }
}
