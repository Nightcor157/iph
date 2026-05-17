using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class FacilityActivityStationService
    {
        public FacilityStation ResolveActivityStation(FacilityPreset facilityPreset, FacilityStation fallbackStation, string productionType)
        {
            var station = facilityPreset == null ? null : facilityPreset.GetBestStationFor(productionType);
            if (station != null && station.SupportsProduction && station.ProductionType == productionType)
            {
                return station;
            }

            return fallbackStation != null && fallbackStation.SupportsProduction
                ? CopyStationForActivity(fallbackStation, productionType)
                : null;
        }

        public FacilityStation CopyStationForActivity(FacilityStation source, string productionType)
        {
            if (source == null)
            {
                return null;
            }

            return new FacilityStation
            {
                Name = source.Name,
                SystemName = source.SystemName,
                RegionId = source.RegionId,
                SolarSystemId = source.SolarSystemId,
                FacilityType = source.FacilityType,
                StructureTypeId = source.StructureTypeId,
                ProductionType = productionType,
                RigSlot1TypeId = source.RigSlot1TypeId,
                RigSlot2TypeId = source.RigSlot2TypeId,
                RigSlot3TypeId = source.RigSlot3TypeId,
                ServiceModule1TypeId = source.ServiceModule1TypeId,
                ServiceModule2TypeId = source.ServiceModule2TypeId,
                ServiceModule3TypeId = source.ServiceModule3TypeId,
                ServiceModule4TypeId = source.ServiceModule4TypeId,
                ServiceModule5TypeId = source.ServiceModule5TypeId,
                Security = source.Security,
                MaterialMultiplier = source.MaterialMultiplier,
                TimeMultiplier = source.TimeMultiplier,
                CostMultiplier = source.CostMultiplier,
                MaterialBonusPercent = source.MaterialBonusPercent,
                IndustryTaxPercent = source.IndustryTaxPercent,
                SalesFeePercent = source.SalesFeePercent,
                FactionWarfareUpgradeLevel = source.FactionWarfareUpgradeLevel,
                SupportsProduction = source.SupportsProduction,
                ValidationMessage = source.ValidationMessage
            };
        }
    }
}
