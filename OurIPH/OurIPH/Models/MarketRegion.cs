namespace OurIPH.Models
{
    public class MarketRegion
    {
        public string Name { get; set; }
        public long RegionId { get; set; }
        public long? StationId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
