using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class ContractPriceStore
    {
        private readonly string _filePath;

        public ContractPriceStore()
        {
            _filePath = AppPaths.GetSettingsPath("ContractPrices.xml");
        }

        public ContractPriceStore(string filePath)
        {
            _filePath = filePath;
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public ObservableCollection<ContractPriceSample> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<ContractPriceSample>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new ObservableCollection<ContractPriceSample>();
            }

            return new ObservableCollection<ContractPriceSample>(
                doc.Root.Elements("Sample").Select(node => new ContractPriceSample
                {
                    TypeId = ReadLong(node, "TypeId"),
                    TypeName = (string)node.Attribute("TypeName") ?? "",
                    Price = ReadDouble(node, "Price"),
                    ObservedAt = ReadDate(node, "ObservedAt", DateTime.MinValue),
                    ContractId = ReadLong(node, "ContractId"),
                    Source = (string)node.Attribute("Source") ?? "",
                    LocationId = ReadLong(node, "LocationId"),
                    Quantity = ReadLong(node, "Quantity"),
                    ItemCount = ReadInt(node, "ItemCount"),
                    Title = (string)node.Attribute("Title") ?? ""
                }).Where(item => item.TypeId > 0 && item.Price > 0));
        }

        public void Save(ObservableCollection<ContractPriceSample> samples)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var cutoff = DateTime.Now.AddDays(-120);
            var doc = new XDocument(new XElement("ContractPrices",
                samples
                    .Where(item => item.TypeId > 0 && item.Price > 0 && item.ObservedAt >= cutoff)
                    .OrderBy(item => item.TypeId)
                    .ThenByDescending(item => item.ObservedAt)
                    .Select(item => new XElement("Sample",
                        new XAttribute("TypeId", item.TypeId),
                        new XAttribute("TypeName", item.TypeName ?? ""),
                        new XAttribute("Price", item.Price),
                        new XAttribute("ObservedAt", item.ObservedAt.ToString("o")),
                        new XAttribute("ContractId", item.ContractId),
                        new XAttribute("Source", item.Source ?? ""),
                        new XAttribute("LocationId", item.LocationId),
                        new XAttribute("Quantity", item.Quantity),
                        new XAttribute("ItemCount", item.ItemCount),
                        new XAttribute("Title", item.Title ?? "")))));

            doc.Save(_filePath);
        }

        private static long ReadLong(XElement node, string attributeName)
        {
            long value;
            return long.TryParse((string)node.Attribute(attributeName), out value) ? value : 0;
        }

        private static double ReadDouble(XElement node, string attributeName)
        {
            double value;
            return double.TryParse((string)node.Attribute(attributeName), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static int ReadInt(XElement node, string attributeName)
        {
            int value;
            return int.TryParse((string)node.Attribute(attributeName), out value) ? value : 0;
        }

        private static DateTime ReadDate(XElement node, string attributeName, DateTime defaultValue)
        {
            DateTime value;
            return DateTime.TryParse((string)node.Attribute(attributeName), out value) ? value : defaultValue;
        }
    }
}
