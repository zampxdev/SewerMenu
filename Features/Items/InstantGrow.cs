using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using Il2CppScheduleOne.Growing;

namespace SewerMenu.Features.Items
{
    public class InstantGrow : FeatureBase
    {
        public override string Id => "instantgrow";
        public override string Name => "Instant Grow";
        public override string Description => "Instantly grow all plants and mushrooms";
        public override FeatureCategory Category => FeatureCategory.Items;
        public override bool IsToggleable => false;

        public void GrowAll()
        {
            SafeExecute(() =>
            {
                int grownCount = 0;

                var plants = Object.FindObjectsOfType<Plant>();
                if (plants != null)
                {
                    foreach (var plant in plants)
                    {
                        if (plant == null) continue;
                        try
                        {
                            if (!plant.IsFullyGrown)
                            {
                                plant.SetNormalizedGrowthProgress(1f);
                                grownCount++;
                            }
                        }
                        catch { }
                    }
                }

                var colonies = Object.FindObjectsOfType<ShroomColony>();
                if (colonies != null)
                {
                    foreach (var colony in colonies)
                    {
                        if (colony == null) continue;
                        try
                        {
                            if (!colony.IsFullyGrown)
                            {
                                colony.SetFullyGrown();
                                grownCount++;
                            }
                        }
                        catch { }
                    }
                }

                if (grownCount > 0)
                    SewerLogger.Success($"Instantly grew {grownCount} plants/colonies!");
                else
                    SewerLogger.Warning("No plants or mushroom colonies found to grow");
                    
            }, "instant growing plants");
        }

        public void GrowPlant(Plant plant)
        {
            if (plant == null) return;

            SafeExecute(() =>
            {
                if (!plant.IsFullyGrown)
                {
                    plant.SetNormalizedGrowthProgress(1f);
                    SewerLogger.Success("Plant fully grown!");
                }
                else
                {
                    SewerLogger.Info("Plant is already fully grown");
                }
            }, "growing plant");
        }

        public void GrowColony(ShroomColony colony)
        {
            if (colony == null) return;

            SafeExecute(() =>
            {
                if (!colony.IsFullyGrown)
                {
                    colony.SetFullyGrown();
                    SewerLogger.Success("Mushroom colony fully grown!");
                }
                else
                {
                    SewerLogger.Info("Colony is already fully grown");
                }
            }, "growing mushroom colony");
        }

        public override void Execute()
        {
            GrowAll();
        }
    }
}
