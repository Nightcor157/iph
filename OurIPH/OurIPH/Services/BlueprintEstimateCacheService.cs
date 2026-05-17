using System.Collections.Generic;
using System.Globalization;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateCacheService
    {
        public string CreateKey(BlueprintSearchResult blueprint, int runs, double materialEfficiency, double timeEfficiency,
            DecryptorOption decryptor, FacilityStation station)
        {
            var blueprintTypeId = blueprint == null ? 0 : blueprint.BlueprintTypeId;
            var decryptorTypeId = decryptor == null ? 0 : decryptor.TypeId;
            var stationName = station == null ? "" : station.Name;
            return blueprintTypeId + ":" + runs + ":" +
                materialEfficiency.ToString("0.###", CultureInfo.InvariantCulture) + ":" +
                timeEfficiency.ToString("0.###", CultureInfo.InvariantCulture) + ":" +
                decryptorTypeId + ":" + stationName;
        }

        public bool TryGet(Dictionary<string, BlueprintEstimate> cache, string key, out BlueprintEstimate estimate)
        {
            estimate = null;
            if (cache == null || !cache.TryGetValue(key, out var cachedEstimate))
            {
                return false;
            }

            estimate = cachedEstimate == null ? null : cachedEstimate.Clone();
            return true;
        }

        public void Store(Dictionary<string, BlueprintEstimate> cache, string key, BlueprintEstimate estimate)
        {
            if (cache == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            cache[key] = estimate == null ? null : estimate.Clone();
        }
    }
}
