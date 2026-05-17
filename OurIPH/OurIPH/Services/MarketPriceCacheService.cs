using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class MarketPriceCacheService
    {
        public Dictionary<long, MarketPrice> GetCachedPrices(IEnumerable<MarketPriceCacheEntry> cache, long regionOrStationId, IEnumerable<long> typeIds)
        {
            var ids = new HashSet<long>(typeIds ?? Enumerable.Empty<long>());
            return (cache ?? Enumerable.Empty<MarketPriceCacheEntry>())
                .Where(item => item.RegionOrStationId == regionOrStationId && ids.Contains(item.TypeId))
                .GroupBy(item => item.TypeId)
                .ToDictionary(group => group.Key, group =>
                {
                    var latest = group.OrderByDescending(item => item.UpdatedAt).First();
                    return new MarketPrice
                    {
                        TypeId = latest.TypeId,
                        SellMin = latest.SellMin,
                        BuyMax = latest.BuyMax,
                        SellVolume = latest.SellVolume,
                        BuyVolume = latest.BuyVolume
                    };
                });
        }

        public void Upsert(
            ObservableCollection<MarketPriceCacheEntry> cache,
            long regionOrStationId,
            string locationName,
            Dictionary<long, MarketPrice> prices,
            IDictionary<long, string> names,
            DateTime? timestamp = null)
        {
            if (cache == null || prices == null || prices.Count == 0)
            {
                return;
            }

            var updatedAt = timestamp ?? DateTime.Now;
            foreach (var item in prices.Values)
            {
                var entry = cache.FirstOrDefault(value => value.RegionOrStationId == regionOrStationId && value.TypeId == item.TypeId);
                if (entry == null)
                {
                    entry = new MarketPriceCacheEntry
                    {
                        RegionOrStationId = regionOrStationId,
                        TypeId = item.TypeId
                    };
                    cache.Add(entry);
                }

                string typeName;
                entry.LocationName = locationName;
                entry.TypeName = names != null && names.TryGetValue(item.TypeId, out typeName) ? typeName : entry.TypeName ?? "";
                entry.SellMin = item.SellMin;
                entry.BuyMax = item.BuyMax;
                entry.SellVolume = item.SellVolume;
                entry.BuyVolume = item.BuyVolume;
                entry.UpdatedAt = updatedAt;
            }
        }
    }
}
