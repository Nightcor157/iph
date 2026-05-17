using System;
using System.Collections.ObjectModel;

namespace OurIPH.Models
{
    public class BuildProject
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ObservableCollection<BuildProjectItem> Items { get; set; } = new ObservableCollection<BuildProjectItem>();
        public ObservableCollection<BuildProjectStock> Stock { get; set; } = new ObservableCollection<BuildProjectStock>();
        public ObservableCollection<BuildProjectBuildBuyDecision> BuildBuyDecisions { get; set; } = new ObservableCollection<BuildProjectBuildBuyDecision>();

        public int CompletedCount
        {
            get
            {
                var count = 0;
                foreach (var item in Items)
                {
                    if (item.IsCompleted)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public string SummaryText
        {
            get
            {
                var totalRootProfit = 0.0;
                var rootCount = 0;
                var lowMarketCount = 0;
                foreach (var item in Items)
                {
                    if (!item.IsRootItem)
                    {
                        continue;
                    }

                    rootCount++;
                    totalRootProfit += item.Profit;
                    if (item.ProductMarketVolume > 0 && item.ProductMarketVolume < item.NeededQuantity)
                    {
                        lowMarketCount++;
                    }
                }

                var text = string.Format("{0} / {1} готово", CompletedCount, Items.Count);
                if (rootCount > 0)
                {
                    text += string.Format(" | прибыль {0:N0} ISK", totalRootProfit);
                }

                if (lowMarketCount > 0)
                {
                    text += string.Format(" | тонкий рынок {0}", lowMarketCount);
                }

                return text;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}  ({1})", Name, SummaryText);
        }
    }

    public class BuildProjectItem
    {
        public long BlueprintTypeId { get; set; }
        public long ProductTypeId { get; set; }
        public string BlueprintName { get; set; }
        public string ProductName { get; set; }
        public string GroupName { get; set; }
        public string ProductionType { get; set; }
        public bool IsRootItem { get; set; }
        public int Wave { get; set; }
        public int Runs { get; set; }
        public int PortionSize { get; set; }
        public int RequiredQuantity { get; set; }
        public int CompletedRuns { get; set; }
        public double MaterialEfficiency { get; set; }
        public double TimeEfficiency { get; set; }
        public long DecryptorTypeId { get; set; }
        public string DecryptorName { get; set; }
        public string BestStationName { get; set; }
        public string BestStationSystem { get; set; }
        public double MaterialCost { get; set; }
        public double InstallationCost { get; set; }
        public double InventionCost { get; set; }
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
        public double ProductionTimeSeconds { get; set; }
        public double Revenue { get; set; }
        public double Profit { get; set; }
        public double ContractUnitPrice { get; set; }
        public string ProductPriceSource { get; set; }
        public string ProductPriceDetails { get; set; }
        public double ReturnOnInvestmentPercent { get; set; }
        public long ProductMarketVolume { get; set; }
        public double SalesVolumeRatio { get; set; }
        public double SvrTimesIskPerHour { get; set; }
        public long TotalItemsSold { get; set; }
        public long TotalOrdersFilled { get; set; }
        public double AverageItemsPerOrder { get; set; }
        public double PriceTrend { get; set; }
        public int BuildMaterialLines { get; set; }
        public int BuyMaterialLines { get; set; }
        public string EstimateStatus { get; set; }

        public int TotalQuantity
        {
            get { return Math.Max(1, Runs) * Math.Max(1, PortionSize); }
        }

        public int CompletedQuantity
        {
            get { return Math.Min(TotalQuantity, Math.Max(0, CompletedRuns) * Math.Max(1, PortionSize)); }
        }

        public int RemainingQuantity
        {
            get { return Math.Max(0, NeededQuantity - CompletedQuantity); }
        }

        public int NeededQuantity
        {
            get { return RequiredQuantity > 0 ? RequiredQuantity : TotalQuantity; }
        }

        public int SurplusQuantity
        {
            get { return Math.Max(0, TotalQuantity - NeededQuantity); }
        }

        public bool IsCompleted
        {
            get { return RemainingQuantity <= 0; }
        }

        public string ProductIconUrl
        {
            get { return string.Format("https://images.evetech.net/types/{0}/icon?size=64", ProductTypeId); }
        }

        public string WaveText
        {
            get { return "Волна " + Wave; }
        }

        public string RemainingText
        {
            get { return IsCompleted ? "Готово" : RemainingQuantity.ToString("N0"); }
        }

        public string SurplusText
        {
            get { return SurplusQuantity > 0 ? SurplusQuantity.ToString("N0") : ""; }
        }

        public string EfficiencyText
        {
            get { return string.Format("ME {0:0.##} / TE {1:0.##}", MaterialEfficiency, TimeEfficiency); }
        }

        public string DecryptorText
        {
            get { return DecryptorTypeId > 0 ? DecryptorName : ""; }
        }

        public string BestStationText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BestStationName))
                {
                    return "";
                }

                return string.IsNullOrWhiteSpace(BestStationSystem)
                    ? BestStationName
                    : BestStationName + " / " + BestStationSystem;
            }
        }

        public string MaterialCostText
        {
            get { return MaterialCost > 0 ? MaterialCost.ToString("N0") : ""; }
        }

        public string InstallationCostText
        {
            get { return InstallationCost > 0 ? InstallationCost.ToString("N0") : ""; }
        }

        public string InventionCostText
        {
            get { return InventionCost > 0 ? InventionCost.ToString("N0") : ""; }
        }

        public string InventionPlanText
        {
            get
            {
                return InventionJobs > 0
                    ? string.Format("{0:0.#}% | jobs {1:N0} | runs {2}", InventionChancePercent, InventionJobs, InventedRunsPerSuccess)
                    : "";
            }
        }

        public string InventionBreakdownText
        {
            get
            {
                if (InventionCost <= 0)
                {
                    return "";
                }

                return string.Format("mats {0:N0} | copy {1:N0} | usage {2:N0}",
                    InventionMaterialsCost, InventionCopyCost, InventionJobUsageCost);
            }
        }

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

        public string TotalBuildCostText
        {
            get
            {
                var total = MaterialCost + InventionCost + InstallationCost;
                return total > 0 ? total.ToString("N0") : "";
            }
        }

        public string RevenueText
        {
            get { return Revenue > 0 ? Revenue.ToString("N0") : ""; }
        }

        public string ProfitText
        {
            get { return Profit != 0 ? Profit.ToString("N0") : ""; }
        }

        public string ContractUnitPriceText
        {
            get { return ContractUnitPrice > 0 ? ContractUnitPrice.ToString("N0") : ""; }
        }

        public string ProductPriceSourceText
        {
            get { return string.IsNullOrWhiteSpace(ProductPriceSource) ? "Market" : ProductPriceSource; }
        }

        public string ProductPriceDetailsText
        {
            get { return string.IsNullOrWhiteSpace(ProductPriceDetails) ? "" : ProductPriceDetails; }
        }

        public string ProfitBrush
        {
            get { return Profit > 0 ? "#1A7F37" : Profit < 0 ? "#B42318" : "#657386"; }
        }

        public string ReturnOnInvestmentText
        {
            get { return TotalBuildCost > 0 ? ReturnOnInvestmentPercent.ToString("N1") + "%" : ""; }
        }

        public string ProductMarketVolumeText
        {
            get { return ProductMarketVolume > 0 ? ProductMarketVolume.ToString("N0") : ""; }
        }

        public string SalesVolumeRatioText
        {
            get { return SalesVolumeRatio > 0 ? SalesVolumeRatio.ToString("N2") : "-"; }
        }

        public string SvrTimesIskPerHourText
        {
            get { return SvrTimesIskPerHour > 0 ? SvrTimesIskPerHour.ToString("N0") : ""; }
        }

        public string TotalItemsSoldText
        {
            get { return TotalItemsSold > 0 ? TotalItemsSold.ToString("N0") : ""; }
        }

        public string TotalOrdersFilledText
        {
            get { return TotalOrdersFilled > 0 ? TotalOrdersFilled.ToString("N0") : ""; }
        }

        public string AverageItemsPerOrderText
        {
            get { return AverageItemsPerOrder > 0 ? AverageItemsPerOrder.ToString("N2") : ""; }
        }

        public string PriceTrendText
        {
            get { return PriceTrend != 0 ? PriceTrend.ToString("P2") : ""; }
        }

        public string PriceTrendBrush
        {
            get { return PriceTrend > 0 ? "#1A7F37" : PriceTrend < 0 ? "#B42318" : "#657386"; }
        }

        public string MarketCoverageText
        {
            get { return ProductMarketVolume > 0 && NeededQuantity > 0 ? (ProductMarketVolume / (double)NeededQuantity).ToString("N1") + "x" : ""; }
        }

        public string BuildBuyText
        {
            get { return BuildMaterialLines > 0 ? string.Format("Build {0} / Buy {1}", BuildMaterialLines, BuyMaterialLines) : ""; }
        }

        public string ProductionTimeText
        {
            get
            {
                if (ProductionTimeSeconds <= 0)
                {
                    return "";
                }

                var value = TimeSpan.FromSeconds(ProductionTimeSeconds);
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
        }

        private static string FormatStation(string name, string system)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            return string.IsNullOrWhiteSpace(system) ? name : name + " / " + system;
        }

        private double TotalBuildCost
        {
            get { return MaterialCost + InventionCost + InstallationCost; }
        }
    }

    public class BuildProjectMaterial
    {
        public long TypeId { get; set; }
        public string Name { get; set; }
        public int Wave { get; set; }
        public long PriorQuantity { get; set; }
        public long Quantity { get; set; }
        public long OwnedQuantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalCost { get; set; }
        public string UsedBy { get; set; }
        public string BuildBuyMode { get; set; }
        public string SourceMode { get; set; }
        public long ProducesTypeId { get; set; }
        public long ProducesQuantity { get; set; }
        public string SourceDetails { get; set; }
        public string TargetStationName { get; set; }
        public string TargetStationSystem { get; set; }
        public string TargetActivity { get; set; }

        public string IconUrl
        {
            get { return string.Format("https://images.evetech.net/types/{0}/icon?size=64", TypeId); }
        }

        public string WaveText
        {
            get { return Wave > 0 ? "Волна " + Wave : ""; }
        }

        public string QuantityText
        {
            get { return Quantity.ToString("N0"); }
        }

        public string PriorQuantityText
        {
            get { return PriorQuantity > 0 ? PriorQuantity.ToString("N0") : ""; }
        }

        public long RequiredThroughWave
        {
            get { return PriorQuantity + Quantity; }
        }

        public string RequiredThroughWaveText
        {
            get { return RequiredThroughWave.ToString("N0"); }
        }

        public long RemainingToBuy
        {
            get { return Math.Max(0, Quantity - OwnedQuantity); }
        }

        public string OwnedQuantityText
        {
            get { return OwnedQuantity > 0 ? OwnedQuantity.ToString("N0") : ""; }
        }

        public string RemainingToBuyText
        {
            get { return RemainingToBuy.ToString("N0"); }
        }

        public string UnitPriceText
        {
            get { return UnitPrice > 0 ? UnitPrice.ToString("N2") : ""; }
        }

        public string TotalCostText
        {
            get { return TotalCost > 0 ? TotalCost.ToString("N0") : ""; }
        }

        public string StatusBrush
        {
            get { return RemainingToBuy <= 0 && Quantity > 0 ? "#1A7F37" : "#1D2733"; }
        }

        public string BuildBuyModeText
        {
            get
            {
                switch (BuildBuyMode)
                {
                    case "Build":
                        return "Строить";
                    case "Buy":
                        return "Купить";
                    default:
                        return "Авто";
                }
            }
        }

        public string SourceText
        {
            get
            {
                if (SourceMode == "Reprocessing")
                {
                    return string.IsNullOrWhiteSpace(SourceDetails) ? "Переработка" : SourceDetails;
                }

                return "";
            }
        }

        public string SourceDisplayText
        {
            get
            {
                switch (SourceMode)
                {
                    case "Reprocessing":
                        return string.IsNullOrWhiteSpace(SourceDetails) ? "Переработка" : SourceDetails;
                    case "Invention":
                        return string.IsNullOrWhiteSpace(SourceDetails) ? "Invention" : SourceDetails;
                    case "Copying":
                        return string.IsNullOrWhiteSpace(SourceDetails) ? "Copying" : SourceDetails;
                    default:
                        return "";
                }
            }
        }

        public string SourceGroupText
        {
            get
            {
                switch (SourceMode)
                {
                    case "Reprocessing":
                        return "Переработка";
                    case "Invention":
                        return "Материалы invention";
                    case "Copying":
                        return "Материалы copy";
                    default:
                        return "Материалы производства";
                }
            }
        }

        public string TargetLocationText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TargetStationName))
                {
                    return "";
                }

                var location = string.IsNullOrWhiteSpace(TargetStationSystem)
                    ? TargetStationName
                    : TargetStationName + " / " + TargetStationSystem;
                return string.IsNullOrWhiteSpace(TargetActivity) ? location : location + " (" + TargetActivity + ")";
            }
        }
    }

    public class BuildProjectStock
    {
        public long TypeId { get; set; }
        public long OwnedQuantity { get; set; }
    }

    public class BuildProjectStageSummary
    {
        public int Wave { get; set; }
        public string Stage { get; set; }
        public int TotalLines { get; set; }
        public int OpenLines { get; set; }
        public long RequiredQuantity { get; set; }
        public long OwnedQuantity { get; set; }
        public long RemainingQuantity { get; set; }
        public double RemainingCost { get; set; }

        public string WaveText
        {
            get { return Wave > 0 ? "Волна " + Wave : ""; }
        }

        public string LinesText
        {
            get { return string.Format("{0} / {1}", OpenLines, TotalLines); }
        }

        public string RequiredQuantityText
        {
            get { return RequiredQuantity.ToString("N0"); }
        }

        public string OwnedQuantityText
        {
            get { return OwnedQuantity > 0 ? OwnedQuantity.ToString("N0") : ""; }
        }

        public string RemainingQuantityText
        {
            get { return RemainingQuantity.ToString("N0"); }
        }

        public string RemainingCostText
        {
            get { return RemainingCost > 0 ? RemainingCost.ToString("N0") : ""; }
        }

        public string StatusText
        {
            get { return OpenLines == 0 && TotalLines > 0 ? "Готово" : "Открыто"; }
        }

        public string StatusBrush
        {
            get { return OpenLines == 0 && TotalLines > 0 ? "#1A7F37" : "#1D2733"; }
        }
    }

    public class BuildProjectBuildBuyDecision
    {
        public long TypeId { get; set; }
        public string Mode { get; set; }
    }
}
