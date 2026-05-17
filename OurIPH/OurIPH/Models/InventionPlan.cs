namespace OurIPH.Models
{
    public class InventionPlan
    {
        public double Chance { get; set; }
        public int Jobs { get; set; }
        public int RunsPerSuccess { get; set; }
        public int SuccessfulJobsNeeded { get; set; }
        public double SourceCost { get; set; }
        public double MaterialCost { get; set; }
        public double DecryptorCost { get; set; }
        public double CopyMaterialCost { get; set; }
        public double InventionUsageCost { get; set; }
        public double CopyUsageCost { get; set; }
        public double CopyTimeSeconds { get; set; }
        public double InventionTimeSeconds { get; set; }
        public string InventionStationName { get; set; }
        public string InventionStationSystem { get; set; }
        public string CopyStationName { get; set; }
        public string CopyStationSystem { get; set; }
    }
}
