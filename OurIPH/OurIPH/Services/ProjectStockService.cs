using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public static class ProjectStockService
    {
        public static void DistributeByWave(IEnumerable<BuildProjectMaterial> lines, IDictionary<long, long> stockByTypeId)
        {
            if (lines == null)
            {
                return;
            }

            var stock = stockByTypeId ?? new Dictionary<long, long>();
            foreach (var group in lines.Where(item => item != null).GroupBy(item => item.TypeId))
            {
                long ownedQuantity;
                stock.TryGetValue(group.Key, out ownedQuantity);
                var consumedBefore = 0L;
                foreach (var line in group.OrderBy(item => item.Wave).ThenBy(item => item.SourceGroupText).ThenBy(item => item.Name))
                {
                    line.PriorQuantity = consumedBefore;
                    line.OwnedQuantity = Math.Max(0, Math.Min(line.Quantity, ownedQuantity - consumedBefore));
                    line.TotalCost = line.RemainingToBuy * line.UnitPrice;
                    consumedBefore += line.Quantity;
                }
            }
        }

        public static long GetTotalOwnedThroughLine(BuildProjectMaterial line, long lineOwnedQuantity)
        {
            if (line == null)
            {
                return 0;
            }

            return Math.Max(0, line.PriorQuantity) + Math.Min(Math.Max(0, line.Quantity), Math.Max(0, lineOwnedQuantity));
        }
    }
}
