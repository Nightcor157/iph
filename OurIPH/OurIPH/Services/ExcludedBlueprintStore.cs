using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OurIPH.Services
{
    public class ExcludedBlueprintStore
    {
        private readonly string _filePath;

        public ExcludedBlueprintStore()
        {
            _filePath = AppPaths.GetSettingsPath("ExcludedBlueprints.xml");
        }

        public HashSet<long> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new HashSet<long>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new HashSet<long>();
            }

            return new HashSet<long>(doc.Root.Elements("Product")
                .Select(node => ReadLong(node, "TypeId"))
                .Where(typeId => typeId > 0));
        }

        public void Save(IEnumerable<long> productTypeIds)
        {
            var doc = new XDocument(new XElement("ExcludedBlueprints",
                productTypeIds
                    .Where(typeId => typeId > 0)
                    .Distinct()
                    .OrderBy(typeId => typeId)
                    .Select(typeId => new XElement("Product", new XAttribute("TypeId", typeId)))));

            doc.Save(_filePath);
        }

        private static long ReadLong(XElement node, string attributeName)
        {
            long value;
            return long.TryParse((string)node.Attribute(attributeName), out value) ? value : 0;
        }
    }
}
