namespace OurIPH.Models
{
    public class UiSettings
    {
        public bool AlwaysBuyRam { get; set; } = true;
        public bool AlwaysBuyFuelBlocks { get; set; } = true;
        public string ReactionDepth { get; set; } = "Advanced";
        public string MaterialPriceMode { get; set; } = "Min Sell";
        public string ProductPriceMode { get; set; } = "Min Sell";
        public string Decryptor { get; set; } = "None";
        public bool AutoDecryptor { get; set; } = true;
        public bool TypeShips { get; set; } = true;
        public bool TypeAmmo { get; set; } = true;
        public bool TypeModules { get; set; } = true;
        public bool TypeRigs { get; set; } = true;
        public bool TypeDrones { get; set; } = true;
        public bool TypeComponents { get; set; } = true;
        public bool TypeStructures { get; set; } = true;
        public bool TypeMisc { get; set; } = true;
        public bool ProfitableOnly { get; set; }
        public bool HideMissingPrices { get; set; } = true;
        public bool HideLowVolume { get; set; } = true;
        public bool HideLimitedBlueprints { get; set; } = true;
        public bool HideRareNoise { get; set; } = true;
        public bool AllowT2 { get; set; } = true;
        public bool AllowT3 { get; set; } = true;
        public bool AllowCapital { get; set; } = true;
        public bool AllowReactions { get; set; } = true;
        public bool ShowExcludedBlueprints { get; set; }
        public bool HideBySkills { get; set; }
        public bool TopOnly { get; set; } = true;
        public string TopBlueprintCount { get; set; } = "10";
        public string MinSvr { get; set; } = "0";
        public string SvrDays { get; set; } = "7";
        public string PriceTrendFilter { get; set; } = "Any";
        public string MinSold { get; set; } = "0";
        public string MinOrders { get; set; } = "0";
        public bool UseContractPrices { get; set; } = true;
        public string MinIskHour { get; set; } = "0";
        public string MinRoi { get; set; } = "0";
        public string MaterialPriceModifier { get; set; } = "0";
        public string ProductPriceModifier { get; set; } = "0";
        public double BlueprintDetailHeight { get; set; } = 280;
    }
}
