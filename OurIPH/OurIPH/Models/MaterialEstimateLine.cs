namespace OurIPH.Models
{
    public sealed class MaterialEstimateLine
    {
        public MaterialRequirement Material { get; set; }
        public long AdjustedQuantity { get; set; }
        public double UnitPrice { get; set; }
        public double BuyCost { get; set; }
        public bool MarketVolumeShort { get; set; }
        public bool IsMineralFallback { get; set; }
        public ChildBlueprintEstimateRequest ChildRequest { get; set; }
        public MaterialBuildBuyDecisionResult BuildBuyDecision { get; set; }
    }
}
