namespace OurIPH.Models
{
    public class BuildQueueItem
    {
        public BlueprintSearchResult Blueprint { get; set; }
        public int Runs { get; set; }
        public double MaterialEfficiency { get; set; }
        public double TimeEfficiency { get; set; }
        public long DecryptorTypeId { get; set; }
        public string DecryptorName { get; set; }

        public long BlueprintTypeId { get { return Blueprint == null ? 0 : Blueprint.BlueprintTypeId; } }
        public long ProductTypeId { get { return Blueprint == null ? 0 : Blueprint.ProductTypeId; } }
        public string BlueprintName { get { return Blueprint == null ? "" : Blueprint.BlueprintName; } }
        public string ProductName { get { return Blueprint == null ? "" : Blueprint.ProductName; } }
        public string GroupName { get { return Blueprint == null ? "" : Blueprint.GroupName; } }
        public int PortionSize { get { return Blueprint == null ? 0 : Blueprint.PortionSize; } }
        public string ProductIconUrl { get { return Blueprint == null ? null : Blueprint.ProductIconUrl; } }
        public string BlueprintIconUrl { get { return Blueprint == null ? null : Blueprint.BlueprintIconUrl; } }
        public string EfficiencyText
        {
            get
            {
                if (Blueprint != null && Blueprint.IsCopyOnlyBlueprint)
                {
                    var runsPerCopy = System.Math.Max(1, Blueprint.MaxProductionLimit);
                    var copies = (int)System.Math.Ceiling(System.Math.Max(1, Runs) / (double)runsPerCopy);
                    return string.Format("BPC {0:N0}x{1:N0} | ME 0 / TE 0", copies, runsPerCopy);
                }

                return string.Format("ME {0:0.##} / TE {1:0.##}", MaterialEfficiency, TimeEfficiency);
            }
        }
        public string DecryptorText { get { return string.IsNullOrWhiteSpace(DecryptorName) ? "" : DecryptorName; } }
        public string ProfitText { get { return Blueprint == null ? "" : Blueprint.ProfitText; } }
        public string ProfitBrush { get { return Blueprint == null ? "#657386" : Blueprint.ProfitBrush; } }
        public string ReturnOnInvestmentText { get { return Blueprint == null ? "" : Blueprint.ReturnOnInvestmentText; } }
        public string IskPerHourText { get { return Blueprint == null ? "" : Blueprint.IskPerHourText; } }
        public string SalesVolumeRatioText { get { return Blueprint == null ? "" : Blueprint.SalesVolumeRatioText; } }
        public string SvrTimesIskPerHourText { get { return Blueprint == null ? "" : Blueprint.SvrTimesIskPerHourText; } }
        public string TotalItemsSoldText { get { return Blueprint == null ? "" : Blueprint.TotalItemsSoldText; } }
        public string PriceTrendText { get { return Blueprint == null ? "" : Blueprint.PriceTrendText; } }
        public string PriceTrendBrush { get { return Blueprint == null ? "#657386" : Blueprint.PriceTrendBrush; } }
        public string MarketRiskText { get { return Blueprint == null ? "" : Blueprint.MarketRiskText; } }
        public string MarketRiskBrush { get { return Blueprint == null ? "#657386" : Blueprint.MarketRiskBrush; } }
        public string SkillStatusText { get { return Blueprint == null ? "" : Blueprint.SkillStatusText; } }
        public string SkillStatusBrush { get { return Blueprint == null ? "#657386" : Blueprint.SkillStatusBrush; } }
        public string RequiredSkillsText { get { return Blueprint == null ? "" : Blueprint.RequiredSkillsText; } }
        public string BestFacilityText { get { return Blueprint == null ? "" : Blueprint.BestFacilityText; } }
    }
}
