using System.Collections.Generic;

namespace OurIPH.Models
{
    public class InventionInfo
    {
        public long SourceBlueprintTypeId { get; set; }
        public int RunsPerSuccess { get; set; }
        public double Probability { get; set; }
        public int BaseCopyTime { get; set; }
        public int BaseInventionTime { get; set; }
        public int MaxProductionLimit { get; set; }
        public List<MaterialRequirement> Materials { get; set; } = new List<MaterialRequirement>();
        public List<MaterialRequirement> CopyMaterials { get; set; } = new List<MaterialRequirement>();
    }
}
