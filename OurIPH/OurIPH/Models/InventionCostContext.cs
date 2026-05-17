using System;

namespace OurIPH.Models
{
    public sealed class InventionCostContext
    {
        public InventionInfo Invention { get; set; }
        public InventionPlan Plan { get; set; }
        public int ManufacturingRuns { get; set; }
        public int TechLevel { get; set; }
        public long DecryptorTypeId { get; set; }
        public Func<long, double> GetUnitPrice { get; set; }
    }
}
