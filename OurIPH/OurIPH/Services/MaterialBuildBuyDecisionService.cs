using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class MaterialBuildBuyDecisionService
    {
        public MaterialBuildBuyDecisionResult Decide(MaterialBuildBuyDecisionContext context)
        {
            context = context ?? new MaterialBuildBuyDecisionContext();
            var hasBuildCost = context.BuildCost < double.MaxValue;
            var shouldBuild = (context.MarketVolumeShort && hasBuildCost)
                || context.BuildCost < context.BuyCost
                || (context.BuyCost <= 0 && hasBuildCost);

            return new MaterialBuildBuyDecisionResult
            {
                ShouldBuild = shouldBuild,
                SelectedCost = shouldBuild ? context.BuildCost : Math.Max(0, context.BuyCost)
            };
        }
    }
}
