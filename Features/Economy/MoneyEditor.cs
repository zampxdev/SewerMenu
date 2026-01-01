using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Economy
{
    public class MoneyEditor : FeatureBase
    {
        public override string Id => "moneyeditor";
        public override string Name => "Money Editor";
        public override string Description => "Edit your cash and bank balance";
        public override FeatureCategory Category => FeatureCategory.Economy;
        public override bool IsToggleable => false;

        public float TargetCash { get; set; } = 100000f;
        public float TargetOnlineBalance { get; set; } = 1000000f;

        public float GetCurrentCash()
        {
            var money = GameTypes.Money;
            if (money == null) return 0f;

            try
            {
                return money.cashBalance;
            }
            catch
            {
                return 0f;
            }
        }

        public float GetOnlineBalance()
        {
            var money = GameTypes.Money;
            if (money == null) return 0f;

            try
            {
                return money.onlineBalance;
            }
            catch
            {
                return 0f;
            }
        }

        // cashBalance is read-only, so we use ChangeCashBalance() to modify
        public void SetCash(float amount)
        {
            SafeExecute(() =>
            {
                var money = GameTypes.Money;
                if (money == null)
                {
                    SewerLogger.Warning("MoneyManager not found");
                    return;
                }

                try
                {
                    float currentCash = money.cashBalance;
                    float difference = amount - currentCash;
                    
                    if (difference != 0)
                    {
                        // ChangeCashBalance(float change, bool visualizeChange, bool playCashSound)
                        money.ChangeCashBalance(difference, true, true);
                        SewerLogger.Success($"Set cash to ${amount:N0} (changed by ${difference:N0})");
                    }
                    else
                    {
                        SewerLogger.Info($"Cash already at ${amount:N0}");
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set cash", ex);
                }
            }, "setting cash");
        }

        public void SetOnlineBalance(float amount)
        {
            SafeExecute(() =>
            {
                var money = GameTypes.Money;
                if (money == null)
                {
                    SewerLogger.Warning("MoneyManager not found");
                    return;
                }

                try
                {
                    float currentBalance = money.onlineBalance;
                    float difference = amount - currentBalance;
                    
                    if (Mathf.Abs(difference) > 0.01f)
                    {
                        // CreateOnlineTransaction provides proper network sync
                        try
                        {
                            money.CreateOnlineTransaction("Deposit", difference, 1f, "SewerMenu");
                            SewerLogger.Success($"Set online balance to ${amount:N0}");
                            return;
                        }
                        catch { }
                        
                        // Fallback: direct setter
                        money.onlineBalance = amount;
                        SewerLogger.Success($"Set online balance to ${amount:N0}");
                    }
                    else
                    {
                        SewerLogger.Info($"Online balance already at ${amount:N0}");
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set online balance", ex);
                }
            }, "setting online balance");
        }

        public void AddCash(float amount)
        {
            SafeExecute(() =>
            {
                var money = GameTypes.Money;
                if (money == null) return;

                try
                {
                    money.ChangeCashBalance(amount, true, true);
                    SewerLogger.Success($"Added ${amount:N0} cash");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to add cash", ex);
                }
            }, "adding cash");
        }

        public void AddOnlineBalance(float amount)
        {
            SafeExecute(() =>
            {
                var money = GameTypes.Money;
                if (money == null) return;

                try
                {
                    // CreateOnlineTransaction provides proper network sync
                    try
                    {
                        money.CreateOnlineTransaction("Deposit", amount, 1f, "SewerMenu");
                        SewerLogger.Success($"Added ${amount:N0} to online balance");
                        return;
                    }
                    catch { }
                    
                    // Fallback: direct addition
                    money.onlineBalance += amount;
                    SewerLogger.Success($"Added ${amount:N0} to online balance");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to add online balance", ex);
                }
            }, "adding online balance");
        }

        public float GetNetWorth()
        {
            var money = GameTypes.Money;
            if (money == null) return 0f;

            try
            {
                return money.GetNetWorth();
            }
            catch
            {
                return 0f;
            }
        }

        public float GetLifetimeEarnings()
        {
            var money = GameTypes.Money;
            if (money == null) return 0f;

            try
            {
                return money.LifetimeEarnings;
            }
            catch
            {
                return 0f;
            }
        }

        public override void Execute()
        {
            SetCash(TargetCash);
            SetOnlineBalance(TargetOnlineBalance);
        }
    }
}
