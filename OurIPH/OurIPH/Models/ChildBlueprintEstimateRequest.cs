namespace OurIPH.Models
{
    public sealed class ChildBlueprintEstimateRequest
    {
        public BlueprintSearchResult ParentBlueprint { get; set; }
        public BlueprintSearchResult ChildBlueprint { get; set; }
        public MaterialRequirement Material { get; set; }
        public long RequiredQuantity { get; set; }
        public int ChildRuns { get; set; }
        public double MaterialEfficiency { get; set; }
        public double TimeEfficiency { get; set; }
        public DecryptorOption Decryptor { get; set; }
    }
}
