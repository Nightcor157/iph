namespace OurIPH.Models
{
    public class DecryptorOption
    {
        public long TypeId { get; set; }
        public string Name { get; set; }
        public int MaterialEfficiencyModifier { get; set; }
        public int TimeEfficiencyModifier { get; set; }
        public int RunModifier { get; set; }
        public double ProbabilityModifier { get; set; } = 1.0;

        public string IconUrl
        {
            get { return TypeId > 0 ? string.Format("https://images.evetech.net/types/{0}/icon?size=64", TypeId) : ""; }
        }

        public string SummaryText
        {
            get
            {
                if (TypeId <= 0)
                {
                    return "без декриптора";
                }

                return string.Format("x{0:0.##}, ME {1:+#;-#;0}, TE {2:+#;-#;0}, runs {3:+#;-#;0}",
                    ProbabilityModifier, MaterialEfficiencyModifier, TimeEfficiencyModifier, RunModifier);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
