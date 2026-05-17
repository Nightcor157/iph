using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class EveDatabaseService
    {
        private readonly string _databasePath;

        public EveDatabaseService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool IsDatabaseAvailable()
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ALL_BLUEPRINTS_FACT'";
                return (long)command.ExecuteScalar() > 0;
            }
        }

        public IReadOnlyList<BlueprintSearchResult> SearchBlueprints(string searchText, int limit)
        {
            var results = new List<BlueprintSearchResult>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    bp.BLUEPRINT_ID,
    bp.ITEM_ID,
    bpt.typeName AS BlueprintName,
    product.typeName AS ProductName,
    groups.groupName,
    categories.categoryName,
    product.groupID,
    groups.categoryID,
    bp.ITEM_MARKET_GROUP,
    bp.BP_MARKET_GROUP,
    COALESCE(bp.META_GROUP, product.metaGroupID, 0),
    bp.TECH_LEVEL,
    bp.PORTION_SIZE,
    bp.BASE_PRODUCTION_TIME,
    bp.MAX_PRODUCTION_LIMIT,
    EXISTS (SELECT 1 FROM ALL_BLUEPRINT_MATERIALS_FACT mats WHERE mats.BLUEPRINT_ID = bp.BLUEPRINT_ID AND mats.ACTIVITY = 11)
FROM ALL_BLUEPRINTS_FACT bp
JOIN INVENTORY_TYPES bpt ON bpt.typeID = bp.BLUEPRINT_ID
JOIN INVENTORY_TYPES product ON product.typeID = bp.ITEM_ID
LEFT JOIN INVENTORY_GROUPS groups ON groups.groupID = product.groupID
LEFT JOIN INVENTORY_CATEGORIES categories ON categories.categoryID = groups.categoryID
WHERE bp.IGNORE = 0
  AND product.published = 1
  AND bpt.published = 1
  AND (@search = ''
       OR bpt.typeName LIKE @search COLLATE NOCASE
       OR product.typeName LIKE @search COLLATE NOCASE
       OR bp.BLUEPRINT_ID = @searchTypeId
       OR bp.ITEM_ID = @searchTypeId)
ORDER BY product.typeName
LIMIT @limit";
                var query = string.IsNullOrWhiteSpace(searchText) ? "" : "%" + searchText.Trim() + "%";
                long searchTypeId;
                command.Parameters.AddWithValue("@search", query);
                command.Parameters.AddWithValue("@searchTypeId", long.TryParse((searchText ?? "").Trim(), out searchTypeId) ? searchTypeId : -1);
                command.Parameters.AddWithValue("@limit", limit);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new BlueprintSearchResult
                        {
                            BlueprintTypeId = reader.GetInt64(0),
                            ProductTypeId = reader.GetInt64(1),
                            BlueprintName = reader.GetString(2),
                            ProductName = reader.GetString(3),
                            GroupName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            CategoryName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            GroupId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            CategoryId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                            ItemMarketGroup = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            BlueprintMarketGroup = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            MetaGroupId = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                            TechLevel = reader.GetInt32(11),
                            PortionSize = reader.GetInt32(12),
                            BaseProductionTime = reader.GetInt32(13),
                            MaxProductionLimit = reader.GetInt32(14),
                            IsCopyOnlyBlueprint = reader.IsDBNull(9) || string.IsNullOrWhiteSpace(reader.GetString(9)),
                            HasReactionActivity = reader.GetInt32(15) != 0
                        });
                    }
                }
            }

            return results;
        }

        public BlueprintSearchResult FindBlueprintByProduct(long productTypeId)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    bp.BLUEPRINT_ID,
    bp.ITEM_ID,
    bpt.typeName AS BlueprintName,
    product.typeName AS ProductName,
    groups.groupName,
    categories.categoryName,
    product.groupID,
    groups.categoryID,
    bp.ITEM_MARKET_GROUP,
    bp.BP_MARKET_GROUP,
    COALESCE(bp.META_GROUP, product.metaGroupID, 0),
    bp.TECH_LEVEL,
    bp.PORTION_SIZE,
    bp.BASE_PRODUCTION_TIME,
    bp.MAX_PRODUCTION_LIMIT,
    EXISTS (SELECT 1 FROM ALL_BLUEPRINT_MATERIALS_FACT mats WHERE mats.BLUEPRINT_ID = bp.BLUEPRINT_ID AND mats.ACTIVITY = 11)
FROM ALL_BLUEPRINTS_FACT bp
JOIN INVENTORY_TYPES bpt ON bpt.typeID = bp.BLUEPRINT_ID
JOIN INVENTORY_TYPES product ON product.typeID = bp.ITEM_ID
LEFT JOIN INVENTORY_GROUPS groups ON groups.groupID = product.groupID
LEFT JOIN INVENTORY_CATEGORIES categories ON categories.categoryID = groups.categoryID
WHERE bp.IGNORE = 0
  AND product.published = 1
  AND bpt.published = 1
  AND bp.ITEM_ID = @productTypeId
ORDER BY bp.BLUEPRINT_ID
LIMIT 1";
                command.Parameters.AddWithValue("@productTypeId", productTypeId);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new BlueprintSearchResult
                    {
                        BlueprintTypeId = reader.GetInt64(0),
                        ProductTypeId = reader.GetInt64(1),
                        BlueprintName = reader.GetString(2),
                        ProductName = reader.GetString(3),
                        GroupName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        CategoryName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        GroupId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        CategoryId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                        ItemMarketGroup = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        BlueprintMarketGroup = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        MetaGroupId = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                        TechLevel = reader.GetInt32(11),
                        PortionSize = reader.GetInt32(12),
                        BaseProductionTime = reader.GetInt32(13),
                        MaxProductionLimit = reader.GetInt32(14),
                        IsCopyOnlyBlueprint = reader.IsDBNull(9) || string.IsNullOrWhiteSpace(reader.GetString(9)),
                        HasReactionActivity = reader.GetInt32(15) != 0
                    };
                }
            }
        }

        public IReadOnlyList<MaterialRequirement> GetManufacturingMaterials(long blueprintTypeId)
        {
            var results = new List<MaterialRequirement>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT mats.MATERIAL_ID, types.typeName, mats.QUANTITY
FROM ALL_BLUEPRINT_MATERIALS_FACT mats
JOIN INVENTORY_TYPES types ON types.typeID = mats.MATERIAL_ID
WHERE mats.BLUEPRINT_ID = @blueprintTypeId
  AND mats.ACTIVITY IN (1, 11)
  AND (mats.MATERIAL_CATEGORY_ID IS NULL OR mats.MATERIAL_CATEGORY_ID <> 16)
ORDER BY types.typeName";
                command.Parameters.AddWithValue("@blueprintTypeId", blueprintTypeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new MaterialRequirement
                        {
                            TypeId = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt64(2)
                        });
                    }
                }
            }

            return results;
        }

        public IReadOnlyList<SkillRequirement> GetRequiredSkills(long blueprintTypeId, int activityId)
        {
            var results = new List<SkillRequirement>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT mats.MATERIAL_ID, types.typeName, mats.QUANTITY
FROM ALL_BLUEPRINT_MATERIALS_FACT mats
JOIN INVENTORY_TYPES types ON types.typeID = mats.MATERIAL_ID
WHERE mats.BLUEPRINT_ID = @blueprintTypeId
  AND mats.ACTIVITY = @activityId
  AND mats.MATERIAL_CATEGORY_ID = 16
ORDER BY types.typeName";
                command.Parameters.AddWithValue("@blueprintTypeId", blueprintTypeId);
                command.Parameters.AddWithValue("@activityId", activityId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new SkillRequirement
                        {
                            TypeId = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            Level = reader.GetInt32(2)
                        });
                    }
                }
            }

            return results;
        }

        public IReadOnlyList<SkillRequirement> GetAllIndustryRequiredSkills()
        {
            var results = new List<SkillRequirement>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT mats.MATERIAL_ID, types.typeName, MAX(mats.QUANTITY) AS RequiredLevel
FROM ALL_BLUEPRINT_MATERIALS_FACT mats
JOIN INVENTORY_TYPES types ON types.typeID = mats.MATERIAL_ID
JOIN ALL_BLUEPRINTS_FACT bp ON bp.BLUEPRINT_ID = mats.BLUEPRINT_ID
WHERE bp.IGNORE = 0
  AND mats.MATERIAL_CATEGORY_ID = 16
  AND mats.ACTIVITY IN (1, 8, 11)
GROUP BY mats.MATERIAL_ID, types.typeName
ORDER BY types.typeName";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new SkillRequirement
                        {
                            TypeId = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            Level = reader.GetInt32(2)
                        });
                    }
                }
            }

            return results;
        }

        public bool IsMineral(long typeId)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT groupID FROM INVENTORY_TYPES WHERE typeID = @typeId LIMIT 1";
                command.Parameters.AddWithValue("@typeId", typeId);
                var value = command.ExecuteScalar();
                return value != null && value != System.DBNull.Value && System.Convert.ToInt32(value) == 18;
            }
        }

        public IReadOnlyList<ReprocessingOption> GetReprocessingOptionsForMineral(long mineralTypeId, bool compressedOnly)
        {
            var results = new List<ReprocessingOption>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT rp.ITEM_ID,
       rp.ITEM_NAME,
       rp.ITEM_VOLUME,
       rp.UNITS_TO_REPROCESS,
       ores.COMPRESSED,
       types.groupID,
       groups.categoryID,
       rp.REFINED_MATERIAL_ID,
       rp.REFINED_MATERIAL,
       rp.REFINED_MATERIAL_QUANTITY
FROM REPROCESSING rp
JOIN ORES ores ON ores.ORE_ID = rp.ITEM_ID
JOIN INVENTORY_TYPES types ON types.typeID = rp.ITEM_ID
LEFT JOIN INVENTORY_GROUPS groups ON groups.groupID = types.groupID
WHERE rp.REFINED_MATERIAL_ID = @mineralTypeId
  AND (@compressedOnly = 0 OR ores.COMPRESSED <> 0)
ORDER BY rp.ITEM_NAME";
                command.Parameters.AddWithValue("@mineralTypeId", mineralTypeId);
                command.Parameters.AddWithValue("@compressedOnly", compressedOnly ? 1 : 0);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ReprocessingOption
                        {
                            OreTypeId = reader.GetInt64(0),
                            OreName = reader.GetString(1),
                            OreVolume = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                            UnitsToReprocess = reader.IsDBNull(3) ? 1 : reader.GetInt32(3),
                            IsCompressed = !reader.IsDBNull(4) && reader.GetInt32(4) != 0,
                            OreGroupId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            OreCategoryId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            MineralTypeId = reader.GetInt64(7),
                            MineralName = reader.GetString(8),
                            MineralQuantity = reader.GetInt64(9)
                        });
                    }
                }
            }

            return results;
        }

        public IReadOnlyList<ReprocessingOption> GetReprocessingOutputsForOres(IEnumerable<long> oreTypeIds)
        {
            var ids = oreTypeIds.Distinct().ToList();
            var results = new List<ReprocessingOption>();
            if (ids.Count == 0)
            {
                return results;
            }

            using (var connection = OpenConnection())
            {
                foreach (var batch in Split(ids, 500))
                using (var command = connection.CreateCommand())
                {
                    var parameterNames = new List<string>();
                    for (var i = 0; i < batch.Count; i++)
                    {
                        var name = "@id" + i;
                        parameterNames.Add(name);
                        command.Parameters.AddWithValue(name, batch[i]);
                    }

                    command.CommandText = @"
SELECT rp.ITEM_ID,
       rp.ITEM_NAME,
       rp.ITEM_VOLUME,
       rp.UNITS_TO_REPROCESS,
       ores.COMPRESSED,
       types.groupID,
       groups.categoryID,
       rp.REFINED_MATERIAL_ID,
       rp.REFINED_MATERIAL,
       rp.REFINED_MATERIAL_QUANTITY
FROM REPROCESSING rp
JOIN ORES ores ON ores.ORE_ID = rp.ITEM_ID
JOIN INVENTORY_TYPES types ON types.typeID = rp.ITEM_ID
LEFT JOIN INVENTORY_GROUPS groups ON groups.groupID = types.groupID
WHERE rp.ITEM_ID IN (" + string.Join(",", parameterNames) + @")
ORDER BY rp.ITEM_NAME, rp.REFINED_MATERIAL";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new ReprocessingOption
                            {
                                OreTypeId = reader.GetInt64(0),
                                OreName = reader.GetString(1),
                                OreVolume = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                UnitsToReprocess = reader.IsDBNull(3) ? 1 : reader.GetInt32(3),
                                IsCompressed = !reader.IsDBNull(4) && reader.GetInt32(4) != 0,
                                OreGroupId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                OreCategoryId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                MineralTypeId = reader.GetInt64(7),
                                MineralName = reader.GetString(8),
                                MineralQuantity = reader.GetInt64(9)
                            });
                        }
                    }
                }
            }

            return results;
        }

        public bool TryGetOwnedBlueprintEfficiency(long blueprintTypeId, out int materialEfficiency, out int timeEfficiency)
        {
            materialEfficiency = 0;
            timeEfficiency = 0;

            using (var connection = OpenConnection())
            using (var tableCommand = connection.CreateCommand())
            {
                tableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='OWNED_BLUEPRINTS'";
                if ((long)tableCommand.ExecuteScalar() == 0)
                {
                    return false;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT ME, TE
FROM OWNED_BLUEPRINTS
WHERE BLUEPRINT_ID = @blueprintTypeId
  AND OWNED <> 0
ORDER BY ME DESC, TE DESC
LIMIT 1";
                    command.Parameters.AddWithValue("@blueprintTypeId", blueprintTypeId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        materialEfficiency = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        timeEfficiency = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        return true;
                    }
                }
            }
        }

        public InventionInfo GetInventionInfo(long targetBlueprintTypeId)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT products.blueprintTypeID,
       products.quantity,
       products.probability,
       COALESCE(bp.BASE_COPY_TIME, 0),
       COALESCE(bp.BASE_INVENTION_TIME, 0),
       COALESCE(bp.MAX_PRODUCTION_LIMIT, products.quantity)
FROM INDUSTRY_ACTIVITY_PRODUCTS products
LEFT JOIN ALL_BLUEPRINTS_FACT bp ON bp.BLUEPRINT_ID = products.blueprintTypeID
WHERE products.productTypeID = @targetBlueprintTypeId
  AND products.activityID = 8
LIMIT 1";
                command.Parameters.AddWithValue("@targetBlueprintTypeId", targetBlueprintTypeId);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var info = new InventionInfo
                    {
                        SourceBlueprintTypeId = reader.GetInt64(0),
                        RunsPerSuccess = reader.IsDBNull(1) ? 1 : reader.GetInt32(1),
                        Probability = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                        BaseCopyTime = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        BaseInventionTime = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        MaxProductionLimit = reader.IsDBNull(5) ? 1 : reader.GetInt32(5)
                    };

                    reader.Close();
                    command.Parameters.Clear();
                    command.CommandText = @"
SELECT mats.MATERIAL_ID, types.typeName, mats.QUANTITY
FROM ALL_BLUEPRINT_MATERIALS_FACT mats
JOIN INVENTORY_TYPES types ON types.typeID = mats.MATERIAL_ID
WHERE mats.BLUEPRINT_ID = @sourceBlueprintTypeId
  AND mats.PRODUCT_ID = @targetBlueprintTypeId
  AND mats.ACTIVITY = 8
  AND (mats.MATERIAL_CATEGORY_ID IS NULL OR mats.MATERIAL_CATEGORY_ID <> 16)
  AND (mats.CONSUME IS NULL OR mats.CONSUME <> 0)
ORDER BY types.typeName";
                    command.Parameters.AddWithValue("@sourceBlueprintTypeId", info.SourceBlueprintTypeId);
                    command.Parameters.AddWithValue("@targetBlueprintTypeId", targetBlueprintTypeId);

                    using (var materialReader = command.ExecuteReader())
                    {
                        while (materialReader.Read())
                        {
                            info.Materials.Add(new MaterialRequirement
                            {
                                TypeId = materialReader.GetInt64(0),
                                Name = materialReader.GetString(1),
                                Quantity = materialReader.GetInt64(2)
                            });
                        }
                    }

                    command.Parameters.Clear();
                    command.CommandText = @"
SELECT mats.MATERIAL_ID, types.typeName, mats.QUANTITY
FROM ALL_BLUEPRINT_MATERIALS_FACT mats
JOIN INVENTORY_TYPES types ON types.typeID = mats.MATERIAL_ID
WHERE mats.BLUEPRINT_ID = @sourceBlueprintTypeId
  AND mats.PRODUCT_ID = @sourceBlueprintTypeId
  AND mats.ACTIVITY = 5
  AND (mats.MATERIAL_CATEGORY_ID IS NULL OR mats.MATERIAL_CATEGORY_ID <> 16)
  AND (mats.CONSUME IS NULL OR mats.CONSUME <> 0)
ORDER BY types.typeName";
                    command.Parameters.AddWithValue("@sourceBlueprintTypeId", info.SourceBlueprintTypeId);

                    using (var copyReader = command.ExecuteReader())
                    {
                        while (copyReader.Read())
                        {
                            info.CopyMaterials.Add(new MaterialRequirement
                            {
                                TypeId = copyReader.GetInt64(0),
                                Name = copyReader.GetString(1),
                                Quantity = copyReader.GetInt64(2)
                            });
                        }
                    }

                    return info;
                }
            }
        }

        public IReadOnlyList<DecryptorOption> LoadDecryptors()
        {
            var result = new List<DecryptorOption>
            {
                new DecryptorOption { TypeId = 0, Name = "None", ProbabilityModifier = 1.0 }
            };

            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT it.typeID,
       it.typeName,
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1112), 1),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1113), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1114), 0),
       COALESCE((SELECT value FROM TYPE_ATTRIBUTES WHERE typeID = it.typeID AND attributeID = 1124), 0)
FROM INVENTORY_TYPES it
WHERE it.groupID = 1304
  AND it.published = 1
ORDER BY it.typeName";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new DecryptorOption
                        {
                            TypeId = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            ProbabilityModifier = reader.IsDBNull(2) ? 1.0 : reader.GetDouble(2),
                            MaterialEfficiencyModifier = reader.IsDBNull(3) ? 0 : (int)reader.GetDouble(3),
                            TimeEfficiencyModifier = reader.IsDBNull(4) ? 0 : (int)reader.GetDouble(4),
                            RunModifier = reader.IsDBNull(5) ? 0 : (int)reader.GetDouble(5)
                        });
                    }
                }
            }

            return result;
        }

        public Dictionary<long, double> GetAdjustedPrices(IEnumerable<long> typeIds)
        {
            var ids = typeIds.Distinct().ToList();
            var result = new Dictionary<long, double>();
            if (ids.Count == 0)
            {
                return result;
            }

            using (var connection = OpenConnection())
            {
                foreach (var batch in Split(ids, 500))
                using (var command = connection.CreateCommand())
                {
                    var parameterNames = new List<string>();
                    for (var i = 0; i < batch.Count; i++)
                    {
                        var name = "@id" + i;
                        parameterNames.Add(name);
                        command.Parameters.AddWithValue(name, batch[i]);
                    }

                    command.CommandText = @"
SELECT ITEM_ID,
       CASE
           WHEN ADJUSTED_PRICE IS NOT NULL AND ADJUSTED_PRICE > 0 THEN ADJUSTED_PRICE
           WHEN AVERAGE_PRICE IS NOT NULL AND AVERAGE_PRICE > 0 THEN AVERAGE_PRICE
           WHEN PRICE IS NOT NULL AND PRICE > 0 THEN PRICE
           ELSE 0
       END AS ESTIMATED_PRICE
FROM ITEM_PRICES_FACT
WHERE ITEM_ID IN (" + string.Join(",", parameterNames) + @")";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result[reader.GetInt64(0)] = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<long, string> GetTypeNames(IEnumerable<long> typeIds)
        {
            var ids = typeIds.Distinct().ToList();
            var result = new Dictionary<long, string>();
            if (ids.Count == 0)
            {
                return result;
            }

            using (var connection = OpenConnection())
            {
                foreach (var batch in Split(ids, 500))
                using (var command = connection.CreateCommand())
                {
                    var parameterNames = new List<string>();
                    for (var i = 0; i < batch.Count; i++)
                    {
                        var name = "@id" + i;
                        parameterNames.Add(name);
                        command.Parameters.AddWithValue(name, batch[i]);
                    }

                    command.CommandText = @"
SELECT typeID, typeName
FROM INVENTORY_TYPES
WHERE typeID IN (" + string.Join(",", parameterNames) + @")";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result[reader.GetInt64(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<long, MarketHistoryStats> GetMarketHistoryStats(IEnumerable<long> typeIds, long regionId, int days)
        {
            var ids = typeIds.Distinct().ToList();
            var result = new Dictionary<long, MarketHistoryStats>();
            if (ids.Count == 0 || days <= 0)
            {
                return result;
            }

            using (var connection = OpenConnection())
            using (var tableCommand = connection.CreateCommand())
            {
                tableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MARKET_HISTORY'";
                if ((long)tableCommand.ExecuteScalar() == 0)
                {
                    return result;
                }

                var startDate = DateTime.UtcNow.Date.AddDays(-(days + 1));
                var endDate = DateTime.UtcNow.Date;
                foreach (var batch in Split(ids, 500))
                using (var command = connection.CreateCommand())
                {
                    var parameterNames = new List<string>();
                    for (var i = 0; i < batch.Count; i++)
                    {
                        var name = "@id" + i;
                        parameterNames.Add(name);
                        command.Parameters.AddWithValue(name, batch[i]);
                    }

                    command.Parameters.AddWithValue("@regionId", regionId);
                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                    command.CommandText = @"
SELECT TYPE_ID,
       SUM(TOTAL_VOLUME_FILLED) AS TotalVolume,
       SUM(TOTAL_ORDERS_FILLED) AS TotalOrders,
       COUNT(PRICE_HISTORY_DATE) AS RecordCount,
       SUM(AVG_PRICE) AS PriceSum,
       SUM(RowNumber * AVG_PRICE) AS XYSum,
       SUM(RowNumber) AS XSum,
       SUM(RowNumber * RowNumber) AS XSquaredSum
FROM (
SELECT TYPE_ID,
       TOTAL_VOLUME_FILLED,
       TOTAL_ORDERS_FILLED,
       PRICE_HISTORY_DATE,
       AVG_PRICE,
       ROW_NUMBER() OVER (PARTITION BY TYPE_ID ORDER BY PRICE_HISTORY_DATE ASC) AS RowNumber
FROM MARKET_HISTORY
WHERE TYPE_ID IN (" + string.Join(",", parameterNames) + @")
  AND REGION_ID = @regionId
  AND DATETIME(PRICE_HISTORY_DATE) >= DATETIME(@startDate)
  AND DATETIME(PRICE_HISTORY_DATE) < DATETIME(@endDate)
  AND TOTAL_VOLUME_FILLED IS NOT NULL
)
GROUP BY TYPE_ID";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var recordCount = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                            if (recordCount <= 0)
                            {
                                continue;
                            }

                            var totalVolume = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            var priceSum = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);
                            var xySum = reader.IsDBNull(5) ? 0 : reader.GetDouble(5);
                            var xSum = reader.IsDBNull(6) ? 0 : reader.GetDouble(6);
                            var xSquaredSum = reader.IsDBNull(7) ? 0 : reader.GetDouble(7);
                            result[reader.GetInt64(0)] = new MarketHistoryStats
                            {
                                TypeId = reader.GetInt64(0),
                                RegionId = regionId,
                                Days = days,
                                TotalVolume = totalVolume,
                                TotalOrders = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                                AverageDailyVolume = totalVolume / (double)recordCount,
                                PriceTrend = CalculatePriceTrend(recordCount, priceSum, xySum, xSum, xSquaredSum)
                            };
                        }
                    }
                }
            }

            return result;
        }

        private static double CalculatePriceTrend(long count, double priceSum, double xySum, double xSum, double xSquaredSum)
        {
            if (count <= 1)
            {
                return 0;
            }

            var denominator = count * xSquaredSum - xSum * xSum;
            if (System.Math.Abs(denominator) < 0.0000001)
            {
                return 0;
            }

            var slope = (count * xySum - xSum * priceSum) / denominator;
            var intercept = (priceSum - slope * xSum) / count;
            var todayTrendLinePrice = slope * count + intercept;
            return todayTrendLinePrice == 0 ? 0 : (todayTrendLinePrice - intercept) / todayTrendLinePrice;
        }

        private static IEnumerable<List<long>> Split(List<long> values, int size)
        {
            for (var i = 0; i < values.Count; i += size)
            {
                yield return values.Skip(i).Take(size).ToList();
            }
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;Read Only=True;");
            connection.Open();
            return connection;
        }
    }
}
