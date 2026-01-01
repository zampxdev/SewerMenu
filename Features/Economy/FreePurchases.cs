using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Economy
{
    public class FreePurchases : FeatureBase
    {
        public override string Id => "freepurchases";
        public override string Name => "Free Purchases (N/I)";
        public override string Description => "Not implemented - requires Harmony patches";
        public override FeatureCategory Category => FeatureCategory.Economy;
        
        public override bool IsToggleable => false;

        public override void Execute()
        {
            SewerLogger.Warning("Free Purchases is not implemented - requires Harmony patches to intercept transactions");
        }
    }
}
