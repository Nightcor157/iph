using System.Collections.Generic;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class SalesVolumeRatioService
    {
        public void ApplyToBlueprint(BlueprintSearchResult blueprint, Dictionary<long, MarketHistoryStats> marketHistory, long producedQuantity, double productionTimeSeconds)
        {
            if (blueprint == null)
            {
                return;
            }

            blueprint.SalesVolumeRatio = 0;
            blueprint.SvrTimesIskPerHour = 0;
            blueprint.TotalItemsSold = 0;
            blueprint.TotalOrdersFilled = 0;
            blueprint.AverageItemsPerOrder = 0;
            blueprint.PriceTrend = 0;
            if (marketHistory == null || productionTimeSeconds <= 0 || producedQuantity <= 0)
            {
                return;
            }

            MarketHistoryStats stats;
            if (!marketHistory.TryGetValue(blueprint.ProductTypeId, out stats))
            {
                return;
            }

            blueprint.TotalItemsSold = stats.TotalVolume;
            blueprint.TotalOrdersFilled = stats.TotalOrders;
            blueprint.AverageItemsPerOrder = stats.TotalOrders > 0 ? stats.TotalVolume / (double)stats.TotalOrders : 0;
            blueprint.PriceTrend = stats.PriceTrend;
            if (stats.AverageDailyVolume <= 0)
            {
                return;
            }

            var itemsPerDay = (24 * 60 * 60) / (productionTimeSeconds / producedQuantity);
            if (itemsPerDay <= 0)
            {
                return;
            }

            blueprint.SalesVolumeRatio = stats.AverageDailyVolume / itemsPerDay;
            blueprint.SvrTimesIskPerHour = blueprint.SalesVolumeRatio * blueprint.IskPerHour;
        }

        public void ApplyToProjectItem(BuildProjectItem item, Dictionary<long, MarketHistoryStats> marketHistory)
        {
            if (item == null)
            {
                return;
            }

            item.SalesVolumeRatio = 0;
            item.SvrTimesIskPerHour = 0;
            item.TotalItemsSold = 0;
            item.TotalOrdersFilled = 0;
            item.AverageItemsPerOrder = 0;
            item.PriceTrend = 0;
            if (marketHistory == null || item.ProductionTimeSeconds <= 0 || item.TotalQuantity <= 0)
            {
                return;
            }

            MarketHistoryStats stats;
            if (!marketHistory.TryGetValue(item.ProductTypeId, out stats) || stats.AverageDailyVolume <= 0)
            {
                return;
            }

            item.TotalItemsSold = stats.TotalVolume;
            item.TotalOrdersFilled = stats.TotalOrders;
            item.AverageItemsPerOrder = stats.TotalOrders > 0 ? stats.TotalVolume / (double)stats.TotalOrders : 0;
            item.PriceTrend = stats.PriceTrend;

            var itemsPerDay = (24 * 60 * 60) / (item.ProductionTimeSeconds / item.TotalQuantity);
            if (itemsPerDay <= 0)
            {
                return;
            }

            item.SalesVolumeRatio = stats.AverageDailyVolume / itemsPerDay;
            var iskPerHour = item.ProductionTimeSeconds > 0 ? item.Profit / item.ProductionTimeSeconds * 3600.0 : 0;
            item.SvrTimesIskPerHour = item.SalesVolumeRatio * iskPerHour;
        }
    }
}
