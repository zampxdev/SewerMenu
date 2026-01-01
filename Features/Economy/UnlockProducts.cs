using System.Collections.Generic;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using Il2CppScheduleOne.Product;

namespace SewerMenu.Features.Economy
{
    /// <summary>
    /// Unlocks/discovers all products for the player.
    /// </summary>
    public class UnlockProducts : FeatureBase
    {
        public override string Id => "unlockproducts";
        public override string Name => "Unlock Products";
        public override string Description => "Discover all products and recipes";
        public override FeatureCategory Category => FeatureCategory.Economy;
        public override bool IsToggleable => false;

        /// <summary>
        /// Gets a list of all available product names.
        /// </summary>
        public List<string> GetAllProducts()
        {
            var products = new List<string>();
            
            SafeExecute(() =>
            {
                var productManager = GameTypes.Products;
                if (productManager == null) return;

                var allProducts = productManager.AllProducts;
                if (allProducts != null)
                {
                    foreach (var product in allProducts)
                    {
                        if (product == null) continue;
                        try
                        {
                            var name = product.Name;
                            if (!string.IsNullOrEmpty(name))
                                products.Add(name);
                        }
                        catch { }
                    }
                }
            }, "getting all products");

            return products;
        }

        /// <summary>
        /// Unlocks all products by discovering them.
        /// </summary>
        public void UnlockAll()
        {
            SafeExecute(() =>
            {
                var productManager = GameTypes.Products;
                if (productManager == null)
                {
                    SewerLogger.Warning("ProductManager not found");
                    return;
                }

                var allProducts = productManager.AllProducts;
                if (allProducts == null)
                {
                    SewerLogger.Warning("No products found in ProductManager");
                    return;
                }

                int count = 0;
                foreach (var product in allProducts)
                {
                    if (product == null) continue;
                    try
                    {
                        // Use DiscoverProduct method with the product ID
                        var productId = product.ID;
                        if (!string.IsNullOrEmpty(productId))
                        {
                            productManager.DiscoverProduct(productId);
                            count++;
                        }
                    }
                    catch { }
                }

                if (count > 0)
                    SewerLogger.Success($"Discovered {count} products!");
                else
                    SewerLogger.Warning("No products were discovered");
                    
            }, "unlocking all products");
        }

        public override void Execute()
        {
            UnlockAll();
        }
    }
}
