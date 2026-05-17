using System;
using System.Collections.Generic;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateMaterialOrchestrationService
    {
        private readonly BlueprintEstimateMaterialTraversalService _materialTraversalService;

        public BlueprintEstimateMaterialOrchestrationService()
            : this(new BlueprintEstimateMaterialTraversalService())
        {
        }

        public BlueprintEstimateMaterialOrchestrationService(BlueprintEstimateMaterialTraversalService materialTraversalService)
        {
            _materialTraversalService = materialTraversalService ?? new BlueprintEstimateMaterialTraversalService();
        }

        public BlueprintEstimateContext CreateContext(
            BlueprintSearchResult blueprint,
            IReadOnlyList<MaterialRequirement> materials,
            FacilityStation station,
            FacilityPreset facilityPreset,
            IReadOnlyDictionary<long, MarketPrice> prices,
            IReadOnlyDictionary<long, double> adjustedPrices,
            int runs,
            double materialEfficiency,
            double timeEfficiency,
            double materialMultiplier,
            DecryptorOption decryptor,
            BlueprintEstimateRecursionState recursionState)
        {
            return new BlueprintEstimateContext
            {
                Blueprint = blueprint,
                Materials = materials,
                Station = station,
                FacilityPreset = facilityPreset,
                Prices = prices,
                AdjustedPrices = adjustedPrices,
                Runs = runs,
                MaterialEfficiency = materialEfficiency,
                TimeEfficiency = timeEfficiency,
                MaterialMultiplier = materialMultiplier,
                Decryptor = decryptor,
                RecursionState = recursionState
            };
        }

        public BlueprintEstimateDependencies CreateDependencies(
            Func<long, BlueprintSearchResult> findBlueprintByProduct,
            Func<long, bool> isMineral,
            Func<BlueprintSearchResult, bool> shouldAlwaysBuy,
            Func<BlueprintSearchResult, BlueprintSearchResult, bool> shouldStopReactionDrilldown,
            Func<BlueprintSearchResult, FacilityPreset, DecryptorOption, Tuple<double, double>> getDefaultChildEfficiency,
            Func<ChildBlueprintEstimateRequest, FacilityStation> getCheapestChildStation,
            Func<ChildBlueprintEstimateRequest, FacilityStation, BlueprintEstimate> calculateChildEstimate,
            Func<MarketPrice, double> getMaterialUnitPrice,
            Func<MarketPrice, double> getProductUnitPrice,
            Func<MarketPrice, long> getAvailableMarketVolume,
            Func<double, FacilityPreset, FacilityStation, double> applySalesTaxesAndFees)
        {
            return new BlueprintEstimateDependencies
            {
                FindBlueprintByProduct = findBlueprintByProduct,
                IsMineral = isMineral,
                ShouldAlwaysBuy = shouldAlwaysBuy,
                ShouldStopReactionDrilldown = shouldStopReactionDrilldown,
                GetDefaultChildEfficiency = getDefaultChildEfficiency,
                GetCheapestChildStation = getCheapestChildStation,
                CalculateChildEstimate = calculateChildEstimate,
                GetMaterialUnitPrice = getMaterialUnitPrice,
                GetProductUnitPrice = getProductUnitPrice,
                GetAvailableMarketVolume = getAvailableMarketVolume,
                ApplySalesTaxesAndFees = applySalesTaxesAndFees
            };
        }

        public BlueprintEstimateMaterialOrchestrationResult TraverseMaterials(
            BlueprintEstimateContext context,
            BlueprintEstimateDependencies dependencies,
            Func<Dictionary<long, long>, double> getMineralPurchaseCost)
        {
            var traversal = _materialTraversalService.Traverse(context, dependencies);
            var mineralCost = getMineralPurchaseCost == null ? 0 : getMineralPurchaseCost(traversal.MineralBuyQuantities);

            return new BlueprintEstimateMaterialOrchestrationResult
            {
                TraversalResult = traversal,
                MaterialCost = traversal.MaterialCost + mineralCost,
                EstimatedInputValue = traversal.EstimatedInputValue,
                BuildMaterialLines = traversal.BuildMaterialLines,
                BuyMaterialLines = traversal.BuyMaterialLines,
                ComponentProductionTimes = traversal.ComponentProductionTimes
            };
        }
    }
}
