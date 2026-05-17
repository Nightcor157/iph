namespace OurIPH.Models
{
    public sealed class InventionTimeContext
    {
        public InventionInfo Invention { get; set; }
        public InventionPlan Plan { get; set; }
        public int TechLevel { get; set; }
        public int AdvancedIndustrySkillLevel { get; set; }
        public int ScienceSkillLevel { get; set; }
        public int LaboratoryLines { get; set; }
        public double CopyTimeMultiplier { get; set; } = 1;
        public double InventionTimeMultiplier { get; set; } = 1;
    }
}
