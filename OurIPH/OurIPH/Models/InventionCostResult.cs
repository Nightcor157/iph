namespace OurIPH.Models
{
    public sealed class InventionCostResult
    {
        public double SourceCost { get; set; }
        public double MaterialCost { get; set; }
        public double DecryptorCost { get; set; }
        public double CopyMaterialCost { get; set; }
        public bool MissingPrice { get; set; }

        public double TotalCost
        {
            get { return SourceCost + MaterialCost + DecryptorCost + CopyMaterialCost; }
        }
    }
}
