namespace OurIPH.Models
{
    public class FacilityStructureType
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public int RigSlots { get; set; }
        public int ServiceSlots { get; set; }
        public int RigSize { get; set; }
        public double Calibration { get; set; }
        public bool DisallowInHighSec { get; set; }
        public bool DisallowInEmpire { get; set; }
        public bool IsNpcStation { get; set; }

        public string IconUrl
        {
            get
            {
                return IsNpcStation
                    ? "https://images.evetech.net/types/35832/icon?size=32"
                    : string.Format("https://images.evetech.net/types/{0}/icon?size=32", TypeId);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class FacilityRigOption
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string RigFamily { get; set; }
        public int RigSize { get; set; }
        public double CalibrationCost { get; set; }
        public bool DisallowInHighSec { get; set; }
        public bool DisallowInEmpire { get; set; }
        public bool IsNone { get; set; }

        public string IconUrl
        {
            get
            {
                return IsNone ? null : string.Format("https://images.evetech.net/types/{0}/icon?size=32", TypeId);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class FacilityServiceModuleOption
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public bool IsNone { get; set; }

        public string IconUrl
        {
            get { return IsNone ? null : string.Format("https://images.evetech.net/types/{0}/icon?size=32", TypeId); }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class FacilityMathResult
    {
        public double MaterialMultiplier { get; set; }
        public double TimeMultiplier { get; set; }
        public double CostMultiplier { get; set; }
        public double MaterialBonusPercent { get; set; }
        public string Warning { get; set; }
    }

    public class EveRegion
    {
        public long RegionId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EveSolarSystem
    {
        public long RegionId { get; set; }
        public long SolarSystemId { get; set; }
        public string Name { get; set; }
        public double Security { get; set; }
        public int NpcStationCount { get; set; }

        public string DisplayName
        {
            get { return string.Format("{0} ({1:0.0})", Name, Security); }
        }

        public string SecurityBrush
        {
            get
            {
                if (Security >= 1.0)
                {
                    return "#2E6FEA";
                }

                if (Security >= 0.9)
                {
                    return "#2E86FF";
                }

                if (Security >= 0.8)
                {
                    return "#00A8E8";
                }

                if (Security >= 0.7)
                {
                    return "#00BFD8";
                }

                if (Security >= 0.6)
                {
                    return "#17B978";
                }

                if (Security >= 0.5)
                {
                    return "#6CC24A";
                }

                if (Security >= 0.4)
                {
                    return "#F2C94C";
                }

                if (Security >= 0.3)
                {
                    return "#F2994A";
                }

                if (Security >= 0.2)
                {
                    return "#EB5757";
                }

                return "#B00020";
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
