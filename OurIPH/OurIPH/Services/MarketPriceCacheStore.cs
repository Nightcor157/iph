using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class MarketPriceCacheStore
    {
        private readonly string _filePath;

        public MarketPriceCacheStore()
        {
            _filePath = AppPaths.GetSettingsPath("MarketPriceCache.xml");
        }

        public MarketPriceCacheStore(string filePath)
        {
            _filePath = filePath;
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public ObservableCollection<MarketPriceCacheEntry> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<MarketPriceCacheEntry>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new ObservableCollection<MarketPriceCacheEntry>();
            }

            return new ObservableCollection<MarketPriceCacheEntry>(
                doc.Root.Elements("Price").Select(node => new MarketPriceCacheEntry
                {
                    RegionOrStationId = ReadLong(node, "RegionOrStationId"),
                    LocationName = (string)node.Attribute("LocationName") ?? "",
                    TypeId = ReadLong(node, "TypeId"),
                    TypeName = (string)node.Attribute("TypeName") ?? "",
                    SellMin = ReadDouble(node, "SellMin"),
                    BuyMax = ReadDouble(node, "BuyMax"),
                    SellVolume = ReadLong(node, "SellVolume"),
                    BuyVolume = ReadLong(node, "BuyVolume"),
                    UpdatedAt = ReadDate(node, "UpdatedAt")
                }));
        }

        public void Save(ObservableCollection<MarketPriceCacheEntry> entries)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var doc = new XDocument(new XElement("MarketPriceCache",
                entries.Select(entry => new XElement("Price",
                    new XAttribute("RegionOrStationId", entry.RegionOrStationId),
                    new XAttribute("LocationName", entry.LocationName ?? ""),
                    new XAttribute("TypeId", entry.TypeId),
                    new XAttribute("TypeName", entry.TypeName ?? ""),
                    new XAttribute("SellMin", entry.SellMin),
                    new XAttribute("BuyMax", entry.BuyMax),
                    new XAttribute("SellVolume", entry.SellVolume),
                    new XAttribute("BuyVolume", entry.BuyVolume),
                    new XAttribute("UpdatedAt", entry.UpdatedAt.ToString("o"))))));

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

        private static DateTime ReadDate(XElement node, string attributeName)
        {
            DateTime value;
            return DateTime.TryParse((string)node.Attribute(attributeName), out value) ? value : default(DateTime);
        }
    }
}
