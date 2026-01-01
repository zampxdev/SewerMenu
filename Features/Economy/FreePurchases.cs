using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Economy
{
    /// <summary>
    /// [NOT IMPLEMENTED] Would make all purchases free.
    /// Requires Harmony patches to intercept purchase/transaction methods.
    /// </summary>
    public class FreePurchases : FeatureBase
    {
        public override string Id => "freepurchases";
        public override string Name => "Free Purchases (N/I)";
        public override string Description => "Not implemented - requires Harmony patches";
        public override FeatureCategory Category => FeatureCategory.Economy;
        
        // This feature cannot be implemented without Harmony patches
        // to intercept MoneyManager.ChangeCashBalance() or shop purchase methods.
        // Marking as non-toggleable action that shows a warning.
        public override bool IsToggleable => false;

        public override void Execute()
        {
            SewerLogger.Warning("Free Purchases is not implemented - requires Harmony patches to intercept transactions");
        }
    }
}
