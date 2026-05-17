using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateResult
    {
        public BlueprintEstimate Estimate { get; set; }
        public List<MaterialEstimateLine> MaterialLines { get; } = new List<MaterialEstimateLine>();
    }
}
