namespace OurIPH.Models
{
    public class MarketPrice
    {
        public long TypeId { get; set; }
        public double SellMin { get; set; }
        public double BuyMax { get; set; }
        public long SellVolume { get; set; }
        public long BuyVolume { get; set; }
    }

    public class MarketPriceCacheEntry
    {
        public long RegionOrStationId { get; set; }
        public string LocationName { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public double SellMin { get; set; }
        public double BuyMax { get; set; }
        public long SellVolume { get; set; }
        public long BuyVolume { get; set; }
        public System.DateTime UpdatedAt { get; set; }

        public string SellMinText
        {
            get { return SellMin > 0 ? SellMin.ToString("N2") : ""; }
        }

        public string BuyMaxText
        {
            get { return BuyMax > 0 ? BuyMax.ToString("N2") : ""; }
        }

        public string SellVolumeText
        {
            get { return SellVolume > 0 ? SellVolume.ToString("N0") : ""; }
        }

        public string BuyVolumeText
        {
            get { return BuyVolume > 0 ? BuyVolume.ToString("N0") : ""; }
        }

        public string UpdatedAtText
        {
            get { return UpdatedAt == default(System.DateTime) ? "" : UpdatedAt.ToString("yyyy-MM-dd HH:mm"); }
        }
    }
}
