namespace OurIPH.Models
{
    public class ReprocessingOption
    {
        public long OreTypeId { get; set; }
        public string OreName { get; set; }
        public double OreVolume { get; set; }
        public int UnitsToReprocess { get; set; }
        public bool IsCompressed { get; set; }
        public int OreGroupId { get; set; }
        public int OreCategoryId { get; set; }
        public long MineralTypeId { get; set; }
        public string MineralName { get; set; }
        public long MineralQuantity { get; set; }
    }
}
