using System;
using System.Collections.Generic;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateMaterialTraversalService
    {
        private readonly BlueprintEstimateTraversalService _traversalService;
        private readonly BlueprintMaterialEstimateLineService _lineService;
        private readonly MaterialBuildBuyDecisionService _buildBuyDecisionService;

        public BlueprintEstimateMaterialTraversalService()
            : this(new BlueprintEstimateTraversalService(), new BlueprintMaterialEstimateLineService(), new MaterialBuildBuyDecisionService())
        {
        }

        public BlueprintEstimateMaterialTraversalService(
            BlueprintEstimateTraversalService traversalService,
            BlueprintMaterialEstimateLineService lineService,
            MaterialBuildBuyDecisionService buildBuyDecisionService)
        {
            _traversalService = traversalService ?? new BlueprintEstimateTraversalService();
            _lineService = lineService ?? new BlueprintMaterialEstimateLineService();
            _buildBuyDecisionService = buildBuyDecisionService ?? new MaterialBuildBuyDecisionService();
        }

        public BlueprintMaterialTraversalResult Traverse(BlueprintEstimateContext context, BlueprintEstimateDependencies dependencies)
        {
            var result = new BlueprintMaterialTraversalResult();
            if (context == null || context.Materials == null)
            {
                return result;
            }

            dependencies = dependencies ?? new BlueprintEstimateDependencies();
            var prices = context.Prices ?? new Dictionary<long, MarketPrice>();
            var adjustedPrices = context.AdjustedPrices ?? new Dictionary<long, double>();
            var state = context.RecursionState ?? new BlueprintEstimateRecursionState();
            var buildWhenMarketVolumeShort = context.FacilityPreset != null && context.FacilityPreset.BuildWhenMarketVolumeShort;

            foreach (var material in context.Materials)
            {
                MarketPrice price;
                prices.TryGetValue(material.TypeId, out price);
                var line = _lineService.CreateLine(
                    material,
                    context.Runs,
                    context.MaterialMultiplier,
                    price,
                    buildWhenMarketVolumeShort,
                    dependencies.GetMaterialUnitPrice,
                    dependencies.GetAvailableMarketVolume);
                result.MaterialLines.Add(line);

                var childCost = double.MaxValue;
                var childBlueprint = _traversalService.ShouldLookupChildBlueprint(state, material.TypeId)
                    ? Invoke(dependencies.FindBlueprintByProduct, material.TypeId)
                    : null;

                if (childBlueprint == null && Invoke(dependencies.IsMineral, material.TypeId))
                {
                    AddMineralBuyQuantity(result.MineralBuyQuantities, material.TypeId, line.AdjustedQuantity);
                    result.BuyMaterialLines++;
                    AddEstimatedInputValue(result, adjustedPrices, material, context.Runs);
                    line.IsMineralFallback = true;
                    continue;
                }

                if (childBlueprint != null
                    && !Invoke(dependencies.ShouldAlwaysBuy, childBlueprint)
                    && !Invoke(dependencies.ShouldStopReactionDrilldown, context.Blueprint, childBlueprint))
                {
                    var efficiency = Invoke(dependencies.GetDefaultChildEfficiency, childBlueprint, context.FacilityPreset, context.Decryptor)
                                     ?? Tuple.Create(0.0, 0.0);
                    var childRequest = _traversalService.CreateChildRequest(
                        context.Blueprint,
                        childBlueprint,
                        material,
                        line.AdjustedQuantity,
                        efficiency.Item1,
                        efficiency.Item2,
                        context.Decryptor);
                    line.ChildRequest = childRequest;

                    var childStation = Invoke(dependencies.GetCheapestChildStation, childRequest);
                    if (childStation != null && childStation.SupportsProduction)
                    {
                        _traversalService.EnterParent(state, context.Blueprint);
                        try
                        {
                            var childEstimate = Invoke(dependencies.CalculateChildEstimate, childRequest, childStation);
                            if (childEstimate != null)
                            {
                                childCost = childEstimate.MaterialCost + childEstimate.InventionCost + childEstimate.InstallationCost;
                                result.ComponentProductionTimes.Add(childEstimate.ProductionTimeSeconds);
                                childCost = _traversalService.ApplySurplusOffset(
                                    childCost,
                                    childBlueprint,
                                    childRequest.ChildRuns,
                                    line.AdjustedQuantity,
                                    typeId =>
                                    {
                                        MarketPrice surplusPrice;
                                        return prices.TryGetValue(typeId, out surplusPrice) && dependencies.GetProductUnitPrice != null
                                            ? dependencies.GetProductUnitPrice(surplusPrice)
                                            : 0;
                                    },
                                    grossValue => dependencies.ApplySalesTaxesAndFees == null
                                        ? grossValue
                                        : dependencies.ApplySalesTaxesAndFees(grossValue, context.FacilityPreset, childStation));
                            }
                        }
                        finally
                        {
                            _traversalService.ExitParent(state, context.Blueprint);
                        }
                    }
                }

                var decision = _buildBuyDecisionService.Decide(new MaterialBuildBuyDecisionContext
                {
                    BuyCost = line.BuyCost,
                    BuildCost = childCost,
                    MarketVolumeShort = line.MarketVolumeShort
                });
                line.BuildBuyDecision = decision;

                result.MaterialCost += decision.SelectedCost;
                if (decision.ShouldBuild)
                {
                    result.BuildMaterialLines++;
                }
                else
                {
                    result.BuyMaterialLines++;
                }

                AddEstimatedInputValue(result, adjustedPrices, material, context.Runs);
            }

            return result;
        }

        private static void AddMineralBuyQuantity(Dictionary<long, long> quantities, long typeId, long quantity)
        {
            long existing;
            quantities.TryGetValue(typeId, out existing);
            quantities[typeId] = existing + quantity;
        }

        private static void AddEstimatedInputValue(BlueprintMaterialTraversalResult result, IReadOnlyDictionary<long, double> adjustedPrices, MaterialRequirement material, int runs)
        {
            double adjustedPrice;
            if (adjustedPrices.TryGetValue(material.TypeId, out adjustedPrice) && adjustedPrice > 0)
            {
                result.EstimatedInputValue += material.Quantity * runs * adjustedPrice;
            }
        }

        private static TResult Invoke<T, TResult>(Func<T, TResult> func, T value)
        {
            return func == null ? default(TResult) : func(value);
        }

        private static TResult Invoke<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 value1, T2 value2)
        {
            return func == null ? default(TResult) : func(value1, value2);
        }

        private static TResult Invoke<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 value1, T2 value2, T3 value3)
        {
            return func == null ? default(TResult) : func(value1, value2, value3);
        }
    }
}
