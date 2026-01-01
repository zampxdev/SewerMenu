using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Economy
{
    /// <summary>
    /// Allows editing the player's XP and level.
    /// Uses direct IL2CPP access via GameTypes.Level.
    /// 
    /// Key properties:
    /// - XP (get/set) - Current XP within tier
    /// - TotalXP (get/set) - Total accumulated XP
    /// - Tier (get/set) - Current tier within rank
    /// - Rank (get/set) - Current rank (ERank enum)
    /// - XPToNextTier (get) - XP required for next tier
    /// 
    /// Key methods:
    /// - AddXP(int xp) - Adds XP (networked)
    /// - AddXPLocal(int xp) - Adds XP locally
    /// - GetFullRank() - Gets current FullRank struct
    /// </summary>
    public class XPEditor : FeatureBase
    {
        public override string Id => "xpeditor";
        public override string Name => "XP Editor";
        public override string Description => "Edit your XP and level";
        public override FeatureCategory Category => FeatureCategory.Economy;
        public override bool IsToggleable => false;

        public int TargetTier { get; set; } = 10;
        public int TargetXP { get; set; } = 100000;

        /// <summary>
        /// Gets the current tier.
        /// </summary>
        public int GetCurrentTier()
        {
            var level = GameTypes.Level;
            if (level == null) return 0;

            try
            {
                return level.Tier;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the current XP within the tier.
        /// </summary>
        public int GetCurrentXP()
        {
            var level = GameTypes.Level;
            if (level == null) return 0;

            try
            {
                return level.XP;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the total accumulated XP.
        /// </summary>
        public int GetTotalXP()
        {
            var level = GameTypes.Level;
            if (level == null) return 0;

            try
            {
                return level.TotalXP;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the XP required for next tier.
        /// </summary>
        public float GetXPForNextTier()
        {
            var level = GameTypes.Level;
            if (level == null) return 0;

            try
            {
                return level.XPToNextTier;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the current rank name.
        /// </summary>
        public string GetRankName()
        {
            var level = GameTypes.Level;
            if (level == null) return "Unknown";

            try
            {
                return level.Rank.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Sets the player's tier.
        /// </summary>
        public void SetTier(int tier)
        {
            SafeExecute(() =>
            {
                var level = GameTypes.Level;
                if (level == null)
                {
                    SewerLogger.Warning("LevelManager not found");
                    return;
                }

                try
                {
                    level.Tier = tier;
                    SewerLogger.Success($"Set tier to {tier}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set tier", ex);
                }
            }, "setting tier");
        }

        /// <summary>
        /// Sets the player's XP.
        /// </summary>
        public void SetXP(int xp)
        {
            SafeExecute(() =>
            {
                var level = GameTypes.Level;
                if (level == null)
                {
                    SewerLogger.Warning("LevelManager not found");
                    return;
                }

                try
                {
                    level.XP = xp;
                    SewerLogger.Success($"Set XP to {xp:N0}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set XP", ex);
                }
            }, "setting XP");
        }

        /// <summary>
        /// Sets the player's total XP.
        /// </summary>
        public void SetTotalXP(int totalXP)
        {
            SafeExecute(() =>
            {
                var level = GameTypes.Level;
                if (level == null)
                {
                    SewerLogger.Warning("LevelManager not found");
                    return;
                }

                try
                {
                    level.TotalXP = totalXP;
                    SewerLogger.Success($"Set total XP to {totalXP:N0}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set total XP", ex);
                }
            }, "setting total XP");
        }

        /// <summary>
        /// Adds XP to the player.
        /// </summary>
        public void AddXP(int amount)
        {
            SafeExecute(() =>
            {
                var level = GameTypes.Level;
                if (level == null)
                {
                    SewerLogger.Warning("LevelManager not found");
                    return;
                }

                try
                {
                    level.AddXP(amount);
                    SewerLogger.Success($"Added {amount:N0} XP");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to add XP", ex);
                }
            }, "adding XP");
        }

        /// <summary>
        /// Increases tier by one.
        /// </summary>
        public void IncreaseTier()
        {
            SafeExecute(() =>
            {
                var level = GameTypes.Level;
                if (level == null)
                {
                    SewerLogger.Warning("LevelManager not found");
                    return;
                }

                try
                {
                    level.IncreaseTier();
                    SewerLogger.Success($"Increased tier to {level.Tier}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to increase tier", ex);
                }
            }, "increasing tier");
        }

        public override void Execute()
        {
            SetTier(TargetTier);
            SetXP(TargetXP);
        }
    }
}
