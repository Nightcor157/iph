namespace OurIPH.Models
{
    public class BlueprintSearchResult
    {
        public long BlueprintTypeId { get; set; }
        public long ProductTypeId { get; set; }
        public string BlueprintName { get; set; }
        public string ProductName { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public int GroupId { get; set; }
        public int CategoryId { get; set; }
        public string ItemMarketGroup { get; set; }
        public string BlueprintMarketGroup { get; set; }
        public int MetaGroupId { get; set; }
        public int TechLevel { get; set; }
        public int PortionSize { get; set; }
        public int BaseProductionTime { get; set; }
        public int MaxProductionLimit { get; set; }
        public bool IsCopyOnlyBlueprint { get; set; }
        public bool HasReactionActivity { get; set; }
        public double MaterialCost { get; set; }
        public double InstallationCost { get; set; }
        public double InventionCost { get; set; }
        public double TotalCost { get; set; }
        public double Revenue { get; set; }
        public double Profit { get; set; }
        public double ContractUnitPrice { get; set; }
        public string ProductPriceSource { get; set; }
        public string ProductPriceDetails { get; set; }
        public double IskPerHour { get; set; }
        public double MarginPercent { get; set; }
        public double ReturnOnInvestmentPercent { get; set; }
        public double FacilityMaterialBonusPercent { get; set; }
        public double CostIndexPercent { get; set; }
        public double ProductionTimeSeconds { get; set; }
        public int ProfitRank { get; set; }
        public double TopScore { get; set; }
        public double InventionChancePercent { get; set; }
        public int InventionJobs { get; set; }
        public int InventedRunsPerSuccess { get; set; }
        public double InventionMaterialsCost { get; set; }
        public double InventionCopyCost { get; set; }
        public double InventionJobUsageCost { get; set; }
        public string InventionStationName { get; set; }
        public string InventionStationSystem { get; set; }
        public string CopyStationName { get; set; }
        public string CopyStationSystem { get; set; }
        public long ProductMarketVolume { get; set; }
        public long ProducedQuantity { get; set; }
        public double SalesVolumeRatio { get; set; }
        public double SvrTimesIskPerHour { get; set; }
        public long TotalItemsSold { get; set; }
        public long TotalOrdersFilled { get; set; }
        public double AverageItemsPerOrder { get; set; }
        public double PriceTrend { get; set; }
        public long CurrentBuyOrders { get; set; }
        public long CurrentSellOrders { get; set; }
        public int RequiredSkillsCount { get; set; }
        public int MissingSkillsCount { get; set; } = -1;
        public string RequiredSkillSummary { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }
        public string EstimateStatus { get; set; }
        public string BestFacilityName { get; set; }
        public string BestFacilitySystem { get; set; }
        public long SelectedDecryptorTypeId { get; set; }
        public string SelectedDecryptorName { get; set; }

        public string ProductIconUrl => $"https://images.evetech.net/types/{ProductTypeId}/icon?size=64";
        public string BlueprintIconUrl => $"https://images.evetech.net/types/{BlueprintTypeId}/bp?size=64";
        public string MetaGroupText => MetaGroupId > 0 ? MetaGroupId.ToString("N0") : "";
        public string MaterialCostText => MaterialCost > 0 ? MaterialCost.ToString("N0") : "";
        public string InstallationCostText => InstallationCost > 0 ? InstallationCost.ToString("N0") : "";
        public string InventionCostText => InventionCost > 0 ? InventionCost.ToString("N0") : "";
        public string TotalCostText => TotalCost > 0 ? TotalCost.ToString("N0") : "";
        public string RevenueText => Revenue > 0 ? Revenue.ToString("N0") : "";
        public string ContractUnitPriceText => ContractUnitPrice > 0 ? ContractUnitPrice.ToString("N0") : "";
        public string ProductPriceSourceText => string.IsNullOrWhiteSpace(ProductPriceSource) ? "Market" : ProductPriceSource;
        public string ProductPriceDetailsText => string.IsNullOrWhiteSpace(ProductPriceDetails) ? "" : ProductPriceDetails;
        public string ProfitText => Profit != 0 ? Profit.ToString("N0") : "";
        public string IskPerHourText => IskPerHour != 0 ? IskPerHour.ToString("N0") : "";
        public string ProfitBrush => Profit > 0 ? "#1A7F37" : Profit < 0 ? "#B42318" : "#657386";
        public string MarginText => Revenue > 0 ? MarginPercent.ToString("N1") + "%" : "";
        public string ReturnOnInvestmentText => TotalCost > 0 ? ReturnOnInvestmentPercent.ToString("N1") + "%" : "";
        public string ProfitRankText => ProfitRank > 0 ? ProfitRank.ToString("N0") : "";
        public int ProfitRankSort => ProfitRank > 0 ? ProfitRank : int.MaxValue;
        public string TopScoreText => TopScore > 0 ? TopScore.ToString("N0") : "";
        public string TopScoreBreakdownText
        {
            get
            {
                if (TopScore <= 0)
                {
                    return "";
                }

                var source = string.IsNullOrWhiteSpace(ProductPriceSource) ? "Market" : ProductPriceSource;
                return string.Format("SVR {0:N2} | profit {1:N0} | ROI {2:N1}% | trend {3:P1} | source {4}",
                    SalesVolumeRatio,
                    Profit,
                    ReturnOnInvestmentPercent,
                    PriceTrend,
                    source);
            }
        }
        public string SalesVolumeRatioText => SalesVolumeRatio > 0 ? SalesVolumeRatio.ToString("N2") : "-";
        public string SvrTimesIskPerHourText => SvrTimesIskPerHour > 0 ? SvrTimesIskPerHour.ToString("N0") : "";
        public string TotalItemsSoldText => TotalItemsSold > 0 ? TotalItemsSold.ToString("N0") : "";
        public string TotalOrdersFilledText => TotalOrdersFilled > 0 ? TotalOrdersFilled.ToString("N0") : "";
        public string AverageItemsPerOrderText => AverageItemsPerOrder > 0 ? AverageItemsPerOrder.ToString("N2") : "";
        public string PriceTrendText => PriceTrend != 0 ? PriceTrend.ToString("P2") : "";
        public string PriceTrendBrush => PriceTrend > 0 ? "#1A7F37" : PriceTrend < 0 ? "#B42318" : "#657386";
        public string CurrentBuyOrdersText => CurrentBuyOrders > 0 ? CurrentBuyOrders.ToString("N0") : "";
        public string CurrentSellOrdersText => CurrentSellOrders > 0 ? CurrentSellOrders.ToString("N0") : "";
        public string SkillStatusText => MissingSkillsCount < 0 ? "" : MissingSkillsCount == 0 ? "OK" : "Нет " + MissingSkillsCount;
        public string SkillStatusBrush => MissingSkillsCount == 0 ? "#1A7F37" : MissingSkillsCount > 0 ? "#B42318" : "#657386";
        public string RequiredSkillsText => string.IsNullOrWhiteSpace(RequiredSkillSummary) ? "" : RequiredSkillSummary;
        public string ProductMarketVolumeText => ProductMarketVolume > 0 ? ProductMarketVolume.ToString("N0") : "";
        public string MarketCoverageText => ProductMarketVolume > 0 && ProducedQuantity > 0
            ? (ProductMarketVolume / (double)ProducedQuantity).ToString("N1") + "x"
            : "";
        public string MarketRiskText
        {
            get
            {
                if (ProductMarketVolume <= 0)
                {
                    return Revenue > 0 ? "No volume" : "";
                }

                if (ProducedQuantity <= 0)
                {
                    return "";
                }

                if (ProductMarketVolume < ProducedQuantity)
                {
                    return "Low";
                }

                return ProductMarketVolume < ProducedQuantity * 5L ? "Thin" : "OK";
            }
        }
        public string MarketRiskBrush
        {
            get
            {
                switch (MarketRiskText)
                {
                    case "OK":
                        return "#1A7F37";
                    case "Thin":
                        return "#B7791F";
                    case "Low":
                    case "No volume":
                        return "#B42318";
                    default:
                        return "#657386";
                }
            }
        }
        public string FacilityMaterialBonusText => FacilityMaterialBonusPercent > 0 ? FacilityMaterialBonusPercent.ToString("N2") + "%" : "";
        public string CostIndexText => CostIndexPercent > 0 ? CostIndexPercent.ToString("N2") + "%" : "";
        public string ProductionTimeText => FormatDuration(ProductionTimeSeconds);
        public string BuildBuyText => BuildMaterialLines > 0 ? string.Format("Build {0} / Buy {1}", BuildMaterialLines, BuyMaterialLines) : "";
        public string TopReasonText
        {
            get
            {
                if (ProfitRank <= 0)
                {
                    return "";
                }

                var source = string.IsNullOrWhiteSpace(ProductPriceSource) ? "Market" : ProductPriceSource;
                var liquidity = SalesVolumeRatio > 0 ? string.Format("SVR {0:N2}", SalesVolumeRatio) : "SVR n/a";
                return string.Format("#{0}: score {1:N0}, {2:N0} ISK profit, {3:N0} ISK/h, {4}, {5}",
                    ProfitRank,
                    TopScore,
                    Profit,
                    IskPerHour,
                    liquidity,
                    source);
            }
        }
        public string DecryptorText => SelectedDecryptorTypeId > 0 ? SelectedDecryptorName : "";
        public string BlueprintAvailabilityText
        {
            get
            {
                if (!IsCopyOnlyBlueprint)
                {
                    return "BPO";
                }

                return "BPC only | max " + System.Math.Max(1, MaxProductionLimit).ToString("N0") + " run(s) | ME/TE 0/0";
            }
        }
        public string InventionPlanText => InventionJobs > 0
            ? string.Format("{0:0.#}% | jobs {1:N0} | runs {2}", InventionChancePercent, InventionJobs, InventedRunsPerSuccess)
            : "";
        public string InventionBreakdownText => InventionCost > 0
            ? string.Format("mats {0:N0} | copy {1:N0} | usage {2:N0}", InventionMaterialsCost, InventionCopyCost, InventionJobUsageCost)
            : "";
        public string ScienceStationText
        {
            get
            {
                var invention = FormatStation(InventionStationName, InventionStationSystem);
                var copy = FormatStation(CopyStationName, CopyStationSystem);
                if (string.IsNullOrWhiteSpace(invention) && string.IsNullOrWhiteSpace(copy))
                {
                    return "";
                }

                return string.IsNullOrWhiteSpace(copy) || copy == invention
                    ? invention
                    : "Inv: " + invention + " | Copy: " + copy;
            }
        }
        public string BestFacilityText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BestFacilityName))
                {
                    return "";
                }

                return string.IsNullOrWhiteSpace(BestFacilitySystem)
                    ? BestFacilityName
                    : BestFacilityName + " (" + BestFacilitySystem + ")";
            }
        }

        private static string FormatDuration(double seconds)
        {
            if (seconds <= 0)
            {
                return "";
            }

            var value = System.TimeSpan.FromSeconds(seconds);
            if (value.TotalDays >= 1)
            {
                return string.Format("{0:0}d {1:00}h", value.TotalDays, value.Hours);
            }

            if (value.TotalHours >= 1)
            {
                return string.Format("{0:0}h {1:00}m", value.TotalHours, value.Minutes);
            }

            return string.Format("{0:0}m", value.TotalMinutes);
        }

        private static string FormatStation(string name, string system)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            return string.IsNullOrWhiteSpace(system) ? name : name + " / " + system;
        }
    }
}
