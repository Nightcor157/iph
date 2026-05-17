using System;
using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateDependencies
    {
        public Func<long, BlueprintSearchResult> FindBlueprintByProduct { get; set; }
        public Func<long, bool> IsMineral { get; set; }
        public Func<BlueprintSearchResult, bool> ShouldAlwaysBuy { get; set; }
        public Func<BlueprintSearchResult, BlueprintSearchResult, bool> ShouldStopReactionDrilldown { get; set; }
        public Func<BlueprintSearchResult, IReadOnlyList<MaterialRequirement>> GetManufacturingMaterials { get; set; }
        public Func<BlueprintSearchResult, FacilityPreset, DecryptorOption, Tuple<double, double>> GetDefaultChildEfficiency { get; set; }
        public Func<ChildBlueprintEstimateRequest, FacilityStation> GetCheapestChildStation { get; set; }
        public Func<ChildBlueprintEstimateRequest, FacilityStation, BlueprintEstimate> CalculateChildEstimate { get; set; }
        public Func<MarketPrice, double> GetMaterialUnitPrice { get; set; }
        public Func<MarketPrice, double> GetProductUnitPrice { get; set; }
        public Func<MarketPrice, long> GetAvailableMarketVolume { get; set; }
        public Func<double, FacilityPreset, FacilityStation, double> ApplySalesTaxesAndFees { get; set; }
    }
}
