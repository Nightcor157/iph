namespace OurIPH.Models
{
    public class MarketHistoryStats
    {
        public long TypeId { get; set; }
        public long RegionId { get; set; }
        public int Days { get; set; }
        public double AverageDailyVolume { get; set; }
        public long TotalVolume { get; set; }
        public long TotalOrders { get; set; }
        public double PriceTrend { get; set; }
        public System.DateTime UpdatedAt { get; set; }
    }
}
