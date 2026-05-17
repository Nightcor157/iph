using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class OrePlanningService
    {
        public List<OrePlanCandidate> BuildCandidates(
            Dictionary<long, List<ReprocessingOption>> oreOutputs,
            IReadOnlyCollection<long> requiredMineralTypeIds,
            Func<ReprocessingOption, double> getYield,
            Func<ReprocessingOption, double> getOreUnitPrice)
        {
            var required = new HashSet<long>(requiredMineralTypeIds ?? new long[0]);
            var candidates = new List<OrePlanCandidate>();
            if (oreOutputs == null || required.Count == 0 || getYield == null || getOreUnitPrice == null)
            {
                return candidates;
            }

            foreach (var group in oreOutputs)
            {
                var first = group.Value == null ? null : group.Value.FirstOrDefault();
                if (first == null)
                {
                    continue;
                }

                var yield = getYield(first);
                var outputByMineral = group.Value
                    .Where(option => required.Contains(option.MineralTypeId))
                    .Select(option => new
                    {
                        option.MineralTypeId,
                        Quantity = GetReprocessedMineralOutput(option, yield)
                    })
                    .Where(option => option.Quantity > 0)
                    .GroupBy(option => option.MineralTypeId)
                    .ToDictionary(output => output.Key, output => output.Sum(item => item.Quantity));
                if (outputByMineral.Count == 0)
                {
                    continue;
                }

                candidates.Add(new OrePlanCandidate
                {
                    OreTypeId = first.OreTypeId,
                    OreName = first.OreName,
                    UnitsToReprocess = Math.Max(1, first.UnitsToReprocess),
                    OreVolume = first.OreVolume,
                    UnitPrice = getOreUnitPrice(first),
                    OutputByMineral = outputByMineral
                });
            }

            return candidates;
        }

        public List<PlannedOre> PlanOreForMinerals(
            Dictionary<long, long> remaining,
            List<OrePlanCandidate> candidates,
            Dictionary<long, double> mineralUnitPrices)
        {
            var plan = new List<PlannedOre>();
            if (remaining == null || candidates == null || candidates.Count == 0)
            {
                return plan;
            }

            var prices = mineralUnitPrices ?? new Dictionary<long, double>();
            var guard = 0;
            while (remaining.Values.Any(quantity => quantity > 0) && guard++ < 100)
            {
                var best = candidates
                    .Select(candidate => new
                    {
                        Candidate = candidate,
                        CoverageValue = GetOreCoverageValue(candidate, remaining, prices),
                        BatchCost = GetOreBatchCost(candidate)
                    })
                    .Where(item => item.CoverageValue > 0)
                    .OrderBy(item => item.BatchCost / item.CoverageValue)
                    .ThenByDescending(item => item.CoverageValue)
                    .FirstOrDefault();
                if (best == null)
                {
                    break;
                }

                var batches = GetOreBatchCount(best.Candidate, remaining);
                if (batches <= 0)
                {
                    break;
                }

                foreach (var output in best.Candidate.OutputByMineral)
                {
                    long current;
                    if (remaining.TryGetValue(output.Key, out current) && current > 0)
                    {
                        remaining[output.Key] = Math.Max(0, current - output.Value * batches);
                    }
                }

                var planned = plan.FirstOrDefault(item => item.Candidate.OreTypeId == best.Candidate.OreTypeId);
                if (planned == null)
                {
                    planned = new PlannedOre { Candidate = best.Candidate };
                    plan.Add(planned);
                }

                planned.Quantity += batches * best.Candidate.UnitsToReprocess;
                planned.ProducedMinerals += best.Candidate.OutputByMineral.Values.Sum() * batches;
                foreach (var output in best.Candidate.OutputByMineral)
                {
                    long current;
                    planned.ProducedByMineral.TryGetValue(output.Key, out current);
                    planned.ProducedByMineral[output.Key] = current + output.Value * batches;
                }
            }

            return plan;
        }

        public double CalculatePurchaseCost(
            Dictionary<long, long> mineralQuantities,
            Dictionary<long, List<ReprocessingOption>> oreOutputs,
            IReadOnlyCollection<long> requiredMineralTypeIds,
            Func<ReprocessingOption, double> getYield,
            Func<ReprocessingOption, double> getOreUnitPrice,
            Dictionary<long, double> mineralUnitPrices)
        {
            if (mineralQuantities == null || mineralQuantities.Count == 0)
            {
                return 0;
            }

            var candidates = BuildCandidates(oreOutputs, requiredMineralTypeIds, getYield, getOreUnitPrice);
            if (candidates.Count == 0)
            {
                return 0;
            }

            var remaining = mineralQuantities.ToDictionary(item => item.Key, item => item.Value);
            var plan = PlanOreForMinerals(remaining, candidates, mineralUnitPrices);
            if (plan.Count == 0 || remaining.Values.Any(quantity => quantity > 0))
            {
                return 0;
            }

            var cost = 0.0;
            foreach (var plannedOre in plan)
            {
                if (plannedOre.Candidate.UnitPrice <= 0)
                {
                    return 0;
                }

                cost += plannedOre.Quantity * plannedOre.Candidate.UnitPrice;
            }

            return cost;
        }

        public long GetReprocessedMineralOutput(ReprocessingOption option, double yield)
        {
            return option == null ? 0 : (long)Math.Floor(option.MineralQuantity * Math.Max(0, yield));
        }

        private static double GetOreCoverageValue(OrePlanCandidate candidate, Dictionary<long, long> remaining, Dictionary<long, double> mineralUnitPrices)
        {
            var value = 0.0;
            foreach (var output in candidate.OutputByMineral)
            {
                long needed;
                if (!remaining.TryGetValue(output.Key, out needed) || needed <= 0)
                {
                    continue;
                }

                double unitValue;
                mineralUnitPrices.TryGetValue(output.Key, out unitValue);
                value += Math.Min(needed, output.Value) * (unitValue > 0 ? unitValue : 1);
            }

            return value;
        }

        private static double GetOreBatchCost(OrePlanCandidate candidate)
        {
            if (candidate.UnitPrice > 0)
            {
                return candidate.UnitPrice * candidate.UnitsToReprocess;
            }

            return Math.Max(0.0001, candidate.OreVolume * candidate.UnitsToReprocess);
        }

        private static long GetOreBatchCount(OrePlanCandidate candidate, Dictionary<long, long> remaining)
        {
            return candidate.OutputByMineral
                .Where(output => output.Value > 0 && remaining.ContainsKey(output.Key) && remaining[output.Key] > 0)
                .Select(output => (long)Math.Ceiling(remaining[output.Key] / (double)output.Value))
                .DefaultIfEmpty(0)
                .Min();
        }
    }

    public class OrePlanCandidate
    {
        public long OreTypeId { get; set; }
        public string OreName { get; set; }
        public int UnitsToReprocess { get; set; }
        public double OreVolume { get; set; }
        public double UnitPrice { get; set; }
        public Dictionary<long, long> OutputByMineral { get; set; }
    }

    public class PlannedOre
    {
        public OrePlanCandidate Candidate { get; set; }
        public long Quantity { get; set; }
        public long ProducedMinerals { get; set; }
        public Dictionary<long, long> ProducedByMineral { get; set; } = new Dictionary<long, long>();
    }
}
