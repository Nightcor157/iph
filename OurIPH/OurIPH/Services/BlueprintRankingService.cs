using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintRankingService
    {
        private readonly BlueprintFilteringService _filteringService;

        public BlueprintRankingService(BlueprintFilteringService filteringService)
        {
            _filteringService = filteringService;
        }

        public void AssignRanks(IEnumerable<BlueprintSearchResult> blueprints)
        {
            AssignRanks(blueprints, true);
        }

        public void AssignRanks(IEnumerable<BlueprintSearchResult> blueprints, bool hideRareOrNoise)
        {
            if (blueprints == null)
            {
                return;
            }

            foreach (var blueprint in blueprints)
            {
                blueprint.ProfitRank = 0;
                blueprint.TopScore = 0;
            }

            var ranked = blueprints
                .Where(blueprint => IsRankable(blueprint, hideRareOrNoise))
                .Select(blueprint => new
                {
                    Blueprint = blueprint,
                    Score = CalculateScore(blueprint)
                })
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.Blueprint.SvrTimesIskPerHour)
                .ThenByDescending(item => item.Blueprint.SalesVolumeRatio)
                .ThenByDescending(item => item.Blueprint.IskPerHour)
                .ThenByDescending(item => item.Blueprint.Profit)
                .ToList();

            for (var i = 0; i < ranked.Count; i++)
            {
                ranked[i].Blueprint.ProfitRank = i + 1;
                ranked[i].Blueprint.TopScore = ranked[i].Score;
            }
        }

        public double CalculateScore(BlueprintSearchResult blueprint)
        {
            if (blueprint == null || blueprint.Profit <= 0)
            {
                return 0;
            }

            var liquidity = GetLiquidityMultiplier(blueprint.SalesVolumeRatio);
            var sourceConfidence = GetSourceConfidence(blueprint.ProductPriceSource, blueprint.ProductPriceDetails);
            var trendMultiplier = blueprint.PriceTrend > 0 ? 1.05 : blueprint.PriceTrend < 0 ? 0.95 : 1.0;
            var marketPenalty = GetMarketCoveragePenalty(blueprint);
            var profitContribution = GetProfitContribution(blueprint);
            var roiBonus = Math.Max(0, Math.Min(blueprint.ReturnOnInvestmentPercent, 250)) * 1000;

            return ((blueprint.IskPerHour * liquidity) + profitContribution + roiBonus)
                   * sourceConfidence
                   * trendMultiplier
                   * marketPenalty;
        }

        private bool IsRankable(BlueprintSearchResult blueprint, bool hideRareOrNoise)
        {
            return blueprint != null
                   && blueprint.Profit > 0
                   && !string.IsNullOrWhiteSpace(blueprint.EstimateStatus)
                   && blueprint.EstimateStatus.StartsWith("OK", StringComparison.OrdinalIgnoreCase)
                   && (!hideRareOrNoise || !_filteringService.IsRareOrNoiseBlueprint(blueprint));
        }

        private static double GetSourceConfidence(string source, string detail)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return 1.0;
            }

            if (source.StartsWith("Contract", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(detail) || detail.IndexOf("1/", StringComparison.OrdinalIgnoreCase) >= 0
                    ? 0.88
                    : 0.96;
            }

            if (source.IndexOf("fallback", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.9;
            }

            return 1.0;
        }

        private static double GetLiquidityMultiplier(double salesVolumeRatio)
        {
            if (salesVolumeRatio <= 0)
            {
                return 0.10;
            }

            if (salesVolumeRatio < 0.25)
            {
                return 0.20;
            }

            if (salesVolumeRatio < 1)
            {
                return 0.45;
            }

            return Math.Min(1.60, 0.70 + Math.Log(1 + salesVolumeRatio, 2) * 0.35);
        }

        private static double GetProfitContribution(BlueprintSearchResult blueprint)
        {
            var rawContribution = Math.Sqrt(Math.Max(0, blueprint.Profit)) * 1000;
            var cap = blueprint.IskPerHour > 0
                ? blueprint.IskPerHour * 1.5
                : blueprint.Profit * 0.005;

            return Math.Min(rawContribution, Math.Max(0, cap));
        }

        private static double GetMarketCoveragePenalty(BlueprintSearchResult blueprint)
        {
            if (blueprint.ProducedQuantity <= 0)
            {
                return 1.0;
            }

            if (blueprint.ProductMarketVolume <= 0)
            {
                return IsContractSource(blueprint.ProductPriceSource) ? 0.75 : 0.35;
            }

            var coverage = blueprint.ProductMarketVolume / (double)blueprint.ProducedQuantity;
            if (coverage < 1)
            {
                return 0.35;
            }

            if (coverage < 5)
            {
                return 0.70;
            }

            if (coverage < 20)
            {
                return 0.90;
            }

            return 1.0;
        }

        private static bool IsContractSource(string source)
        {
            return !string.IsNullOrWhiteSpace(source)
                   && source.StartsWith("Contract", StringComparison.OrdinalIgnoreCase);
        }
    }
}
