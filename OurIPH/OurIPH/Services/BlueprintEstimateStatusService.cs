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
                return "РќРµС‚ РґР°РЅРЅС‹С… invention";
            }

            if (missingPrices > 0)
            {
                return "РќРµС‚ С†РµРЅ: " + missingPrices;
            }

            return estimate.InventionCost > 0 ? "OK + invention" : "OK";
        }

        public string GetProjectItemStatus(BlueprintEstimate estimate, bool hasPriceCache)
        {
            estimate = estimate ?? new BlueprintEstimate();
            if (estimate.InventionMissing)
            {
                return "РќРµС‚ РґР°РЅРЅС‹С… invention";
            }

            if (!hasPriceCache)
            {
                return "РќРµС‚ РєРµС€Р° С†РµРЅ";
            }

            return estimate.InventionCost > 0 ? "OK + invention" : "OK";
        }
    }
}
