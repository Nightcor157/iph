namespace OurIPH.Models
{
    public class MarketOrderStats
    {
        public long TypeId { get; set; }
        public long RegionId { get; set; }
        public long BuyOrders { get; set; }
        public long SellOrders { get; set; }
        public long BuyVolume { get; set; }
        public long SellVolume { get; set; }
    }
}
