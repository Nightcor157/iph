using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BuildProjectStore
    {
        private readonly string _filePath;

        public BuildProjectStore()
        {
            _filePath = AppPaths.GetSettingsPath("BuildProjects.xml");
        }

        public BuildProjectStore(string filePath)
        {
            _filePath = filePath;
            var settingsDir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
        }

        public ObservableCollection<BuildProject> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<BuildProject>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new ObservableCollection<BuildProject>();
            }

            return new ObservableCollection<BuildProject>(
                doc.Root.Elements("Project").Select(projectNode =>
                {
                    var project = new BuildProject
                    {
                        Name = (string)projectNode.Attribute("Name") ?? "",
                        CreatedAt = ReadDate(projectNode, "CreatedAt", DateTime.Now)
                    };

                    foreach (var itemNode in projectNode.Element("Items")?.Elements("Item") ?? Enumerable.Empty<XElement>())
                    {
                        project.Items.Add(new BuildProjectItem
                        {
                            BlueprintTypeId = ReadLong(itemNode, "BlueprintTypeId"),
                            ProductTypeId = ReadLong(itemNode, "ProductTypeId"),
                            BlueprintName = (string)itemNode.Attribute("BlueprintName") ?? "",
                            ProductName = (string)itemNode.Attribute("ProductName") ?? "",
                            GroupName = (string)itemNode.Attribute("GroupName") ?? "",
                            ProductionType = (string)itemNode.Attribute("ProductionType") ?? "",
                            IsRootItem = ReadBool(itemNode, "IsRootItem", false),
                            Wave = ReadInt(itemNode, "Wave", 1),
                            Runs = ReadInt(itemNode, "Runs", 1),
                            PortionSize = ReadInt(itemNode, "PortionSize", 1),
                            RequiredQuantity = ReadInt(itemNode, "RequiredQuantity", 0),
                            CompletedRuns = ReadInt(itemNode, "CompletedRuns", 0),
                            MaterialEfficiency = ReadDouble(itemNode, "ME"),
                            TimeEfficiency = ReadDouble(itemNode, "TE"),
                            DecryptorTypeId = ReadLong(itemNode, "DecryptorTypeId"),
                            DecryptorName = (string)itemNode.Attribute("DecryptorName") ?? "",
                            BestStationName = (string)itemNode.Attribute("BestStationName") ?? "",
                            BestStationSystem = (string)itemNode.Attribute("BestStationSystem") ?? "",
                            MaterialCost = ReadDouble(itemNode, "MaterialCost"),
                            InstallationCost = ReadDouble(itemNode, "InstallationCost"),
                            InventionCost = ReadDouble(itemNode, "InventionCost"),
                            InventionChancePercent = ReadDouble(itemNode, "InventionChancePercent"),
                            InventionJobs = ReadInt(itemNode, "InventionJobs", 0),
                            InventedRunsPerSuccess = ReadInt(itemNode, "InventedRunsPerSuccess", 0),
                            InventionMaterialsCost = ReadDouble(itemNode, "InventionMaterialsCost"),
                            InventionCopyCost = ReadDouble(itemNode, "InventionCopyCost"),
                            InventionJobUsageCost = ReadDouble(itemNode, "InventionJobUsageCost"),
                            InventionStationName = (string)itemNode.Attribute("InventionStationName") ?? "",
                            InventionStationSystem = (string)itemNode.Attribute("InventionStationSystem") ?? "",
                            CopyStationName = (string)itemNode.Attribute("CopyStationName") ?? "",
                            CopyStationSystem = (string)itemNode.Attribute("CopyStationSystem") ?? "",
                            ProductionTimeSeconds = ReadDouble(itemNode, "ProductionTime"),
                            Revenue = ReadDouble(itemNode, "Revenue"),
                            Profit = ReadDouble(itemNode, "Profit"),
                            ContractUnitPrice = ReadDouble(itemNode, "ContractUnitPrice"),
                            ProductPriceSource = (string)itemNode.Attribute("ProductPriceSource") ?? "",
                            ProductPriceDetails = (string)itemNode.Attribute("ProductPriceDetails") ?? "",
                            ReturnOnInvestmentPercent = ReadDouble(itemNode, "ReturnOnInvestmentPercent"),
                            ProductMarketVolume = ReadLong(itemNode, "ProductMarketVolume"),
                            SalesVolumeRatio = ReadDouble(itemNode, "SalesVolumeRatio"),
                            SvrTimesIskPerHour = ReadDouble(itemNode, "SvrTimesIskPerHour"),
                            TotalItemsSold = ReadLong(itemNode, "TotalItemsSold"),
                            TotalOrdersFilled = ReadLong(itemNode, "TotalOrdersFilled"),
                            AverageItemsPerOrder = ReadDouble(itemNode, "AverageItemsPerOrder"),
                            PriceTrend = ReadDouble(itemNode, "PriceTrend"),
                            BuildMaterialLines = ReadInt(itemNode, "BuildMaterialLines", 0),
                            BuyMaterialLines = ReadInt(itemNode, "BuyMaterialLines", 0),
                            EstimateStatus = (string)itemNode.Attribute("EstimateStatus") ?? ""
                        });
                    }

                    foreach (var stockNode in projectNode.Element("Stock")?.Elements("Material") ?? Enumerable.Empty<XElement>())
                    {
                        project.Stock.Add(new BuildProjectStock
                        {
                            TypeId = ReadLong(stockNode, "TypeId"),
                            OwnedQuantity = ReadLong(stockNode, "OwnedQuantity")
                        });
                    }

                    foreach (var decisionNode in projectNode.Element("BuildBuyDecisions")?.Elements("Decision") ?? Enumerable.Empty<XElement>())
                    {
                        var mode = (string)decisionNode.Attribute("Mode") ?? "";
                        if (!string.IsNullOrWhiteSpace(mode))
                        {
                            project.BuildBuyDecisions.Add(new BuildProjectBuildBuyDecision
                            {
                                TypeId = ReadLong(decisionNode, "TypeId"),
                                Mode = mode
                            });
                        }
                    }

                    return project;
                }));
        }

        public void Save(ObservableCollection<BuildProject> projects)
        {
            var doc = new XDocument(new XElement("BuildProjects",
                projects.Select(project => new XElement("Project",
                    new XAttribute("Name", project.Name ?? ""),
                    new XAttribute("CreatedAt", project.CreatedAt.ToString("o")),
                    new XElement("Items",
                        project.Items.Select(item => new XElement("Item",
                            new XAttribute("BlueprintTypeId", item.BlueprintTypeId),
                            new XAttribute("ProductTypeId", item.ProductTypeId),
                            new XAttribute("BlueprintName", item.BlueprintName ?? ""),
                            new XAttribute("ProductName", item.ProductName ?? ""),
                            new XAttribute("GroupName", item.GroupName ?? ""),
                            new XAttribute("ProductionType", item.ProductionType ?? ""),
                            new XAttribute("IsRootItem", item.IsRootItem),
                            new XAttribute("Wave", item.Wave),
                            new XAttribute("Runs", item.Runs),
                            new XAttribute("PortionSize", item.PortionSize),
                            new XAttribute("RequiredQuantity", item.NeededQuantity),
                            new XAttribute("CompletedRuns", item.CompletedRuns),
                            new XAttribute("ME", item.MaterialEfficiency),
                            new XAttribute("TE", item.TimeEfficiency),
                            new XAttribute("DecryptorTypeId", item.DecryptorTypeId),
                            new XAttribute("DecryptorName", item.DecryptorName ?? ""),
                            new XAttribute("BestStationName", item.BestStationName ?? ""),
                            new XAttribute("BestStationSystem", item.BestStationSystem ?? ""),
                            new XAttribute("MaterialCost", item.MaterialCost),
                            new XAttribute("InstallationCost", item.InstallationCost),
                            new XAttribute("InventionCost", item.InventionCost),
                            new XAttribute("InventionChancePercent", item.InventionChancePercent),
                            new XAttribute("InventionJobs", item.InventionJobs),
                            new XAttribute("InventedRunsPerSuccess", item.InventedRunsPerSuccess),
                            new XAttribute("InventionMaterialsCost", item.InventionMaterialsCost),
                            new XAttribute("InventionCopyCost", item.InventionCopyCost),
                            new XAttribute("InventionJobUsageCost", item.InventionJobUsageCost),
                            new XAttribute("InventionStationName", item.InventionStationName ?? ""),
                            new XAttribute("InventionStationSystem", item.InventionStationSystem ?? ""),
                            new XAttribute("CopyStationName", item.CopyStationName ?? ""),
                            new XAttribute("CopyStationSystem", item.CopyStationSystem ?? ""),
                            new XAttribute("ProductionTime", item.ProductionTimeSeconds),
                            new XAttribute("Revenue", item.Revenue),
                            new XAttribute("Profit", item.Profit),
                            new XAttribute("ContractUnitPrice", item.ContractUnitPrice),
                            new XAttribute("ProductPriceSource", item.ProductPriceSource ?? ""),
                            new XAttribute("ProductPriceDetails", item.ProductPriceDetails ?? ""),
                            new XAttribute("ReturnOnInvestmentPercent", item.ReturnOnInvestmentPercent),
                            new XAttribute("ProductMarketVolume", item.ProductMarketVolume),
                            new XAttribute("SalesVolumeRatio", item.SalesVolumeRatio),
                            new XAttribute("SvrTimesIskPerHour", item.SvrTimesIskPerHour),
                            new XAttribute("TotalItemsSold", item.TotalItemsSold),
                            new XAttribute("TotalOrdersFilled", item.TotalOrdersFilled),
                            new XAttribute("AverageItemsPerOrder", item.AverageItemsPerOrder),
                            new XAttribute("PriceTrend", item.PriceTrend),
                            new XAttribute("BuildMaterialLines", item.BuildMaterialLines),
                            new XAttribute("BuyMaterialLines", item.BuyMaterialLines),
                            new XAttribute("EstimateStatus", item.EstimateStatus ?? "")))),
                    new XElement("Stock",
                        project.Stock.Select(stock => new XElement("Material",
                            new XAttribute("TypeId", stock.TypeId),
                            new XAttribute("OwnedQuantity", stock.OwnedQuantity)))),
                    new XElement("BuildBuyDecisions",
                        project.BuildBuyDecisions
                            .Where(decision => decision.TypeId > 0 && !string.IsNullOrWhiteSpace(decision.Mode))
                            .Select(decision => new XElement("Decision",
                                new XAttribute("TypeId", decision.TypeId),
                                new XAttribute("Mode", decision.Mode ?? ""))))))));

            doc.Save(_filePath);
        }

        private static long ReadLong(XElement node, string attributeName)
        {
            long value;
            return long.TryParse((string)node.Attribute(attributeName), out value) ? value : 0;
        }

        private static int ReadInt(XElement node, string attributeName, int defaultValue)
        {
            int value;
            return int.TryParse((string)node.Attribute(attributeName), out value) ? value : defaultValue;
        }

        private static double ReadDouble(XElement node, string attributeName)
        {
            double value;
            return double.TryParse((string)node.Attribute(attributeName), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static bool ReadBool(XElement node, string attributeName, bool defaultValue)
        {
            bool value;
            return bool.TryParse((string)node.Attribute(attributeName), out value) ? value : defaultValue;
        }

        private static DateTime ReadDate(XElement node, string attributeName, DateTime defaultValue)
        {
            DateTime value;
            return DateTime.TryParse((string)node.Attribute(attributeName), out value) ? value : defaultValue;
        }
    }
}
