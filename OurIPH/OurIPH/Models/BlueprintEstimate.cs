namespace OurIPH.Models
{
    public sealed class BlueprintEstimate
    {
        public double MaterialCost { get; set; }
        public double InstallationCost { get; set; }
        public double FacilityMaterialBonusPercent { get; set; }
        public double CostIndexPercent { get; set; }
        public double ProductionTimeSeconds { get; set; }
        public double InventionCost { get; set; }
        public bool InventionMissing { get; set; }
        public double InventionChancePercent { get; set; }
        public int InventionJobs { get; set; }
        public int InventedRunsPerSuccess { get; set; }
        public double InventionMaterialsCost { get; set; }
        public double InventionCopyCost { get; set; }
        public double InventionJobUsageCost { get; set; }
        public string InventionStationName { get; set; }
        public string InventionStationSystem { get; set; }
        public string CopyStationName { get; set; }
        public string CopyStationSystem { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }

        public BlueprintEstimate Clone()
        {
            return (BlueprintEstimate)MemberwiseClone();
        }
    }
}
