using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Items
{
    /// <summary>
    /// [NOT IMPLEMENTED] Would prevent items from being consumed when used.
    /// Requires Harmony patches to intercept item consumption methods.
    /// </summary>
    public class InfiniteItems : FeatureBase
    {
        public override string Id => "infiniteitems";
        public override string Name => "Infinite Items (N/I)";
        public override string Description => "Not implemented - requires Harmony patches";
        public override FeatureCategory Category => FeatureCategory.Items;
        
        // This feature cannot be implemented without Harmony patches
        // to intercept ItemInstance.ChangeQuantity() or similar methods.
        // Marking as non-toggleable action that shows a warning.
        public override bool IsToggleable => false;

        public override void Execute()
        {
            SewerLogger.Warning("Infinite Items is not implemented - requires Harmony patches to intercept item consumption");
        }
    }
}
