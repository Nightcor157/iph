namespace OurIPH.Models
{
    public sealed class InventionUsageCostContext
    {
        public double EstimatedInputValue { get; set; }
        public int Jobs { get; set; }
        public int TotalInventedRuns { get; set; }
        public int RequestedRuns { get; set; }
        public double CostIndex { get; set; }
        public double FactionWarfareMultiplier { get; set; }
        public double FacilityCostMultiplier { get; set; } = 1;
        public double IndustryTaxPercent { get; set; }
    }
}
