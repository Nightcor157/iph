using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintMaterialTraversalResult
    {
        public double MaterialCost { get; set; }
        public double EstimatedInputValue { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }
        public List<double> ComponentProductionTimes { get; } = new List<double>();
        public Dictionary<long, long> MineralBuyQuantities { get; } = new Dictionary<long, long>();
        public List<MaterialEstimateLine> MaterialLines { get; } = new List<MaterialEstimateLine>();
    }
}
