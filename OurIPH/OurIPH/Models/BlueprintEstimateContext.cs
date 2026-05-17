using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateContext
    {
        public BlueprintSearchResult Blueprint { get; set; }
        public IReadOnlyList<MaterialRequirement> Materials { get; set; }
        public FacilityStation Station { get; set; }
        public FacilityPreset FacilityPreset { get; set; }
        public IReadOnlyDictionary<long, MarketPrice> Prices { get; set; }
        public IReadOnlyDictionary<long, double> AdjustedPrices { get; set; }
        public int Runs { get; set; }
        public double MaterialEfficiency { get; set; }
        public double TimeEfficiency { get; set; }
        public double MaterialMultiplier { get; set; }
        public DecryptorOption Decryptor { get; set; }
        public BlueprintEstimateRecursionState RecursionState { get; set; }
    }
}
