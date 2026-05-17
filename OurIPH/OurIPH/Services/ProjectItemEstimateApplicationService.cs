using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class ProjectItemEstimateApplicationService
    {
        public void ResetUnavailable(BuildProjectItem item, string status)
        {
            if (item == null)
            {
                return;
            }

            item.BestStationName = "";
            item.BestStationSystem = "";
            item.MaterialCost = 0;
            item.InstallationCost = 0;
            item.InventionCost = 0;
            item.InventionChancePercent = 0;
            item.InventionJobs = 0;
            item.InventedRunsPerSuccess = 0;
            item.InventionMaterialsCost = 0;
            item.InventionCopyCost = 0;
            item.InventionJobUsageCost = 0;
            item.InventionStationName = "";
            item.InventionStationSystem = "";
            item.CopyStationName = "";
            item.CopyStationSystem = "";
            item.ProductionTimeSeconds = 0;
            item.Revenue = 0;
            item.Profit = 0;
            item.ReturnOnInvestmentPercent = 0;
            item.ProductMarketVolume = 0;
            item.SalesVolumeRatio = 0;
            item.SvrTimesIskPerHour = 0;
            item.TotalItemsSold = 0;
            item.TotalOrdersFilled = 0;
            item.AverageItemsPerOrder = 0;
            item.PriceTrend = 0;
            item.BuildMaterialLines = 0;
            item.BuyMaterialLines = 0;
            item.EstimateStatus = status;
        }

        public void Apply(BuildProjectItem item, BlueprintEstimate estimate, double revenue, ContractPriceResult productPriceResult,
            long productMarketVolume, string status, FacilityStation station)
        {
            if (item == null)
            {
                return;
            }

            estimate = estimate ?? new BlueprintEstimate();
            productPriceResult = productPriceResult ?? ContractPriceResult.Empty("");

            item.BestStationName = station?.Name;
            item.BestStationSystem = station?.SystemName;
            item.MaterialCost = estimate.MaterialCost;
            item.InstallationCost = estimate.InstallationCost;
            item.InventionCost = estimate.InventionCost;
            item.InventionChancePercent = estimate.InventionChancePercent;
            item.InventionJobs = estimate.InventionJobs;
            item.InventedRunsPerSuccess = estimate.InventedRunsPerSuccess;
            item.InventionMaterialsCost = estimate.InventionMaterialsCost;
            item.InventionCopyCost = estimate.InventionCopyCost;
            item.InventionJobUsageCost = estimate.InventionJobUsageCost;
            item.InventionStationName = estimate.InventionStationName;
            item.InventionStationSystem = estimate.InventionStationSystem;
            item.CopyStationName = estimate.CopyStationName;
            item.CopyStationSystem = estimate.CopyStationSystem;
            item.ProductionTimeSeconds = estimate.ProductionTimeSeconds;
            item.Revenue = revenue;
            item.ContractUnitPrice = productPriceResult.ContractUnitPrice;
            item.ProductPriceSource = productPriceResult.Source;
            item.ProductPriceDetails = productPriceResult.Detail;

            var totalBuildCost = item.MaterialCost + item.InventionCost + item.InstallationCost;
            var profitability = BlueprintProfitabilityService.Calculate(totalBuildCost, item.Revenue, item.ProductionTimeSeconds);
            item.Profit = profitability.Profit;
            item.ReturnOnInvestmentPercent = profitability.ReturnOnInvestmentPercent;
            item.ProductMarketVolume = productMarketVolume;
            item.BuildMaterialLines = estimate.BuildMaterialLines;
            item.BuyMaterialLines = estimate.BuyMaterialLines;
            item.EstimateStatus = status;
        }
    }
}
