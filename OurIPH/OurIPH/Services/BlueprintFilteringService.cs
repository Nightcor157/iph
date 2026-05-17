using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintFilteringService
    {
        private readonly BlueprintFilterRules _rules;

        public BlueprintFilteringService()
            : this(BlueprintFilterRules.CreateDefault())
        {
        }

        public BlueprintFilteringService(BlueprintFilterRules rules)
        {
            _rules = rules ?? BlueprintFilterRules.CreateDefault();
        }

        public bool IsLimitedSourceBlueprint(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            if (_rules.LimitedMetaGroupIds.Contains(blueprint.MetaGroupId))
            {
                return true;
            }

            return ContainsAny(blueprint.ItemMarketGroup, _rules.LimitedMarketGroupKeywords)
                   || ContainsAny(blueprint.GroupName, _rules.LimitedGroupKeywords);
        }

        public bool IsRareOrNoiseBlueprint(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            return IsLimitedSourceBlueprint(blueprint)
                   || ContainsAny(blueprint.ProductName, _rules.RareProductKeywords)
                   || ContainsAny(blueprint.BlueprintName, _rules.RareBlueprintKeywords)
                   || ContainsAny(blueprint.GroupName, _rules.RareGroupKeywords)
                   || ContainsAny(blueprint.ItemMarketGroup, _rules.RareMarketGroupKeywords);
        }

        public bool IsCapitalBlueprint(BlueprintSearchResult blueprint)
        {
            return ContainsAny(blueprint?.GroupName, _rules.CapitalKeywords)
                   || ContainsAny(blueprint?.ItemMarketGroup, _rules.CapitalKeywords);
        }

        public bool IsReactionBlueprint(BlueprintSearchResult blueprint)
        {
            return blueprint != null
                   && (blueprint.HasReactionActivity
                       || ContainsAny(blueprint.GroupName, _rules.ReactionGroupKeywords)
                       || ContainsAny(blueprint.ItemMarketGroup, _rules.ReactionMarketGroupKeywords));
        }

        public string GetTopReason(BlueprintSearchResult blueprint)
        {
            if (blueprint == null || blueprint.ProfitRank <= 0)
            {
                return "";
            }

            var source = string.IsNullOrWhiteSpace(blueprint.ProductPriceSource) ? "Market" : blueprint.ProductPriceSource;
            var liquidity = blueprint.SalesVolumeRatio > 0
                ? string.Format("SVR {0:N2}", blueprint.SalesVolumeRatio)
                : "SVR n/a";
            return string.Format("#{0}: {1:N0} ISK profit, {2:N0} ISK/h, {3}, {4}",
                blueprint.ProfitRank,
                blueprint.Profit,
                blueprint.IskPerHour,
                liquidity,
                source);
        }

        private static bool ContainsAny(string value, IEnumerable<string> needles)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return needles != null && needles.Any(needle =>
                !string.IsNullOrWhiteSpace(needle)
                && value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
