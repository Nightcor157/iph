namespace OurIPH.Models
{
    public class ContractPriceSampleReview
    {
        public long ContractId { get; set; }
        public string TypeName { get; set; }
        public double Price { get; set; }
        public System.DateTime ObservedAt { get; set; }
        public string Source { get; set; }
        public long Quantity { get; set; }
        public int ItemCount { get; set; }
        public string Title { get; set; }
        public string ReviewStatus { get; set; }
        public string ReviewReason { get; set; }

        public string PriceText => Price > 0 ? Price.ToString("N0") : "";
        public string ObservedAtText => ObservedAt == default(System.DateTime) ? "" : ObservedAt.ToString("yyyy-MM-dd HH:mm");
        public string SourceText => string.IsNullOrWhiteSpace(Source) ? "Manual" : Source;
        public string QuantityText => Quantity > 0 ? Quantity.ToString("N0") : "";
        public string ItemCountText => ItemCount > 0 ? ItemCount.ToString("N0") : "";
    }
}
