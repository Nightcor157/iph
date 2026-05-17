using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OurIPH.Services
{
    public class CharacterSkillStore
    {
        private readonly string _filePath;

        public CharacterSkillStore()
        {
            _filePath = AppPaths.GetSettingsPath("CharacterSkills.xml");
        }

        public Dictionary<long, int> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new Dictionary<long, int>();
            }

            var doc = XDocument.Load(_filePath);
            if (doc.Root == null)
            {
                return new Dictionary<long, int>();
            }

            return doc.Root.Elements("Skill")
                .Select(node => new { TypeId = ReadLong(node, "TypeId"), Level = ReadInt(node, "Level", 0) })
                .Where(item => item.TypeId > 0)
                .GroupBy(item => item.TypeId)
                .ToDictionary(group => group.Key, group => group.Max(item => item.Level));
        }

        public void Save(Dictionary<long, int> skills)
        {
            var doc = new XDocument(new XElement("CharacterSkills",
                skills
                    .Where(item => item.Key > 0)
                    .OrderBy(item => item.Key)
                    .Select(item => new XElement("Skill",
                        new XAttribute("TypeId", item.Key),
                        new XAttribute("Level", item.Value)))));

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
    }
}
