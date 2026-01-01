using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Items
{
    public class InfiniteItems : FeatureBase
    {
        public override string Id => "infiniteitems";
        public override string Name => "Infinite Items (N/I)";
        public override string Description => "Not implemented - requires Harmony patches";
        public override FeatureCategory Category => FeatureCategory.Items;
        
        public override bool IsToggleable => false;

        public override void Execute()
        {
            SewerLogger.Warning("Infinite Items is not implemented - requires Harmony patches to intercept item consumption");
        }
    }
}
