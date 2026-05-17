using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class FacilityCatalogService
    {
        private const int AttributeHiSecModifier = 2355;
        private const int AttributeLowSecModifier = 2356;
        private const int AttributeNullSecModifier = 2357;
        private const int AttributeEngRigTimeBonus = 2593;
        private const int AttributeEngRigMaterialBonus = 2594;
        private const int AttributeEngRigCostBonus = 2595;
        private const int AttributeThukkerMaterialBonus = 2653;
        private const int AttributeRefiningYield = 717;
        private const int AttributeRefRigTimeBonus = 2713;
        private const int AttributeRefRigMaterialBonus = 2714;

        private readonly string _databasePath;

        public FacilityCatalogService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public IReadOnlyList<FacilityStructureType> LoadStructureTypes()
        {
            var result = new List<FacilityStructureType>
            {
                new FacilityStructureType { TypeId = 0, Name = "NPC Station", IsNpcStation = true }
            };

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT it.typeID, it.typeName, it.groupID,
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1137), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 2056), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1547), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1132), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1970), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1074), 0)
FROM INVENTORY_TYPES it
JOIN INVENTORY_GROUPS groups ON groups.groupID = it.groupID
WHERE it.published = 1
  AND it.typeID IN (35825,35826,35827,35832,35833,35834,35835,35836,40340,47512,47513,47514,47515,47516)
ORDER BY it.typeName";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new FacilityStructureType
                        {
                            TypeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            GroupId = reader.GetInt32(2),
                            RigSlots = Convert.ToInt32(reader.GetDouble(3)),
                            ServiceSlots = Convert.ToInt32(reader.GetDouble(4)),
                            RigSize = Convert.ToInt32(reader.GetDouble(5)),
                            Calibration = reader.GetDouble(6),
                            DisallowInHighSec = reader.GetDouble(7) != 0,
                            DisallowInEmpire = reader.GetDouble(8) != 0
                        });
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<FacilityServiceModuleOption> LoadServiceModuleOptions(FacilityStructureType structure, double security)
        {
            var result = new List<FacilityServiceModuleOption>();
            if (structure == null || structure.IsNpcStation)
            {
                return result;
            }

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT DISTINCT it.typeID, it.typeName, groups.groupName,
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1970), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1074), 0)
FROM INVENTORY_TYPES it
JOIN INVENTORY_GROUPS groups ON groups.groupID = it.groupID
WHERE it.published = 1
  AND ABS(groups.categoryID) = 66
  AND groups.groupName LIKE '%Service Module%'
ORDER BY groups.groupName, it.typeName";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var typeId = reader.GetInt32(0);
                        var disallowHighSec = reader.GetDouble(3) != 0;
                        var disallowEmpire = reader.GetDouble(4) != 0;
                        if (security >= 0.45 && (disallowHighSec || disallowEmpire))
                        {
                            continue;
                        }

                        if (ServiceModuleCanFit(connection, typeId, structure.TypeId))
                        {
                            result.Add(new FacilityServiceModuleOption
                            {
                                TypeId = typeId,
                                Name = reader.GetString(1),
                                GroupName = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<FacilityStructureType> LoadStructureTypes(string productionType)
        {
            return LoadStructureTypes(productionType, 0.5);
        }

        public IReadOnlyList<FacilityStructureType> LoadStructureTypes(string productionType, double security)
        {
            return LoadStructureTypes(productionType, security, true);
        }

        public IReadOnlyList<FacilityStructureType> LoadStructureTypes(string productionType, double security, bool includeNpcStation)
        {
            var allStructures = LoadStructureTypes();
            var moduleIds = GetRequiredServiceModuleIds(productionType);
            if (moduleIds.Length == 0)
            {
                return allStructures;
            }

            var result = new List<FacilityStructureType>();
            using (var connection = OpenConnection())
            {
                foreach (var structure in allStructures)
                {
                    if (!StructureAllowedInSecurity(structure, security))
                    {
                        continue;
                    }

                    if (!StructureAllowedForProductionGroup(structure, productionType))
                    {
                        continue;
                    }

                    if (structure.IsNpcStation)
                    {
                        if (!includeNpcStation)
                        {
                            continue;
                        }

                        if (productionType == "Manufacturing")
                        {
                            result.Add(structure);
                        }

                        continue;
                    }

                    if (moduleIds.Any(moduleId => ServiceModuleCanFit(connection, moduleId, structure.TypeId)))
                    {
                        result.Add(structure);
                    }
                }
            }

            return result;
        }

        private static bool StructureAllowedInSecurity(FacilityStructureType structure, double security)
        {
            if (structure == null || structure.IsNpcStation)
            {
                return true;
            }

            return !(security >= 0.45 && (structure.DisallowInHighSec || structure.DisallowInEmpire));
        }

        private static bool StructureAllowedForProductionGroup(FacilityStructureType structure, string productionType)
        {
            if (structure == null)
            {
                return false;
            }

            if (structure.IsNpcStation)
            {
                return productionType == "Manufacturing";
            }

            switch (productionType)
            {
                case "Reactions":
                    return structure.GroupId == 1406;
                case "Reprocessing":
                    return structure.GroupId == 1406;
                case "Moon Mining":
                    return structure.GroupId == 1406;
                case "Components":
                case "Capital Components":
                case "T3 Cruiser Manufacturing":
                case "T3 Destroyer Manufacturing":
                case "Subsystem Manufacturing":
                case "Booster Manufacturing":
                    return structure.GroupId == 1404;
                case "Capital Manufacturing":
                    return structure.GroupId == 1404 || structure.GroupId == 1657;
                case "Supercapital Manufacturing":
                    return structure.TypeId == 35827;
                default:
                    return true;
            }
        }

        public IReadOnlyList<EveRegion> LoadRegions()
        {
            var result = new List<EveRegion>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT regionID, regionName
FROM REGIONS
WHERE regionID BETWEEN 10000001 AND 10000070
  AND regionID NOT IN (10000004, 10000017, 10000019)
ORDER BY regionName";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new EveRegion
                        {
                            RegionId = reader.GetInt64(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<EveSolarSystem> LoadSystems(long regionId)
        {
            var result = new List<EveSolarSystem>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT solarSystemID, solarSystemName, security,
       (SELECT COUNT(*) FROM STATIONS WHERE STATIONS.SOLAR_SYSTEM_ID = SOLAR_SYSTEMS.solarSystemID)
FROM SOLAR_SYSTEMS
WHERE regionID = @regionId
ORDER BY solarSystemName";
                command.Parameters.AddWithValue("@regionId", regionId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new EveSolarSystem
                        {
                            RegionId = regionId,
                            SolarSystemId = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            Security = reader.GetDouble(2),
                            NpcStationCount = reader.GetInt32(3)
                        });
                    }
                }
            }

            return result;
        }

        public EveSolarSystem FindSystem(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
            {
                return null;
            }

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT regionID, solarSystemID, solarSystemName, security,
       (SELECT COUNT(*) FROM STATIONS WHERE STATIONS.SOLAR_SYSTEM_ID = SOLAR_SYSTEMS.solarSystemID)
FROM SOLAR_SYSTEMS
WHERE solarSystemName = @name COLLATE NOCASE
LIMIT 1";
                command.Parameters.AddWithValue("@name", systemName.Trim());
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new EveSolarSystem
                    {
                        RegionId = reader.GetInt64(0),
                        SolarSystemId = reader.GetInt64(1),
                        Name = reader.GetString(2),
                        Security = reader.GetDouble(3),
                        NpcStationCount = reader.GetInt32(4)
                    };
                }
            }
        }

        public bool HasNpcStations(long solarSystemId, string systemName)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                if (solarSystemId > 0)
                {
                    command.CommandText = "SELECT COUNT(*) FROM STATIONS WHERE SOLAR_SYSTEM_ID = @systemId";
                    command.Parameters.AddWithValue("@systemId", solarSystemId);
                }
                else
                {
                    command.CommandText = @"
SELECT COUNT(*)
FROM STATIONS
JOIN SOLAR_SYSTEMS ON SOLAR_SYSTEMS.solarSystemID = STATIONS.SOLAR_SYSTEM_ID
WHERE SOLAR_SYSTEMS.solarSystemName = @name COLLATE NOCASE";
                    command.Parameters.AddWithValue("@name", systemName ?? "");
                }

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public IReadOnlyList<FacilityRigOption> LoadRigOptions(FacilityStructureType structure, double security)
        {
            return LoadRigOptions(structure, security, "Manufacturing");
        }

        public IReadOnlyList<FacilityRigOption> LoadRigOptions(FacilityStructureType structure, double security, string productionType)
        {
            var result = new List<FacilityRigOption>
            {
                new FacilityRigOption { TypeId = 0, Name = "No rig", IsNone = true }
            };

            if (structure == null || structure.IsNpcStation)
            {
                return result;
            }

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT DISTINCT it.typeID, it.typeName, groups.groupName,
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1547), -1),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1153), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1970), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1074), 0)
FROM INVENTORY_TYPES it
JOIN INVENTORY_GROUPS groups ON groups.groupID = it.groupID
WHERE it.published = 1
  AND ABS(groups.categoryID) = 66
  AND (" + BuildRigGroupFilter(productionType) + @")
ORDER BY groups.groupName, it.typeName";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rig = new FacilityRigOption
                        {
                            TypeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            GroupName = reader.GetString(2),
                            RigFamily = GetRigFamily(reader.GetString(2)),
                            RigSize = Convert.ToInt32(reader.GetDouble(3)),
                            CalibrationCost = reader.GetDouble(4),
                            DisallowInHighSec = reader.GetDouble(5) != 0,
                            DisallowInEmpire = reader.GetDouble(6) != 0
                        };

                        if (IsRigAllowed(connection, structure, rig, security))
                        {
                            result.Add(rig);
                        }
                    }
                }
            }

            return result;
        }

        private static string BuildRigGroupFilter(string productionType)
        {
            switch (productionType)
            {
                case "Reactions":
                    return "groups.groupName LIKE '%Reactor Rig%'";
                case "Reprocessing":
                    return "groups.groupName LIKE '%Resource Rig%'";
                case "Moon Mining":
                    return "groups.groupName LIKE '%Drilling Rig%'";
                default:
                    return "groups.groupName LIKE '%Engineering Rig%'";
            }
        }

        private static string GetRigFamily(string groupName)
        {
            if (groupName == null)
            {
                return "";
            }

            if (groupName.IndexOf("Reactor Rig", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "ReactionRigs";
            }

            if (groupName.IndexOf("Resource Rig", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "ReprocessingRigs";
            }

            if (groupName.IndexOf("Drilling Rig", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "DrillingRigs";
            }

            if (groupName.IndexOf("Engineering Rig", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "EngineeringRigs";
            }

            return "";
        }

        public double GetSystemSecurity(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
            {
                return 0.5;
            }

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT security FROM SOLAR_SYSTEMS WHERE solarSystemName = @name COLLATE NOCASE LIMIT 1";
                command.Parameters.AddWithValue("@name", systemName.Trim());
                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0.5 : Convert.ToDouble(value);
            }
        }

        public double GetSystemCostIndex(long solarSystemId, string productionType)
        {
            if (solarSystemId <= 0)
            {
                return 0;
            }

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COST_INDEX
FROM INDUSTRY_SYSTEMS_COST_INDICIES
WHERE SOLAR_SYSTEM_ID = @systemId
  AND ACTIVITY_ID = @activityId
LIMIT 1";
                command.Parameters.AddWithValue("@systemId", solarSystemId);
                command.Parameters.AddWithValue("@activityId", GetActivityId(productionType));
                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToDouble(value);
            }
        }

        public FacilityMathResult Calculate(FacilityStation station, IEnumerable<int> rigTypeIds)
        {
            return Calculate(station, rigTypeIds, null, null);
        }

        public FacilityMathResult Calculate(FacilityStation station, IEnumerable<int> rigTypeIds, int? itemGroupId, int? itemCategoryId)
        {
            var result = new FacilityMathResult { MaterialMultiplier = 1, TimeMultiplier = 1, CostMultiplier = 1 };
            if (station == null || station.StructureTypeId == 0)
            {
                return result;
            }

            using (var connection = OpenConnection())
            {
                result = LoadBaseMultipliers(connection, station.StructureTypeId, GetActivityId(station.ProductionType));
                var security = GetSystemSecurity(station.SystemName);
                if (!itemGroupId.HasValue || !itemCategoryId.HasValue)
                {
                    result.Warning = "Rig bonuses depend on the concrete blueprint group/category";
                    result.MaterialBonusPercent = station.ProductionType == "Reprocessing"
                        ? Math.Max(0, result.MaterialMultiplier * 100)
                        : Math.Max(0, (1 - result.MaterialMultiplier) * 100);
                    return result;
                }

                foreach (var rigId in rigTypeIds.Where(id => id > 0).Distinct())
                {
                    ApplyRigBonuses(connection, rigId, security, GetActivityId(station.ProductionType), itemGroupId.Value, itemCategoryId.Value, result);
                }
            }

            result.MaterialBonusPercent = station.ProductionType == "Reprocessing"
                ? Math.Max(0, result.MaterialMultiplier * 100)
                : Math.Max(0, (1 - result.MaterialMultiplier) * 100);
            return result;
        }

        public string ValidateProduction(FacilityStation station)
        {
            if (station == null)
            {
                return "Station is not selected";
            }

            if (station.StructureTypeId == 0)
            {
                if (station.ProductionType != "Manufacturing")
                {
                    return "NPC Station supports only Manufacturing";
                }

                return HasNpcStations(station.SolarSystemId, station.SystemName)
                    ? null
                    : string.Format("System {0} has no NPC stations", station.SystemName);
            }

            var structure = LoadStructureTypes().FirstOrDefault(item => item.TypeId == station.StructureTypeId);
            if (!StructureAllowedInSecurity(structure, station.Security))
            {
                return string.Format("{0} cannot be used in a system with security {1:0.0}", station.FacilityType, station.Security);
            }

            if (!StructureAllowedForProductionGroup(structure, station.ProductionType))
            {
                return string.Format("{0} does not support production type {1}", station.FacilityType, station.ProductionType);
            }

            var selectedRigIds = new[] { station.RigSlot1TypeId, station.RigSlot2TypeId, station.RigSlot3TypeId }
                .Where(id => id > 0)
                .ToArray();
            if (structure != null && selectedRigIds.Length > structure.RigSlots)
            {
                return string.Format("{0} supports only {1} rig slots", station.FacilityType, structure.RigSlots);
            }

            if (selectedRigIds.Length != selectedRigIds.Distinct().Count())
            {
                return "Duplicate rigs cannot be installed on one structure";
            }

            var allowedRigs = LoadRigOptions(structure, station.Security, station.ProductionType)
                .Where(item => !item.IsNone)
                .Select(item => item.TypeId)
                .ToHashSet();
            var invalidRigId = selectedRigIds.FirstOrDefault(id => !allowedRigs.Contains(id));
            if (invalidRigId > 0)
            {
                return string.Format("{0} is not valid for {1} / {2}", GetTypeName(invalidRigId), station.FacilityType, station.ProductionType);
            }

            using (var connection = OpenConnection())
            {
                var calibrationUsed = selectedRigIds.Sum(id => GetRigCalibration(connection, id));
                if (structure != null && structure.Calibration > 0 && calibrationUsed > structure.Calibration)
                {
                    return string.Format("Rigs require {0:0.##} calibration; {1} has {2:0.##}", calibrationUsed, station.FacilityType, structure.Calibration);
                }
            }

            var moduleIds = GetRequiredServiceModuleIds(station.ProductionType);
            if (moduleIds.Length == 0)
            {
                return null;
            }

            var installedModules = new[]
            {
                station.ServiceModule1TypeId,
                station.ServiceModule2TypeId,
                station.ServiceModule3TypeId,
                station.ServiceModule4TypeId,
                station.ServiceModule5TypeId
            };
            var duplicateServiceModuleId = installedModules
                .Where(id => id > 0)
                .GroupBy(id => id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();
            if (duplicateServiceModuleId > 0)
            {
                return string.Format("Duplicate service modules cannot be installed: {0}", GetTypeName(duplicateServiceModuleId));
            }

            using (var connection = OpenConnection())
            {
                var serviceSlots = structure == null ? 0 : structure.ServiceSlots;
                if (installedModules.Count(id => id > 0) > serviceSlots)
                {
                    return string.Format("{0} supports only {1} service modules", station.FacilityType, serviceSlots);
                }

                var installedModuleIds = installedModules.Where(id => id > 0).ToArray();
                var invalidFitModuleId = installedModuleIds.FirstOrDefault(id => !ServiceModuleCanFit(connection, id, station.StructureTypeId));
                if (invalidFitModuleId > 0)
                {
                    return string.Format("{0} cannot be installed on {1}", GetTypeName(invalidFitModuleId), station.FacilityType);
                }

                var invalidSecurityModuleId = installedModuleIds.FirstOrDefault(id => !ServiceModuleAllowedInSecurity(connection, id, station.Security));
                if (invalidSecurityModuleId > 0)
                {
                    return string.Format("{0} cannot be used in a system with security {1:0.0}", GetTypeName(invalidSecurityModuleId), station.Security);
                }

                var requiredModuleIds = new HashSet<int>(moduleIds);
                if (!installedModuleIds.Any(requiredModuleIds.Contains))
                {
                    return string.Format("Production type {0} requires one of these service modules: {1}", station.ProductionType, string.Join(" / ", moduleIds.Select(GetTypeName)));
                }

                return installedModuleIds.Any(id => requiredModuleIds.Contains(id)
                                                   && ServiceModuleCanFit(connection, id, station.StructureTypeId)
                                                   && ServiceModuleAllowedInSecurity(connection, id, station.Security))
                    ? null
                    : string.Format("{0} does not support required service module for {1}", station.FacilityType, station.ProductionType);
            }
        }

        public int GetActivityId(string productionType)
        {
            switch (productionType)
            {
                case "Copying":
                    return 5;
                case "Research":
                    return 4;
                case "Invention":
                case "T3 Invention":
                    return 8;
                case "Reactions":
                    return 11;
                case "Reprocessing":
                    return -2;
                default:
                    return 1;
            }
        }

        public int[] GetRequiredServiceModuleIds(string productionType)
        {
            switch (productionType)
            {
                case "Capital Manufacturing":
                    return new[] { 35881 };
                case "Supercapital Manufacturing":
                    return new[] { 35877 };
                case "Reactions":
                    return new[] { 45537, 45538, 45539 };
                case "Reprocessing":
                    return new[] { 35899 };
                case "Invention":
                case "T3 Invention":
                    return new[] { 35886 };
                case "Research":
                case "Copying":
                    return new[] { 35891, 45550 };
                case "Moon Mining":
                    return new[] { 45009 };
                case "Manufacturing":
                case "Components":
                case "Capital Components":
                case "T3 Cruiser Manufacturing":
                case "T3 Destroyer Manufacturing":
                case "Subsystem Manufacturing":
                case "Booster Manufacturing":
                    return new[] { 35878 };
                default:
                    return new int[0];
            }
        }
        private string GetTypeName(int typeId)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT typeName FROM INVENTORY_TYPES WHERE typeID = @typeId";
                command.Parameters.AddWithValue("@typeId", typeId);
                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? typeId.ToString() : value.ToString();
            }
        }

        private static bool ServiceModuleAllowedInSecurity(SQLiteConnection connection, int moduleTypeId, double security)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
  COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = @moduleId AND attributeID = 1970), 0),
  COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = @moduleId AND attributeID = 1074), 0)";
                command.Parameters.AddWithValue("@moduleId", moduleTypeId);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return true;
                    }

                    var disallowHighSec = reader.GetDouble(0) != 0;
                    var disallowEmpire = reader.GetDouble(1) != 0;
                    return security < 0.45 || (!disallowHighSec && !disallowEmpire);
                }
            }
        }

        private static bool ServiceModuleCanFit(SQLiteConnection connection, int moduleTypeId, int structureTypeId)
        {
            using (var groupCommand = connection.CreateCommand())
            {
                groupCommand.CommandText = "SELECT groupID FROM INVENTORY_TYPES WHERE typeID = @typeId";
                groupCommand.Parameters.AddWithValue("@typeId", structureTypeId);
                var groupValue = groupCommand.ExecuteScalar();
                var structureGroupId = groupValue == null || groupValue == DBNull.Value ? 0 : Convert.ToInt32(groupValue);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT attr.value
FROM TYPE_ATTRIBUTES attr
JOIN ATTRIBUTE_TYPES types ON types.attributeID = attr.attributeID
WHERE attr.typeID = @moduleId
  AND (types.attributeName LIKE 'canFitShipType%' OR types.attributeName LIKE 'canFitShipGroup%')";
                    command.Parameters.AddWithValue("@moduleId", moduleTypeId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var allowedId = Convert.ToInt32(reader.GetDouble(0));
                            if (allowedId == structureTypeId || allowedId == structureGroupId)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsRigAllowed(SQLiteConnection connection, FacilityStructureType structure, FacilityRigOption rig, double security)
        {
            if (rig.RigSize != -1 && structure.RigSize != rig.RigSize)
            {
                return false;
            }

            if (security >= 0.45 && (rig.DisallowInHighSec || rig.DisallowInEmpire))
            {
                return false;
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT attr.value
FROM TYPE_ATTRIBUTES attr
JOIN ATTRIBUTE_TYPES types ON types.attributeID = attr.attributeID
WHERE attr.typeID = @rigId
  AND (types.attributeName LIKE 'canFitShipType%' OR types.attributeName LIKE 'canFitShipGroup%')";
                command.Parameters.AddWithValue("@rigId", rig.TypeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var allowedId = Convert.ToInt32(reader.GetDouble(0));
                        if (allowedId == structure.TypeId || allowedId == structure.GroupId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static double GetRigCalibration(SQLiteConnection connection, int rigTypeId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = @typeId AND attributeID = 1153 LIMIT 1";
                command.Parameters.AddWithValue("@typeId", rigTypeId);
                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToDouble(value);
            }
        }

        private FacilityMathResult LoadBaseMultipliers(SQLiteConnection connection, int structureTypeId, int activityId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT MATERIAL_MULTIPLIER, TIME_MULTIPLIER, COST_MULTIPLIER
FROM UPWELL_STRUCTURES
WHERE UPWELL_STRUCTURE_TYPE_ID = @typeId AND ACTIVITY_ID = @activityId
LIMIT 1";
                command.Parameters.AddWithValue("@typeId", structureTypeId);
                command.Parameters.AddWithValue("@activityId", activityId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new FacilityMathResult
                        {
                            MaterialMultiplier = reader.IsDBNull(0) ? 1 : reader.GetDouble(0),
                            TimeMultiplier = reader.IsDBNull(1) ? 1 : reader.GetDouble(1),
                            CostMultiplier = reader.IsDBNull(2) ? 1 : reader.GetDouble(2)
                        };
                    }
                }
            }

            return new FacilityMathResult { MaterialMultiplier = 1, TimeMultiplier = 1, CostMultiplier = 1 };
        }

        private void ApplyRigBonuses(SQLiteConnection connection, int rigTypeId, double security, int activityId, int itemGroupId, int itemCategoryId, FacilityMathResult result)
        {
            if (!RigAppliesToItem(connection, rigTypeId, activityId, itemGroupId, itemCategoryId))
            {
                return;
            }

            foreach (var bonus in GetRigBonuses(connection, rigTypeId, security))
            {
                switch (bonus.AttributeId)
                {
                    case AttributeRefiningYield:
                        if (activityId == -2)
                        {
                            result.MaterialMultiplier += bonus.Value;
                        }
                        break;
                    case AttributeEngRigMaterialBonus:
                    case AttributeRefRigMaterialBonus:
                        result.MaterialMultiplier *= 1 - bonus.Value;
                        break;
                    case AttributeThukkerMaterialBonus:
                        if (itemGroupId == 873 || itemGroupId == 913)
                        {
                            result.MaterialMultiplier *= 1 - bonus.Value;
                        }
                        break;
                    case AttributeEngRigTimeBonus:
                    case AttributeRefRigTimeBonus:
                        result.TimeMultiplier *= 1 - bonus.Value;
                        break;
                    case AttributeEngRigCostBonus:
                        result.CostMultiplier *= 1 - bonus.Value;
                        break;
                }
            }
        }

        private static bool RigAppliesToItem(SQLiteConnection connection, int rigTypeId, int activityId, int itemGroupId, int itemCategoryId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COUNT(*)
FROM ENGINEERING_RIG_BONUSES
WHERE typeID = @rigId
  AND activityID = @activityId
  AND ((categoryID = @categoryId AND groupID IS NULL) OR (categoryID IS NULL AND groupID = @groupId))";
                command.Parameters.AddWithValue("@rigId", rigTypeId);
                command.Parameters.AddWithValue("@activityId", activityId);
                command.Parameters.AddWithValue("@categoryId", itemCategoryId);
                command.Parameters.AddWithValue("@groupId", itemGroupId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static IReadOnlyList<RigBonus> GetRigBonuses(SQLiteConnection connection, int rigTypeId, double security)
        {
            var result = new List<RigBonus>();
            var securityAttribute = security <= 0.0 ? AttributeNullSecModifier : security < 0.45 ? AttributeLowSecModifier : AttributeHiSecModifier;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT attributeID, ABS(value * (SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = @rigId AND attributeID = @secAttribute) / 100.0)
FROM TYPE_ATTRIBUTES
WHERE typeID = @rigId
  AND attributeID IN (@timeAttribute, @matAttribute, @costAttribute, @thukkerAttribute, @refYieldAttribute, @refTimeAttribute, @refMatAttribute)
  AND value <> 0";
                command.Parameters.AddWithValue("@rigId", rigTypeId);
                command.Parameters.AddWithValue("@secAttribute", securityAttribute);
                command.Parameters.AddWithValue("@timeAttribute", AttributeEngRigTimeBonus);
                command.Parameters.AddWithValue("@matAttribute", AttributeEngRigMaterialBonus);
                command.Parameters.AddWithValue("@costAttribute", AttributeEngRigCostBonus);
                command.Parameters.AddWithValue("@thukkerAttribute", AttributeThukkerMaterialBonus);
                command.Parameters.AddWithValue("@refYieldAttribute", AttributeRefiningYield);
                command.Parameters.AddWithValue("@refTimeAttribute", AttributeRefRigTimeBonus);
                command.Parameters.AddWithValue("@refMatAttribute", AttributeRefRigMaterialBonus);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new RigBonus
                        {
                            AttributeId = reader.GetInt32(0),
                            Value = reader.GetDouble(1)
                        });
                    }
                }
            }

            return result;
        }

        private sealed class RigBonus
        {
            public int AttributeId { get; set; }
            public double Value { get; set; }
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;Read Only=True;");
            connection.Open();
            return connection;
        }
    }
}
