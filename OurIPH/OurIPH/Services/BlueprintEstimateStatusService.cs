using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateStatusService
    {
        public string GetBlueprintStatus(BlueprintEstimate estimate, int missingPrices)
        {
            estimate = estimate ?? new BlueprintEstimate();
            if (estimate.InventionMissing)
            {
                return "Нет данных invention";
            }

            if (missingPrices > 0)
            {
                return "Нет цен: " + missingPrices;
            }

            return estimate.InventionCost > 0 ? "OK + invention" : "OK";
        }

        public string GetProjectItemStatus(BlueprintEstimate estimate, bool hasPriceCache)
        {
            estimate = estimate ?? new BlueprintEstimate();
            if (estimate.InventionMissing)
            {
                return "Нет данных invention";
            }

            if (!hasPriceCache)
            {
                return "Нет кеша цен";
            }

            return estimate.InventionCost > 0 ? "OK + invention" : "OK";
        }
    }
}
