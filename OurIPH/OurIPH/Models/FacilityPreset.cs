using System.Collections.ObjectModel;
using System.Linq;

namespace OurIPH.Models
{
    public class FacilityPreset
    {
        public string Name { get; set; }
        public int IndustrySkillLevel { get; set; } = 5;
        public int AdvancedIndustrySkillLevel { get; set; } = 5;
        public int AccountingSkillLevel { get; set; } = 5;
        public int BrokerRelationsSkillLevel { get; set; } = 5;
        public double BrokerFactionStanding { get; set; } = 5;
        public double BrokerCorpStanding { get; set; } = 5;
        public double ManufacturingImplantPercent { get; set; }
        public double SccIndustryFeePercent { get; set; } = 4.0;
        public double AlphaAccountTaxPercent { get; set; }
        public double BaseSalesTaxPercent { get; set; } = 4.5;
        public double BaseBrokerFeePercent { get; set; } = 3.0;
        public double SccBrokerFeeSurchargePercent { get; set; } = 0.5;
        public double SpecialBrokerFeePercent { get; set; }
        public int BrokerFeeMode { get; set; } = 1;
        public bool IncludeSalesTax { get; set; } = true;
        public bool IncludeBuyOrderBrokerFee { get; set; } = true;
        public int DefaultBlueprintMe { get; set; } = 10;
        public int DefaultBlueprintTe { get; set; } = 20;
        public int ProductionLines { get; set; } = 10;
        public int LaboratoryLines { get; set; } = 10;
        public bool SuggestBuildBlueprintsNotOwned { get; set; } = true;
        public bool BuildWhenMarketVolumeShort { get; set; } = true;
        public bool ConvertMineralsToOre { get; set; }
        public bool PreferCompressedOre { get; set; } = true;
        public int RefiningSkillLevel { get; set; } = 5;
        public int ReprocessingSkillLevel { get; set; } = 5;
        public int OreProcessingSkillLevel { get; set; } = 4;
        public double ReprocessingImplantPercent { get; set; }
        public int EncryptionSkillLevel { get; set; } = 4;
        public int DatacoreSkill1Level { get; set; } = 4;
        public int DatacoreSkill2Level { get; set; } = 4;
        public int ScienceSkillLevel { get; set; } = 5;
        public ObservableCollection<FacilityStation> Stations { get; set; } = new ObservableCollection<FacilityStation>();

        public FacilityStation GetBestStationFor(string productionType)
        {
            var candidates = Stations.Where(station => station.ProductionType == productionType).ToList();
            if (candidates.Count == 0)
            {
                candidates = Stations.ToList();
            }

            return candidates.OrderByDescending(station => station.SupportsProduction)
                             .ThenByDescending(station => station.MaterialBonusPercent)
                             .ThenBy(station => station.IndustryTaxPercent)
                             .ThenBy(station => station.SalesFeePercent)
                             .FirstOrDefault()
                   ?? FacilityStation.CreateDefault();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class FacilityStation
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
        public long RegionId { get; set; }
        public long SolarSystemId { get; set; }
        public string FacilityType { get; set; }
        public int StructureTypeId { get; set; }
        public string ProductionType { get; set; }
        public int RigSlot1TypeId { get; set; }
        public int RigSlot2TypeId { get; set; }
        public int RigSlot3TypeId { get; set; }
        public int ServiceModule1TypeId { get; set; }
        public int ServiceModule2TypeId { get; set; }
        public int ServiceModule3TypeId { get; set; }
        public int ServiceModule4TypeId { get; set; }
        public int ServiceModule5TypeId { get; set; }
        public double Security { get; set; }
        public double MaterialMultiplier { get; set; }
        public double TimeMultiplier { get; set; }
        public double CostMultiplier { get; set; }
        public double MaterialBonusPercent { get; set; }
        public double IndustryTaxPercent { get; set; }
        public double SalesFeePercent { get; set; }
        public int FactionWarfareUpgradeLevel { get; set; }
        public bool SupportsProduction { get; set; }
        public string ValidationMessage { get; set; }

        public static FacilityStation CreateDefault()
        {
            return new FacilityStation
            {
                Name = "Jita IV - Moon 4 - Caldari Navy Assembly Plant",
                SystemName = "Jita",
                RegionId = 10000002,
                SolarSystemId = 30000142,
                FacilityType = "NPC Station",
                StructureTypeId = 0,
                ProductionType = "Manufacturing",
                Security = 0.9,
                MaterialMultiplier = 1,
                TimeMultiplier = 1,
                CostMultiplier = 1,
                MaterialBonusPercent = 0,
                IndustryTaxPercent = 0.25,
                SalesFeePercent = 0,
                FactionWarfareUpgradeLevel = 0,
                SupportsProduction = true
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
