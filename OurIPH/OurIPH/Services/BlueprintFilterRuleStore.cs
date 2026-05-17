using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintFilterRuleStore
    {
        private readonly string _filePath;

        public BlueprintFilterRuleStore()
        {
            _filePath = AppPaths.GetSettingsPath("BlueprintFilterRules.xml");
        }

        public BlueprintFilterRuleStore(string filePath)
        {
            _filePath = filePath;
            var settingsDir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
        }

        public BlueprintFilterRules LoadOrCreateDefaults()
        {
            if (!File.Exists(_filePath))
            {
                var defaults = BlueprintFilterRules.CreateDefault();
                Save(defaults);
                return defaults;
            }

            return Load();
        }

        public BlueprintFilterRules Load()
        {
            if (!File.Exists(_filePath))
            {
                return BlueprintFilterRules.CreateDefault();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return BlueprintFilterRules.CreateDefault();
            }

            var defaults = BlueprintFilterRules.CreateDefault();
            return new BlueprintFilterRules
            {
                LimitedMetaGroupIds = ReadIntList(doc.Root, "LimitedMetaGroupIds", defaults.LimitedMetaGroupIds),
                LimitedMarketGroupKeywords = ReadStringList(doc.Root, "LimitedMarketGroupKeywords", defaults.LimitedMarketGroupKeywords),
                LimitedGroupKeywords = ReadStringList(doc.Root, "LimitedGroupKeywords", defaults.LimitedGroupKeywords),
                RareProductKeywords = ReadStringList(doc.Root, "RareProductKeywords", defaults.RareProductKeywords),
                RareBlueprintKeywords = ReadStringList(doc.Root, "RareBlueprintKeywords", defaults.RareBlueprintKeywords),
                RareGroupKeywords = ReadStringList(doc.Root, "RareGroupKeywords", defaults.RareGroupKeywords),
                RareMarketGroupKeywords = ReadStringList(doc.Root, "RareMarketGroupKeywords", defaults.RareMarketGroupKeywords),
                CapitalKeywords = ReadStringList(doc.Root, "CapitalKeywords", defaults.CapitalKeywords),
                ReactionGroupKeywords = ReadStringList(doc.Root, "ReactionGroupKeywords", defaults.ReactionGroupKeywords),
                ReactionMarketGroupKeywords = ReadStringList(doc.Root, "ReactionMarketGroupKeywords", defaults.ReactionMarketGroupKeywords)
            };
        }

        public void Save(BlueprintFilterRules rules)
        {
            var value = rules ?? BlueprintFilterRules.CreateDefault();
            var doc = new XDocument(new XElement("BlueprintFilterRules",
                WriteIntList("LimitedMetaGroupIds", value.LimitedMetaGroupIds),
                WriteStringList("LimitedMarketGroupKeywords", value.LimitedMarketGroupKeywords),
                WriteStringList("LimitedGroupKeywords", value.LimitedGroupKeywords),
                WriteStringList("RareProductKeywords", value.RareProductKeywords),
                WriteStringList("RareBlueprintKeywords", value.RareBlueprintKeywords),
                WriteStringList("RareGroupKeywords", value.RareGroupKeywords),
                WriteStringList("RareMarketGroupKeywords", value.RareMarketGroupKeywords),
                WriteStringList("CapitalKeywords", value.CapitalKeywords),
                WriteStringList("ReactionGroupKeywords", value.ReactionGroupKeywords),
                WriteStringList("ReactionMarketGroupKeywords", value.ReactionMarketGroupKeywords)));

            doc.Save(_filePath);
        }

        private static List<int> ReadIntList(XElement root, string elementName, IEnumerable<int> defaults)
        {
            var items = root.Element(elementName)?.Elements("Id")
                .Select(item =>
                {
                    int value;
                    return int.TryParse((string)item.Attribute("Value"), out value) ? value : 0;
                })
                .Where(value => value != 0)
                .Distinct()
                .ToList();

            return items != null && items.Count > 0 ? items : defaults.ToList();
        }

        private static List<string> ReadStringList(XElement root, string elementName, IEnumerable<string> defaults)
        {
            var items = root.Element(elementName)?.Elements("Keyword")
                .Select(item => ((string)item.Attribute("Value") ?? "").Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .ToList();

            return items != null && items.Count > 0 ? items : defaults.ToList();
        }

        private static XElement WriteIntList(string elementName, IEnumerable<int> values)
        {
            return new XElement(elementName,
                (values ?? Enumerable.Empty<int>())
                    .Distinct()
                    .Select(value => new XElement("Id", new XAttribute("Value", value))));
        }

        private static XElement WriteStringList(string elementName, IEnumerable<string> values)
        {
            return new XElement(elementName,
                (values ?? Enumerable.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct()
                    .Select(value => new XElement("Keyword", new XAttribute("Value", value))));
        }
    }
}
