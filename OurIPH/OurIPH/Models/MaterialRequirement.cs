namespace OurIPH.Models
{
    public class MaterialRequirement
    {
        public long TypeId { get; set; }
        public string Name { get; set; }
        public long Quantity { get; set; }
        public long AdjustedQuantity { get; set; }

        public string IconUrl
        {
            get { return string.Format("https://images.evetech.net/types/{0}/icon?size=64", TypeId); }
        }

        public string QuantityText
        {
            get { return Quantity.ToString("N0"); }
        }

        public string AdjustedQuantityText
        {
            get { return AdjustedQuantity > 0 ? AdjustedQuantity.ToString("N0") : ""; }
        }
    }
}
