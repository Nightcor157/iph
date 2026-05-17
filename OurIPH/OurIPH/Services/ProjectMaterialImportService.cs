using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class ProjectMaterialImportService
    {
        public Dictionary<long, long> ImportBoughtQuantities(string text, IEnumerable<BuildProjectMaterial> materials)
        {
            var materialNames = (materials ?? Enumerable.Empty<BuildProjectMaterial>())
                .GroupBy(item => item.TypeId)
                .Select(group => new { TypeId = group.Key, Name = group.First().Name })
                .Where(item => item.TypeId > 0 && !string.IsNullOrWhiteSpace(item.Name))
                .OrderByDescending(item => item.Name.Length)
                .ToList();
            var exactNames = materialNames
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().TypeId, StringComparer.OrdinalIgnoreCase);
            var orderedNames = materialNames.Select(item => Tuple.Create(item.Name, item.TypeId)).ToList();
            var imported = new Dictionary<long, long>();

            foreach (var line in (text ?? "").Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                long typeId;
                long quantity;
                if (!TryParseInventoryLine(line, exactNames, orderedNames, out typeId, out quantity))
                {
                    continue;
                }

                if (imported.ContainsKey(typeId))
                {
                    imported[typeId] += quantity;
                }
                else
                {
                    imported[typeId] = quantity;
                }
            }

            return imported;
        }

        public bool TryParseInventoryLine(string line, Dictionary<string, long> exactNames,
            IEnumerable<Tuple<string, long>> orderedNames, out long typeId, out long quantity)
        {
            typeId = 0;
            quantity = 0;
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var text = line.Trim();
            var tabParts = text.Split('\t').Select(part => part.Trim()).Where(part => part.Length > 0).ToList();
            if (tabParts.Count >= 2 && exactNames.TryGetValue(tabParts[0], out typeId))
            {
                foreach (var part in tabParts.Skip(1))
                {
                    if (TryParseQuantity(part, out quantity))
                    {
                        return quantity > 0;
                    }
                }
            }

            foreach (var item in orderedNames ?? Enumerable.Empty<Tuple<string, long>>())
            {
                if (!text.StartsWith(item.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                typeId = item.Item2;
                return TryParseQuantity(text.Substring(item.Item1.Length), out quantity) && quantity > 0;
            }

            return false;
        }

        public bool TryParseQuantity(string text, out long quantity)
        {
            quantity = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            foreach (Match match in Regex.Matches(text, @"\d[\d\s\u00A0,\.]*"))
            {
                var digits = new string(match.Value.Where(char.IsDigit).ToArray());
                long value;
                if (long.TryParse(digits, out value) && value > 0)
                {
                    quantity = value;
                    return true;
                }
            }

            return false;
        }
    }
}
