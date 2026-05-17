using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class FacilityPresetStore
    {
        private readonly string _filePath;

        public FacilityPresetStore()
            : this(AppPaths.GetSettingsPath("FacilityPresets.xml"))
        {
        }

        public FacilityPresetStore(string filePath)
        {
            _filePath = filePath;
        }

        public ObservableCollection<FacilityPreset> Load()
        {
            if (!File.Exists(_filePath))
            {
                return CreateDefaultPresets();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return CreateDefaultPresets();
            }

            if (doc.Root.Name == "Facilities")
            {
                return LoadLegacyFacilities(doc);
            }

            return new ObservableCollection<FacilityPreset>(
                doc.Root.Elements("Preset").Select(node =>
                {
                    var preset = new FacilityPreset
                    {
                        Name = NormalizeText((string)node.Attribute("Name") ?? ""),
                        IndustrySkillLevel = ReadInt(node, "IndustrySkill", 5),
                        AdvancedIndustrySkillLevel = ReadInt(node, "AdvancedIndustrySkill", 5),
                        AccountingSkillLevel = ReadInt(node, "AccountingSkill", 5),
                        BrokerRelationsSkillLevel = ReadInt(node, "BrokerRelationsSkill", 5),
                        BrokerFactionStanding = ReadDouble(node, "BrokerFactionStanding", 5),
                        BrokerCorpStanding = ReadDouble(node, "BrokerCorpStanding", 5),
                        ManufacturingImplantPercent = ReadDouble(node, "ManufacturingImplant"),
                        SccIndustryFeePercent = ReadDouble(node, "SccIndustryFee", 4.0),
                        AlphaAccountTaxPercent = ReadDouble(node, "AlphaAccountTax", 0),
                        BaseSalesTaxPercent = ReadDouble(node, "BaseSalesTax", 4.5),
                        BaseBrokerFeePercent = ReadDouble(node, "BaseBrokerFee", 3.0),
                        SccBrokerFeeSurchargePercent = ReadDouble(node, "SccBrokerSurcharge", 0.5),
                        SpecialBrokerFeePercent = ReadDouble(node, "SpecialBrokerFee", 0),
                        BrokerFeeMode = ReadInt(node, "BrokerFeeMode", 1),
                        IncludeSalesTax = ReadBool(node, "IncludeSalesTax", true),
                        IncludeBuyOrderBrokerFee = ReadBool(node, "IncludeBuyOrderBrokerFee", true),
                        DefaultBlueprintMe = ReadInt(node, "DefaultBlueprintMe", 10),
                        DefaultBlueprintTe = ReadInt(node, "DefaultBlueprintTe", 20),
                        ProductionLines = ReadInt(node, "ProductionLines", 10),
                        LaboratoryLines = ReadInt(node, "LaboratoryLines", 10),
                        SuggestBuildBlueprintsNotOwned = ReadBool(node, "SuggestBuildBlueprintsNotOwned", true),
                        BuildWhenMarketVolumeShort = ReadBool(node, "BuildWhenMarketVolumeShort", true),
                        ConvertMineralsToOre = ReadBool(node, "ConvertMineralsToOre", false),
                        PreferCompressedOre = ReadBool(node, "PreferCompressedOre", true),
                        RefiningSkillLevel = ReadInt(node, "RefiningSkill", 5),
                        ReprocessingSkillLevel = ReadInt(node, "ReprocessingSkill", 5),
                        OreProcessingSkillLevel = ReadInt(node, "OreProcessingSkill", 4),
                        ReprocessingImplantPercent = ReadDouble(node, "ReprocessingImplant", 0),
                        EncryptionSkillLevel = ReadInt(node, "EncryptionSkill", 4),
                        DatacoreSkill1Level = ReadInt(node, "DatacoreSkill1", 4),
                        DatacoreSkill2Level = ReadInt(node, "DatacoreSkill2", 4),
                        ScienceSkillLevel = ReadInt(node, "ScienceSkill", 5)
                    };
                    foreach (var stationNode in node.Element("Stations")?.Elements("Station") ?? Enumerable.Empty<XElement>())
                    {
                        preset.Stations.Add(ReadStation(stationNode));
                    }

                    if (preset.Stations.Count == 0)
                    {
                        preset.Stations.Add(FacilityStation.CreateDefault());
                    }

                    return preset;
                }));
        }

        public void Save(ObservableCollection<FacilityPreset> presets)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var doc = new XDocument(new XElement("FacilityPresets",
                presets.Select(preset => new XElement("Preset",
                    new XAttribute("Name", preset.Name ?? ""),
                    new XAttribute("IndustrySkill", preset.IndustrySkillLevel),
                    new XAttribute("AdvancedIndustrySkill", preset.AdvancedIndustrySkillLevel),
                    new XAttribute("AccountingSkill", preset.AccountingSkillLevel),
                    new XAttribute("BrokerRelationsSkill", preset.BrokerRelationsSkillLevel),
                    new XAttribute("BrokerFactionStanding", preset.BrokerFactionStanding),
                    new XAttribute("BrokerCorpStanding", preset.BrokerCorpStanding),
                    new XAttribute("ManufacturingImplant", preset.ManufacturingImplantPercent),
                    new XAttribute("SccIndustryFee", preset.SccIndustryFeePercent),
                    new XAttribute("AlphaAccountTax", preset.AlphaAccountTaxPercent),
                    new XAttribute("BaseSalesTax", preset.BaseSalesTaxPercent),
                    new XAttribute("BaseBrokerFee", preset.BaseBrokerFeePercent),
                    new XAttribute("SccBrokerSurcharge", preset.SccBrokerFeeSurchargePercent),
                    new XAttribute("SpecialBrokerFee", preset.SpecialBrokerFeePercent),
                    new XAttribute("BrokerFeeMode", preset.BrokerFeeMode),
                    new XAttribute("IncludeSalesTax", preset.IncludeSalesTax),
                    new XAttribute("IncludeBuyOrderBrokerFee", preset.IncludeBuyOrderBrokerFee),
                    new XAttribute("DefaultBlueprintMe", preset.DefaultBlueprintMe),
                    new XAttribute("DefaultBlueprintTe", preset.DefaultBlueprintTe),
                    new XAttribute("ProductionLines", preset.ProductionLines),
                    new XAttribute("LaboratoryLines", preset.LaboratoryLines),
                    new XAttribute("SuggestBuildBlueprintsNotOwned", preset.SuggestBuildBlueprintsNotOwned),
                    new XAttribute("BuildWhenMarketVolumeShort", preset.BuildWhenMarketVolumeShort),
                    new XAttribute("ConvertMineralsToOre", preset.ConvertMineralsToOre),
                    new XAttribute("PreferCompressedOre", preset.PreferCompressedOre),
                    new XAttribute("RefiningSkill", preset.RefiningSkillLevel),
                    new XAttribute("ReprocessingSkill", preset.ReprocessingSkillLevel),
                    new XAttribute("OreProcessingSkill", preset.OreProcessingSkillLevel),
                    new XAttribute("ReprocessingImplant", preset.ReprocessingImplantPercent),
                    new XAttribute("EncryptionSkill", preset.EncryptionSkillLevel),
                    new XAttribute("DatacoreSkill1", preset.DatacoreSkill1Level),
                    new XAttribute("DatacoreSkill2", preset.DatacoreSkill2Level),
                    new XAttribute("ScienceSkill", preset.ScienceSkillLevel),
                    new XElement("Stations",
                        preset.Stations.Select(station => new XElement("Station",
                            new XAttribute("Name", station.Name ?? ""),
                            new XAttribute("System", station.SystemName ?? ""),
                            new XAttribute("RegionId", station.RegionId),
                            new XAttribute("SolarSystemId", station.SolarSystemId),
                            new XAttribute("FacilityType", station.FacilityType ?? ""),
                            new XAttribute("StructureTypeId", station.StructureTypeId),
                            new XAttribute("ProductionType", station.ProductionType ?? ""),
                            new XAttribute("RigSlot1", station.RigSlot1TypeId),
                            new XAttribute("RigSlot2", station.RigSlot2TypeId),
                            new XAttribute("RigSlot3", station.RigSlot3TypeId),
                            new XAttribute("ServiceModule1", station.ServiceModule1TypeId),
                            new XAttribute("ServiceModule2", station.ServiceModule2TypeId),
                            new XAttribute("ServiceModule3", station.ServiceModule3TypeId),
                            new XAttribute("ServiceModule4", station.ServiceModule4TypeId),
                            new XAttribute("ServiceModule5", station.ServiceModule5TypeId),
                            new XAttribute("Security", station.Security),
                            new XAttribute("MaterialMultiplier", station.MaterialMultiplier),
                            new XAttribute("TimeMultiplier", station.TimeMultiplier),
                            new XAttribute("CostMultiplier", station.CostMultiplier),
                            new XAttribute("MaterialBonus", station.MaterialBonusPercent),
                            new XAttribute("IndustryTax", station.IndustryTaxPercent),
                            new XAttribute("SalesFee", station.SalesFeePercent),
                            new XAttribute("FactionWarfareUpgradeLevel", station.FactionWarfareUpgradeLevel),
                            new XAttribute("SupportsProduction", station.SupportsProduction),
                            new XAttribute("ValidationMessage", station.ValidationMessage ?? ""))))))));

            doc.Save(_filePath);
        }

        private static ObservableCollection<FacilityPreset> CreateDefaultPresets()
        {
            return new ObservableCollection<FacilityPreset>
            {
                new FacilityPreset
                {
                    Name = "Jita NPC",
                    Stations = new ObservableCollection<FacilityStation> { FacilityStation.CreateDefault() }
                }
            };
        }

        private static ObservableCollection<FacilityPreset> LoadLegacyFacilities(XDocument doc)
        {
            return new ObservableCollection<FacilityPreset>(
                doc.Root.Elements("Facility").Select(node => new FacilityPreset
                {
                    Name = NormalizeText((string)node.Attribute("Name") ?? ""),
                    AccountingSkillLevel = 5,
                    BrokerRelationsSkillLevel = 5,
                    BrokerFactionStanding = 5,
                    BrokerCorpStanding = 5,
                    Stations = new ObservableCollection<FacilityStation>
                    {
                        new FacilityStation
                        {
                            Name = NormalizeText((string)node.Attribute("Facility") ?? ""),
                            SystemName = NormalizeText((string)node.Attribute("System") ?? ""),
                            RegionId = ReadLong(node, "RegionId"),
                            SolarSystemId = ReadLong(node, "SolarSystemId"),
                            FacilityType = "Custom",
                            StructureTypeId = 0,
                            ProductionType = (string)node.Attribute("ProductionType") ?? "",
                            MaterialMultiplier = 1,
                            TimeMultiplier = 1,
                            CostMultiplier = 1,
                            MaterialBonusPercent = ReadDouble(node, "MaterialBonus"),
                            IndustryTaxPercent = ReadDouble(node, "IndustryTax"),
                            SalesFeePercent = ReadDouble(node, "SalesFee"),
                            FactionWarfareUpgradeLevel = ReadInt(node, "FactionWarfareUpgradeLevel", 0),
                            SupportsProduction = true
                        }
                    }
                }));
        }

        private static FacilityStation ReadStation(XElement node)
        {
            return new FacilityStation
            {
                Name = NormalizeText((string)node.Attribute("Name") ?? ""),
                SystemName = NormalizeText((string)node.Attribute("System") ?? ""),
                RegionId = ReadLong(node, "RegionId"),
                SolarSystemId = ReadLong(node, "SolarSystemId"),
                FacilityType = (string)node.Attribute("FacilityType") ?? "",
                StructureTypeId = ReadInt(node, "StructureTypeId"),
                ProductionType = (string)node.Attribute("ProductionType") ?? "",
                RigSlot1TypeId = ReadInt(node, "RigSlot1"),
                RigSlot2TypeId = ReadInt(node, "RigSlot2"),
                RigSlot3TypeId = ReadInt(node, "RigSlot3"),
                ServiceModule1TypeId = ReadInt(node, "ServiceModule1"),
                ServiceModule2TypeId = ReadInt(node, "ServiceModule2"),
                ServiceModule3TypeId = ReadInt(node, "ServiceModule3"),
                ServiceModule4TypeId = ReadInt(node, "ServiceModule4"),
                ServiceModule5TypeId = ReadInt(node, "ServiceModule5"),
                Security = ReadDouble(node, "Security"),
                MaterialMultiplier = ReadDouble(node, "MaterialMultiplier", 1),
                TimeMultiplier = ReadDouble(node, "TimeMultiplier", 1),
                CostMultiplier = ReadDouble(node, "CostMultiplier", 1),
                MaterialBonusPercent = ReadDouble(node, "MaterialBonus"),
                IndustryTaxPercent = ReadDouble(node, "IndustryTax"),
                SalesFeePercent = ReadDouble(node, "SalesFee"),
                FactionWarfareUpgradeLevel = ReadInt(node, "FactionWarfareUpgradeLevel", 0),
                SupportsProduction = ReadBool(node, "SupportsProduction", true),
                ValidationMessage = NormalizeText((string)node.Attribute("ValidationMessage") ?? "")
            };
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrEmpty(value) || !LooksLikeMojibake(value))
            {
                return value ?? "";
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Encoding.GetEncoding(1251).GetBytes(value));
                return string.IsNullOrWhiteSpace(decoded) ? value : decoded;
            }
            catch
            {
                return value;
            }
        }

        private static bool LooksLikeMojibake(string value)
        {
            return value.IndexOf('Р') >= 0
                   && (value.Contains("Рќ")
                       || value.Contains("Рџ")
                       || value.Contains("РЎ")
                       || value.Contains("Р”")
                       || value.Contains("СЃ")
                       || value.Contains("С‚")
                       || value.Contains("С‹")
                       || value.Contains("вЂ"));
        }

        private static long ReadLong(XElement node, string attributeName)
        {
            var value = (string)node.Attribute(attributeName);
            long result;
            return long.TryParse(value, out result) ? result : 0;
        }

        private static int ReadInt(XElement node, string attributeName, int defaultValue = 0)
        {
            var value = (string)node.Attribute(attributeName);
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        private static bool ReadBool(XElement node, string attributeName, bool defaultValue)
        {
            var value = (string)node.Attribute(attributeName);
            bool result;
            return bool.TryParse(value, out result) ? result : defaultValue;
        }

        private static double ReadDouble(XElement node, string attributeName, double defaultValue = 0)
        {
            var value = (string)node.Attribute(attributeName);
            double result;
            return double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result)
                ? result
                : defaultValue;
        }
    }
}
