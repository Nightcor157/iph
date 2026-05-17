using System;

namespace OurIPH.Models
{
    public class ContractPriceSample
    {
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public double Price { get; set; }
        public DateTime ObservedAt { get; set; }
        public long ContractId { get; set; }
        public string Source { get; set; }
        public long LocationId { get; set; }
        public long Quantity { get; set; }
        public int ItemCount { get; set; }
        public string Title { get; set; }
    }
}
