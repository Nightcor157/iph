using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintListFilterService
    {
        public bool Passes(BlueprintSearchResult blueprint, BlueprintListFilterOptions options)
        {
            if (blueprint == null)
            {
                return false;
            }

            options = options ?? new BlueprintListFilterOptions();
            if (options.MatchesExplicitSearch != null && options.MatchesExplicitSearch(blueprint))
            {
                return true;
            }

            if (options.ProfitableOnly && blueprint.Profit <= 0)
            {
                return false;
            }

            if (options.HideMissingPrices
                && !string.IsNullOrWhiteSpace(blueprint.EstimateStatus)
                && !blueprint.EstimateStatus.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (options.HideLowVolume
                && blueprint.ProducedQuantity > 0
                && blueprint.ProductMarketVolume < blueprint.ProducedQuantity)
            {
                return false;
            }

            if (!options.ShowExcluded
                && options.ExcludedProductIds != null
                && options.ExcludedProductIds.Contains(blueprint.ProductTypeId))
            {
                return false;
            }

            if (options.AllowedByTypeFilter != null && !options.AllowedByTypeFilter(blueprint))
            {
                return false;
            }

            if (options.HideBySkills
                && options.HasRequiredSkills != null
                && !options.HasRequiredSkills(blueprint))
            {
                return false;
            }

            if (options.AllowedByBuildProfile != null && !options.AllowedByBuildProfile(blueprint))
            {
                return false;
            }

            if (options.MinSalesVolumeRatio > 0 && blueprint.SalesVolumeRatio < options.MinSalesVolumeRatio)
            {
                return false;
            }

            if (string.Equals(options.TrendFilter, "Up", StringComparison.OrdinalIgnoreCase) && blueprint.PriceTrend < 0)
            {
                return false;
            }

            if (string.Equals(options.TrendFilter, "Down", StringComparison.OrdinalIgnoreCase) && blueprint.PriceTrend > 0)
            {
                return false;
            }

            if (options.MinSold > 0 && blueprint.TotalItemsSold < options.MinSold)
            {
                return false;
            }

            if (options.MinOrders > 0 && blueprint.TotalOrdersFilled < options.MinOrders)
            {
                return false;
            }

            if (options.TopOnly && options.TopCount > 0 && !string.IsNullOrWhiteSpace(blueprint.EstimateStatus))
            {
                return blueprint.ProfitRank > 0 && blueprint.ProfitRank <= options.TopCount;
            }

            if (options.MinIskPerHour > 0 && blueprint.IskPerHour < options.MinIskPerHour)
            {
                return false;
            }

            return options.MinRoi <= 0 || blueprint.ReturnOnInvestmentPercent >= options.MinRoi;
        }
    }
}
