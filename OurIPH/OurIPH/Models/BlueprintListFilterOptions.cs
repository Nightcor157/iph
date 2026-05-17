using System;
using System.Collections.Generic;

namespace OurIPH.Models
{
    public class BlueprintListFilterOptions
    {
        public bool ProfitableOnly { get; set; }
        public bool HideMissingPrices { get; set; }
        public bool HideLowVolume { get; set; }
        public bool ShowExcluded { get; set; }
        public bool HideBySkills { get; set; }
        public bool TopOnly { get; set; }
        public int TopCount { get; set; }
        public double MinSalesVolumeRatio { get; set; }
        public string TrendFilter { get; set; }
        public long MinSold { get; set; }
        public long MinOrders { get; set; }
        public double MinIskPerHour { get; set; }
        public double MinRoi { get; set; }
        public ISet<long> ExcludedProductIds { get; set; }
        public Func<BlueprintSearchResult, bool> MatchesExplicitSearch { get; set; }
        public Func<BlueprintSearchResult, bool> AllowedByTypeFilter { get; set; }
        public Func<BlueprintSearchResult, bool> HasRequiredSkills { get; set; }
        public Func<BlueprintSearchResult, bool> AllowedByBuildProfile { get; set; }

        public BlueprintListFilterOptions()
        {
            TrendFilter = "Any";
            ExcludedProductIds = new HashSet<long>();
            MatchesExplicitSearch = blueprint => false;
            AllowedByTypeFilter = blueprint => true;
            HasRequiredSkills = blueprint => true;
            AllowedByBuildProfile = blueprint => true;
        }
    }
}
