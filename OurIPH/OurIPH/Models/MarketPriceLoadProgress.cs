namespace OurIPH.Models
{
    public class MarketPriceLoadProgress
    {
        public int TotalTypeIds { get; set; }
        public int ProcessedTypeIds { get; set; }
        public int FreshDownloadedCount { get; set; }
        public int MissingCount { get; set; }
        public string LastError { get; set; }
    }
}
