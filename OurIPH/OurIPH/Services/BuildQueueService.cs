using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BuildQueueService
    {
        public bool AddOrMerge(IList<BuildQueueItem> queue, BuildQueueItem item)
        {
            if (queue == null || item == null)
            {
                return false;
            }

            var existing = FindMatchingQueueItem(queue, item);
            if (existing == null)
            {
                queue.Add(item);
                return true;
            }

            existing.Blueprint = item.Blueprint;
            existing.Runs += Math.Max(1, item.Runs);
            return false;
        }

        public string FormatStatus(int count)
        {
            return string.Format("Очередь стройки: {0:N0}", Math.Max(0, count));
        }

        public IReadOnlyList<BlueprintSearchResult> SelectProfitableCandidates(IEnumerable<BlueprintSearchResult> blueprints, int maxCount)
        {
            return (blueprints ?? Enumerable.Empty<BlueprintSearchResult>())
                .Where(item => item != null
                               && item.Profit > 0
                               && !string.IsNullOrWhiteSpace(item.EstimateStatus)
                               && item.EstimateStatus.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.ProfitRank <= 0 ? int.MaxValue : item.ProfitRank)
                .ThenByDescending(item => item.SvrTimesIskPerHour)
                .ThenByDescending(item => item.Profit)
                .Take(Math.Max(1, maxCount))
                .ToList();
        }

        private static BuildQueueItem FindMatchingQueueItem(IEnumerable<BuildQueueItem> queue, BuildQueueItem item)
        {
            foreach (var queueItem in queue)
            {
                if (queueItem.ProductTypeId == item.ProductTypeId
                    && queueItem.DecryptorTypeId == item.DecryptorTypeId
                    && Math.Abs(queueItem.MaterialEfficiency - item.MaterialEfficiency) < 0.0001
                    && Math.Abs(queueItem.TimeEfficiency - item.TimeEfficiency) < 0.0001)
                {
                    return queueItem;
                }
            }

            return null;
        }
    }
}
