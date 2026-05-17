using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class MarketHistoryCacheService
    {
        public Dictionary<long, MarketHistoryStats> MergeCachedStats(
            Dictionary<long, MarketHistoryStats> result,
            IEnumerable<MarketHistoryStats> cache,
            IEnumerable<long> typeIds,
            long regionId,
            int days)
        {
            var merged = result ?? new Dictionary<long, MarketHistoryStats>();
            var ids = new HashSet<long>(typeIds ?? Enumerable.Empty<long>());
            foreach (var cached in (cache ?? Enumerable.Empty<MarketHistoryStats>())
                         .Where(item => ids.Contains(item.TypeId) && item.RegionId == regionId && item.Days == days)
                         .GroupBy(item => item.TypeId)
                         .Select(group => group.OrderByDescending(item => item.UpdatedAt).First()))
            {
                if (!merged.ContainsKey(cached.TypeId) || merged[cached.TypeId].AverageDailyVolume <= 0)
                {
                    merged[cached.TypeId] = cached;
                }
            }

            return merged;
        }

        public void Upsert(ObservableCollection<MarketHistoryStats> cache, IEnumerable<MarketHistoryStats> stats, DateTime? timestamp = null)
        {
            if (cache == null || stats == null)
            {
                return;
            }

            foreach (var item in stats.Where(item => item != null))
            {
                var entry = cache.FirstOrDefault(value =>
                    value.TypeId == item.TypeId && value.RegionId == item.RegionId && value.Days == item.Days);
                if (entry == null)
                {
                    entry = new MarketHistoryStats
                    {
                        TypeId = item.TypeId,
                        RegionId = item.RegionId,
                        Days = item.Days
                    };
                    cache.Add(entry);
                }

                entry.AverageDailyVolume = item.AverageDailyVolume;
                entry.TotalVolume = item.TotalVolume;
                entry.TotalOrders = item.TotalOrders;
                entry.PriceTrend = item.PriceTrend;
                entry.UpdatedAt = item.UpdatedAt == default(DateTime) ? timestamp ?? DateTime.Now : item.UpdatedAt;
            }
        }
    }
}
