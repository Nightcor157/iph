using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BuildQueueStore
    {
        private readonly string _filePath;

        public BuildQueueStore()
        {
            _filePath = AppPaths.GetSettingsPath("BuildQueue.xml");
        }

        public ObservableCollection<BuildQueueItem> Load(EveDatabaseService database)
        {
            var queue = new ObservableCollection<BuildQueueItem>();
            if (!File.Exists(_filePath) || database == null)
            {
                return queue;
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return queue;
            }

            foreach (var node in doc.Root.Elements("Item"))
            {
                var productTypeId = ReadLong(node, "ProductTypeId");
                if (productTypeId <= 0)
                {
                    continue;
                }

                var blueprint = database.FindBlueprintByProduct(productTypeId);
                if (blueprint == null)
                {
                    continue;
                }

                RestoreBlueprintSnapshot(blueprint, node);
                var queueItem = new BuildQueueItem
                {
                    Blueprint = blueprint,
                    Runs = ReadInt(node, "Runs", 1),
                    MaterialEfficiency = ReadDouble(node, "ME"),
                    TimeEfficiency = ReadDouble(node, "TE"),
                    DecryptorTypeId = ReadLong(node, "DecryptorTypeId"),
                    DecryptorName = (string)node.Attribute("DecryptorName") ?? ""
                };
                if (blueprint.IsCopyOnlyBlueprint)
                {
                    queueItem.MaterialEfficiency = 0;
                    queueItem.TimeEfficiency = 0;
                }

                queue.Add(queueItem);
            }

            return queue;
        }

        public void Save(ObservableCollection<BuildQueueItem> queue)
        {
            var doc = new XDocument(new XElement("BuildQueue",
                (queue ?? new ObservableCollection<BuildQueueItem>())
                .Where(item => item != null && item.ProductTypeId > 0)
                .Select(item => new XElement("Item",
                    new XAttribute("ProductTypeId", item.ProductTypeId),
                    new XAttribute("Runs", item.Runs),
                    new XAttribute("ME", item.MaterialEfficiency),
                    new XAttribute("TE", item.TimeEfficiency),
                    new XAttribute("DecryptorTypeId", item.DecryptorTypeId),
                    new XAttribute("DecryptorName", item.DecryptorName ?? ""),
                    new XAttribute("MaterialCost", item.Blueprint == null ? 0 : item.Blueprint.MaterialCost),
                    new XAttribute("InstallationCost", item.Blueprint == null ? 0 : item.Blueprint.InstallationCost),
                    new XAttribute("InventionCost", item.Blueprint == null ? 0 : item.Blueprint.InventionCost),
                    new XAttribute("TotalCost", item.Blueprint == null ? 0 : item.Blueprint.TotalCost),
                    new XAttribute("Revenue", item.Blueprint == null ? 0 : item.Blueprint.Revenue),
                    new XAttribute("Profit", item.Blueprint == null ? 0 : item.Blueprint.Profit),
                    new XAttribute("IskPerHour", item.Blueprint == null ? 0 : item.Blueprint.IskPerHour),
                    new XAttribute("MarginPercent", item.Blueprint == null ? 0 : item.Blueprint.MarginPercent),
                    new XAttribute("ReturnOnInvestmentPercent", item.Blueprint == null ? 0 : item.Blueprint.ReturnOnInvestmentPercent),
                    new XAttribute("ProductionTimeSeconds", item.Blueprint == null ? 0 : item.Blueprint.ProductionTimeSeconds),
                    new XAttribute("ProductMarketVolume", item.Blueprint == null ? 0 : item.Blueprint.ProductMarketVolume),
                    new XAttribute("ProducedQuantity", item.Blueprint == null ? 0 : item.Blueprint.ProducedQuantity),
                    new XAttribute("SalesVolumeRatio", item.Blueprint == null ? 0 : item.Blueprint.SalesVolumeRatio),
                    new XAttribute("SvrTimesIskPerHour", item.Blueprint == null ? 0 : item.Blueprint.SvrTimesIskPerHour),
                    new XAttribute("TotalItemsSold", item.Blueprint == null ? 0 : item.Blueprint.TotalItemsSold),
                    new XAttribute("TotalOrdersFilled", item.Blueprint == null ? 0 : item.Blueprint.TotalOrdersFilled),
                    new XAttribute("AverageItemsPerOrder", item.Blueprint == null ? 0 : item.Blueprint.AverageItemsPerOrder),
                    new XAttribute("PriceTrend", item.Blueprint == null ? 0 : item.Blueprint.PriceTrend),
                    new XAttribute("BestFacilityName", item.Blueprint == null ? "" : item.Blueprint.BestFacilityName ?? ""),
                    new XAttribute("BestFacilitySystem", item.Blueprint == null ? "" : item.Blueprint.BestFacilitySystem ?? ""),
                    new XAttribute("EstimateStatus", item.Blueprint == null ? "" : item.Blueprint.EstimateStatus ?? ""),
                    new XAttribute("MissingSkillsCount", item.Blueprint == null ? -1 : item.Blueprint.MissingSkillsCount),
                    new XAttribute("RequiredSkillSummary", item.Blueprint == null ? "" : item.Blueprint.RequiredSkillSummary ?? "")))));

            doc.Save(_filePath);
        }

        private static void RestoreBlueprintSnapshot(BlueprintSearchResult blueprint, XElement node)
        {
            blueprint.MaterialCost = ReadDouble(node, "MaterialCost");
            blueprint.InstallationCost = ReadDouble(node, "InstallationCost");
            blueprint.InventionCost = ReadDouble(node, "InventionCost");
            blueprint.TotalCost = ReadDouble(node, "TotalCost");
            blueprint.Revenue = ReadDouble(node, "Revenue");
            blueprint.Profit = ReadDouble(node, "Profit");
            blueprint.IskPerHour = ReadDouble(node, "IskPerHour");
            blueprint.MarginPercent = ReadDouble(node, "MarginPercent");
            blueprint.ReturnOnInvestmentPercent = ReadDouble(node, "ReturnOnInvestmentPercent");
            blueprint.ProductionTimeSeconds = ReadDouble(node, "ProductionTimeSeconds");
            blueprint.ProductMarketVolume = ReadLong(node, "ProductMarketVolume");
            blueprint.ProducedQuantity = ReadLong(node, "ProducedQuantity");
            blueprint.SalesVolumeRatio = ReadDouble(node, "SalesVolumeRatio");
            blueprint.SvrTimesIskPerHour = ReadDouble(node, "SvrTimesIskPerHour");
            blueprint.TotalItemsSold = ReadLong(node, "TotalItemsSold");
            blueprint.TotalOrdersFilled = ReadLong(node, "TotalOrdersFilled");
            blueprint.AverageItemsPerOrder = ReadDouble(node, "AverageItemsPerOrder");
            blueprint.PriceTrend = ReadDouble(node, "PriceTrend");
            blueprint.BestFacilityName = (string)node.Attribute("BestFacilityName") ?? "";
            blueprint.BestFacilitySystem = (string)node.Attribute("BestFacilitySystem") ?? "";
            blueprint.EstimateStatus = (string)node.Attribute("EstimateStatus") ?? "";
            blueprint.MissingSkillsCount = ReadInt(node, "MissingSkillsCount", -1);
            blueprint.RequiredSkillSummary = (string)node.Attribute("RequiredSkillSummary") ?? "";
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
    }
}
