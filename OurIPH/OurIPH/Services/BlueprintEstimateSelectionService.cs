using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateSelectionService
    {
        public IReadOnlyList<FacilityStation> GetSupportedStationCandidates(FacilityPreset facilityPreset, string productionType)
        {
            if (facilityPreset == null)
            {
                return new List<FacilityStation>();
            }

            var candidates = facilityPreset.Stations
                .Where(station => station.ProductionType == productionType && station.SupportsProduction)
                .ToList();
            return candidates.Count > 0
                ? candidates
                : facilityPreset.Stations.Where(station => station.SupportsProduction).ToList();
        }

        public FacilityStation SelectCheapestStation(IEnumerable<BlueprintStationEstimateCandidate> candidates)
        {
            return (candidates ?? Enumerable.Empty<BlueprintStationEstimateCandidate>())
                .Where(item => item != null && item.Station != null && item.Estimate != null)
                .OrderBy(item => item.Estimate.MaterialCost + item.Estimate.InventionCost + item.Estimate.InstallationCost)
                .ThenBy(item => item.Estimate.ProductionTimeSeconds)
                .Select(item => item.Station)
                .FirstOrDefault();
        }

        public DecryptorOption SelectBestDecryptor(IEnumerable<BlueprintDecryptorEstimateCandidate> candidates, DecryptorOption fallback)
        {
            var best = (candidates ?? Enumerable.Empty<BlueprintDecryptorEstimateCandidate>())
                .Where(item => item != null && item.Decryptor != null)
                .OrderByDescending(item => item.Profit)
                .FirstOrDefault();

            return best == null ? fallback : best.Decryptor;
        }
    }
}
