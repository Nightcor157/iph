using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateApplicationService
    {
        public void Apply(BlueprintSearchResult blueprint, BlueprintEstimate estimate, double totalCost, double revenue, string status, FacilityStation station)
        {
            if (blueprint == null)
            {
                return;
            }

            estimate = estimate ?? new BlueprintEstimate();
            blueprint.MaterialCost = estimate.MaterialCost;
            blueprint.InstallationCost = estimate.InstallationCost;
            blueprint.InventionCost = estimate.InventionCost;

            var profitability = BlueprintProfitabilityService.Calculate(totalCost, revenue, estimate.ProductionTimeSeconds);
            blueprint.TotalCost = profitability.TotalCost;
            blueprint.Revenue = profitability.Revenue;
            blueprint.Profit = profitability.Profit;
            blueprint.IskPerHour = profitability.IskPerHour;
            blueprint.MarginPercent = profitability.MarginPercent;
            blueprint.ReturnOnInvestmentPercent = profitability.ReturnOnInvestmentPercent;

            blueprint.FacilityMaterialBonusPercent = estimate.FacilityMaterialBonusPercent;
            blueprint.CostIndexPercent = estimate.CostIndexPercent;
            blueprint.ProductionTimeSeconds = estimate.ProductionTimeSeconds;
            blueprint.InventionChancePercent = estimate.InventionChancePercent;
            blueprint.InventionJobs = estimate.InventionJobs;
            blueprint.InventedRunsPerSuccess = estimate.InventedRunsPerSuccess;
            blueprint.InventionMaterialsCost = estimate.InventionMaterialsCost;
            blueprint.InventionCopyCost = estimate.InventionCopyCost;
            blueprint.InventionJobUsageCost = estimate.InventionJobUsageCost;
            blueprint.InventionStationName = estimate.InventionStationName;
            blueprint.InventionStationSystem = estimate.InventionStationSystem;
            blueprint.CopyStationName = estimate.CopyStationName;
            blueprint.CopyStationSystem = estimate.CopyStationSystem;

            blueprint.ProducedQuantity = 0;
            blueprint.ProductMarketVolume = 0;
            blueprint.ContractUnitPrice = 0;
            blueprint.ProductPriceSource = "";
            blueprint.SalesVolumeRatio = 0;
            blueprint.SvrTimesIskPerHour = 0;
            blueprint.TotalItemsSold = 0;
            blueprint.TotalOrdersFilled = 0;
            blueprint.AverageItemsPerOrder = 0;
            blueprint.PriceTrend = 0;
            blueprint.CurrentBuyOrders = 0;
            blueprint.CurrentSellOrders = 0;

            blueprint.BuildMaterialLines = estimate.BuildMaterialLines;
            blueprint.BuyMaterialLines = estimate.BuyMaterialLines;
            blueprint.EstimateStatus = status;
            blueprint.BestFacilityName = station?.Name;
            blueprint.BestFacilitySystem = station?.SystemName;
        }

        public void Apply(BlueprintSearchResult blueprint, BlueprintEstimate estimate, double totalCost, double revenue,
            ContractPriceResult productPriceResult, long producedQuantity, long productMarketVolume, string status, FacilityStation station)
        {
            Apply(blueprint, estimate, totalCost, revenue, status, station);

            if (blueprint == null)
            {
                return;
            }

            productPriceResult = productPriceResult ?? ContractPriceResult.Empty("");
            blueprint.ProducedQuantity = producedQuantity;
            blueprint.ProductMarketVolume = productMarketVolume;
            blueprint.ContractUnitPrice = productPriceResult.ContractUnitPrice;
            blueprint.ProductPriceSource = productPriceResult.Source;
            blueprint.ProductPriceDetails = productPriceResult.Detail;
        }
    }
}
