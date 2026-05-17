using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateMaterialOrchestrationResult
    {
        public BlueprintMaterialTraversalResult TraversalResult { get; set; }
        public double MaterialCost { get; set; }
        public double EstimatedInputValue { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }
        public IReadOnlyList<double> ComponentProductionTimes { get; set; }
    }
}
