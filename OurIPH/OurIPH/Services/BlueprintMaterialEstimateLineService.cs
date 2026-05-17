using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintMaterialEstimateLineService
    {
        public MaterialEstimateLine CreateLine(MaterialRequirement material, int runs, double materialMultiplier, MarketPrice price,
            bool buildWhenMarketVolumeShort, Func<MarketPrice, double> getUnitPrice, Func<MarketPrice, long> getAvailableVolume)
        {
            var adjustedQuantity = BlueprintMaterialMathService.CalculateAdjustedQuantity(material == null ? 0 : material.Quantity, runs, materialMultiplier);
            var unitPrice = price == null || getUnitPrice == null ? 0 : getUnitPrice(price);
            var buyCost = unitPrice > 0 ? adjustedQuantity * unitPrice : 0;
            var availableVolume = price == null || getAvailableVolume == null ? 0 : getAvailableVolume(price);

            return new MaterialEstimateLine
            {
                Material = material,
                AdjustedQuantity = adjustedQuantity,
                UnitPrice = unitPrice,
                BuyCost = buyCost,
                MarketVolumeShort = buildWhenMarketVolumeShort && price != null && availableVolume > 0 && availableVolume < adjustedQuantity
            };
        }
    }
}
