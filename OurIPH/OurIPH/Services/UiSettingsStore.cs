using System.Globalization;
using System.IO;
using System.Xml.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class UiSettingsStore
    {
        private readonly string _filePath;

        public UiSettingsStore(string filePath)
        {
            _filePath = filePath;
        }

        public UiSettings Load()
        {
            var settings = new UiSettings();
            if (!File.Exists(_filePath))
            {
                return settings;
            }

            var doc = XDocument.Load(_filePath);
            var root = doc.Root;
            if (root == null)
            {
                return settings;
            }

            settings.AlwaysBuyRam = ReadBool(root, "AlwaysBuyRam", settings.AlwaysBuyRam);
            settings.AlwaysBuyFuelBlocks = ReadBool(root, "AlwaysBuyFuelBlocks", settings.AlwaysBuyFuelBlocks);
            settings.ReactionDepth = ReadString(root, "ReactionDepth", settings.ReactionDepth);
            settings.MaterialPriceMode = ReadString(root, "MaterialPriceMode", settings.MaterialPriceMode);
            settings.ProductPriceMode = ReadString(root, "ProductPriceMode", settings.ProductPriceMode);
            settings.Decryptor = ReadString(root, "Decryptor", settings.Decryptor);
            settings.AutoDecryptor = ReadBool(root, "AutoDecryptor", settings.AutoDecryptor);
            settings.TypeShips = ReadBool(root, "TypeShips", settings.TypeShips);
            settings.TypeAmmo = ReadBool(root, "TypeAmmo", settings.TypeAmmo);
            settings.TypeModules = ReadBool(root, "TypeModules", settings.TypeModules);
            settings.TypeRigs = ReadBool(root, "TypeRigs", settings.TypeRigs);
            settings.TypeDrones = ReadBool(root, "TypeDrones", settings.TypeDrones);
            settings.TypeComponents = ReadBool(root, "TypeComponents", settings.TypeComponents);
            settings.TypeStructures = ReadBool(root, "TypeStructures", settings.TypeStructures);
            settings.TypeMisc = ReadBool(root, "TypeMisc", settings.TypeMisc);
            settings.ProfitableOnly = ReadBool(root, "ProfitableOnly", settings.ProfitableOnly);
            settings.HideMissingPrices = ReadBool(root, "HideMissingPrices", settings.HideMissingPrices);
            settings.HideLowVolume = ReadBool(root, "HideLowVolume", settings.HideLowVolume);
            settings.HideLimitedBlueprints = ReadBool(root, "HideLimitedBlueprints", settings.HideLimitedBlueprints);
            settings.HideRareNoise = ReadBool(root, "HideRareNoise", settings.HideRareNoise);
            settings.AllowT2 = ReadBool(root, "AllowT2", settings.AllowT2);
            settings.AllowT3 = ReadBool(root, "AllowT3", settings.AllowT3);
            settings.AllowCapital = ReadBool(root, "AllowCapital", settings.AllowCapital);
            settings.AllowReactions = ReadBool(root, "AllowReactions", settings.AllowReactions);
            settings.ShowExcludedBlueprints = ReadBool(root, "ShowExcludedBlueprints", settings.ShowExcludedBlueprints);
            settings.HideBySkills = ReadBool(root, "HideBySkills", settings.HideBySkills);
            settings.TopOnly = ReadBool(root, "TopOnly", settings.TopOnly);
            settings.TopBlueprintCount = ReadString(root, "TopBlueprintCount", settings.TopBlueprintCount);
            settings.MinSvr = ReadString(root, "MinSvr", settings.MinSvr);
            settings.SvrDays = ReadString(root, "SvrDays", settings.SvrDays);
            settings.PriceTrendFilter = ReadString(root, "PriceTrendFilter", settings.PriceTrendFilter);
            settings.MinSold = ReadString(root, "MinSold", settings.MinSold);
            settings.MinOrders = ReadString(root, "MinOrders", settings.MinOrders);
            settings.UseContractPrices = ReadBool(root, "UseContractPrices", settings.UseContractPrices);
            settings.MinIskHour = ReadString(root, "MinIskHour", settings.MinIskHour);
            settings.MinRoi = ReadString(root, "MinRoi", settings.MinRoi);
            settings.MaterialPriceModifier = ReadString(root, "MaterialPriceModifier", settings.MaterialPriceModifier);
            settings.ProductPriceModifier = ReadString(root, "ProductPriceModifier", settings.ProductPriceModifier);
            settings.BlueprintDetailHeight = ReadDouble(root, "BlueprintDetailHeight", settings.BlueprintDetailHeight);
            return settings;
        }

        public void Save(UiSettings settings)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var doc = new XDocument(new XElement("UiSettings",
                new XAttribute("AlwaysBuyRam", settings.AlwaysBuyRam),
                new XAttribute("AlwaysBuyFuelBlocks", settings.AlwaysBuyFuelBlocks),
                new XAttribute("ReactionDepth", settings.ReactionDepth ?? "Advanced"),
                new XAttribute("Decryptor", settings.Decryptor ?? "None"),
                new XAttribute("AutoDecryptor", settings.AutoDecryptor),
                new XAttribute("TypeShips", settings.TypeShips),
                new XAttribute("TypeAmmo", settings.TypeAmmo),
                new XAttribute("TypeModules", settings.TypeModules),
                new XAttribute("TypeRigs", settings.TypeRigs),
                new XAttribute("TypeDrones", settings.TypeDrones),
                new XAttribute("TypeComponents", settings.TypeComponents),
                new XAttribute("TypeStructures", settings.TypeStructures),
                new XAttribute("TypeMisc", settings.TypeMisc),
                new XAttribute("ProfitableOnly", settings.ProfitableOnly),
                new XAttribute("HideMissingPrices", settings.HideMissingPrices),
                new XAttribute("HideLowVolume", settings.HideLowVolume),
                new XAttribute("HideLimitedBlueprints", settings.HideLimitedBlueprints),
                new XAttribute("HideRareNoise", settings.HideRareNoise),
                new XAttribute("AllowT2", settings.AllowT2),
                new XAttribute("AllowT3", settings.AllowT3),
                new XAttribute("AllowCapital", settings.AllowCapital),
                new XAttribute("AllowReactions", settings.AllowReactions),
                new XAttribute("ShowExcludedBlueprints", settings.ShowExcludedBlueprints),
                new XAttribute("HideBySkills", settings.HideBySkills),
                new XAttribute("TopOnly", settings.TopOnly),
                new XAttribute("TopBlueprintCount", settings.TopBlueprintCount ?? "10"),
                new XAttribute("MinSvr", settings.MinSvr ?? "0"),
                new XAttribute("SvrDays", settings.SvrDays ?? "7"),
                new XAttribute("PriceTrendFilter", settings.PriceTrendFilter ?? "Any"),
                new XAttribute("MinSold", settings.MinSold ?? "0"),
                new XAttribute("MinOrders", settings.MinOrders ?? "0"),
                new XAttribute("UseContractPrices", settings.UseContractPrices),
                new XAttribute("MinIskHour", settings.MinIskHour ?? "0"),
                new XAttribute("MinRoi", settings.MinRoi ?? "0"),
                new XAttribute("MaterialPriceMode", settings.MaterialPriceMode ?? "Min Sell"),
                new XAttribute("ProductPriceMode", settings.ProductPriceMode ?? "Min Sell"),
                new XAttribute("MaterialPriceModifier", settings.MaterialPriceModifier ?? "0"),
                new XAttribute("ProductPriceModifier", settings.ProductPriceModifier ?? "0"),
                new XAttribute("BlueprintDetailHeight", settings.BlueprintDetailHeight.ToString(CultureInfo.InvariantCulture))));

            doc.Save(_filePath);
        }

        private static string ReadString(XElement root, string attributeName, string defaultValue)
        {
            return (string)root.Attribute(attributeName) ?? defaultValue;
        }

        private static bool ReadBool(XElement root, string attributeName, bool defaultValue)
        {
            bool value;
            return bool.TryParse((string)root.Attribute(attributeName), out value) ? value : defaultValue;
        }

        private static double ReadDouble(XElement root, string attributeName, double defaultValue)
        {
            double value;
            return double.TryParse((string)root.Attribute(attributeName), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : defaultValue;
        }
    }
}
