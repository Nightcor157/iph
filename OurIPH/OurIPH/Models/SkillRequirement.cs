namespace OurIPH.Models
{
    public class SkillRequirement
    {
        public long TypeId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int OwnedLevel { get; set; } = -1;

        public string IconUrl
        {
            get { return string.Format("https://images.evetech.net/types/{0}/icon?size=64", TypeId); }
        }

        public string LevelText
        {
            get { return Level.ToString(); }
        }

        public string OwnedLevelText
        {
            get { return OwnedLevel >= 0 ? OwnedLevel.ToString() : "-"; }
        }

        public string SkillStatusText
        {
            get { return OwnedLevel >= Level ? "OK" : OwnedLevel >= 0 ? "Нужно " + Level : "Не задан"; }
        }

        public string SkillStatusBrush
        {
            get { return OwnedLevel >= Level ? "#1A7F37" : "#B42318"; }
        }
    }
}
