using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class MarketHistoryCacheStore
    {
        private readonly string _filePath;

        public MarketHistoryCacheStore()
        {
            _filePath = AppPaths.GetSettingsPath("MarketHistoryCache.xml");
        }

        public MarketHistoryCacheStore(string filePath)
        {
            _filePath = filePath;
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public ObservableCollection<MarketHistoryStats> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<MarketHistoryStats>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new ObservableCollection<MarketHistoryStats>();
            }

            return new ObservableCollection<MarketHistoryStats>(
                doc.Root.Elements("History").Select(node => new MarketHistoryStats
                {
                    TypeId = ReadLong(node, "TypeId"),
                    RegionId = ReadLong(node, "RegionId"),
                    Days = ReadInt(node, "Days", 7),
                    AverageDailyVolume = ReadDouble(node, "AverageDailyVolume"),
                    TotalVolume = ReadLong(node, "TotalVolume"),
                    TotalOrders = ReadLong(node, "TotalOrders"),
                    PriceTrend = ReadDouble(node, "PriceTrend"),
                    UpdatedAt = ReadDate(node, "UpdatedAt", DateTime.MinValue)
                }));
        }

        public void Save(ObservableCollection<MarketHistoryStats> entries)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var doc = new XDocument(new XElement("MarketHistoryCache",
                entries.Select(entry => new XElement("History",
                    new XAttribute("TypeId", entry.TypeId),
                    new XAttribute("RegionId", entry.RegionId),
                    new XAttribute("Days", entry.Days),
                    new XAttribute("AverageDailyVolume", entry.AverageDailyVolume),
                    new XAttribute("TotalVolume", entry.TotalVolume),
                    new XAttribute("TotalOrders", entry.TotalOrders),
                    new XAttribute("PriceTrend", entry.PriceTrend),
                    new XAttribute("UpdatedAt", entry.UpdatedAt.ToString("o"))))));

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

        private static DateTime ReadDate(XElement node, string attributeName, DateTime defaultValue)
        {
            DateTime value;
            return DateTime.TryParse((string)node.Attribute(attributeName), out value) ? value : defaultValue;
        }
    }
}
