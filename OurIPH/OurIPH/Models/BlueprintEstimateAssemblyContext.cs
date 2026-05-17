using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateAssemblyContext
    {
        public BlueprintSearchResult Blueprint { get; set; }
        public FacilityPreset FacilityPreset { get; set; }
        public InventionPlan InventionPlan { get; set; }
        public IReadOnlyList<double> ComponentProductionTimes { get; set; }
        public int Runs { get; set; }
        public double TimeMultiplier { get; set; }
        public double InventionTimeSeconds { get; set; }
        public double MaterialCost { get; set; }
        public double InstallationCost { get; set; }
        public double FacilityMaterialBonusPercent { get; set; }
        public double CostIndexPercent { get; set; }
        public double InventionCost { get; set; }
        public bool InventionMissing { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }
    }
}
