using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OurIPH.Models;
using OurIPH.Services;

namespace OurIPH
{
    public partial class MainWindow : Window
    {
        private readonly EveDatabaseService _database;
        private readonly FuzzworkMarketService _market;
        private readonly EsiMarketHistoryService _marketHistoryService;
        private readonly EsiMarketOrderService _marketOrderService;
        private readonly FacilityPresetStore _facilityStore;
        private readonly BuildQueueStore _buildQueueStore;
        private readonly BuildProjectStore _projectStore;
        private readonly MarketPriceCacheStore _priceCacheStore;
        private readonly MarketHistoryCacheStore _marketHistoryCacheStore;
        private readonly ExcludedBlueprintStore _excludedBlueprintStore;
        private readonly CharacterSkillStore _characterSkillStore;
        private readonly ContractPriceStore _contractPriceStore;
        private readonly FacilityCatalogService _facilityCatalog;
        private readonly BuildProjectDecisionService _buildProjectDecisionService;
        private readonly BuildQueueService _buildQueueService;
        private readonly ContractPricingService _contractPricingService;
        private readonly EsiPublicContractService _esiPublicContractService;
        private readonly FacilityActivityStationService _facilityActivityStationService;
        private readonly BlueprintBuildProfileService _blueprintBuildProfileService;
        private readonly BlueprintCopyStatusService _blueprintCopyStatusService;
        private readonly BlueprintEfficiencyService _blueprintEfficiencyService;
        private readonly BlueprintEstimateApplicationService _blueprintEstimateApplicationService;
        private readonly BlueprintEstimateCacheService _blueprintEstimateCacheService;
        private readonly BlueprintEstimateMaterialOrchestrationService _blueprintEstimateMaterialOrchestrationService;
        private readonly BlueprintEstimateResultAssembler _blueprintEstimateResultAssembler;
        private readonly BlueprintEstimateSelectionService _blueprintEstimateSelectionService;
        private readonly BlueprintEstimateStatusService _blueprintEstimateStatusService;
        private readonly BlueprintFilteringService _blueprintFilteringService;
        private readonly BlueprintInventionCostService _blueprintInventionCostService;
        private readonly BlueprintInventionTimeService _blueprintInventionTimeService;
        private readonly BlueprintListFilterService _blueprintListFilterService;
        private readonly BlueprintManufacturingMathService _blueprintManufacturingMathService;
        private readonly BlueprintProductionTypeService _blueprintProductionTypeService;
        private readonly BlueprintTypeFilterService _blueprintTypeFilterService;
        private readonly BlueprintRankingService _blueprintRankingService;
        private readonly BlueprintSkillRequirementService _blueprintSkillRequirementService;
        private readonly NumericInputValidationService _numericInputValidationService;
        private readonly PriceUpdateStatusService _priceUpdateStatusService;
        private readonly MarketPriceCacheService _marketPriceCacheService;
        private readonly MarketHistoryCacheService _marketHistoryCacheService;
        private readonly MarketPriceSelectionService _marketPriceSelectionService;
        private readonly OrePlanningService _orePlanningService;
        private readonly SalesFeeService _salesFeeService;
        private readonly SalesVolumeRatioService _salesVolumeRatioService;
        private readonly ProjectMaterialImportService _projectMaterialImportService;
        private readonly ProjectItemEstimateDisplayService _projectItemEstimateDisplayService;
        private readonly ProjectQueueDisplayService _projectQueueDisplayService;
        private readonly UsedByListService _usedByListService;
        private readonly UiSettingsStore _uiSettingsStore;
        private IReadOnlyList<FacilityStructureType> _structureTypes;
        private IReadOnlyList<FacilityRigOption> _rigOptions;
        private IReadOnlyList<FacilityServiceModuleOption> _serviceModuleOptions;
        private IReadOnlyList<EveRegion> _regions;
        private IReadOnlyList<EveSolarSystem> _systems;
        private IReadOnlyList<DecryptorOption> _decryptors;
        private readonly ObservableCollection<BlueprintSearchResult> _blueprints = new ObservableCollection<BlueprintSearchResult>();
        private readonly ObservableCollection<BuildQueueItem> _buildQueue = new ObservableCollection<BuildQueueItem>();
        private readonly ObservableCollection<MaterialRequirement> _selectedBlueprintMaterials = new ObservableCollection<MaterialRequirement>();
        private readonly ObservableCollection<MaterialRequirement> _selectedBlueprintRawMaterials = new ObservableCollection<MaterialRequirement>();
        private readonly ObservableCollection<SkillRequirement> _selectedBlueprintSkills = new ObservableCollection<SkillRequirement>();
        private readonly ObservableCollection<ContractPriceSampleReview> _selectedContractSampleReviews = new ObservableCollection<ContractPriceSampleReview>();
        private readonly ObservableCollection<BuildProjectMaterial> _projectMaterials = new ObservableCollection<BuildProjectMaterial>();
        private readonly ObservableCollection<BuildProjectStageSummary> _projectStageSummaries = new ObservableCollection<BuildProjectStageSummary>();
        private ObservableCollection<BuildProject> _projects;
        private ObservableCollection<FacilityPreset> _facilities;
        private ObservableCollection<MarketPriceCacheEntry> _priceCache;
        private ObservableCollection<MarketHistoryStats> _marketHistoryCache;
        private ObservableCollection<ContractPriceSample> _contractPrices;
        private HashSet<long> _excludedBlueprintProductIds;
        private Dictionary<long, int> _characterSkills;
        private readonly Dictionary<long, IReadOnlyList<SkillRequirement>> _requiredSkillCache = new Dictionary<long, IReadOnlyList<SkillRequirement>>();
        private readonly Dictionary<TextBox, NumericInputRule> _numericInputRules = new Dictionary<TextBox, NumericInputRule>();
        private IReadOnlyList<MarketRegion> _marketRegions;
        private string _activeBlueprintSearchText = "";
        private bool _loadingFacilitySelection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNumericInputValidation();

            _database = new EveDatabaseService(AppPaths.EveIphDatabasePath);
            _market = new FuzzworkMarketService();
            _marketHistoryService = new EsiMarketHistoryService();
            _marketOrderService = new EsiMarketOrderService();
            _facilityStore = new FacilityPresetStore();
            _buildQueueStore = new BuildQueueStore();
            _projectStore = new BuildProjectStore();
            _priceCacheStore = new MarketPriceCacheStore();
            _marketHistoryCacheStore = new MarketHistoryCacheStore();
            _excludedBlueprintStore = new ExcludedBlueprintStore();
            _characterSkillStore = new CharacterSkillStore();
            _contractPriceStore = new ContractPriceStore();
            _facilityCatalog = new FacilityCatalogService(AppPaths.EveIphDatabasePath);
            _buildProjectDecisionService = new BuildProjectDecisionService();
            _buildQueueService = new BuildQueueService();
            _contractPricingService = new ContractPricingService();
            _esiPublicContractService = new EsiPublicContractService();
            _facilityActivityStationService = new FacilityActivityStationService();
            _blueprintFilteringService = new BlueprintFilteringService(new BlueprintFilterRuleStore().LoadOrCreateDefaults());
            _blueprintCopyStatusService = new BlueprintCopyStatusService();
            _blueprintEfficiencyService = new BlueprintEfficiencyService();
            _blueprintEstimateApplicationService = new BlueprintEstimateApplicationService();
            _blueprintEstimateCacheService = new BlueprintEstimateCacheService();
            _blueprintEstimateMaterialOrchestrationService = new BlueprintEstimateMaterialOrchestrationService();
            _blueprintEstimateResultAssembler = new BlueprintEstimateResultAssembler();
            _blueprintEstimateSelectionService = new BlueprintEstimateSelectionService();
            _blueprintEstimateStatusService = new BlueprintEstimateStatusService();
            _blueprintBuildProfileService = new BlueprintBuildProfileService(_blueprintFilteringService);
            _blueprintInventionCostService = new BlueprintInventionCostService();
            _blueprintInventionTimeService = new BlueprintInventionTimeService();
            _blueprintListFilterService = new BlueprintListFilterService();
            _blueprintManufacturingMathService = new BlueprintManufacturingMathService();
            _blueprintProductionTypeService = new BlueprintProductionTypeService();
            _blueprintTypeFilterService = new BlueprintTypeFilterService();
            _blueprintRankingService = new BlueprintRankingService(_blueprintFilteringService);
            _blueprintSkillRequirementService = new BlueprintSkillRequirementService();
            _numericInputValidationService = new NumericInputValidationService();
            _priceUpdateStatusService = new PriceUpdateStatusService();
            _marketPriceCacheService = new MarketPriceCacheService();
            _marketHistoryCacheService = new MarketHistoryCacheService();
            _marketPriceSelectionService = new MarketPriceSelectionService();
            _orePlanningService = new OrePlanningService();
            _salesFeeService = new SalesFeeService();
            _salesVolumeRatioService = new SalesVolumeRatioService();
            _projectMaterialImportService = new ProjectMaterialImportService();
            _projectItemEstimateDisplayService = new ProjectItemEstimateDisplayService(
                new ProjectItemEstimateApplicationService(),
                _salesFeeService,
                _blueprintEstimateStatusService,
                _blueprintCopyStatusService);
            _projectQueueDisplayService = new ProjectQueueDisplayService();
            _usedByListService = new UsedByListService();
            _uiSettingsStore = new UiSettingsStore(AppPaths.GetSettingsPath("UiSettings.xml"));

            blueprintsGrid.ItemsSource = _blueprints;
            analysisBlueprintsGrid.ItemsSource = _blueprints;
            blueprintMaterialsGrid.ItemsSource = _selectedBlueprintMaterials;
            blueprintRawMaterialsGrid.ItemsSource = _selectedBlueprintRawMaterials;
            blueprintSkillsGrid.ItemsSource = _selectedBlueprintSkills;
            contractSamplesGrid.ItemsSource = _selectedContractSampleReviews;
            queueGrid.ItemsSource = _buildQueue;
            _projects = _projectStore.Load();
            _priceCache = _priceCacheStore.Load();
            _marketHistoryCache = _marketHistoryCacheStore.Load();
            _contractPrices = _contractPriceStore.Load();
            _excludedBlueprintProductIds = _excludedBlueprintStore.Load();
            _characterSkills = _characterSkillStore.Load();
            foreach (var queueItem in _buildQueueStore.Load(_database))
            {
                if (queueItem.Blueprint != null && queueItem.Blueprint.MissingSkillsCount < 0)
                {
                    UpdateBlueprintSkillStatus(queueItem.Blueprint);
                }

                _buildQueue.Add(queueItem);
            }
            UpdateQueueStatus();

            projectsList.ItemsSource = _projects;
            projectItemsGrid.ItemsSource = _projects.FirstOrDefault()?.Items;
            projectMaterialsGrid.ItemsSource = _projectMaterials;
            projectStageSummaryGrid.ItemsSource = _projectStageSummaries;
            priceCacheGrid.ItemsSource = _priceCache;

            _facilities = _facilityStore.Load();
            _structureTypes = _facilityCatalog.LoadStructureTypes("Manufacturing", 0.9, true);
            _regions = _facilityCatalog.LoadRegions();
            _decryptors = _database.LoadDecryptors();
            _marketRegions = BuildMarketRegions(_regions);
            marketRegionBox.ItemsSource = _marketRegions;
            blueprintRegionBox.ItemsSource = _marketRegions;
            blueprintDecryptorBox.ItemsSource = _decryptors;
            blueprintDecryptorBox.SelectedIndex = 0;
            marketRegionBox.SelectedItem = _marketRegions.FirstOrDefault(item => item.Name == "Jita 4-4") ?? _marketRegions.FirstOrDefault();
            blueprintRegionBox.SelectedItem = marketRegionBox.SelectedItem;
            facilityTypeBox.Items.Clear();
            facilityRigPresetBox.Items.Clear();
            facilityRigSlot2Box.Items.Clear();
            facilityRigSlot3Box.Items.Clear();
            facilityRegionBox.ItemsSource = _regions;
            facilityTypeBox.ItemsSource = _structureTypes;
            SetRigItemsSource(new[] { new FacilityRigOption { TypeId = 0, Name = "No rig", IsNone = true } });
            facilityList.ItemsSource = _facilities;
            blueprintFacilityBox.ItemsSource = _facilities;
            if (_facilities.Count > 0)
            {
                facilityList.SelectedIndex = 0;
                blueprintFacilityBox.SelectedIndex = 0;
            }

            InitializeDatabaseStatus();
            LoadUiSettings();
            SearchBlueprints("Revelation", 200);
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveBuildQueue();
            SaveUiSettings();
            base.OnClosed(e);
        }

        private void InitializeNumericInputValidation()
        {
            RegisterIntegerTextBox(runsBox, "1");
            RegisterDecimalTextBox(meBox, "10", false);
            RegisterDecimalTextBox(teBox, "20", false);
            RegisterDecimalTextBox(materialPriceModifierBox, "0", true);
            RegisterDecimalTextBox(productPriceModifierBox, "0", true);
            RegisterIntegerTextBox(topBlueprintCountBox, "10");
            RegisterIntegerTextBox(skillOwnedLevelBox, "5");
            RegisterDecimalTextBox(minSvrBox, "0", false);
            RegisterIntegerTextBox(svrDaysBox, "7");
            RegisterIntegerTextBox(minSoldBox, "0");
            RegisterIntegerTextBox(minOrdersBox, "0");
            RegisterDecimalTextBox(minIskHourBox, "0", false);
            RegisterDecimalTextBox(minRoiBox, "0", false);
            RegisterDecimalTextBox(contractPriceBox, "0", false);
            RegisterDecimalTextBox(facilityIndustryTaxBox, "0", false);
            RegisterDecimalTextBox(facilitySalesFeeBox, "0", false);
            RegisterIntegerTextBox(facilityIndustrySkillBox, "5");
            RegisterIntegerTextBox(facilityAdvancedIndustrySkillBox, "5");
            RegisterDecimalTextBox(facilityManufacturingImplantBox, "0", false);
            RegisterDecimalTextBox(facilitySccIndustryFeeBox, "4", false);
            RegisterIntegerTextBox(facilityAccountingSkillBox, "5");
            RegisterIntegerTextBox(facilityBrokerRelationsBox, "5");
            RegisterDecimalTextBox(facilityBrokerFactionStandingBox, "5", true);
            RegisterDecimalTextBox(facilityBrokerCorpStandingBox, "5", true);
            RegisterDecimalTextBox(facilityBaseSalesTaxBox, "4.5", false);
            RegisterDecimalTextBox(facilityBaseBrokerFeeBox, "3", false);
            RegisterDecimalTextBox(facilitySccBrokerFeeBox, "0.5", false);
            RegisterDecimalTextBox(facilityAlphaTaxBox, "0", false);
            RegisterDecimalTextBox(facilitySpecialBrokerFeeBox, "0", false);
            RegisterIntegerTextBox(facilityDefaultBpMeBox, "10");
            RegisterIntegerTextBox(facilityDefaultBpTeBox, "20");
            RegisterIntegerTextBox(facilityFwUpgradeBox, "0");
            RegisterIntegerTextBox(facilityProductionLinesBox, "10");
            RegisterIntegerTextBox(facilityLaboratoryLinesBox, "10");
            RegisterIntegerTextBox(facilityRefiningSkillBox, "5");
            RegisterIntegerTextBox(facilityReprocessingSkillBox, "5");
            RegisterIntegerTextBox(facilityOreProcessingSkillBox, "4");
            RegisterDecimalTextBox(facilityReprocessingImplantBox, "0", false);
            RegisterIntegerTextBox(facilityEncryptionSkillBox, "4");
            RegisterIntegerTextBox(facilityDatacoreSkill1Box, "4");
            RegisterIntegerTextBox(facilityDatacoreSkill2Box, "4");
            RegisterIntegerTextBox(facilityScienceSkillBox, "5");
            RegisterIntegerTextBox(projectCompletedRunsBox, "0");
            RegisterIntegerTextBox(projectOwnedMaterialBox, "0");
            RegisterIntegerTextBox(projectOwnedMaterialBoxOld, "0");
        }

        private void RegisterIntegerTextBox(TextBox textBox, string defaultText)
        {
            RegisterNumericTextBox(textBox, allowDecimal: false, allowNegative: false, defaultText: defaultText);
        }

        private void RegisterDecimalTextBox(TextBox textBox, string defaultText, bool allowNegative)
        {
            RegisterNumericTextBox(textBox, allowDecimal: true, allowNegative: allowNegative, defaultText: defaultText);
        }

        private void RegisterNumericTextBox(TextBox textBox, bool allowDecimal, bool allowNegative, string defaultText)
        {
            if (textBox == null)
            {
                return;
            }

            _numericInputRules[textBox] = new NumericInputRule
            {
                AllowDecimal = allowDecimal,
                AllowNegative = allowNegative,
                DefaultText = defaultText
            };
            textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            textBox.LostFocus += NumericTextBox_LostFocus;
            DataObject.AddPastingHandler(textBox, NumericTextBox_Pasting);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            NumericInputRule rule;
            if (textBox == null || !_numericInputRules.TryGetValue(textBox, out rule))
            {
                return;
            }

            var proposed = GetProposedText(textBox, e.Text);
            e.Handled = !_numericInputValidationService.IsValid(proposed, rule, allowPartial: true);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            NumericInputRule rule;
            if (textBox == null || !_numericInputRules.TryGetValue(textBox, out rule))
            {
                return;
            }

            var pastedText = e.DataObject.GetDataPresent(DataFormats.Text)
                ? e.DataObject.GetData(DataFormats.Text) as string
                : null;
            var proposed = GetProposedText(textBox, pastedText ?? "");
            if (!_numericInputValidationService.IsValid(proposed, rule, allowPartial: true))
            {
                e.CancelCommand();
            }
        }

        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            NumericInputRule rule;
            if (textBox == null || !_numericInputRules.TryGetValue(textBox, out rule))
            {
                return;
            }

            if (!_numericInputValidationService.IsValid(textBox.Text, rule, allowPartial: false))
            {
                textBox.Text = rule.DefaultText;
            }
        }

        private static string GetProposedText(TextBox textBox, string newText)
        {
            return textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, newText ?? "");
        }

        private void LoadUiSettings()
        {
            var settings = _uiSettingsStore.Load();
            alwaysBuyRamBox.IsChecked = settings.AlwaysBuyRam;
            alwaysBuyFuelBlocksBox.IsChecked = settings.AlwaysBuyFuelBlocks;
            SelectComboText(reactionDepthBox, settings.ReactionDepth);
            SelectComboText(materialPriceModeBox, settings.MaterialPriceMode);
            SelectComboText(productPriceModeBox, settings.ProductPriceMode);
            SelectDecryptor(settings.Decryptor);
            autoDecryptorBox.IsChecked = settings.AutoDecryptor;
            typeShipsBox.IsChecked = settings.TypeShips;
            typeAmmoBox.IsChecked = settings.TypeAmmo;
            typeModulesBox.IsChecked = settings.TypeModules;
            typeRigsBox.IsChecked = settings.TypeRigs;
            typeDronesBox.IsChecked = settings.TypeDrones;
            typeComponentsBox.IsChecked = settings.TypeComponents;
            typeStructuresBox.IsChecked = settings.TypeStructures;
            typeMiscBox.IsChecked = settings.TypeMisc;
            profitableOnlyBox.IsChecked = settings.ProfitableOnly;
            hideMissingPricesBox.IsChecked = settings.HideMissingPrices;
            hideLowVolumeBox.IsChecked = settings.HideLowVolume;
            hideLimitedBlueprintsBox.IsChecked = settings.HideLimitedBlueprints;
            hideRareNoiseBox.IsChecked = settings.HideRareNoise;
            allowT2Box.IsChecked = settings.AllowT2;
            allowT3Box.IsChecked = settings.AllowT3;
            allowCapitalBox.IsChecked = settings.AllowCapital;
            allowReactionsBox.IsChecked = settings.AllowReactions;
            showExcludedBlueprintsBox.IsChecked = settings.ShowExcludedBlueprints;
            hideBySkillsBox.IsChecked = settings.HideBySkills;
            topOnlyBox.IsChecked = settings.TopOnly;
            topBlueprintCountBox.Text = settings.TopBlueprintCount;
            minSvrBox.Text = settings.MinSvr;
            svrDaysBox.Text = settings.SvrDays;
            SelectComboText(priceTrendFilterBox, settings.PriceTrendFilter);
            minSoldBox.Text = settings.MinSold;
            minOrdersBox.Text = settings.MinOrders;
            useContractPricesBox.IsChecked = settings.UseContractPrices;
            minIskHourBox.Text = settings.MinIskHour;
            minRoiBox.Text = settings.MinRoi;
            materialPriceModifierBox.Text = settings.MaterialPriceModifier;
            productPriceModifierBox.Text = settings.ProductPriceModifier;
            var detailHeight = settings.BlueprintDetailHeight;
            blueprintDetailRow.Height = new GridLength(Math.Max(220, Math.Min(520, detailHeight)));
        }

        private void SaveUiSettings()
        {
            _uiSettingsStore.Save(CreateUiSettings());
        }

        private UiSettings CreateUiSettings()
        {
            return new UiSettings
            {
                AlwaysBuyRam = alwaysBuyRamBox.IsChecked == true,
                AlwaysBuyFuelBlocks = alwaysBuyFuelBlocksBox.IsChecked == true,
                ReactionDepth = GetSelectedPriceMode(reactionDepthBox),
                Decryptor = GetSelectedDecryptor().Name ?? "None",
                AutoDecryptor = autoDecryptorBox.IsChecked == true,
                TypeShips = typeShipsBox.IsChecked == true,
                TypeAmmo = typeAmmoBox.IsChecked == true,
                TypeModules = typeModulesBox.IsChecked == true,
                TypeRigs = typeRigsBox.IsChecked == true,
                TypeDrones = typeDronesBox.IsChecked == true,
                TypeComponents = typeComponentsBox.IsChecked == true,
                TypeStructures = typeStructuresBox.IsChecked == true,
                TypeMisc = typeMiscBox.IsChecked == true,
                ProfitableOnly = profitableOnlyBox.IsChecked == true,
                HideMissingPrices = hideMissingPricesBox.IsChecked == true,
                HideLowVolume = hideLowVolumeBox.IsChecked == true,
                HideLimitedBlueprints = hideLimitedBlueprintsBox.IsChecked == true,
                HideRareNoise = hideRareNoiseBox.IsChecked == true,
                AllowT2 = allowT2Box.IsChecked == true,
                AllowT3 = allowT3Box.IsChecked == true,
                AllowCapital = allowCapitalBox.IsChecked == true,
                AllowReactions = allowReactionsBox.IsChecked == true,
                ShowExcludedBlueprints = showExcludedBlueprintsBox.IsChecked == true,
                HideBySkills = hideBySkillsBox.IsChecked == true,
                TopOnly = topOnlyBox.IsChecked == true,
                TopBlueprintCount = topBlueprintCountBox.Text,
                MinSvr = minSvrBox.Text,
                SvrDays = svrDaysBox.Text,
                PriceTrendFilter = GetSelectedPriceMode(priceTrendFilterBox),
                MinSold = minSoldBox.Text,
                MinOrders = minOrdersBox.Text,
                UseContractPrices = useContractPricesBox.IsChecked == true,
                MinIskHour = minIskHourBox.Text,
                MinRoi = minRoiBox.Text,
                MaterialPriceMode = GetSelectedPriceMode(materialPriceModeBox),
                ProductPriceMode = GetSelectedPriceMode(productPriceModeBox),
                MaterialPriceModifier = materialPriceModifierBox.Text,
                ProductPriceModifier = productPriceModifierBox.Text,
                BlueprintDetailHeight = Math.Max(220, blueprintDetailRow.ActualHeight > 0 ? blueprintDetailRow.ActualHeight : blueprintDetailRow.Height.Value)
            };
        }

        private static void SelectComboText(ComboBox comboBox, string value)
        {
            foreach (var item in comboBox.Items.OfType<ComboBoxItem>())
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private void SelectDecryptor(string name)
        {
            if (blueprintDecryptorBox == null || _decryptors == null)
            {
                return;
            }

            blueprintDecryptorBox.SelectedItem = _decryptors.FirstOrDefault(item => item.Name == name)
                ?? _decryptors.FirstOrDefault();
        }

        private static IReadOnlyList<MarketRegion> BuildMarketRegions(IEnumerable<EveRegion> regions)
        {
            var result = new List<MarketRegion>
            {
                new MarketRegion { Name = "Jita 4-4", RegionId = 10000002, StationId = 60003760 },
                new MarketRegion { Name = "Amarr VIII", RegionId = 10000043, StationId = 60008494 },
                new MarketRegion { Name = "Dodixie", RegionId = 10000032, StationId = 60011866 },
                new MarketRegion { Name = "Rens", RegionId = 10000030, StationId = 60004588 },
                new MarketRegion { Name = "Hek", RegionId = 10000042, StationId = 60005686 }
            };

            result.AddRange(regions.Select(region => new MarketRegion
            {
                Name = region.Name,
                RegionId = region.RegionId
            }));

            return result
                .GroupBy(region => region.StationId.HasValue ? "S" + region.StationId.Value : "R" + region.RegionId)
                .Select(group => group.First())
                .ToList();
        }

        private void InitializeDatabaseStatus()
        {
            try
            {
                databaseStatus.Text = _database.IsDatabaseAvailable()
                    ? "Р‘Р°Р·Р° РїРѕРґРєР»СЋС‡РµРЅР°: EVEIPH DB.sqlite"
                    : "Р‘Р°Р·Р° РЅР°Р№РґРµРЅР°, РЅРѕ С‚Р°Р±Р»РёС†С‹ С‡РµСЂС‚РµР¶РµР№ РЅРµ РѕР±РЅР°СЂСѓР¶РµРЅС‹";
            }
            catch (Exception ex)
            {
                databaseStatus.Text = "Р‘Р°Р·Р° РЅРµ РїРѕРґРєР»СЋС‡РµРЅР°: " + ex.Message;
            }
        }

        private void SearchBlueprints_Click(object sender, RoutedEventArgs e)
        {
            SearchBlueprints(blueprintSearchBox.Text, 200);
        }

        private void LoadAllBlueprints_Click(object sender, RoutedEventArgs e)
        {
            SearchBlueprints("", 10000);
        }

        private void ClearBlueprints_Click(object sender, RoutedEventArgs e)
        {
            ClearBlueprintResults("РЎРїРёСЃРѕРє С‡РµСЂС‚РµР¶РµР№ РѕС‡РёС‰РµРЅ");
        }

        private async void RefreshTopSvr_Click(object sender, RoutedEventArgs e)
        {
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (region == null)
            {
                return;
            }

            var days = ReadInt(svrDaysBox == null ? "7" : svrDaysBox.Text, 7);
            var topCount = ReadInt(topBlueprintCountBox == null ? "10" : topBlueprintCountBox.Text, 10);
            var candidates = _blueprints
                .Where(item => item.Profit > 0 && item.ProducedQuantity > 0 && item.ProductionTimeSeconds > 0)
                .OrderBy(item => item.ProfitRank <= 0 ? int.MaxValue : item.ProfitRank)
                .ThenByDescending(item => item.Profit)
                .Take(Math.Max(1, topCount))
                .ToList();
            if (candidates.Count == 0)
            {
                return;
            }

            try
            {
                var stats = await Task.Run(() => _marketHistoryService.LoadStats(region.RegionId, candidates.Select(item => item.ProductTypeId), days));
                Dictionary<long, MarketOrderStats> orderStats = null;
                try
                {
                    orderStats = await Task.Run(() => _marketOrderService.LoadStats(region.RegionId, candidates.Select(item => item.ProductTypeId)));
                }
                catch
                {
                    orderStats = new Dictionary<long, MarketOrderStats>();
                }

                UpsertMarketHistoryCache(stats.Values);
                foreach (var blueprint in candidates)
                {
                    _salesVolumeRatioService.ApplyToBlueprint(blueprint, stats, blueprint.ProducedQuantity, blueprint.ProductionTimeSeconds);
                    ApplyMarketOrderStats(blueprint, orderStats);
                }

                SetBlueprintRanks();
                ApplyBlueprintFilters(true);
                blueprintsGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("РќРµ СѓРґР°Р»РѕСЃСЊ РѕР±РЅРѕРІРёС‚СЊ SVR С‡РµСЂРµР· ESI: " + ex.Message, "Our IPH", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static void ApplyMarketOrderStats(BlueprintSearchResult blueprint, Dictionary<long, MarketOrderStats> orderStats)
        {
            blueprint.CurrentBuyOrders = 0;
            blueprint.CurrentSellOrders = 0;
            if (blueprint == null || orderStats == null)
            {
                return;
            }

            MarketOrderStats stats;
            if (!orderStats.TryGetValue(blueprint.ProductTypeId, out stats))
            {
                return;
            }

            blueprint.CurrentBuyOrders = stats.BuyOrders;
            blueprint.CurrentSellOrders = stats.SellOrders;
            if (stats.BuyVolume > 0 || stats.SellVolume > 0)
            {
                blueprint.ProductMarketVolume = GetOrderVolumeForDisplay(stats);
            }
        }

        private static long GetOrderVolumeForDisplay(MarketOrderStats stats)
        {
            return stats.BuyVolume > 0 ? stats.BuyVolume : stats.SellVolume;
        }

        private void BlueprintSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBlueprints(blueprintSearchBox.Text, 200);
            }
        }

        private void SearchBlueprints(string text, int limit)
        {
            var searchText = (text ?? "").Trim();
            _activeBlueprintSearchText = searchText;
            _blueprints.Clear();
            _selectedBlueprintMaterials.Clear();
            _selectedBlueprintRawMaterials.Clear();
            _selectedBlueprintSkills.Clear();
            _selectedContractSampleReviews.Clear();

            var updateSkillStatus = limit <= 500 || (hideBySkillsBox != null && hideBySkillsBox.IsChecked == true);
            foreach (var blueprint in _database.SearchBlueprints(searchText, limit))
            {
                if (updateSkillStatus)
                {
                    UpdateBlueprintSkillStatus(blueprint);
                }

                _blueprints.Add(blueprint);
            }

            ApplyBlueprintFilters(false);
            var visibleBlueprints = GetVisibleBlueprints().ToList();
            var firstVisible = visibleBlueprints.FirstOrDefault();
            blueprintsGrid.SelectedItem = firstVisible;
            if (firstVisible != null)
            {
                blueprintsGrid.ScrollIntoView(firstVisible);
            }
            SetBlueprintListStatus(string.Format("Р—Р°РіСЂСѓР¶РµРЅРѕ С‡РµСЂС‚РµР¶РµР№: {0:N0}", _blueprints.Count));
            SetBlueprintListStatus(GetBlueprintSearchStatus(searchText, _blueprints.Count, visibleBlueprints.Count));
        }

        private void ClearBlueprintResults(string statusText)
        {
            _activeBlueprintSearchText = "";
            _blueprints.Clear();
            _selectedBlueprintMaterials.Clear();
            _selectedBlueprintRawMaterials.Clear();
            _selectedBlueprintSkills.Clear();
            _selectedContractSampleReviews.Clear();
            if (blueprintsGrid != null)
            {
                blueprintsGrid.SelectedIndex = -1;
                blueprintsGrid.Items.Refresh();
            }

            if (contractReviewStatus != null)
            {
                contractReviewStatus.Text = "";
            }

            SetBlueprintListStatus(statusText);
        }

        private void SetBlueprintListStatus(string statusText)
        {
            if (blueprintListStatus != null)
            {
                blueprintListStatus.Text = string.IsNullOrWhiteSpace(statusText)
                    ? string.Format("Р§РµСЂС‚РµР¶Рё: {0:N0}", _blueprints.Count)
                    : statusText;
            }
        }

        private void UpdateBlueprintSkillStatuses()
        {
            foreach (var blueprint in _blueprints)
            {
                UpdateBlueprintSkillStatus(blueprint);
            }
        }

        private void UpdateBlueprintSkillStatus(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return;
            }

            var skills = GetRequiredSkillsForBlueprint(blueprint).ToList();
            var missing = 0;
            var missingNames = new List<string>();
            foreach (var skill in skills)
            {
                ApplyOwnedSkillLevel(skill);
                if (skill.OwnedLevel < skill.Level)
                {
                    missing++;
                    if (missingNames.Count < 4)
                    {
                        missingNames.Add(skill.Name + " " + skill.Level);
                    }
                }
            }

            blueprint.RequiredSkillsCount = skills.Count;
            blueprint.MissingSkillsCount = missing;
            blueprint.RequiredSkillSummary = missing == 0
                ? "РќР°РІС‹РєРё OK"
                : "РќРµ С…РІР°С‚Р°РµС‚: " + string.Join(", ", missingNames) + (missing > missingNames.Count ? "..." : "");
        }

        private void BlueprintFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            ApplyBlueprintFilters(true);
        }

        private void BlueprintFilter_Changed(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            ApplyBlueprintFilters(true);
        }

        private void BlueprintFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            ApplyBlueprintFilters(true);
        }

        private void ResetBlueprintFilter_Click(object sender, RoutedEventArgs e)
        {
            profitableOnlyBox.IsChecked = false;
            hideMissingPricesBox.IsChecked = false;
            hideLowVolumeBox.IsChecked = true;
            hideLimitedBlueprintsBox.IsChecked = true;
            hideRareNoiseBox.IsChecked = true;
            typeShipsBox.IsChecked = true;
            typeAmmoBox.IsChecked = true;
            typeModulesBox.IsChecked = true;
            typeRigsBox.IsChecked = true;
            typeDronesBox.IsChecked = true;
            typeComponentsBox.IsChecked = true;
            typeStructuresBox.IsChecked = true;
            typeMiscBox.IsChecked = true;
            allowT2Box.IsChecked = true;
            allowT3Box.IsChecked = true;
            allowCapitalBox.IsChecked = true;
            allowReactionsBox.IsChecked = true;
            showExcludedBlueprintsBox.IsChecked = false;
            hideBySkillsBox.IsChecked = false;
            topOnlyBox.IsChecked = false;
            topBlueprintCountBox.Text = "10";
            minSvrBox.Text = "0";
            svrDaysBox.Text = "7";
            SelectComboText(priceTrendFilterBox, "Any");
            minSoldBox.Text = "0";
            minOrdersBox.Text = "0";
            minIskHourBox.Text = "0";
            minRoiBox.Text = "0";
            ApplyBlueprintFilters(true);
        }

        private void ApplyBlueprintFilters(bool sortByProfit)
        {
            if (hideBySkillsBox != null && hideBySkillsBox.IsChecked == true && _blueprints.Any(item => item.MissingSkillsCount < 0))
            {
                UpdateBlueprintSkillStatuses();
            }

            var view = CollectionViewSource.GetDefaultView(_blueprints);
            if (view == null)
            {
                return;
            }

            var options = CreateBlueprintListFilterOptions();
            view.Filter = item => _blueprintListFilterService.Passes(item as BlueprintSearchResult, options);
            view.SortDescriptions.Clear();
            if (sortByProfit)
            {
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProfitRankSort", System.ComponentModel.ListSortDirection.Ascending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("SvrTimesIskPerHour", System.ComponentModel.ListSortDirection.Descending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Profit", System.ComponentModel.ListSortDirection.Descending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("IskPerHour", System.ComponentModel.ListSortDirection.Descending));
            }

            view.Refresh();
            UpdateBlueprintListFilterStatus();
        }

        private BlueprintListFilterOptions CreateBlueprintListFilterOptions()
        {
            return new BlueprintListFilterOptions
            {
                ProfitableOnly = profitableOnlyBox != null && profitableOnlyBox.IsChecked == true,
                HideMissingPrices = hideMissingPricesBox != null && hideMissingPricesBox.IsChecked == true,
                HideLowVolume = hideLowVolumeBox != null && hideLowVolumeBox.IsChecked == true,
                ShowExcluded = showExcludedBlueprintsBox != null && showExcludedBlueprintsBox.IsChecked == true,
                HideBySkills = hideBySkillsBox != null && hideBySkillsBox.IsChecked == true,
                TopOnly = topOnlyBox != null && topOnlyBox.IsChecked == true,
                TopCount = ReadInt(topBlueprintCountBox == null ? "10" : topBlueprintCountBox.Text, 10),
                MinSalesVolumeRatio = ReadDouble(minSvrBox == null ? "0" : minSvrBox.Text, 0),
                TrendFilter = GetSelectedPriceMode(priceTrendFilterBox),
                MinSold = ReadLong(minSoldBox == null ? "0" : minSoldBox.Text, 0),
                MinOrders = ReadLong(minOrdersBox == null ? "0" : minOrdersBox.Text, 0),
                MinIskPerHour = ReadDouble(minIskHourBox == null ? "0" : minIskHourBox.Text, 0),
                MinRoi = ReadDouble(minRoiBox == null ? "0" : minRoiBox.Text, 0),
                ExcludedProductIds = _excludedBlueprintProductIds ?? new HashSet<long>(),
                MatchesExplicitSearch = BlueprintMatchesActiveSearch,
                AllowedByTypeFilter = BlueprintAllowedByTypeFilter,
                HasRequiredSkills = HasRequiredSkills,
                AllowedByBuildProfile = BlueprintAllowedByBuildProfile
            };
        }

        private IEnumerable<BlueprintSearchResult> GetVisibleBlueprints()
        {
            var view = CollectionViewSource.GetDefaultView(_blueprints);
            if (view == null)
            {
                return _blueprints;
            }

            return view.Cast<BlueprintSearchResult>();
        }

        private void UpdateBlueprintListFilterStatus()
        {
            if (blueprintListStatus == null)
            {
                return;
            }

            var visibleCount = GetVisibleBlueprints().Count();
            if (!string.IsNullOrWhiteSpace(_activeBlueprintSearchText))
            {
                SetBlueprintListStatus(GetBlueprintSearchStatus(_activeBlueprintSearchText, _blueprints.Count, visibleCount));
                return;
            }

            SetBlueprintListStatus(GetBlueprintFilterStatus(_blueprints.Count, visibleCount));
        }

        private static string GetBlueprintSearchStatus(string searchText, int loadedCount, int visibleCount)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (loadedCount == 0)
                {
                    return string.Format("РџРѕ Р·Р°РїСЂРѕСЃСѓ \"{0}\" РЅРёС‡РµРіРѕ РЅРµ РЅР°Р№РґРµРЅРѕ", searchText);
                }

                return string.Format("РќР°Р№РґРµРЅРѕ: {0:N0}; РїРѕРєР°Р·Р°РЅРѕ: {1:N0}", loadedCount, visibleCount);
            }

            return string.Format("Р—Р°РіСЂСѓР¶РµРЅРѕ С‡РµСЂС‚РµР¶РµР№: {0:N0}; РїРѕРєР°Р·Р°РЅРѕ: {1:N0}", loadedCount, visibleCount);
        }

        private string GetBlueprintFilterStatus(int loadedCount, int visibleCount)
        {
            var status = string.Format("Р—Р°РіСЂСѓР¶РµРЅРѕ: {0:N0}; РїРѕРєР°Р·Р°РЅРѕ: {1:N0}", loadedCount, visibleCount);
            if (topOnlyBox != null && topOnlyBox.IsChecked == true)
            {
                var topCount = ReadInt(topBlueprintCountBox == null ? "10" : topBlueprintCountBox.Text, 10);
                var rankedCount = _blueprints.Count(blueprint => blueprint.ProfitRank > 0);
                status += rankedCount > 0
                    ? string.Format("; Top {0} РїРѕ score РёР· {1:N0}", Math.Min(topCount, rankedCount), rankedCount)
                    : string.Format("; Top {0} РїРѕСЏРІРёС‚СЃСЏ РїРѕСЃР»Рµ РћС†РµРЅРёС‚СЊ", topCount);
            }

            return status;
        }

        private bool BlueprintMatchesActiveSearch(BlueprintSearchResult blueprint)
        {
            if (blueprint == null || string.IsNullOrWhiteSpace(_activeBlueprintSearchText))
            {
                return false;
            }

            long typeId;
            if (long.TryParse(_activeBlueprintSearchText, NumberStyles.Integer, CultureInfo.InvariantCulture, out typeId)
                && (blueprint.ProductTypeId == typeId || blueprint.BlueprintTypeId == typeId))
            {
                return true;
            }

            return TextContainsAny(blueprint.ProductName, _activeBlueprintSearchText)
                   || TextContainsAny(blueprint.BlueprintName, _activeBlueprintSearchText);
        }

        private bool BlueprintAllowedByTypeFilter(BlueprintSearchResult blueprint)
        {
            return _blueprintTypeFilterService.Passes(blueprint, CreateBlueprintTypeFilterOptions());
        }

        private BlueprintTypeFilterOptions CreateBlueprintTypeFilterOptions()
        {
            return new BlueprintTypeFilterOptions
            {
                Ships = typeShipsBox.IsChecked == true,
                AmmoCharges = typeAmmoBox.IsChecked == true,
                Modules = typeModulesBox.IsChecked == true,
                Rigs = typeRigsBox.IsChecked == true,
                Drones = typeDronesBox.IsChecked == true,
                Components = typeComponentsBox.IsChecked == true,
                Structures = typeStructuresBox.IsChecked == true,
                Misc = typeMiscBox.IsChecked == true
            };
        }

        private bool BlueprintAllowedByBuildProfile(BlueprintSearchResult blueprint)
        {
            return _blueprintBuildProfileService.Passes(blueprint, CreateBlueprintBuildProfileOptions());
        }

        private BlueprintBuildProfileOptions CreateBlueprintBuildProfileOptions()
        {
            return new BlueprintBuildProfileOptions
            {
                AllowTech2 = allowT2Box == null || allowT2Box.IsChecked == true,
                AllowTech3 = allowT3Box == null || allowT3Box.IsChecked == true,
                AllowCapital = allowCapitalBox == null || allowCapitalBox.IsChecked == true,
                AllowReactions = allowReactionsBox == null || allowReactionsBox.IsChecked == true,
                HideLimitedSource = hideLimitedBlueprintsBox != null && hideLimitedBlueprintsBox.IsChecked == true,
                HideRareOrNoise = hideRareNoiseBox != null && hideRareNoiseBox.IsChecked == true
            };
        }

        private static bool TextContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return needles.Any(needle => value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void BlueprintsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedBlueprintMaterials();
        }

        private void BlueprintInputs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            RefreshSelectedBlueprintMaterials();
        }

        private void BlueprintDecryptorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            RefreshSelectedBlueprintMaterials();
        }

        private void RefreshSelectedBlueprintMaterials()
        {
            _selectedBlueprintMaterials.Clear();
            _selectedBlueprintRawMaterials.Clear();
            _selectedBlueprintSkills.Clear();
            _selectedContractSampleReviews.Clear();
            var selected = blueprintsGrid.SelectedItem as BlueprintSearchResult;
            if (selected == null)
            {
                if (contractReviewStatus != null)
                {
                    contractReviewStatus.Text = "";
                }

                return;
            }

            var runs = ReadInt(runsBox.Text, 1);
            var me = ReadDouble(meBox.Text, 0);
            var te = ReadDouble(teBox.Text, 0);
            GetEffectiveBlueprintEfficiency(selected, me, te, out me, out te);
            var facilityPreset = blueprintFacilityBox.SelectedItem as FacilityPreset;
            var materialMultiplier = GetMaterialMultiplier(selected, facilityPreset, me);
            foreach (var material in _database.GetManufacturingMaterials(selected.BlueprintTypeId))
            {
                material.AdjustedQuantity = CalculateAdjustedQuantity(material.Quantity, runs, materialMultiplier);
                _selectedBlueprintMaterials.Add(material);
            }

            var rawMaterials = new Dictionary<long, MaterialRequirement>();
            AccumulateRawMaterials(selected, runs, materialMultiplier, facilityPreset, GetSelectedDecryptor(), new HashSet<long>(), rawMaterials);
            foreach (var material in rawMaterials.Values.OrderBy(item => item.Name))
            {
                _selectedBlueprintRawMaterials.Add(material);
            }

            foreach (var skill in GetRequiredSkillsForBlueprint(selected))
            {
                ApplyOwnedSkillLevel(skill);
                _selectedBlueprintSkills.Add(skill);
            }

            RefreshSelectedContractSamples(selected);
        }

        private void AccumulateRawMaterials(BlueprintSearchResult blueprint, int runs, double materialMultiplier, FacilityPreset facilityPreset,
            DecryptorOption decryptor, HashSet<long> path, Dictionary<long, MaterialRequirement> totals)
        {
            if (blueprint == null || runs <= 0 || path.Contains(blueprint.ProductTypeId))
            {
                return;
            }

            path.Add(blueprint.ProductTypeId);
            foreach (var material in _database.GetManufacturingMaterials(blueprint.BlueprintTypeId))
            {
                var adjustedQuantity = CalculateAdjustedQuantity(material.Quantity, runs, materialMultiplier);
                var childBlueprint = path.Contains(material.TypeId) ? null : _database.FindBlueprintByProduct(material.TypeId);
                if (childBlueprint != null && !ShouldAlwaysBuy(childBlueprint) && !ShouldStopReactionDrilldown(blueprint, childBlueprint))
                {
                    double childMe;
                    double childTe;
                    GetDefaultEfficiencyForChildBlueprint(childBlueprint, facilityPreset, decryptor, out childMe, out childTe);
                    var childRuns = (int)Math.Ceiling(adjustedQuantity / (double)Math.Max(1, childBlueprint.PortionSize));
                    AccumulateRawMaterials(childBlueprint, childRuns, GetMaterialMultiplier(childBlueprint, facilityPreset, childMe), facilityPreset, decryptor, path, totals);
                    continue;
                }

                MaterialRequirement existing;
                if (!totals.TryGetValue(material.TypeId, out existing))
                {
                    existing = new MaterialRequirement
                    {
                        TypeId = material.TypeId,
                        Name = material.Name,
                        Quantity = 0,
                        AdjustedQuantity = 0
                    };
                    totals[material.TypeId] = existing;
                }

                existing.Quantity += adjustedQuantity;
                existing.AdjustedQuantity += adjustedQuantity;
            }

            path.Remove(blueprint.ProductTypeId);
        }

        private void RefreshSelectedContractSamples(BlueprintSearchResult selected)
        {
            _selectedContractSampleReviews.Clear();
            if (selected == null)
            {
                if (contractReviewStatus != null)
                {
                    contractReviewStatus.Text = "";
                }

                return;
            }

            var review = _contractPricingService.ReviewSamples(selected, _contractPrices, 30);
            foreach (var item in review.Reviews)
            {
                _selectedContractSampleReviews.Add(item);
            }

            if (contractReviewStatus != null)
            {
                contractReviewStatus.Text = review.Detail ?? "";
            }
        }

        private void SetSelectedSkillLevel_Click(object sender, RoutedEventArgs e)
        {
            var skill = blueprintSkillsGrid.SelectedItem as SkillRequirement;
            if (skill == null)
            {
                return;
            }

            SetCharacterSkillLevel(skill.TypeId, ReadInt(skillOwnedLevelBox.Text, skill.Level));
        }

        private void SetVisibleSkillsLevelFive_Click(object sender, RoutedEventArgs e)
        {
            foreach (var skill in _selectedBlueprintSkills)
            {
                SetCharacterSkillLevel(skill.TypeId, 5, false);
            }

            _characterSkillStore.Save(_characterSkills);
            UpdateBlueprintSkillStatuses();
            RefreshSelectedBlueprintMaterials();
            ApplyBlueprintFilters(true);
            blueprintsGrid.Items.Refresh();
        }

        private void SetAllIndustrySkillsLevelFive_Click(object sender, RoutedEventArgs e)
        {
            foreach (var skill in _database.GetAllIndustryRequiredSkills())
            {
                SetCharacterSkillLevel(skill.TypeId, 5, false);
            }

            _characterSkillStore.Save(_characterSkills);
            UpdateBlueprintSkillStatuses();
            RefreshSelectedBlueprintMaterials();
            ApplyBlueprintFilters(true);
            blueprintsGrid.Items.Refresh();
        }

        private void AddSelectedContractPrice_Click(object sender, RoutedEventArgs e)
        {
            var selected = blueprintsGrid.SelectedItem as BlueprintSearchResult;
            if (selected == null)
            {
                return;
            }

            var price = ReadDouble(contractPriceBox.Text, 0);
            if (price <= 0)
            {
                return;
            }

            _contractPrices.Add(new ContractPriceSample
            {
                TypeId = selected.ProductTypeId,
                TypeName = selected.ProductName,
                Price = price,
                ObservedAt = DateTime.Now
            });
            _contractPriceStore.Save(_contractPrices);
            var contract = _contractPricingService.SelectContractPrice(selected, _contractPrices, 30, ReadDouble(productPriceModifierBox == null ? "0" : productPriceModifierBox.Text, 0));
            selected.ContractUnitPrice = contract.ContractUnitPrice;
            selected.ProductPriceDetails = contract.Detail;
            RefreshSelectedContractSamples(selected);
            blueprintsGrid.Items.Refresh();
        }

        private async void LoadSelectedEsiContracts_Click(object sender, RoutedEventArgs e)
        {
            var selected = blueprintsGrid.SelectedItem as BlueprintSearchResult;
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (selected == null || region == null)
            {
                return;
            }

            try
            {
                selected.ProductPriceDetails = "ESI contracts: loading public contracts...";
                blueprintsGrid.Items.Refresh();

                var samples = await Task.Run(() => _esiPublicContractService.LoadSamples(region.RegionId, new[] { selected }, 3, 80));
                var added = AddContractSamples(samples);

                if (added > 0)
                {
                    _contractPriceStore.Save(_contractPrices);
                    ApplyContractPricingToBlueprint(selected, string.Format(" (ESI imported {0})", added));
                    RefreshSelectedContractSamples(selected);
                }
                else
                {
                    selected.ProductPriceDetails = "ESI contracts: no exact public item_exchange sample";
                    RefreshSelectedContractSamples(selected);
                }
            }
            catch (Exception ex)
            {
                selected.ProductPriceDetails = "ESI contracts error: " + ex.Message;
                RefreshSelectedContractSamples(selected);
            }
            finally
            {
                blueprintsGrid.Items.Refresh();
            }
        }

        private async void LoadTopEsiContracts_Click(object sender, RoutedEventArgs e)
        {
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (region == null)
            {
                return;
            }

            var targets = blueprintsGrid.Items
                .OfType<BlueprintSearchResult>()
                .Where(IsBroadContractScanTarget)
                .GroupBy(item => item.ProductTypeId)
                .Select(group => group.First())
                .Take(25)
                .ToList();

            if (targets.Count == 0)
            {
                return;
            }

            try
            {
                foreach (var target in targets)
                {
                    target.ProductPriceDetails = "ESI contracts: scanning public region contracts...";
                }

                blueprintsGrid.Items.Refresh();

                var samples = await Task.Run(() => _esiPublicContractService.LoadSamples(region.RegionId, targets, 5, 200));
                var added = AddContractSamples(samples);
                if (added > 0)
                {
                    _contractPriceStore.Save(_contractPrices);
                    foreach (var target in targets)
                    {
                        var importedForTarget = samples.Count(sample => sample.TypeId == target.ProductTypeId);
                        if (importedForTarget > 0)
                        {
                            ApplyContractPricingToBlueprint(target, string.Format(" (ESI scan imported {0})", importedForTarget));
                        }
                    }
                }
                else
                {
                    foreach (var target in targets)
                    {
                        target.ProductPriceDetails = "ESI contracts: no exact public item_exchange sample in scan";
                    }
                }

                RefreshSelectedContractSamples(blueprintsGrid.SelectedItem as BlueprintSearchResult);
            }
            catch (Exception ex)
            {
                foreach (var target in targets)
                {
                    target.ProductPriceDetails = "ESI contracts scan error: " + ex.Message;
                }

                RefreshSelectedContractSamples(blueprintsGrid.SelectedItem as BlueprintSearchResult);
            }
            finally
            {
                blueprintsGrid.Items.Refresh();
            }
        }

        private int AddContractSamples(IEnumerable<ContractPriceSample> samples)
        {
            var added = 0;
            foreach (var sample in samples ?? Enumerable.Empty<ContractPriceSample>())
            {
                if (sample.ContractId > 0 && _contractPrices.Any(item => item.TypeId == sample.TypeId && item.ContractId == sample.ContractId))
                {
                    continue;
                }

                _contractPrices.Add(sample);
                added++;
            }

            return added;
        }

        private void ApplyContractPricingToBlueprint(BlueprintSearchResult blueprint, string detailSuffix)
        {
            if (blueprint == null)
            {
                return;
            }

            var contract = _contractPricingService.SelectContractPrice(
                blueprint,
                _contractPrices,
                30,
                ReadDouble(productPriceModifierBox == null ? "0" : productPriceModifierBox.Text, 0));
            blueprint.ContractUnitPrice = contract.ContractUnitPrice;
            blueprint.ProductPriceSource = contract.Source;
            blueprint.ProductPriceDetails = contract.Detail + (detailSuffix ?? "");
        }

        private bool IsBroadContractScanTarget(BlueprintSearchResult blueprint)
        {
            return blueprint != null
                   && blueprint.ProductTypeId > 0
                   && !string.IsNullOrWhiteSpace(blueprint.ProductName)
                   && (blueprint.ProfitRank > 0
                       || _blueprintFilteringService.IsCapitalBlueprint(blueprint)
                       || _blueprintFilteringService.IsReactionBlueprint(blueprint));
        }

        private void SetCharacterSkillLevel(long typeId, int level, bool save = true)
        {
            if (typeId <= 0)
            {
                return;
            }

            _characterSkills[typeId] = Math.Max(0, Math.Min(5, level));
            if (save)
            {
                _characterSkillStore.Save(_characterSkills);
                UpdateBlueprintSkillStatuses();
                RefreshSelectedBlueprintMaterials();
                ApplyBlueprintFilters(true);
                blueprintsGrid.Items.Refresh();
            }
        }

        private void ApplyOwnedSkillLevel(SkillRequirement skill)
        {
            if (skill == null)
            {
                return;
            }

            int ownedLevel;
            skill.OwnedLevel = _characterSkills != null && _characterSkills.TryGetValue(skill.TypeId, out ownedLevel)
                ? ownedLevel
                : -1;
        }

        private bool HasRequiredSkills(BlueprintSearchResult blueprint)
        {
            foreach (var skill in GetRequiredSkillsForBlueprint(blueprint))
            {
                int ownedLevel;
                if (_characterSkills == null || !_characterSkills.TryGetValue(skill.TypeId, out ownedLevel) || ownedLevel < skill.Level)
                {
                    return false;
                }
            }

            return true;
        }

        private IReadOnlyList<SkillRequirement> GetRequiredSkillsForBlueprint(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return new List<SkillRequirement>();
            }

            return _blueprintSkillRequirementService.BuildRequiredSkills(
                blueprint,
                GetOwnRequiredSkillsForBlueprint,
                _database.GetManufacturingMaterials,
                _database.FindBlueprintByProduct,
                ShouldAlwaysBuy,
                ShouldStopReactionDrilldown);
        }

        private IReadOnlyList<SkillRequirement> GetOwnRequiredSkillsForBlueprint(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return new List<SkillRequirement>();
            }

            IReadOnlyList<SkillRequirement> cached;
            if (_requiredSkillCache.TryGetValue(blueprint.BlueprintTypeId, out cached))
            {
                return cached.Select(CloneSkillRequirement).ToList();
            }

            var skillsByType = new Dictionary<long, SkillRequirement>();
            foreach (var skill in _database.GetRequiredSkills(blueprint.BlueprintTypeId, 1))
            {
                AddRequiredSkill(skillsByType, skill);
            }

            var invention = ShouldUseInventionCosts(blueprint) ? _database.GetInventionInfo(blueprint.BlueprintTypeId) : null;
            if (invention != null && invention.SourceBlueprintTypeId > 0)
            {
                foreach (var skill in _database.GetRequiredSkills(invention.SourceBlueprintTypeId, 8))
                {
                    AddRequiredSkill(skillsByType, skill);
                }
            }

            cached = skillsByType.Values.OrderBy(skill => skill.Name).ToList();
            _requiredSkillCache[blueprint.BlueprintTypeId] = cached;
            return cached.Select(CloneSkillRequirement).ToList();
        }

        private static void AddRequiredSkill(Dictionary<long, SkillRequirement> skillsByType, SkillRequirement skill)
        {
            if (skill == null || skill.TypeId <= 0)
            {
                return;
            }

            SkillRequirement existing;
            if (!skillsByType.TryGetValue(skill.TypeId, out existing) || skill.Level > existing.Level)
            {
                skillsByType[skill.TypeId] = CloneSkillRequirement(skill);
            }
        }

        private static SkillRequirement CloneSkillRequirement(SkillRequirement skill)
        {
            return new SkillRequirement
            {
                TypeId = skill.TypeId,
                Name = skill.Name,
                Level = skill.Level,
                OwnedLevel = skill.OwnedLevel
            };
        }

        private void AddBlueprintToQueue_Click(object sender, RoutedEventArgs e)
        {
            var selected = blueprintsGrid.SelectedItem as BlueprintSearchResult;
            if (selected == null)
            {
                return;
            }

            AddOrMergeQueueItem(CreateQueueItem(selected));
            navBuildQueue.IsChecked = true;
        }

        private void ExcludeSelectedBlueprint_Click(object sender, RoutedEventArgs e)
        {
            var selected = blueprintsGrid.SelectedItem as BlueprintSearchResult;
            if (selected == null || selected.ProductTypeId <= 0)
            {
                return;
            }

            _excludedBlueprintProductIds.Add(selected.ProductTypeId);
            _excludedBlueprintStore.Save(_excludedBlueprintProductIds);
            ApplyBlueprintFilters(true);
        }

        private void ResetExcludedBlueprints_Click(object sender, RoutedEventArgs e)
        {
            _excludedBlueprintProductIds.Clear();
            _excludedBlueprintStore.Save(_excludedBlueprintProductIds);
            ApplyBlueprintFilters(true);
        }

        private void AddVisibleProfitableToQueue_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(_blueprints);
            if (view == null)
            {
                return;
            }

            var maxToAdd = Math.Max(1, ReadInt(topBlueprintCountBox == null ? "10" : topBlueprintCountBox.Text, 10));
            var candidates = _buildQueueService.SelectProfitableCandidates(
                view.Cast<object>().OfType<BlueprintSearchResult>(),
                maxToAdd);
            foreach (var blueprint in candidates)
            {
                AddOrMergeQueueItem(CreateQueueItem(blueprint));
            }

            if (candidates.Count > 0)
            {
                navBuildQueue.IsChecked = true;
            }
        }

        private void AddOrMergeQueueItem(BuildQueueItem item)
        {
            if (item == null)
            {
                return;
            }

            var added = _buildQueueService.AddOrMerge(_buildQueue, item);
            if (!added)
            {
                queueGrid.Items.Refresh();
            }

            SaveBuildQueue();
            UpdateQueueStatus();
        }

        private void SaveBuildQueue()
        {
            _buildQueueStore.Save(_buildQueue);
        }

        private void UpdateQueueStatus()
        {
            if (queueStatusText != null)
            {
                queueStatusText.Text = _buildQueueService.FormatStatus(_buildQueue.Count);
            }
        }

        private BuildQueueItem CreateQueueItem(BlueprintSearchResult blueprint)
        {
            var decryptor = ShouldUseInventionCosts(blueprint) ? GetEstimatedDecryptor(blueprint) : GetNoDecryptor();
            return new BuildQueueItem
            {
                Blueprint = blueprint,
                Runs = ReadInt(runsBox.Text, 1),
                MaterialEfficiency = GetEffectiveMaterialEfficiency(blueprint, ReadDouble(meBox.Text, 0), ReadDouble(teBox.Text, 0), decryptor),
                TimeEfficiency = GetEffectiveTimeEfficiency(blueprint, ReadDouble(meBox.Text, 0), ReadDouble(teBox.Text, 0), decryptor),
                DecryptorTypeId = decryptor.TypeId,
                DecryptorName = decryptor.TypeId > 0 ? decryptor.Name : ""
            };
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            _buildQueue.Clear();
            SaveBuildQueue();
            UpdateQueueStatus();
            SetBlueprintListStatus("РћС‡РµСЂРµРґСЊ СЃС‚СЂРѕР№РєРё РѕС‡РёС‰РµРЅР°");
        }

        private void RemoveQueueItem_Click(object sender, RoutedEventArgs e)
        {
            var selected = queueGrid.SelectedItem as BuildQueueItem;
            if (selected == null)
            {
                return;
            }

            _buildQueue.Remove(selected);
            SaveBuildQueue();
            UpdateQueueStatus();
            SetBlueprintListStatus("РЈРґР°Р»РµРЅРѕ РёР· РѕС‡РµСЂРµРґРё: " + selected.ProductName);
        }

        private void CreateProjectFromQueue_Click(object sender, RoutedEventArgs e)
        {
            if (_buildQueue.Count == 0)
            {
                return;
            }

            var project = new BuildProject
            {
                Name = "РџСЂРѕРµРєС‚ " + DateTime.Now.ToString("yyyy-MM-dd HH-mm")
            };

            var projectItemsByProduct = new Dictionary<string, BuildProjectItem>();
            var facilityPreset = blueprintFacilityBox.SelectedItem as FacilityPreset;
            var priceTypeIds = new HashSet<long>();
            foreach (var queueItem in _buildQueue)
            {
                CollectBlueprintTreeTypeIds(queueItem.Blueprint, priceTypeIds, GetDecryptorByTypeId(queueItem.DecryptorTypeId), new HashSet<long>());
            }

            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            var prices = new Dictionary<long, MarketPrice>();
            var adjustedPrices = _database.GetAdjustedPrices(priceTypeIds);
            if (region != null && priceTypeIds.Count > 0)
            {
                var id = region.StationId ?? region.RegionId;
                prices = LoadPricesWithCache(id, region.Name, priceTypeIds, region.StationId.HasValue);
            }

            foreach (var queueItem in _buildQueue)
            {
                AddBlueprintTreeToProject(project, projectItemsByProduct, queueItem.Blueprint, queueItem.Runs,
                    Math.Max(1, queueItem.Runs * queueItem.PortionSize), queueItem.MaterialEfficiency, queueItem.TimeEfficiency,
                    GetDecryptorByTypeId(queueItem.DecryptorTypeId), facilityPreset, prices, adjustedPrices, new HashSet<long>(), true);
            }

            ApplyProjectItemEstimates(project, facilityPreset, prices, adjustedPrices);

            _projects.Add(project);
            _projectStore.Save(_projects);
            projectsList.Items.Refresh();
            projectsList.SelectedItem = project;
            _buildQueue.Clear();
            SaveBuildQueue();
            UpdateQueueStatus();
            navProjects.IsChecked = true;
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null)
            {
                projectItemsGrid.ItemsSource = null;
                projectStatus.Text = "";
                return;
            }

            RefreshProjectItemEstimates(project);
            var view = CollectionViewSource.GetDefaultView(project.Items);
            view.GroupDescriptions.Clear();
            view.SortDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("WaveText"));
            view.GroupDescriptions.Add(new PropertyGroupDescription("ProductionType"));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Wave", System.ComponentModel.ListSortDirection.Ascending));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProductionType", System.ComponentModel.ListSortDirection.Ascending));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProductName", System.ComponentModel.ListSortDirection.Ascending));
            projectItemsGrid.ItemsSource = view;
            projectStatus.Text = project.SummaryText;
            RefreshProjectMaterials(project);
        }

        private void RefreshProjectItemEstimates(BuildProject project)
        {
            if (project == null || project.Items.Count == 0)
            {
                return;
            }

            var facilityPreset = blueprintFacilityBox.SelectedItem as FacilityPreset;
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (facilityPreset == null || region == null)
            {
                return;
            }

            var typeIds = new HashSet<long>();
            foreach (var item in project.Items)
            {
                AddBlueprintAndMaterialTypeIds(typeIds, item.BlueprintTypeId, item.ProductTypeId, item.DecryptorTypeId);
            }

            var id = region.StationId ?? region.RegionId;
            var prices = GetCachedPrices(id, typeIds);
            if (prices.Count == 0)
            {
                return;
            }

            var adjustedPrices = _database.GetAdjustedPrices(typeIds);
            ApplyProjectItemEstimates(project, facilityPreset, prices, adjustedPrices);
        }

        private void ApplyProjectItemEstimates(BuildProject project, FacilityPreset facilityPreset,
            Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices)
        {
            if (project == null || facilityPreset == null)
            {
                return;
            }

            var plannedProducts = project.Items
                .Where(projectItem => !string.Equals(GetProjectBuildBuyMode(project, projectItem.ProductTypeId), "Buy", StringComparison.OrdinalIgnoreCase))
                .Select(projectItem => projectItem.ProductTypeId)
                .ToHashSet();
            var svrDays = ReadInt(svrDaysBox == null ? "7" : svrDaysBox.Text, 7);
            var region = blueprintRegionBox == null ? null : blueprintRegionBox.SelectedItem as MarketRegion;
            var marketHistory = region == null
                ? new Dictionary<long, MarketHistoryStats>()
                : GetMarketHistoryStats(project.Items.Select(projectItem => projectItem.ProductTypeId), region.RegionId, svrDays);
            foreach (var item in project.Items)
            {
                var blueprint = _database.FindBlueprintByProduct(item.ProductTypeId);
                if (blueprint == null)
                {
                    item.EstimateStatus = "РќРµС‚ С‡РµСЂС‚РµР¶Р°";
                    continue;
                }

                var materials = _database.GetManufacturingMaterials(blueprint.BlueprintTypeId);
                var estimateCache = new Dictionary<string, BlueprintEstimate>();
                var decryptor = GetDecryptorByTypeId(item.DecryptorTypeId);
                if (blueprint.IsCopyOnlyBlueprint)
                {
                    item.MaterialEfficiency = 0;
                    item.TimeEfficiency = 0;
                }
                var station = GetBestStationForBlueprint(facilityPreset, item.ProductionType, blueprint, materials, prices, adjustedPrices,
                    item.Runs, item.MaterialEfficiency, item.TimeEfficiency, decryptor, estimateCache);
                if (station == null || !station.SupportsProduction)
                {
                    _projectItemEstimateDisplayService.ResetUnavailable(item, station?.ValidationMessage ?? "РќРµС‚ СЃС‚Р°РЅС†РёРё");
                    continue;
                }

                var estimate = CalculateProjectItemEstimate(blueprint, materials, station, facilityPreset, prices, adjustedPrices,
                    item.Runs, item.MaterialEfficiency, plannedProducts, decryptor);
                MarketPrice productPrice;
                prices.TryGetValue(item.ProductTypeId, out productPrice);
                var productPriceResult = GetEffectiveProductPrice(blueprint, productPrice);
                _projectItemEstimateDisplayService.Apply(item, blueprint, estimate, productPriceResult,
                    GetProductMarketVolume(productPrice), facilityPreset, station, prices.Count > 0);
                _salesVolumeRatioService.ApplyToProjectItem(item, marketHistory);
            }
        }

        private BlueprintEstimate CalculateProjectItemEstimate(BlueprintSearchResult blueprint, IReadOnlyList<MaterialRequirement> materials,
            FacilityStation station, FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices,
            int runs, double me, HashSet<long> plannedProducts, DecryptorOption decryptor)
        {
            var stationMath = _facilityCatalog.Calculate(station, GetSelectedRigIds(station), blueprint.GroupId, blueprint.CategoryId);
            var industrySkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.IndustrySkillLevel);
            var advancedIndustrySkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.AdvancedIndustrySkillLevel);
            var materialMultiplier = _blueprintManufacturingMathService.CalculateMaterialMultiplier(me, stationMath.MaterialMultiplier);
            var timeMultiplier = _blueprintManufacturingMathService.CalculateTimeMultiplier(0, stationMath.TimeMultiplier,
                facilityPreset == null ? 0 : facilityPreset.ManufacturingImplantPercent,
                industrySkill,
                advancedIndustrySkill,
                GetAdvancedManufacturingSkillMultiplier(blueprint.BlueprintTypeId));
            var sccIndustryFeeRate = Math.Max(0, (facilityPreset == null ? 4.0 : facilityPreset.SccIndustryFeePercent) / 100.0);
            var alphaAccountTaxRate = Math.Max(0, (facilityPreset == null ? 0 : facilityPreset.AlphaAccountTaxPercent) / 100.0);
            var materialCost = 0.0;
            var estimatedInputValue = 0.0;
            var buildLines = 0;
            var buyLines = 0;
            var mineralBuyQuantities = new Dictionary<long, long>();

            foreach (var material in materials)
            {
                var adjustedQuantity = CalculateAdjustedQuantity(material.Quantity, runs, materialMultiplier);
                double adjustedPrice;
                if (adjustedPrices.TryGetValue(material.TypeId, out adjustedPrice) && adjustedPrice > 0)
                {
                    estimatedInputValue += material.Quantity * runs * adjustedPrice;
                }

                if (plannedProducts.Contains(material.TypeId))
                {
                    buildLines++;
                    continue;
                }

                if (_database.IsMineral(material.TypeId))
                {
                    AddMineralBuyQuantity(mineralBuyQuantities, material.TypeId, adjustedQuantity);
                    buyLines++;
                    continue;
                }

                MarketPrice price;
                var unitPrice = prices.TryGetValue(material.TypeId, out price) ? GetMaterialUnitPrice(price) : 0;
                if (unitPrice > 0)
                {
                    materialCost += adjustedQuantity * unitPrice;
                }
                buyLines++;
            }

            materialCost += GetMineralPurchaseCost(mineralBuyQuantities, facilityPreset, prices);

            bool inventionMissing;
            double inventionTime;
            InventionPlan inventionPlan;
            var inventionCost = CalculateInventionCost(blueprint, prices, runs, facilityPreset, station, estimatedInputValue, decryptor,
                out inventionTime, out inventionMissing, out inventionPlan);

            var costIndex = _facilityCatalog.GetSystemCostIndex(station.SolarSystemId, station.ProductionType);
            var taxRate = station.IndustryTaxPercent / 100.0;
            var factionWarfareMultiplier = _blueprintProductionTypeService.GetFactionWarfareCostMultiplier(station.FactionWarfareUpgradeLevel);
            return new BlueprintEstimate
            {
                MaterialCost = materialCost,
                InstallationCost = _blueprintManufacturingMathService.CalculateInstallationCost(estimatedInputValue, costIndex,
                    stationMath.CostMultiplier, factionWarfareMultiplier, taxRate, sccIndustryFeeRate, alphaAccountTaxRate),
                FacilityMaterialBonusPercent = _blueprintManufacturingMathService.CalculateFacilityMaterialBonusPercent(stationMath.MaterialMultiplier),
                CostIndexPercent = costIndex * 100,
                ProductionTimeSeconds = _blueprintProductionTypeService.CalculateBlueprintProductionTime(blueprint.BaseProductionTime, runs, timeMultiplier, facilityPreset) + inventionTime,
                InventionCost = inventionCost,
                InventionMissing = inventionMissing,
                InventionChancePercent = inventionPlan.Chance * 100.0,
                InventionJobs = inventionPlan.Jobs,
                InventedRunsPerSuccess = inventionPlan.RunsPerSuccess,
                InventionMaterialsCost = inventionPlan.SourceCost + inventionPlan.MaterialCost + inventionPlan.DecryptorCost,
                InventionCopyCost = inventionPlan.CopyMaterialCost,
                InventionJobUsageCost = inventionPlan.InventionUsageCost + inventionPlan.CopyUsageCost,
                InventionStationName = inventionPlan.InventionStationName,
                InventionStationSystem = inventionPlan.InventionStationSystem,
                CopyStationName = inventionPlan.CopyStationName,
                CopyStationSystem = inventionPlan.CopyStationSystem,
                BuildMaterialLines = buildLines,
                BuyMaterialLines = buyLines
            };
        }

        private int AddBlueprintTreeToProject(BuildProject project, Dictionary<string, BuildProjectItem> itemsByProduct,
            BlueprintSearchResult blueprint, int runs, long requiredQuantity, double me, double te, DecryptorOption decryptor, FacilityPreset facilityPreset,
            Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices, HashSet<long> path, bool isRootItem = false)
        {
            if (blueprint == null || runs <= 0)
            {
                return 0;
            }

            if (path.Contains(blueprint.ProductTypeId))
            {
                return 0;
            }

            path.Add(blueprint.ProductTypeId);
            var itemKey = GetProjectItemKey(blueprint.ProductTypeId, me, te, decryptor);
            BuildProjectItem existingItem;
            var portionSize = Math.Max(1, blueprint.PortionSize);
            var plannedRuns = Math.Max(runs, (int)Math.Ceiling(Math.Max(1, requiredQuantity) / (double)portionSize));
            var plannedRequiredQuantity = (int)Math.Max(1, requiredQuantity);
            if (itemsByProduct.TryGetValue(itemKey, out existingItem))
            {
                var newRequiredQuantity = existingItem.RequiredQuantity + plannedRequiredQuantity;
                var newRuns = Math.Max(existingItem.Runs, (int)Math.Ceiling(newRequiredQuantity / (double)portionSize));
                var extraRuns = newRuns - existingItem.Runs;
                existingItem.RequiredQuantity = newRequiredQuantity;
                existingItem.Runs = newRuns;
                existingItem.IsRootItem = existingItem.IsRootItem || isRootItem;
                if (extraRuns <= 0)
                {
                    path.Remove(blueprint.ProductTypeId);
                    return existingItem.Wave;
                }

                runs = extraRuns;
            }
            else
            {
                runs = plannedRuns;
            }

            var maxChildWave = 0;
            foreach (var material in _database.GetManufacturingMaterials(blueprint.BlueprintTypeId))
            {
                var childBlueprint = _database.FindBlueprintByProduct(material.TypeId);
                if (childBlueprint == null)
                {
                    continue;
                }

                if (ShouldStopReactionDrilldown(blueprint, childBlueprint))
                {
                    continue;
                }

                var neededQuantity = CalculateAdjustedQuantity(material.Quantity, runs, GetMaterialMultiplier(blueprint, facilityPreset, me));
                var childRuns = (int)Math.Ceiling(neededQuantity / (double)Math.Max(1, childBlueprint.PortionSize));
                double childMe;
                double childTe;
                GetDefaultEfficiencyForChildBlueprint(childBlueprint, facilityPreset, decryptor, out childMe, out childTe);
                if (ShouldBuildChildMaterial(childBlueprint, childRuns, neededQuantity, facilityPreset, prices, adjustedPrices, childMe, childTe, decryptor, path,
                    GetProjectBuildBuyMode(project, childBlueprint.ProductTypeId)))
                {
                    maxChildWave = Math.Max(maxChildWave, AddBlueprintTreeToProject(project, itemsByProduct, childBlueprint, childRuns, neededQuantity,
                        childMe, childTe, decryptor, facilityPreset, prices, adjustedPrices, path));
                }
            }

            path.Remove(blueprint.ProductTypeId);
            var wave = maxChildWave + 1;
            BuildProjectItem item;
            if (!itemsByProduct.TryGetValue(itemKey, out item))
            {
                item = new BuildProjectItem
                {
                    BlueprintTypeId = blueprint.BlueprintTypeId,
                    ProductTypeId = blueprint.ProductTypeId,
                    BlueprintName = blueprint.BlueprintName,
                    ProductName = blueprint.ProductName,
                    GroupName = blueprint.GroupName,
                    ProductionType = GetProductionTypeForBlueprint(blueprint),
                    IsRootItem = isRootItem,
                    Wave = wave,
                    Runs = runs,
                    PortionSize = portionSize,
                    RequiredQuantity = plannedRequiredQuantity,
                    MaterialEfficiency = me,
                    TimeEfficiency = te,
                    DecryptorTypeId = decryptor == null ? 0 : decryptor.TypeId,
                    DecryptorName = decryptor != null && decryptor.TypeId > 0 ? decryptor.Name : ""
                };
                itemsByProduct[itemKey] = item;
                project.Items.Add(item);
            }
            else
            {
                item.Wave = Math.Max(item.Wave, wave);
                item.IsRootItem = item.IsRootItem || isRootItem;
                item.MaterialEfficiency = me;
                item.TimeEfficiency = te;
            }

            return item.Wave;
        }

        private static string GetProjectItemKey(long productTypeId, double me, double te, DecryptorOption decryptor)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1:0.####}:{2:0.####}:{3}", productTypeId, me, te, decryptor == null ? 0 : decryptor.TypeId);
        }

        private bool ShouldBuildChildMaterial(BlueprintSearchResult childBlueprint, int childRuns, long neededQuantity,
            FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices,
            double me, double te, DecryptorOption decryptor, HashSet<long> path, string manualMode)
        {
            if (string.Equals(manualMode, "Buy", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ShouldAlwaysBuy(childBlueprint))
            {
                return false;
            }

            if (string.Equals(manualMode, "Build", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (facilityPreset == null)
            {
                return true;
            }

            if (!facilityPreset.SuggestBuildBlueprintsNotOwned && !CanBuildWithOwnedOrInventedBlueprint(childBlueprint))
            {
                return false;
            }

            MarketPrice buyPrice;
            var buyUnitPrice = prices.TryGetValue(childBlueprint.ProductTypeId, out buyPrice) ? GetMaterialUnitPrice(buyPrice) : 0;
            var buyCost = buyUnitPrice > 0
                ? neededQuantity * buyUnitPrice
                : double.MaxValue;
            var marketVolumeShort = facilityPreset.BuildWhenMarketVolumeShort
                && buyPrice != null
                && GetAvailableMarketVolume(buyPrice) > 0
                && GetAvailableMarketVolume(buyPrice) < neededQuantity;

            var estimateCache = new Dictionary<string, BlueprintEstimate>();
            var station = GetCheapestStationForBlueprint(facilityPreset, childBlueprint, prices, adjustedPrices, childRuns, me, te, decryptor, path, estimateCache);
            if (station == null)
            {
                return false;
            }

            var estimate = CalculateBlueprintEstimate(childBlueprint, _database.GetManufacturingMaterials(childBlueprint.BlueprintTypeId),
                station, facilityPreset, prices, adjustedPrices, childRuns, me, te, decryptor, new HashSet<long>(path), estimateCache);
            var buildCost = estimate.MaterialCost + estimate.InventionCost + estimate.InstallationCost;
            var producedQuantity = childRuns * Math.Max(1, childBlueprint.PortionSize);
            var surplusQuantity = Math.Max(0, producedQuantity - neededQuantity);
            if (surplusQuantity > 0)
            {
                MarketPrice surplusPrice;
                var surplusUnitPrice = prices.TryGetValue(childBlueprint.ProductTypeId, out surplusPrice)
                    ? GetProductUnitPrice(surplusPrice)
                    : 0;
                if (surplusUnitPrice > 0)
                {
                    buildCost = Math.Max(0, buildCost - _salesFeeService.ApplySalesTaxesAndFees(surplusQuantity * surplusUnitPrice, facilityPreset, station));
                }
            }

            return marketVolumeShort || buildCost < buyCost || buyCost == double.MaxValue;
        }

        private bool CanBuildWithOwnedOrInventedBlueprint(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            int ownedMe;
            int ownedTe;
            if (_database.TryGetOwnedBlueprintEfficiency(blueprint.BlueprintTypeId, out ownedMe, out ownedTe))
            {
                return true;
            }

            return ShouldUseInventionCosts(blueprint) && _database.GetInventionInfo(blueprint.BlueprintTypeId) != null;
        }

        private bool ShouldAlwaysBuy(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            if (blueprint.GroupId == 332 && alwaysBuyRamBox.IsChecked == true)
            {
                return true;
            }

            if (blueprint.GroupId == 1136 && alwaysBuyFuelBlocksBox.IsChecked == true)
            {
                return true;
            }

            return false;
        }

        private void CompleteProjectItem_Click(object sender, RoutedEventArgs e)
        {
            var item = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (item == null)
            {
                return;
            }

            item.CompletedRuns = item.Runs;
            SaveAndRefreshProject();
        }

        private void ResetProjectItem_Click(object sender, RoutedEventArgs e)
        {
            var item = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (item == null)
            {
                return;
            }

            item.CompletedRuns = 0;
            SaveAndRefreshProject();
        }

        private void SetProjectItemRuns_Click(object sender, RoutedEventArgs e)
        {
            var item = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (item == null)
            {
                return;
            }

            var completedRuns = ReadNonNegativeInt(projectCompletedRunsBox.Text, item.CompletedRuns);
            item.CompletedRuns = Math.Max(0, Math.Min(item.Runs, completedRuns));
            SaveAndRefreshProject();
        }

        private void SetProjectMaterialOwned_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var material = projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            if (project == null || material == null)
            {
                return;
            }

            var ownedQuantity = ReadLong(projectOwnedMaterialBox.Text, material.OwnedQuantity);
            SetProjectStockOwnedQuantity(project, material.TypeId, ownedQuantity);
            SaveAndRefreshProject();
        }

        private void ProjectMaterialsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            var material = e.Row == null ? null : e.Row.Item as BuildProjectMaterial;
            var textBox = e.EditingElement as TextBox;
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null || material == null || textBox == null || e.Column == null || e.Column.Header == null || e.Column.Header.ToString() != "РљСѓРїР»РµРЅРѕ")
            {
                return;
            }

            var lineOwnedQuantity = ReadLong(textBox.Text, material.OwnedQuantity);
            var totalOwnedThroughLine = ProjectStockService.GetTotalOwnedThroughLine(material, lineOwnedQuantity);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SetProjectStockOwnedQuantity(project, material.TypeId, totalOwnedThroughLine);
                SaveAndRefreshProject();
                projectStatus.Text = string.Format("РћСЃС‚Р°С‚РѕРє РѕР±РЅРѕРІР»РµРЅ РёР· С‚Р°Р±Р»РёС†С‹: {0} = {1:N0}", material.Name, totalOwnedThroughLine);
            }));
        }

        private void ProjectMaterialsGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column == null || e.Column.Header == null || e.Column.Header.ToString() != "РљСѓРїР»РµРЅРѕ")
            {
                e.Cancel = true;
            }
        }

        private static void SetProjectStockOwnedQuantity(BuildProject project, long typeId, long ownedQuantity)
        {
            var stock = project.Stock.FirstOrDefault(item => item.TypeId == typeId);
            if (stock == null)
            {
                stock = new BuildProjectStock { TypeId = typeId };
                project.Stock.Add(stock);
            }

            stock.OwnedQuantity = Math.Max(0, ownedQuantity);
        }

        private void ImportProjectBoughtFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null || _projectMaterials.Count == 0 || !Clipboard.ContainsText())
            {
                return;
            }

            var imported = _projectMaterialImportService.ImportBoughtQuantities(Clipboard.GetText(), _projectMaterials);

            foreach (var item in imported)
            {
                SetProjectStockOwnedQuantity(project, item.Key, item.Value);
            }

            SaveAndRefreshProject();
            projectStatus.Text = imported.Count == 0
                ? "Р’ Р±СѓС„РµСЂРµ РЅРµ РЅР°Р№РґРµРЅРѕ РјР°С‚РµСЂРёР°Р»РѕРІ С‚РµРєСѓС‰РµРіРѕ РїСЂРѕРµРєС‚Р°"
                : "РРјРїРѕСЂС‚РёСЂРѕРІР°РЅРѕ РєСѓРїР»РµРЅРЅРѕРµ: " + imported.Count + " РїРѕР·РёС†РёР№";
        }

        private void SetProjectMaterialBoughtAll_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectMaterialOwnedToQuantity(material => material.RequiredThroughWave);
        }

        private void SetProjectMaterialBoughtPriorWave_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectMaterialOwnedToQuantity(material => material.PriorQuantity);
        }

        private void SetProjectMaterialBoughtThisWave_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectMaterialOwnedToQuantity(material => material.PriorQuantity + material.Quantity);
        }

        private void ResetProjectMaterialBought_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectMaterialOwnedToQuantity(material => 0);
        }

        private void SetProjectStageBoughtAll_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectStageOwnedToQuantity(material => material.RequiredThroughWave, "Р­С‚Р°Рї РѕС‚РјРµС‡РµРЅ РєСѓРїР»РµРЅРЅС‹Рј");
        }

        private void ResetProjectStageBought_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedProjectStageOwnedToQuantity(material => material.PriorQuantity, "РџРѕРєСѓРїРєРё СЌС‚Р°РїР° СЃР±СЂРѕС€РµРЅС‹");
        }

        private void FilterProjectMaterialsToStage_Click(object sender, RoutedEventArgs e)
        {
            var stage = projectStageSummaryGrid.SelectedItem as BuildProjectStageSummary;
            if (stage == null)
            {
                return;
            }

            var view = CollectionViewSource.GetDefaultView(_projectMaterials);
            if (view == null)
            {
                return;
            }

            view.Filter = item =>
            {
                var material = item as BuildProjectMaterial;
                return material != null && material.Wave == stage.Wave && material.SourceGroupText == stage.Stage;
            };
            RefreshProjectMaterialGroups();
            projectStatus.Text = "РџРѕРєР°Р·Р°РЅ СЌС‚Р°Рї: " + stage.WaveText + " / " + stage.Stage;
        }

        private void ClearProjectMaterialFilter_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(_projectMaterials);
            if (view == null)
            {
                return;
            }

            view.Filter = null;
            RefreshProjectMaterialGroups();
            projectStatus.Text = "РџРѕРєР°Р·Р°РЅС‹ РІСЃРµ РјР°С‚РµСЂРёР°Р»С‹ РїСЂРѕРµРєС‚Р°";
        }

        private void SetSelectedProjectStageOwnedToQuantity(Func<BuildProjectMaterial, long> quantitySelector, string statusText)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var stage = projectStageSummaryGrid.SelectedItem as BuildProjectStageSummary;
            if (project == null || stage == null)
            {
                return;
            }

            foreach (var material in _projectMaterials.Where(item => item.Wave == stage.Wave && item.SourceGroupText == stage.Stage))
            {
                var stock = project.Stock.FirstOrDefault(item => item.TypeId == material.TypeId);
                if (stock == null)
                {
                    stock = new BuildProjectStock { TypeId = material.TypeId };
                    project.Stock.Add(stock);
                }

                stock.OwnedQuantity = Math.Max(0, quantitySelector(material));
            }

            SaveAndRefreshProject();
            projectStatus.Text = statusText + ": " + stage.WaveText + " / " + stage.Stage;
        }

        private void SetSelectedProjectMaterialOwnedToQuantity(Func<BuildProjectMaterial, long> quantitySelector)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var material = projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            if (project == null || material == null)
            {
                return;
            }

            var stock = project.Stock.FirstOrDefault(item => item.TypeId == material.TypeId);
            if (stock == null)
            {
                stock = new BuildProjectStock { TypeId = material.TypeId };
                project.Stock.Add(stock);
            }

            stock.OwnedQuantity = Math.Max(0, quantitySelector(material));
            SaveAndRefreshProject();
            projectStatus.Text = "РћСЃС‚Р°С‚РѕРє РѕР±РЅРѕРІР»РµРЅ: " + material.Name;
        }

        private void ProjectMaterialsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedProjectMaterialEditor();
        }

        private void RefreshSelectedProjectMaterialEditor()
        {
            var material = projectMaterialsGrid == null ? null : projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            if (material == null)
            {
                if (projectSelectedMaterialStatus != null)
                {
                    projectSelectedMaterialStatus.Text = "Р’С‹Р±РµСЂРёС‚Рµ РјР°С‚РµСЂРёР°Р» РґР»СЏ СЂРµРґР°РєС‚РёСЂРѕРІР°РЅРёСЏ РѕСЃС‚Р°С‚РєР°";
                }

                if (projectOwnedMaterialBox != null)
                {
                    projectOwnedMaterialBox.Text = "";
                }

                return;
            }

            if (projectOwnedMaterialBox != null)
            {
                projectOwnedMaterialBox.Text = material.OwnedQuantity > 0 ? material.OwnedQuantity.ToString(CultureInfo.InvariantCulture) : "";
            }

            if (projectSelectedMaterialStatus != null)
            {
                projectSelectedMaterialStatus.Text = string.Format(
                    "{0}: РЅСѓР¶РЅРѕ {1:N0}, РєСѓРїР»РµРЅРѕ {2:N0}, РѕСЃС‚Р°Р»РѕСЃСЊ {3:N0}, Рє РІРѕР»РЅРµ {4:N0}",
                    material.Name,
                    material.Quantity,
                    material.OwnedQuantity,
                    material.RemainingToBuy,
                    material.RequiredThroughWave);
            }
        }

        private void BuySelectedProjectMaterial_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var material = projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            if (project == null || material == null)
            {
                return;
            }

            SetProjectBuildBuyMode(project, material.TypeId, "Buy");
            SaveAndRefreshProject();
            projectStatus.Text = "РњР°С‚РµСЂРёР°Р» РїСЂРёРЅСѓРґРёС‚РµР»СЊРЅРѕ РѕСЃС‚Р°РІР»РµРЅ РІ Р·Р°РєСѓРїРєРµ: " + material.Name;
        }

        private void AutoSelectedProjectMaterial_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var material = projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            if (project == null || material == null)
            {
                return;
            }

            SetProjectBuildBuyMode(project, material.TypeId, "");
            SaveAndRefreshProject();
            projectStatus.Text = "РњР°С‚РµСЂРёР°Р» РІРѕР·РІСЂР°С‰РµРЅ РІ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРёР№ Build/Buy: " + material.Name;
        }

        private void ForceBuyProjectItem_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var item = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (project == null || item == null)
            {
                return;
            }

            SetProjectBuildBuyMode(project, item.ProductTypeId, "Buy");
            project.Items.Remove(item);
            SaveAndRefreshProject();
            projectStatus.Text = "РџСѓРЅРєС‚ СѓРґР°Р»РµРЅ РёР· СЃС‚СЂРѕР№РєРё Рё Р±СѓРґРµС‚ РїРѕРєСѓРїР°С‚СЊСЃСЏ: " + item.ProductName;
        }

        private void CopyProjectMaterials_Click(object sender, RoutedEventArgs e)
        {
            if (_projectMaterials.Count == 0)
            {
                return;
            }

            var lines = new[] { "Wave\tStage\tItem\tNeed this wave\tNeed through wave\tBought\tRemaining\tSource\tDestination" }
                .Concat(_projectMaterials
                    .Where(item => item.RemainingToBuy > 0)
                    .OrderBy(item => item.Wave)
                    .ThenBy(item => item.SourceGroupText)
                    .ThenBy(item => item.Name)
                    .Select(item => string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                        item.Wave,
                        item.SourceGroupText,
                        item.Name,
                        item.Quantity,
                        item.RequiredThroughWave,
                        item.OwnedQuantity,
                        item.RemainingToBuy,
                        item.SourceDisplayText,
                        item.TargetLocationText)));
            Clipboard.SetText(string.Join(Environment.NewLine, lines));
            projectStatus.Text = "РЎРїРёСЃРѕРє Р·Р°РєСѓРїРєРё СЃРєРѕРїРёСЂРѕРІР°РЅ";
        }

        private void CopyProjectEveBuy_Click(object sender, RoutedEventArgs e)
        {
            var text = BuildEveMultiBuyText(_projectMaterials);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Clipboard.SetText(text);
            projectStatus.Text = "EVE Р·Р°РєСѓРїРєР° РІСЃРµРіРѕ РїСЂРѕРµРєС‚Р° СЃРєРѕРїРёСЂРѕРІР°РЅР°: " + CountEveMultiBuyLines(_projectMaterials) + " СЃС‚СЂРѕРє";
        }

        private void CopyProjectEveBuyWithHauling_Click(object sender, RoutedEventArgs e)
        {
            var buyText = BuildEveMultiBuyText(_projectMaterials);
            if (string.IsNullOrWhiteSpace(buyText))
            {
                return;
            }

            var lines = new List<string>();
            foreach (var destinationGroup in _projectMaterials
                         .Where(item => item.Quantity > 0)
                         .GroupBy(item => string.IsNullOrWhiteSpace(item.TargetLocationText) ? "Р‘РµР· СЃС‚Р°РЅС†РёРё" : item.TargetLocationText)
                         .OrderBy(group => group.Key))
            {
                lines.Add(destinationGroup.Key);
                foreach (var item in destinationGroup
                             .GroupBy(item => new { item.TypeId, item.Name, item.Wave, item.SourceGroupText })
                             .OrderBy(group => group.Key.Wave)
                             .ThenBy(group => group.Key.SourceGroupText)
                             .ThenBy(group => group.Key.Name))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, "Wave {0}\t{1}\t{2}\t{3}",
                        item.Key.Wave,
                        item.Key.SourceGroupText,
                        item.Key.Name,
                        item.Sum(line => line.Quantity)));
                }

                lines.Add("");
            }

            Clipboard.SetText("EVE Multi-Buy" + Environment.NewLine
                + buyText + Environment.NewLine + Environment.NewLine
                + "РџР»Р°РЅ СЂР°Р·РІРѕР·Р°" + Environment.NewLine
                + string.Join(Environment.NewLine, lines).TrimEnd());
            projectStatus.Text = "EVE Р·Р°РєСѓРїРєР° Рё РїР»Р°РЅ СЂР°Р·РІРѕР·Р° СЃРєРѕРїРёСЂРѕРІР°РЅС‹";
        }

        private void CopyProjectHaulingPlan_Click(object sender, RoutedEventArgs e)
        {
            if (_projectMaterials.Count == 0)
            {
                return;
            }

            var lines = new List<string>();
            foreach (var destinationGroup in _projectMaterials
                         .Where(item => item.Quantity > 0)
                         .GroupBy(item => string.IsNullOrWhiteSpace(item.TargetLocationText) ? "Р‘РµР· СЃС‚Р°РЅС†РёРё" : item.TargetLocationText)
                         .OrderBy(group => group.Key))
            {
                lines.Add(destinationGroup.Key);
                foreach (var item in destinationGroup
                             .GroupBy(item => new { item.TypeId, item.Name, item.Wave, item.SourceGroupText })
                             .OrderBy(group => group.Key.Wave)
                             .ThenBy(group => group.Key.SourceGroupText)
                             .ThenBy(group => group.Key.Name))
                {
                    lines.Add(string.Format(CultureInfo.InvariantCulture, "Wave {0}\t{1}\t{2}\t{3}",
                        item.Key.Wave,
                        item.Key.SourceGroupText,
                        item.Key.Name,
                        item.Sum(line => line.Quantity)));
                }

                lines.Add("");
            }

            Clipboard.SetText(string.Join(Environment.NewLine, lines).TrimEnd());
            projectStatus.Text = "РџР»Р°РЅ СЂР°Р·РІРѕР·Р° СЃРєРѕРїРёСЂРѕРІР°РЅ";
        }

        private void CopyProjectStageEveBuy_Click(object sender, RoutedEventArgs e)
        {
            var stage = projectStageSummaryGrid.SelectedItem as BuildProjectStageSummary;
            if (stage == null)
            {
                return;
            }

            var stageMaterials = _projectMaterials.Where(item => item.Wave == stage.Wave && item.SourceGroupText == stage.Stage);
            var text = BuildEveMultiBuyText(stageMaterials);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Clipboard.SetText(text);
            projectStatus.Text = "EVE Р·Р°РєСѓРїРєР° С‚РѕР»СЊРєРѕ СЌС‚Р°РїР° СЃРєРѕРїРёСЂРѕРІР°РЅР°: " + stage.WaveText + " / " + stage.Stage;
        }

        private void CopyProjectWaveJobs_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var wave = GetSelectedProjectWave(project);
            if (project == null || wave <= 0)
            {
                return;
            }

            var jobs = project.Items
                .Where(item => item.Wave == wave && !item.IsCompleted)
                .OrderBy(item => item.BestStationText)
                .ThenBy(item => item.ProductionType)
                .ThenBy(item => item.ProductName)
                .ToList();
            if (jobs.Count == 0)
            {
                projectStatus.Text = "Р’ РІС‹Р±СЂР°РЅРЅРѕР№ РІРѕР»РЅРµ РЅРµС‚ РЅРµР·Р°РІРµСЂС€РµРЅРЅС‹С… jobs";
                return;
            }

            var lines = new List<string>
            {
                "Wave\tStation\tMaterials\tActivity\tItem\tRuns left\tOutput left\tME\tTE\tDecryptor\tInvention\tScience\tTime"
            };
            lines.AddRange(jobs.Select(item => string.Format(CultureInfo.InvariantCulture,
                "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7:0.##}\t{8:0.##}\t{9}\t{10}\t{11}\t{12}",
                item.Wave,
                item.BestStationText,
                _projectQueueDisplayService.GetJobMaterialStatus(item, _projectMaterials),
                item.ProductionType,
                item.ProductName,
                Math.Max(0, item.Runs - item.CompletedRuns),
                item.RemainingQuantity,
                item.MaterialEfficiency,
                item.TimeEfficiency,
                item.DecryptorText,
                item.InventionPlanText,
                item.ScienceStationText,
                item.ProductionTimeText)));

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
            projectStatus.Text = string.Format("РџР»Р°РЅ jobs РІРѕР»РЅС‹ {0} СЃРєРѕРїРёСЂРѕРІР°РЅ: {1} СЃС‚СЂРѕРє", wave, jobs.Count);
        }

        private void CompleteProjectWave_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var wave = GetSelectedProjectWave(project);
            if (project == null || wave <= 0)
            {
                return;
            }

            foreach (var item in project.Items.Where(item => item.Wave == wave))
            {
                item.CompletedRuns = item.Runs;
            }

            SaveAndRefreshProject();
            projectStatus.Text = "Р’РѕР»РЅР° " + wave + " РѕС‚РјРµС‡РµРЅР° РіРѕС‚РѕРІРѕР№";
        }

        private int GetSelectedProjectWave(BuildProject project)
        {
            var stage = projectStageSummaryGrid.SelectedItem as BuildProjectStageSummary;
            if (stage != null && stage.Wave > 0)
            {
                return stage.Wave;
            }

            var selectedItem = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (selectedItem != null && selectedItem.Wave > 0)
            {
                return selectedItem.Wave;
            }

            return project == null
                ? 0
                : project.Items.Where(projectItem => !projectItem.IsCompleted).OrderBy(projectItem => projectItem.Wave).Select(projectItem => projectItem.Wave).FirstOrDefault();
        }

        private static string BuildEveMultiBuyText(IEnumerable<BuildProjectMaterial> materials)
        {
            var lines = materials
                .Where(item => item.RemainingToBuy > 0)
                .GroupBy(item => item.TypeId)
                .Select(group => new
                {
                    Name = group.First().Name,
                    Quantity = group.Sum(item => item.RemainingToBuy)
                })
                .Where(item => item.Quantity > 0)
                .OrderBy(item => item.Name)
                .Select(item => string.Format(CultureInfo.InvariantCulture, "{0} {1}", item.Name, item.Quantity));

            return string.Join(Environment.NewLine, lines);
        }

        private static int CountEveMultiBuyLines(IEnumerable<BuildProjectMaterial> materials)
        {
            return materials
                .Where(item => item.RemainingToBuy > 0)
                .GroupBy(item => item.TypeId)
                .Count();
        }

        private void CopyProjectPlan_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null || project.Items.Count == 0)
            {
                return;
            }

            var lines = new List<string>
            {
                "Wave\tRoot\tType\tItem\tRuns\tRemaining\tStation\tME\tTE\tDecryptor\tProfit\tROI\tSVR\tSVR*ISK/h\tSold\tOrders\tAvg/order\tTrend\tMarket Volume\tCoverage\tInvention\tScience"
            };
            lines.AddRange(project.Items
                .OrderBy(item => item.Wave)
                .ThenBy(item => item.BestStationText)
                .ThenBy(item => item.ProductionType)
                .ThenBy(item => item.ProductName)
                .Select(item => string.Format(CultureInfo.InvariantCulture,
                    "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7:0.##}\t{8:0.##}\t{9}\t{10:0}\t{11:0.##}%\t{12}\t{13:0}\t{14}\t{15}\t{16:0.##}\t{17:0.####}\t{18}\t{19}\t{20}\t{21}",
                    item.Wave,
                    item.IsRootItem ? "Yes" : "",
                    item.ProductionType,
                    item.ProductName,
                    item.Runs,
                    item.RemainingQuantity,
                    item.BestStationText,
                    item.MaterialEfficiency,
                    item.TimeEfficiency,
                    item.DecryptorText,
                    item.Profit,
                    item.ReturnOnInvestmentPercent,
                    item.SalesVolumeRatioText,
                    item.SvrTimesIskPerHour,
                    item.TotalItemsSold,
                    item.TotalOrdersFilled,
                    item.AverageItemsPerOrder,
                    item.PriceTrend,
                    item.ProductMarketVolume,
                    item.MarketCoverageText,
                    item.InventionPlanText,
                    item.ScienceStationText)));
            Clipboard.SetText(string.Join(Environment.NewLine, lines));
            projectStatus.Text = "РџР»Р°РЅ РїСЂРѕРµРєС‚Р° СЃРєРѕРїРёСЂРѕРІР°РЅ";
        }

        private void BuildSelectedProjectMaterial_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var material = projectMaterialsGrid.SelectedItem as BuildProjectMaterial;
            var facilityPreset = blueprintFacilityBox.SelectedItem as FacilityPreset;
            if (project == null || material == null || facilityPreset == null)
            {
                return;
            }

            var blueprint = _database.FindBlueprintByProduct(material.TypeId);
            if (blueprint == null)
            {
                projectStatus.Text = "Р”Р»СЏ РІС‹Р±СЂР°РЅРЅРѕРіРѕ РјР°С‚РµСЂРёР°Р»Р° РЅРµС‚ С‡РµСЂС‚РµР¶Р°";
                return;
            }

            double me;
            double te;
            var decryptor = GetSelectedDecryptor();
            GetDefaultEfficiencyForChildBlueprint(blueprint, facilityPreset, decryptor, out me, out te);
            var requiredQuantity = Math.Max(1, material.RemainingToBuy);
            var runs = (int)Math.Ceiling(requiredQuantity / (double)Math.Max(1, blueprint.PortionSize));
            var itemIndex = project.Items
                .GroupBy(item => GetProjectItemKey(item.ProductTypeId, item.MaterialEfficiency, item.TimeEfficiency, GetDecryptorByTypeId(item.DecryptorTypeId)))
                .ToDictionary(group => group.Key, group => group.First());
            var typeIds = new HashSet<long>();
            CollectBlueprintTreeTypeIds(blueprint, typeIds, decryptor, new HashSet<long>());
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            var prices = region == null
                ? new Dictionary<long, MarketPrice>()
                : GetCachedPrices(region.StationId ?? region.RegionId, typeIds);
            var adjustedPrices = _database.GetAdjustedPrices(typeIds);

            SetProjectBuildBuyMode(project, material.TypeId, "Build");
            AddBlueprintTreeToProject(project, itemIndex, blueprint, runs, requiredQuantity, me, te, decryptor, facilityPreset, prices, adjustedPrices, new HashSet<long>());
            ApplyProjectItemEstimates(project, facilityPreset, prices, adjustedPrices);
            SaveAndRefreshProject();
            projectItemsGrid.Items.Refresh();
            projectsList.Items.Refresh();
            projectStatus.Text = "РњР°С‚РµСЂРёР°Р» РґРѕР±Р°РІР»РµРЅ РІ СЃС‚СЂРѕР№РєСѓ: " + blueprint.ProductName;
        }

        private void DeleteProjectItem_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            var item = projectItemsGrid.SelectedItem as BuildProjectItem;
            if (project == null || item == null)
            {
                return;
            }

            project.Items.Remove(item);
            SaveAndRefreshProject();
        }

        private void RecalculateProject_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null)
            {
                return;
            }

            var before = project.Items.Sum(item => item.MaterialCost + item.InventionCost + item.InstallationCost);
            RefreshProjectItemEstimates(project);
            RefreshProjectMaterials(project);
            projectItemsGrid.Items.Refresh();
            projectMaterialsGrid.Items.Refresh();
            _projectStore.Save(_projects);
            var after = project.Items.Sum(item => item.MaterialCost + item.InventionCost + item.InstallationCost);
            projectStatus.Text = string.Format("{0} | РџРµСЂРµСЃС‡РµС‚: {1:N0} -> {2:N0} ISK", project.SummaryText, before, after);
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = projectsList.SelectedItem as BuildProject;
            if (project == null)
            {
                return;
            }

            _projects.Remove(project);
            _projectStore.Save(_projects);
            projectsList.Items.Refresh();
            projectItemsGrid.ItemsSource = null;
            _projectMaterials.Clear();
            _projectStageSummaries.Clear();
        }

        private void SaveAndRefreshProject()
        {
            _projectStore.Save(_projects);
            projectItemsGrid.Items.Refresh();
            projectsList.Items.Refresh();
            var project = projectsList.SelectedItem as BuildProject;
            RefreshProjectMaterials(project);
            if (project == null)
            {
                projectStatus.Text = "";
            }
        }

        private void RefreshProjectMaterials(BuildProject project)
        {
            _projectMaterials.Clear();
            _projectStageSummaries.Clear();
            if (project == null)
            {
                return;
            }

            var totals = new Dictionary<string, BuildProjectMaterial>();
            var plannedProducts = project.Items
                .Where(item => !string.Equals(GetProjectBuildBuyMode(project, item.ProductTypeId), "Buy", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.ProductTypeId)
                .ToHashSet();
            var stockByTypeId = project.Stock.ToDictionary(item => item.TypeId, item => item.OwnedQuantity);
            var facilityPreset = blueprintFacilityBox.SelectedItem as FacilityPreset;
            foreach (var item in project.Items.Where(item => !item.IsCompleted))
            {
                var remainingRuns = Math.Max(0, item.Runs - item.CompletedRuns);
                if (remainingRuns == 0)
                {
                    continue;
                }

                foreach (var material in _database.GetManufacturingMaterials(item.BlueprintTypeId))
                {
                    if (plannedProducts.Contains(material.TypeId))
                    {
                        continue;
                    }

                    var blueprint = _database.FindBlueprintByProduct(item.ProductTypeId);
                    var station = FindProjectItemStation(facilityPreset, item);
                    BuildProjectMaterial line;
                    var materialKey = item.Wave + ":Manufacturing:" + material.TypeId + ":" + GetStationKey(station);
                    if (!totals.TryGetValue(materialKey, out line))
                    {
                        line = new BuildProjectMaterial
                        {
                            TypeId = material.TypeId,
                            Name = material.Name,
                            Wave = item.Wave,
                            TargetStationName = station == null ? "" : station.Name,
                            TargetStationSystem = station == null ? "" : station.SystemName,
                            TargetActivity = item.ProductionType
                        };
                        totals[materialKey] = line;
                    }

                    line.Quantity += CalculateAdjustedQuantity(material.Quantity, remainingRuns,
                        GetMaterialMultiplier(blueprint, station, item.MaterialEfficiency));
                    line.UsedBy = AddUsedBy(line.UsedBy, item.ProductName);
                }

                AddProjectInventionMaterials(totals, item, remainingRuns, facilityPreset);
            }

            foreach (var line in totals.Values)
            {
                line.BuildBuyMode = GetProjectBuildBuyMode(project, line.TypeId);
            }

            ConvertProjectMineralsToOreByWave(totals, facilityPreset);
            ApplyProjectMaterialPrices(totals.Values);
            ProjectStockService.DistributeByWave(totals.Values, stockByTypeId);

            foreach (var line in totals.Values.OrderBy(item => item.Wave).ThenBy(item => item.SourceGroupText).ThenByDescending(item => item.TotalCost).ThenBy(item => item.Name))
            {
                _projectMaterials.Add(line);
            }

            RefreshProjectStageSummaries();
            RefreshProjectMaterialGroups();
            projectStatus.Text = string.Format("{0} | Р—Р°РєСѓРїРєР°: {1:N0} ISK", project.SummaryText, totals.Values.Sum(item => item.TotalCost));
        }

        private void RefreshProjectStageSummaries()
        {
            _projectStageSummaries.Clear();
            foreach (var group in _projectMaterials
                         .GroupBy(item => new { item.Wave, item.SourceGroupText })
                         .OrderBy(group => group.Key.Wave)
                         .ThenBy(group => group.Key.SourceGroupText))
            {
                _projectStageSummaries.Add(new BuildProjectStageSummary
                {
                    Wave = group.Key.Wave,
                    Stage = group.Key.SourceGroupText,
                    TotalLines = group.Count(),
                    OpenLines = group.Count(item => item.RemainingToBuy > 0),
                    RequiredQuantity = group.Sum(item => item.Quantity),
                    OwnedQuantity = group.Sum(item => Math.Min(item.Quantity, item.OwnedQuantity)),
                    RemainingQuantity = group.Sum(item => item.RemainingToBuy),
                    RemainingCost = group.Sum(item => item.TotalCost)
                });
            }
        }

        private void AddProjectInventionMaterials(Dictionary<string, BuildProjectMaterial> totals, BuildProjectItem item, int remainingRuns, FacilityPreset facilityPreset)
        {
            var blueprint = _database.FindBlueprintByProduct(item.ProductTypeId);
            if (!ShouldUseInventionCosts(blueprint))
            {
                return;
            }

            var invention = _database.GetInventionInfo(blueprint.BlueprintTypeId);
            if (invention == null || invention.Probability <= 0)
            {
                return;
            }

            var decryptor = GetDecryptorByTypeId(item.DecryptorTypeId);
            var plan = CreateInventionPlan(blueprint, invention, remainingRuns, facilityPreset, decryptor);
            if (plan.Chance <= 0 || plan.Jobs <= 0)
            {
                return;
            }

            var inventionJobs = (long)plan.Jobs;
            var inventionStation = ResolveProjectScienceStation(facilityPreset, item, "Invention");
            var copyStation = ResolveProjectScienceStation(facilityPreset, item, "Copying");

            if (blueprint.TechLevel == 3 && invention.SourceBlueprintTypeId > 0)
            {
                var sourceNames = _database.GetTypeNames(new[] { invention.SourceBlueprintTypeId });
                string sourceName;
                AddProjectMaterial(totals, item.Wave, invention.SourceBlueprintTypeId,
                    sourceNames.TryGetValue(invention.SourceBlueprintTypeId, out sourceName) ? sourceName : "Ancient Relic / Source",
                    inventionJobs, item.ProductName, "Invention", "Source item", inventionStation);
            }

            foreach (var material in invention.Materials)
            {
                AddProjectMaterial(totals, item.Wave, material.TypeId, material.Name, material.Quantity * inventionJobs, item.ProductName, "Invention", "Datacore/material", inventionStation);
            }

            if (decryptor.TypeId > 0)
            {
                AddProjectMaterial(totals, item.Wave, decryptor.TypeId, decryptor.Name, inventionJobs, item.ProductName, "Invention", "Decryptor", inventionStation);
            }

            foreach (var material in invention.CopyMaterials)
            {
                AddProjectMaterial(totals, item.Wave, material.TypeId, material.Name, material.Quantity * inventionJobs, item.ProductName, "Copying", "Copy material", copyStation);
            }
        }

        private void AddProjectMaterial(Dictionary<string, BuildProjectMaterial> totals, int wave, long typeId, string name, long quantity, string usedBy,
            string sourceMode = "", string sourceDetails = "", FacilityStation targetStation = null)
        {
            if (typeId <= 0 || quantity <= 0)
            {
                return;
            }

            BuildProjectMaterial line;
            var materialKey = wave + ":" + sourceMode + ":" + typeId + ":" + GetStationKey(targetStation);
            if (!totals.TryGetValue(materialKey, out line))
            {
                line = new BuildProjectMaterial
                {
                    TypeId = typeId,
                    Name = name,
                    Wave = wave,
                    SourceMode = sourceMode,
                    SourceDetails = sourceDetails,
                    TargetStationName = targetStation == null ? "" : targetStation.Name,
                    TargetStationSystem = targetStation == null ? "" : targetStation.SystemName,
                    TargetActivity = sourceMode
                };
                totals[materialKey] = line;
            }

            line.Quantity += quantity;
            line.UsedBy = AddUsedBy(line.UsedBy, usedBy);
        }

        private static string GetStationKey(FacilityStation station)
        {
            return station == null ? "" : (station.Name ?? "") + "|" + (station.SystemName ?? "");
        }

        private static void RemoveProjectMaterialLine(Dictionary<string, BuildProjectMaterial> totals, BuildProjectMaterial line)
        {
            var key = totals.FirstOrDefault(pair => ReferenceEquals(pair.Value, line)).Key;
            if (!string.IsNullOrEmpty(key))
            {
                totals.Remove(key);
            }
        }

        private FacilityStation ResolveProjectScienceStation(FacilityPreset facilityPreset, BuildProjectItem item, string productionType)
        {
            if (facilityPreset == null || item == null)
            {
                return null;
            }

            string stationName;
            string stationSystem;
            if (productionType == "Copying")
            {
                stationName = item.CopyStationName;
                stationSystem = item.CopyStationSystem;
            }
            else
            {
                stationName = item.InventionStationName;
                stationSystem = item.InventionStationSystem;
            }

            return facilityPreset.Stations.FirstOrDefault(station =>
                       string.Equals(station.Name, stationName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(station.SystemName, stationSystem, StringComparison.OrdinalIgnoreCase))
                   ?? facilityPreset.GetBestStationFor(productionType);
        }

        private void ConvertProjectMineralsToOre(Dictionary<string, BuildProjectMaterial> totals, FacilityPreset facilityPreset)
        {
            if (facilityPreset == null || !facilityPreset.ConvertMineralsToOre || totals.Count == 0)
            {
                return;
            }

            var reprocessingStation = facilityPreset.Stations
                .Where(station => station.ProductionType == "Reprocessing" && station.SupportsProduction)
                .OrderByDescending(station => station.MaterialMultiplier)
                .FirstOrDefault();
            if (reprocessingStation == null)
            {
                return;
            }

            var mineralLines = totals.Values
                .Where(line => line.Quantity > 0 && _database.IsMineral(line.TypeId))
                .ToList();
            if (mineralLines.Count == 0)
            {
                return;
            }

            var optionLookup = mineralLines
                .Select(line => line.TypeId)
                .Distinct()
                .ToDictionary(typeId => typeId, typeId => _database.GetReprocessingOptionsForMineral(typeId, facilityPreset.PreferCompressedOre));
            var oreTypeIds = optionLookup.Values.SelectMany(options => options.Select(option => option.OreTypeId)).Distinct().ToList();
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            var orePrices = region == null || oreTypeIds.Count == 0
                ? new Dictionary<long, MarketPrice>()
                : LoadPricesWithCache(region.StationId ?? region.RegionId, region.Name, oreTypeIds, region.StationId.HasValue);

            foreach (var mineral in mineralLines)
            {
                IReadOnlyList<ReprocessingOption> options;
                if (!optionLookup.TryGetValue(mineral.TypeId, out options) || options.Count == 0)
                {
                    continue;
                }

                var best = options
                    .Select(option => new
                    {
                        Option = option,
                        BatchOutput = GetReprocessedMineralOutput(option, GetReprocessingYield(facilityPreset, reprocessingStation, option)),
                        UnitPrice = GetOreUnitPrice(orePrices, option.OreTypeId)
                    })
                    .Where(option => option.BatchOutput > 0)
                    .OrderBy(option => option.UnitPrice > 0 ? option.UnitPrice * option.Option.UnitsToReprocess / option.BatchOutput : double.MaxValue)
                    .ThenBy(option => option.Option.OreVolume * option.Option.UnitsToReprocess / option.BatchOutput)
                    .FirstOrDefault();
                if (best == null)
                {
                    continue;
                }

                var batches = (long)Math.Ceiling(mineral.Quantity / (double)best.BatchOutput);
                var oreQuantity = batches * Math.Max(1, best.Option.UnitsToReprocess);
                var producedMinerals = batches * best.BatchOutput;
                RemoveProjectMaterialLine(totals, mineral);

                var oreKey = string.Format(CultureInfo.InvariantCulture, "{0}:ore:{1}:{2}:{3}", mineral.Wave, best.Option.OreTypeId, mineral.TypeId, GetStationKey(reprocessingStation));
                BuildProjectMaterial oreLine;
                if (!totals.TryGetValue(oreKey, out oreLine))
                {
                    oreLine = new BuildProjectMaterial
                    {
                        TypeId = best.Option.OreTypeId,
                        Name = best.Option.OreName,
                        Wave = mineral.Wave,
                        SourceMode = "Reprocessing",
                        ProducesTypeId = mineral.TypeId,
                        TargetStationName = reprocessingStation.Name,
                        TargetStationSystem = reprocessingStation.SystemName,
                        TargetActivity = "Reprocessing"
                    };
                    totals[oreKey] = oreLine;
                }

                oreLine.Quantity += oreQuantity;
                oreLine.ProducesQuantity += producedMinerals;
                oreLine.UsedBy = AddUsedBy(oreLine.UsedBy, mineral.Name + " РґР»СЏ " + mineral.UsedBy);
            }
        }

        private long GetReprocessedMineralOutput(ReprocessingOption option, double yield)
        {
            return _orePlanningService.GetReprocessedMineralOutput(option, yield);
        }

        private void ConvertProjectMineralsToOreByWave(Dictionary<string, BuildProjectMaterial> totals, FacilityPreset facilityPreset)
        {
            if (facilityPreset == null || !facilityPreset.ConvertMineralsToOre || totals.Count == 0)
            {
                return;
            }

            var reprocessingStation = facilityPreset.Stations
                .Where(station => station.ProductionType == "Reprocessing" && station.SupportsProduction)
                .OrderByDescending(station => station.MaterialMultiplier)
                .FirstOrDefault();
            if (reprocessingStation == null)
            {
                return;
            }

            var mineralLines = totals.Values
                .Where(line => line.Quantity > 0 && _database.IsMineral(line.TypeId))
                .ToList();
            if (mineralLines.Count == 0)
            {
                return;
            }

            var requiredMineralTypeIds = mineralLines.Select(line => line.TypeId).Distinct().ToList();
            var oreTypeIds = requiredMineralTypeIds
                .SelectMany(typeId => _database.GetReprocessingOptionsForMineral(typeId, facilityPreset.PreferCompressedOre)
                    .Select(option => option.OreTypeId))
                .Distinct()
                .ToList();
            var oreOutputs = _database.GetReprocessingOutputsForOres(oreTypeIds)
                .GroupBy(option => option.OreTypeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            var priceTypeIds = oreTypeIds.Concat(requiredMineralTypeIds).Distinct().ToList();
            var prices = region == null || priceTypeIds.Count == 0
                ? new Dictionary<long, MarketPrice>()
                : LoadPricesWithCache(region.StationId ?? region.RegionId, region.Name, priceTypeIds, region.StationId.HasValue);
            var candidates = _orePlanningService.BuildCandidates(
                oreOutputs,
                requiredMineralTypeIds,
                option => GetReprocessingYield(facilityPreset, reprocessingStation, option),
                option => GetOreUnitPrice(prices, option.OreTypeId));
            if (candidates.Count == 0)
            {
                return;
            }

            foreach (var waveGroup in mineralLines.GroupBy(line => line.Wave))
            {
                var remaining = waveGroup.ToDictionary(line => line.TypeId, line => line.Quantity);
                var plan = _orePlanningService.PlanOreForMinerals(remaining, candidates, BuildMaterialUnitPriceLookup(requiredMineralTypeIds, prices));
                if (plan.Count == 0 || remaining.Values.Any(quantity => quantity > 0))
                {
                    continue;
                }

                foreach (var mineral in waveGroup)
                {
                    RemoveProjectMaterialLine(totals, mineral);
                }

                var usedBy = "Reprocessing РґР»СЏ " + string.Join(", ", waveGroup.Select(line => line.Name).Distinct());
                var sourceDetails = BuildReprocessingDetails(waveGroup, plan);
                foreach (var plannedOre in plan)
                {
                    var oreKey = string.Format(CultureInfo.InvariantCulture, "{0}:ore:{1}:{2}", waveGroup.Key, plannedOre.Candidate.OreTypeId, GetStationKey(reprocessingStation));
                    BuildProjectMaterial oreLine;
                    if (!totals.TryGetValue(oreKey, out oreLine))
                    {
                        oreLine = new BuildProjectMaterial
                        {
                            TypeId = plannedOre.Candidate.OreTypeId,
                            Name = plannedOre.Candidate.OreName,
                            Wave = waveGroup.Key,
                            SourceMode = "Reprocessing",
                            TargetStationName = reprocessingStation.Name,
                            TargetStationSystem = reprocessingStation.SystemName,
                            TargetActivity = "Reprocessing"
                        };
                        totals[oreKey] = oreLine;
                    }

                    oreLine.Quantity += plannedOre.Quantity;
                    oreLine.ProducesQuantity += plannedOre.ProducedMinerals;
                    oreLine.SourceDetails = sourceDetails;
                    oreLine.UsedBy = AddUsedBy(oreLine.UsedBy, usedBy);
                }
            }
        }

        private double GetReprocessingYield(FacilityPreset preset, FacilityStation station, ReprocessingOption option)
        {
            var math = _facilityCatalog.Calculate(station, GetSelectedRigIds(station), option.OreGroupId, option.OreCategoryId);
            var baseYield = 0.5 + Math.Max(0, math.MaterialMultiplier);
            var refining = ClampSkill(preset.RefiningSkillLevel);
            var reprocessing = ClampSkill(preset.ReprocessingSkillLevel);
            var oreProcessing = ClampSkill(preset.OreProcessingSkillLevel);
            var implant = Math.Max(0, preset.ReprocessingImplantPercent) / 100.0;
            return Math.Min(1.0, baseYield
                * (1 + refining * 0.03)
                * (1 + reprocessing * 0.02)
                * (1 + oreProcessing * 0.02)
                * (1 + implant));
        }

        private static string BuildReprocessingDetails(IEnumerable<BuildProjectMaterial> requiredMinerals, IEnumerable<PlannedOre> plan)
        {
            var requiredByTypeId = requiredMinerals
                .GroupBy(line => line.TypeId)
                .ToDictionary(group => group.Key, group => new
                {
                    Name = group.First().Name,
                    Quantity = group.Sum(line => line.Quantity)
                });
            var producedByTypeId = plan
                .SelectMany(item => item.ProducedByMineral)
                .GroupBy(item => item.Key)
                .ToDictionary(group => group.Key, group => group.Sum(item => item.Value));

            var parts = new List<string>();
            foreach (var required in requiredByTypeId.OrderBy(item => item.Value.Name))
            {
                long produced;
                producedByTypeId.TryGetValue(required.Key, out produced);
                var surplus = Math.Max(0, produced - required.Value.Quantity);
                parts.Add(surplus > 0
                    ? string.Format(CultureInfo.CurrentCulture, "{0} {1:N0} (+{2:N0})", required.Value.Name, produced, surplus)
                    : string.Format(CultureInfo.CurrentCulture, "{0} {1:N0}", required.Value.Name, produced));
            }

            return "РџРµСЂРµСЂР°Р±РѕС‚РєР°: " + string.Join(", ", parts);
        }

        private double GetOreUnitPrice(Dictionary<long, MarketPrice> orePrices, long oreTypeId)
        {
            MarketPrice price;
            return orePrices.TryGetValue(oreTypeId, out price) ? GetMaterialUnitPrice(price) : 0;
        }

        private Dictionary<long, double> BuildMaterialUnitPriceLookup(IEnumerable<long> typeIds, Dictionary<long, MarketPrice> prices)
        {
            return typeIds
                .Distinct()
                .ToDictionary(typeId => typeId, typeId =>
                {
                    MarketPrice price;
                    return prices.TryGetValue(typeId, out price) ? GetMaterialUnitPrice(price) : 0;
                });
        }

        private static void AddMineralBuyQuantity(Dictionary<long, long> mineralBuyQuantities, long typeId, long quantity)
        {
            if (typeId <= 0 || quantity <= 0)
            {
                return;
            }

            long current;
            mineralBuyQuantities.TryGetValue(typeId, out current);
            mineralBuyQuantities[typeId] = current + quantity;
        }

        private double GetMineralPurchaseCost(Dictionary<long, long> mineralQuantities, FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices)
        {
            if (mineralQuantities == null || mineralQuantities.Count == 0)
            {
                return 0;
            }

            var directCost = 0.0;
            var directComplete = true;
            foreach (var mineral in mineralQuantities)
            {
                MarketPrice price;
                var unitPrice = prices.TryGetValue(mineral.Key, out price) ? GetMaterialUnitPrice(price) : 0;
                if (unitPrice <= 0)
                {
                    directComplete = false;
                    continue;
                }

                directCost += mineral.Value * unitPrice;
            }

            var oreCost = GetOrePlanPurchaseCost(mineralQuantities, facilityPreset, prices);
            if (oreCost > 0 && (!directComplete || directCost <= 0 || oreCost < directCost))
            {
                return oreCost;
            }

            return directCost;
        }

        private double GetOrePlanPurchaseCost(Dictionary<long, long> mineralQuantities, FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices)
        {
            if (facilityPreset == null || !facilityPreset.ConvertMineralsToOre || mineralQuantities == null || mineralQuantities.Count == 0)
            {
                return 0;
            }

            var reprocessingStation = facilityPreset.Stations
                .Where(station => station.ProductionType == "Reprocessing" && station.SupportsProduction)
                .OrderByDescending(station => station.MaterialMultiplier)
                .FirstOrDefault();
            if (reprocessingStation == null)
            {
                return 0;
            }

            var requiredMineralTypeIds = mineralQuantities.Keys.ToList();
            var oreTypeIds = requiredMineralTypeIds
                .SelectMany(typeId => _database.GetReprocessingOptionsForMineral(typeId, facilityPreset.PreferCompressedOre)
                    .Select(option => option.OreTypeId))
                .Distinct()
                .ToList();
            if (oreTypeIds.Count == 0)
            {
                return 0;
            }

            var oreOutputs = _database.GetReprocessingOutputsForOres(oreTypeIds)
                .GroupBy(option => option.OreTypeId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var candidates = _orePlanningService.BuildCandidates(
                oreOutputs,
                requiredMineralTypeIds,
                option => GetReprocessingYield(facilityPreset, reprocessingStation, option),
                option => GetOreUnitPrice(prices, option.OreTypeId));
            if (candidates.Count == 0)
            {
                return 0;
            }

            var remaining = mineralQuantities.ToDictionary(item => item.Key, item => item.Value);
            var plan = _orePlanningService.PlanOreForMinerals(remaining, candidates, BuildMaterialUnitPriceLookup(requiredMineralTypeIds, prices));
            if (plan.Count == 0 || remaining.Values.Any(quantity => quantity > 0))
            {
                return 0;
            }

            var cost = 0.0;
            foreach (var plannedOre in plan)
            {
                if (plannedOre.Candidate.UnitPrice <= 0)
                {
                    return 0;
                }

                cost += plannedOre.Quantity * plannedOre.Candidate.UnitPrice;
            }

            return cost;
        }

        private double GetMineralOreReplacementCost(long mineralTypeId, long mineralQuantity, FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices)
        {
            if (facilityPreset == null || !facilityPreset.ConvertMineralsToOre || mineralQuantity <= 0 || !_database.IsMineral(mineralTypeId))
            {
                return 0;
            }

            var reprocessingStation = facilityPreset.Stations
                .Where(station => station.ProductionType == "Reprocessing" && station.SupportsProduction)
                .OrderByDescending(station => station.MaterialMultiplier)
                .FirstOrDefault();
            if (reprocessingStation == null)
            {
                return 0;
            }

            var options = _database.GetReprocessingOptionsForMineral(mineralTypeId, facilityPreset.PreferCompressedOre);
            var bestCost = double.MaxValue;
            foreach (var option in options)
            {
                var batchOutput = GetReprocessedMineralOutput(option, GetReprocessingYield(facilityPreset, reprocessingStation, option));
                if (batchOutput <= 0)
                {
                    continue;
                }

                MarketPrice price;
                var unitPrice = prices.TryGetValue(option.OreTypeId, out price) ? GetMaterialUnitPrice(price) : 0;
                if (unitPrice <= 0)
                {
                    continue;
                }

                var batches = Math.Ceiling(mineralQuantity / (double)batchOutput);
                var cost = batches * Math.Max(1, option.UnitsToReprocess) * unitPrice;
                if (cost < bestCost)
                {
                    bestCost = cost;
                }
            }

            return bestCost == double.MaxValue ? 0 : bestCost;
        }

        private void RefreshProjectMaterialGroups()
        {
            var view = CollectionViewSource.GetDefaultView(_projectMaterials);
            if (view == null)
            {
                return;
            }

            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("WaveText"));
            view.GroupDescriptions.Add(new PropertyGroupDescription("SourceGroupText"));
            view.Refresh();
        }

        private void ApplyProjectMaterialPrices(IEnumerable<BuildProjectMaterial> totals)
        {
            var materialLines = totals.ToList();
            if (materialLines.Count == 0)
            {
                return;
            }

            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (region == null)
            {
                return;
            }

            try
            {
                var id = region.StationId ?? region.RegionId;
                var prices = LoadPricesWithCache(id, region.Name, materialLines.Select(item => item.TypeId).Distinct(), region.StationId.HasValue);

                foreach (var line in materialLines)
                {
                    MarketPrice price;
                    var unitPrice = prices.TryGetValue(line.TypeId, out price) ? GetMaterialUnitPrice(price) : 0;
                    if (unitPrice > 0)
                    {
                        line.UnitPrice = unitPrice;
                        line.TotalCost = line.Quantity * unitPrice;
                    }
                }
            }
            catch
            {
                // Price refresh can fail because this data comes from Fuzzwork. Keep quantities visible.
            }
        }

        private string AddUsedBy(string current, string productName)
        {
            return _usedByListService.Add(current, productName);
        }

        private string GetProjectBuildBuyMode(BuildProject project, long typeId)
        {
            return _buildProjectDecisionService.GetBuildBuyMode(project, typeId);
        }

        private void SetProjectBuildBuyMode(BuildProject project, long typeId, string mode)
        {
            _buildProjectDecisionService.SetBuildBuyMode(project, typeId, mode);
        }

        private double GetMaterialUnitPrice(MarketPrice price)
        {
            var useBuyOrder = GetSelectedPriceMode(materialPriceModeBox) == "Max Buy";
            var value = useBuyOrder ? price.BuyMax : price.SellMin;
            var modifiedValue = _marketPriceSelectionService.ApplyModifier(
                value,
                ReadDouble(materialPriceModifierBox == null ? "0" : materialPriceModifierBox.Text, 0));
            if (!useBuyOrder || modifiedValue <= 0)
            {
                return modifiedValue;
            }

            var preset = blueprintFacilityBox == null ? null : blueprintFacilityBox.SelectedItem as FacilityPreset;
            return preset != null && preset.IncludeBuyOrderBrokerFee
                ? modifiedValue + _salesFeeService.GetSalesBrokerFee(modifiedValue, preset)
                : modifiedValue;
        }

        private long GetAvailableMarketVolume(MarketPrice price)
        {
            return _marketPriceSelectionService.GetStrictVolume(price, GetSelectedPriceMode(materialPriceModeBox));
        }

        private double GetProductUnitPrice(MarketPrice price)
        {
            return _marketPriceSelectionService.GetUnitPrice(
                price,
                GetSelectedPriceMode(productPriceModeBox),
                ReadDouble(productPriceModifierBox == null ? "0" : productPriceModifierBox.Text, 0));
        }

        private double GetEffectiveProductUnitPrice(BlueprintSearchResult blueprint, MarketPrice price, out double contractPrice, out string source)
        {
            var result = GetEffectiveProductPrice(blueprint, price);
            contractPrice = result.ContractUnitPrice;
            source = result.Source;
            return result.UnitPrice;
        }

        private ContractPriceResult GetEffectiveProductPrice(BlueprintSearchResult blueprint, MarketPrice price)
        {
            var modifier = ReadDouble(productPriceModifierBox == null ? "0" : productPriceModifierBox.Text, 0);
            return _contractPricingService.GetEffectiveProductPrice(
                blueprint,
                price,
                _contractPrices,
                useContractPricesBox != null && useContractPricesBox.IsChecked == true,
                GetProductUnitPrice,
                GetProductMarketVolume,
                modifier,
                30);
        }

        private long GetProductMarketVolume(MarketPrice price)
        {
            return _marketPriceSelectionService.GetProductVolume(price, GetSelectedPriceMode(productPriceModeBox));
        }

        private static string GetSelectedPriceMode(ComboBox comboBox)
        {
            var item = comboBox == null ? null : comboBox.SelectedItem as ComboBoxItem;
            return item == null ? "Min Sell" : item.Content.ToString();
        }

        private DecryptorOption GetSelectedDecryptor()
        {
            return (blueprintDecryptorBox == null ? null : blueprintDecryptorBox.SelectedItem as DecryptorOption)
                ?? GetNoDecryptor();
        }

        private DecryptorOption GetEstimatedDecryptor(BlueprintSearchResult blueprint)
        {
            if (blueprint != null && blueprint.SelectedDecryptorTypeId > 0)
            {
                return GetDecryptorByTypeId(blueprint.SelectedDecryptorTypeId);
            }

            return GetSelectedDecryptor();
        }

        private IReadOnlyList<DecryptorOption> GetBlueprintDecryptorCandidates(BlueprintSearchResult blueprint)
        {
            if (!ShouldUseInventionCosts(blueprint))
            {
                return new[] { GetNoDecryptor() };
            }

            if (autoDecryptorBox != null && autoDecryptorBox.IsChecked == true && _decryptors != null && _decryptors.Count > 0)
            {
                return _decryptors;
            }

            return new[] { GetSelectedDecryptor() };
        }

        private static void SetBlueprintDecryptor(BlueprintSearchResult blueprint, DecryptorOption decryptor)
        {
            decryptor = decryptor ?? GetNoDecryptor();
            blueprint.SelectedDecryptorTypeId = decryptor.TypeId;
            blueprint.SelectedDecryptorName = decryptor.TypeId > 0 ? decryptor.Name : "";
        }

        private DecryptorOption GetDecryptorByTypeId(long typeId)
        {
            if (_decryptors == null)
            {
                return GetNoDecryptor();
            }

            return _decryptors.FirstOrDefault(item => item.TypeId == typeId) ?? GetNoDecryptor();
        }

        private static DecryptorOption GetNoDecryptor()
        {
            return new DecryptorOption { TypeId = 0, Name = "None", ProbabilityModifier = 1.0 };
        }

        private void GetEffectiveBlueprintEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe, out double me, out double te)
        {
            GetEffectiveBlueprintEfficiency(blueprint, enteredMe, enteredTe, GetSelectedDecryptor(), out me, out te);
        }

        private void GetEffectiveBlueprintEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe, DecryptorOption decryptor, out double me, out double te)
        {
            _blueprintEfficiencyService.GetEffectiveEfficiency(
                blueprint,
                enteredMe,
                enteredTe,
                decryptor,
                ShouldUseInventionCosts(blueprint),
                out me,
                out te);
        }

        private double GetEffectiveMaterialEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe)
        {
            return GetEffectiveMaterialEfficiency(blueprint, enteredMe, enteredTe, GetSelectedDecryptor());
        }

        private double GetEffectiveMaterialEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe, DecryptorOption decryptor)
        {
            double me;
            double te;
            GetEffectiveBlueprintEfficiency(blueprint, enteredMe, enteredTe, decryptor, out me, out te);
            return me;
        }

        private double GetEffectiveTimeEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe)
        {
            return GetEffectiveTimeEfficiency(blueprint, enteredMe, enteredTe, GetSelectedDecryptor());
        }

        private double GetEffectiveTimeEfficiency(BlueprintSearchResult blueprint, double enteredMe, double enteredTe, DecryptorOption decryptor)
        {
            double me;
            double te;
            GetEffectiveBlueprintEfficiency(blueprint, enteredMe, enteredTe, decryptor, out me, out te);
            return te;
        }

        private int GetBuildWave(string productionType)
        {
            return _blueprintProductionTypeService.GetBuildWave(productionType);
        }

        private double GetMaterialMultiplier(BlueprintSearchResult blueprint, FacilityPreset facilityPreset, double me)
        {
            var station = facilityPreset?.GetBestStationFor(GetProductionTypeForBlueprint(blueprint));
            return GetMaterialMultiplier(blueprint, station, me);
        }

        private double GetMaterialMultiplier(BlueprintSearchResult blueprint, FacilityStation station, double me)
        {
            if (blueprint == null)
            {
                return Math.Max(0, 1 - me / 100.0);
            }

            if (station == null || !station.SupportsProduction)
            {
                return _blueprintManufacturingMathService.CalculateMaterialMultiplier(me, 1);
            }

            var math = _facilityCatalog.Calculate(station, GetSelectedRigIds(station), blueprint.GroupId, blueprint.CategoryId);
            return _blueprintManufacturingMathService.CalculateMaterialMultiplier(me, math.MaterialMultiplier);
        }

        private static FacilityStation FindProjectItemStation(FacilityPreset facilityPreset, BuildProjectItem item)
        {
            if (facilityPreset == null || item == null)
            {
                return null;
            }

            return facilityPreset.Stations.FirstOrDefault(station =>
                       string.Equals(station.Name, item.BestStationName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(station.SystemName, item.BestStationSystem, StringComparison.OrdinalIgnoreCase))
                   ?? facilityPreset.GetBestStationFor(item.ProductionType);
        }

        private void EstimateBlueprints_Click(object sender, RoutedEventArgs e)
        {
            var region = blueprintRegionBox.SelectedItem as MarketRegion;
            if (region == null || _blueprints.Count == 0)
            {
                return;
            }

            var runs = ReadInt(runsBox.Text, 1);
            var me = ReadDouble(meBox.Text, 0);
            var te = ReadDouble(teBox.Text, 0);
            var facility = blueprintFacilityBox.SelectedItem as FacilityPreset;
            if (facility == null)
            {
                MessageBox.Show("РЎРЅР°С‡Р°Р»Р° РґРѕР±Р°РІСЊС‚Рµ РёР»Рё РІС‹Р±РµСЂРёС‚Рµ РїСЂРµСЃРµС‚ СЃС‚Р°РЅС†РёР№ РЅР° РІРєР»Р°РґРєРµ РЎС‚Р°РЅС†РёРё.", "Our IPH", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                EstimateBlueprints(region, runs, me, te, facility);
                ApplyBlueprintFilters(true);
                blueprintsGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("РќРµ СѓРґР°Р»РѕСЃСЊ РѕС†РµРЅРёС‚СЊ С‡РµСЂС‚РµР¶Рё: " + ex.Message, "Our IPH", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EstimateBlueprints(MarketRegion region, int runs, double me, double te, FacilityPreset facilityPreset)
        {
            var materialCache = new Dictionary<long, IReadOnlyList<MaterialRequirement>>();
            var allTypeIds = new HashSet<long>();

            foreach (var blueprint in _blueprints)
            {
                var materials = _database.GetManufacturingMaterials(blueprint.BlueprintTypeId);
                materialCache[blueprint.BlueprintTypeId] = materials;
                foreach (var decryptor in GetBlueprintDecryptorCandidates(blueprint))
                {
                    CollectBlueprintTreeTypeIds(blueprint, allTypeIds, decryptor, new HashSet<long>());
                }
            }

            var id = region.StationId ?? region.RegionId;
            var prices = LoadPricesWithCache(id, region.Name, allTypeIds, region.StationId.HasValue);
            var adjustedPrices = _database.GetAdjustedPrices(allTypeIds);
            var svrDays = ReadInt(svrDaysBox == null ? "7" : svrDaysBox.Text, 7);
            var marketHistory = GetMarketHistoryStats(_blueprints.Select(item => item.ProductTypeId), region.RegionId, svrDays);

            foreach (var blueprint in _blueprints)
            {
                var materials = materialCache[blueprint.BlueprintTypeId];
                var productionType = GetProductionTypeForBlueprint(blueprint);
                var decryptor = GetBestDecryptorForBlueprint(blueprint, productionType, materials, facilityPreset, prices, adjustedPrices, runs, me, te);
                SetBlueprintDecryptor(blueprint, decryptor);
                double effectiveMe;
                double effectiveTe;
                GetEffectiveBlueprintEfficiency(blueprint, me, te, decryptor, out effectiveMe, out effectiveTe);
                var estimateCache = new Dictionary<string, BlueprintEstimate>();
                var bestStation = GetBestStationForBlueprint(facilityPreset, productionType, blueprint, materials, prices, adjustedPrices, runs, effectiveMe, effectiveTe, decryptor, estimateCache);
                if (bestStation != null && !bestStation.SupportsProduction)
                {
                    _blueprintEstimateApplicationService.Apply(blueprint, new BlueprintEstimate(), 0, 0, ContractPriceResult.Empty(""), 0, 0,
                        bestStation.ValidationMessage ?? "Production is not supported by this structure", bestStation);
                    continue;
                }

                if (materials.Count == 0)
                {
                    _blueprintEstimateApplicationService.Apply(blueprint, new BlueprintEstimate(), 0, 0, ContractPriceResult.Empty(""), 0, 0,
                        "РќРµС‚ РјР°С‚РµСЂРёР°Р»РѕРІ", bestStation);
                    continue;
                }

                var estimate = CalculateBlueprintEstimate(blueprint, materials, bestStation, facilityPreset, prices, adjustedPrices, runs, effectiveMe, effectiveTe, decryptor, new HashSet<long>(), estimateCache);
                var missingPrices = 0;
                foreach (var material in materials)
                {
                    MarketPrice price;
                    if (!prices.TryGetValue(material.TypeId, out price) || GetMaterialUnitPrice(price) <= 0)
                    {
                        var adjustedQuantity = CalculateAdjustedQuantity(material.Quantity, runs, GetMaterialMultiplier(blueprint, bestStation, effectiveMe));
                        if (GetOrePlanPurchaseCost(new Dictionary<long, long> { { material.TypeId, adjustedQuantity } }, facilityPreset, prices) > 0)
                        {
                            continue;
                        }

                        missingPrices++;
                        continue;
                    }
                }

                MarketPrice productPrice;
                prices.TryGetValue(blueprint.ProductTypeId, out productPrice);
                var productPriceResult = GetEffectiveProductPrice(blueprint, productPrice);
                var sellPrice = productPriceResult.UnitPrice;
                var produced = Math.Max(1, blueprint.PortionSize) * runs;
                var revenue = _salesFeeService.ApplySalesTaxesAndFees(sellPrice * produced, facilityPreset, bestStation);
                var totalCost = estimate.MaterialCost + estimate.InventionCost + estimate.InstallationCost;
                var status = _blueprintEstimateStatusService.GetBlueprintStatus(estimate, missingPrices);
                status = _blueprintCopyStatusService.AddCopyStatus(blueprint, runs, status);
                _blueprintEstimateApplicationService.Apply(blueprint, estimate, totalCost, revenue, productPriceResult, produced,
                    GetProductMarketVolume(productPrice), status, bestStation);
                _salesVolumeRatioService.ApplyToBlueprint(blueprint, marketHistory, produced, estimate.ProductionTimeSeconds);
            }

            SetBlueprintRanks();
        }

        private void SetBlueprintRanks()
        {
            _blueprintRankingService.AssignRanks(_blueprints, hideRareNoiseBox == null || hideRareNoiseBox.IsChecked == true);
        }

        private FacilityStation GetBestStationForBlueprint(FacilityPreset facilityPreset, string productionType, BlueprintSearchResult blueprint,
            IReadOnlyList<MaterialRequirement> materials, Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices, int runs, double me, double te,
            DecryptorOption decryptor, Dictionary<string, BlueprintEstimate> estimateCache)
        {
            var candidates = _blueprintEstimateSelectionService.GetSupportedStationCandidates(facilityPreset, productionType);
            if (candidates.Count == 0)
            {
                return facilityPreset.GetBestStationFor(productionType);
            }

            return _blueprintEstimateSelectionService.SelectCheapestStation(candidates
                .Select(station => new BlueprintStationEstimateCandidate
                {
                    Station = station,
                    Estimate = CalculateBlueprintEstimate(blueprint, materials, station, facilityPreset, prices, adjustedPrices, runs, me, te, decryptor, new HashSet<long>(), estimateCache)
                }));
        }

        private DecryptorOption GetBestDecryptorForBlueprint(BlueprintSearchResult blueprint, string productionType, IReadOnlyList<MaterialRequirement> materials,
            FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices, int runs, double enteredMe, double enteredTe)
        {
            var candidates = GetBlueprintDecryptorCandidates(blueprint);
            if (candidates.Count <= 1)
            {
                return candidates.FirstOrDefault() ?? GetNoDecryptor();
            }

            var decryptorCandidates = new List<BlueprintDecryptorEstimateCandidate>();
            foreach (var decryptor in candidates)
            {
                double effectiveMe;
                double effectiveTe;
                GetEffectiveBlueprintEfficiency(blueprint, enteredMe, enteredTe, decryptor, out effectiveMe, out effectiveTe);
                var estimateCache = new Dictionary<string, BlueprintEstimate>();
                var station = GetBestStationForBlueprint(facilityPreset, productionType, blueprint, materials, prices, adjustedPrices,
                    runs, effectiveMe, effectiveTe, decryptor, estimateCache);
                if (station != null && !station.SupportsProduction)
                {
                    continue;
                }

                var estimate = CalculateBlueprintEstimate(blueprint, materials, station, facilityPreset, prices, adjustedPrices,
                    runs, effectiveMe, effectiveTe, decryptor, new HashSet<long>(), estimateCache);
                MarketPrice productPrice;
                prices.TryGetValue(blueprint.ProductTypeId, out productPrice);
                double contractPrice;
                string productPriceSource;
                var sellPrice = GetEffectiveProductUnitPrice(blueprint, productPrice, out contractPrice, out productPriceSource);
                var revenue = _salesFeeService.ApplySalesTaxesAndFees(sellPrice * Math.Max(1, blueprint.PortionSize) * runs, facilityPreset, station);
                var profit = revenue - estimate.MaterialCost - estimate.InventionCost - estimate.InstallationCost;
                decryptorCandidates.Add(new BlueprintDecryptorEstimateCandidate
                {
                    Decryptor = decryptor,
                    Profit = profit
                });
            }

            return _blueprintEstimateSelectionService.SelectBestDecryptor(decryptorCandidates, candidates.FirstOrDefault() ?? GetNoDecryptor());
        }

        private void CollectBlueprintTreeTypeIds(BlueprintSearchResult blueprint, HashSet<long> typeIds, DecryptorOption decryptor, HashSet<long> path)
        {
            if (blueprint == null || path.Contains(blueprint.ProductTypeId))
            {
                return;
            }

            path.Add(blueprint.ProductTypeId);
            typeIds.Add(blueprint.ProductTypeId);
            AddInventionTypeIds(blueprint, typeIds, decryptor);
            foreach (var material in _database.GetManufacturingMaterials(blueprint.BlueprintTypeId))
            {
                typeIds.Add(material.TypeId);
                AddReprocessingOreTypeIds(material.TypeId, typeIds);
                var childBlueprint = _database.FindBlueprintByProduct(material.TypeId);
                if (childBlueprint != null && !ShouldStopReactionDrilldown(blueprint, childBlueprint))
                {
                    CollectBlueprintTreeTypeIds(childBlueprint, typeIds, decryptor, path);
                }
            }

            path.Remove(blueprint.ProductTypeId);
        }

        private void AddInventionTypeIds(BlueprintSearchResult blueprint, HashSet<long> typeIds, DecryptorOption decryptor)
        {
            if (!ShouldUseInventionCosts(blueprint))
            {
                return;
            }

            var invention = _database.GetInventionInfo(blueprint.BlueprintTypeId);
            if (invention == null)
            {
                return;
            }

            if (invention.SourceBlueprintTypeId > 0)
            {
                typeIds.Add(invention.SourceBlueprintTypeId);
            }

            foreach (var material in invention.Materials)
            {
                typeIds.Add(material.TypeId);
            }

            foreach (var material in invention.CopyMaterials)
            {
                typeIds.Add(material.TypeId);
            }

            decryptor = decryptor ?? GetNoDecryptor();
            if (decryptor.TypeId > 0)
            {
                typeIds.Add(decryptor.TypeId);
            }
        }

        private void AddReprocessingOreTypeIds(long materialTypeId, HashSet<long> typeIds)
        {
            var facilityPreset = blueprintFacilityBox == null ? null : blueprintFacilityBox.SelectedItem as FacilityPreset;
            if (facilityPreset == null || !facilityPreset.ConvertMineralsToOre || !_database.IsMineral(materialTypeId))
            {
                return;
            }

            foreach (var option in _database.GetReprocessingOptionsForMineral(materialTypeId, facilityPreset.PreferCompressedOre))
            {
                typeIds.Add(option.OreTypeId);
            }
        }

        private FacilityStation GetCheapestStationForBlueprint(FacilityPreset facilityPreset, BlueprintSearchResult blueprint,
            Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices, int runs, double me, double te,
            DecryptorOption decryptor, HashSet<long> path, Dictionary<string, BlueprintEstimate> estimateCache)
        {
            var productionType = GetProductionTypeForBlueprint(blueprint);
            var candidates = facilityPreset.Stations
                .Where(station => station.ProductionType == productionType && station.SupportsProduction)
                .ToList();
            if (candidates.Count == 0)
            {
                candidates = facilityPreset.Stations.Where(station => station.SupportsProduction).ToList();
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            var materials = _database.GetManufacturingMaterials(blueprint.BlueprintTypeId);
            return candidates
                .Select(station => new
                {
                    Station = station,
                    Estimate = CalculateBlueprintEstimate(blueprint, materials, station, facilityPreset, prices, adjustedPrices, runs, me, te, decryptor, new HashSet<long>(path), estimateCache)
                })
                .OrderBy(item => item.Estimate.MaterialCost + item.Estimate.InventionCost + item.Estimate.InstallationCost)
                .ThenBy(item => item.Estimate.ProductionTimeSeconds)
                .Select(item => item.Station)
                .FirstOrDefault();
        }

        private BlueprintEstimate CalculateBlueprintEstimate(BlueprintSearchResult blueprint, IReadOnlyList<MaterialRequirement> materials,
            FacilityStation station, FacilityPreset facilityPreset, Dictionary<long, MarketPrice> prices, Dictionary<long, double> adjustedPrices,
            int runs, double me, double te, DecryptorOption decryptor, HashSet<long> path, Dictionary<string, BlueprintEstimate> estimateCache)
        {
            var cacheKey = _blueprintEstimateCacheService.CreateKey(blueprint, runs, me, te, decryptor, station);
            BlueprintEstimate cachedEstimate;
            if (_blueprintEstimateCacheService.TryGet(estimateCache, cacheKey, out cachedEstimate))
            {
                return cachedEstimate;
            }

            var stationMath = _facilityCatalog.Calculate(station, GetSelectedRigIds(station), blueprint.GroupId, blueprint.CategoryId);
            var industrySkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.IndustrySkillLevel);
            var advancedIndustrySkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.AdvancedIndustrySkillLevel);
            var sccIndustryFeeRate = Math.Max(0, (facilityPreset == null ? 4.0 : facilityPreset.SccIndustryFeePercent) / 100.0);
            var alphaAccountTaxRate = Math.Max(0, (facilityPreset == null ? 0 : facilityPreset.AlphaAccountTaxPercent) / 100.0);
            var facilityBonus = _blueprintManufacturingMathService.CalculateFacilityMaterialBonusPercent(stationMath.MaterialMultiplier);
            var materialMultiplier = _blueprintManufacturingMathService.CalculateMaterialMultiplier(me, stationMath.MaterialMultiplier);
            var timeMultiplier = _blueprintManufacturingMathService.CalculateTimeMultiplier(te, stationMath.TimeMultiplier,
                facilityPreset == null ? 0 : facilityPreset.ManufacturingImplantPercent,
                industrySkill,
                advancedIndustrySkill,
                GetAdvancedManufacturingSkillMultiplier(blueprint.BlueprintTypeId));
            var materialContext = _blueprintEstimateMaterialOrchestrationService.CreateContext(
                blueprint,
                materials,
                station,
                facilityPreset,
                prices,
                adjustedPrices,
                runs,
                me,
                te,
                materialMultiplier,
                decryptor,
                new BlueprintEstimateRecursionState(path, estimateCache));
            var materialDependencies = _blueprintEstimateMaterialOrchestrationService.CreateDependencies(
                typeId => _database.FindBlueprintByProduct(typeId),
                typeId => _database.IsMineral(typeId),
                ShouldAlwaysBuy,
                ShouldStopReactionDrilldown,
                (childBlueprint, preset, selectedDecryptor) =>
                {
                    double childMe;
                    double childTe;
                    GetDefaultEfficiencyForChildBlueprint(childBlueprint, preset, selectedDecryptor, out childMe, out childTe);
                    return Tuple.Create(childMe, childTe);
                },
                request => GetCheapestStationForBlueprint(
                    facilityPreset,
                    request.ChildBlueprint,
                    prices,
                    adjustedPrices,
                    request.ChildRuns,
                    request.MaterialEfficiency,
                    request.TimeEfficiency,
                    request.Decryptor,
                    path,
                    estimateCache),
                (request, childStation) => CalculateBlueprintEstimate(
                    request.ChildBlueprint,
                    _database.GetManufacturingMaterials(request.ChildBlueprint.BlueprintTypeId),
                    childStation,
                    facilityPreset,
                    prices,
                    adjustedPrices,
                    request.ChildRuns,
                    request.MaterialEfficiency,
                    request.TimeEfficiency,
                    request.Decryptor,
                    path,
                    estimateCache),
                GetMaterialUnitPrice,
                GetProductUnitPrice,
                GetAvailableMarketVolume,
                (grossValue, preset, childStation) => _salesFeeService.ApplySalesTaxesAndFees(grossValue, preset, childStation));
            var materialResult = _blueprintEstimateMaterialOrchestrationService.TraverseMaterials(
                materialContext,
                materialDependencies,
                mineralQuantities => GetMineralPurchaseCost(mineralQuantities, facilityPreset, prices));

            bool inventionMissing;
            double inventionTime;
            InventionPlan inventionPlan;
            var inventionCost = CalculateInventionCost(blueprint, prices, runs, facilityPreset, station, materialResult.EstimatedInputValue, decryptor,
                out inventionTime, out inventionMissing, out inventionPlan);

            var costIndex = _facilityCatalog.GetSystemCostIndex(station.SolarSystemId, station.ProductionType);
            var taxRate = station.IndustryTaxPercent / 100.0;
            var factionWarfareMultiplier = _blueprintProductionTypeService.GetFactionWarfareCostMultiplier(station.FactionWarfareUpgradeLevel);
            var installationCost = _blueprintManufacturingMathService.CalculateInstallationCost(materialResult.EstimatedInputValue, costIndex,
                stationMath.CostMultiplier, factionWarfareMultiplier, taxRate, sccIndustryFeeRate, alphaAccountTaxRate);

            var estimate = _blueprintEstimateResultAssembler.Assemble(new BlueprintEstimateAssemblyContext
            {
                Blueprint = blueprint,
                FacilityPreset = facilityPreset,
                InventionPlan = inventionPlan,
                ComponentProductionTimes = materialResult.ComponentProductionTimes,
                Runs = runs,
                TimeMultiplier = timeMultiplier,
                InventionTimeSeconds = inventionTime,
                MaterialCost = materialResult.MaterialCost,
                InstallationCost = installationCost,
                FacilityMaterialBonusPercent = facilityBonus,
                CostIndexPercent = costIndex * 100,
                InventionCost = inventionCost,
                InventionMissing = inventionMissing,
                BuildMaterialLines = materialResult.BuildMaterialLines,
                BuyMaterialLines = materialResult.BuyMaterialLines
            });

            _blueprintEstimateCacheService.Store(estimateCache, cacheKey, estimate);
            return estimate;
        }

        private double CalculateInventionCost(BlueprintSearchResult blueprint, Dictionary<long, MarketPrice> prices, int manufacturingRuns,
            FacilityPreset facilityPreset, FacilityStation fallbackStation, double estimatedInputValue, DecryptorOption decryptor,
            out double extraTimeSeconds, out bool missing, out InventionPlan plan)
        {
            extraTimeSeconds = 0;
            missing = false;
            plan = new InventionPlan();
            if (!ShouldUseInventionCosts(blueprint))
            {
                return 0;
            }

            var invention = _database.GetInventionInfo(blueprint.BlueprintTypeId);
            if (invention == null || invention.Probability <= 0)
            {
                missing = true;
                return 0;
            }

            decryptor = decryptor ?? GetNoDecryptor();
            plan = CreateInventionPlan(blueprint, invention, manufacturingRuns, facilityPreset, decryptor);
            if (plan.Chance <= 0)
            {
                missing = true;
                return 0;
            }

            var inventionJobs = plan.Jobs;
            var totalInventedRuns = Math.Max(1, plan.SuccessfulJobsNeeded * plan.RunsPerSuccess);
            var cost = 0.0;
            var inventionMaterialCosts = _blueprintInventionCostService.CalculateMaterialCosts(new InventionCostContext
            {
                Invention = invention,
                Plan = plan,
                ManufacturingRuns = manufacturingRuns,
                TechLevel = blueprint.TechLevel,
                DecryptorTypeId = decryptor.TypeId,
                GetUnitPrice = typeId =>
                {
                    MarketPrice price;
                    return prices.TryGetValue(typeId, out price) ? GetMaterialUnitPrice(price) : 0;
                }
            });
            plan.SourceCost += inventionMaterialCosts.SourceCost;
            plan.MaterialCost += inventionMaterialCosts.MaterialCost;
            plan.DecryptorCost += inventionMaterialCosts.DecryptorCost;
            plan.CopyMaterialCost = inventionMaterialCosts.CopyMaterialCost;
            missing = missing || inventionMaterialCosts.MissingPrice;
            cost += inventionMaterialCosts.TotalCost;

            var advancedIndustrySkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.AdvancedIndustrySkillLevel);
            var scienceSkill = ClampSkill(facilityPreset == null ? 5 : facilityPreset.ScienceSkillLevel);
            var inventionStation = _facilityActivityStationService.ResolveActivityStation(facilityPreset, fallbackStation, "Invention");
            var copyStation = _facilityActivityStationService.ResolveActivityStation(facilityPreset, fallbackStation, "Copying");
            plan.InventionStationName = inventionStation == null ? "" : inventionStation.Name;
            plan.InventionStationSystem = inventionStation == null ? "" : inventionStation.SystemName;
            plan.CopyStationName = copyStation == null ? "" : copyStation.Name;
            plan.CopyStationSystem = copyStation == null ? "" : copyStation.SystemName;
            var inventionMath = inventionStation == null
                ? new FacilityMathResult { MaterialMultiplier = 1, TimeMultiplier = 1, CostMultiplier = 1 }
                : _facilityCatalog.Calculate(inventionStation, GetSelectedRigIds(inventionStation), blueprint.GroupId, blueprint.CategoryId);
            var copyMath = copyStation == null
                ? new FacilityMathResult { MaterialMultiplier = 1, TimeMultiplier = 1, CostMultiplier = 1 }
                : _facilityCatalog.Calculate(copyStation, GetSelectedRigIds(copyStation), blueprint.GroupId, blueprint.CategoryId);
            var inventionTimes = _blueprintInventionTimeService.CalculateScienceTimes(new InventionTimeContext
            {
                Invention = invention,
                Plan = plan,
                TechLevel = blueprint.TechLevel,
                AdvancedIndustrySkillLevel = advancedIndustrySkill,
                ScienceSkillLevel = scienceSkill,
                LaboratoryLines = facilityPreset == null ? 10 : facilityPreset.LaboratoryLines,
                CopyTimeMultiplier = copyMath.TimeMultiplier,
                InventionTimeMultiplier = inventionMath.TimeMultiplier
            });
            plan.CopyTimeSeconds = inventionTimes.CopyTimeSeconds;
            plan.InventionTimeSeconds = inventionTimes.InventionTimeSeconds;
            extraTimeSeconds += inventionTimes.ExtraTimeSeconds;

            var usageEiv = Math.Max(0, estimatedInputValue);
            if (usageEiv > 0)
            {
                var inventionUsage = CalculateScienceJobUsage(usageEiv, inventionJobs, totalInventedRuns, manufacturingRuns,
                    facilityPreset, fallbackStation, GetInventionProductionTypeForBlueprint(blueprint), blueprint.GroupId, blueprint.CategoryId);
                var copyUsage = blueprint.TechLevel == 3
                    ? 0
                    : CalculateScienceJobUsage(usageEiv, inventionJobs, totalInventedRuns, manufacturingRuns,
                        facilityPreset, fallbackStation, "Copying", blueprint.GroupId, blueprint.CategoryId);
                plan.InventionUsageCost = inventionUsage;
                plan.CopyUsageCost = copyUsage;
                cost += plan.InventionUsageCost + plan.CopyUsageCost;
            }

            return cost;
        }

        private static InventionPlan CreateInventionPlan(BlueprintSearchResult blueprint, InventionInfo invention, int manufacturingRuns,
            FacilityPreset facilityPreset, DecryptorOption decryptor)
        {
            return BlueprintInventionMathService.CreatePlan(blueprint, invention, manufacturingRuns, facilityPreset, decryptor);
        }

        private static double GetInventionChance(double baseProbability, DecryptorOption decryptor, FacilityPreset facilityPreset)
        {
            return BlueprintInventionMathService.CalculateChance(baseProbability, decryptor, facilityPreset);
        }

        private double CalculateScienceJobUsage(double estimatedInputValue, int jobs, int totalInventedRuns, int requestedRuns,
            FacilityPreset facilityPreset, FacilityStation fallbackStation, string productionType, int itemGroupId, int itemCategoryId)
        {
            if (jobs <= 0 || totalInventedRuns <= 0 || requestedRuns <= 0)
            {
                return 0;
            }

            var station = _facilityActivityStationService.ResolveActivityStation(facilityPreset, fallbackStation, productionType);
            if (station == null)
            {
                return 0;
            }

            var math = _facilityCatalog.Calculate(station, GetSelectedRigIds(station), itemGroupId, itemCategoryId);
            var costIndex = _facilityCatalog.GetSystemCostIndex(station.SolarSystemId, productionType);
            var factionWarfareMultiplier = _blueprintProductionTypeService.GetFactionWarfareCostMultiplier(station.FactionWarfareUpgradeLevel);
            return _blueprintInventionCostService.CalculateUsageCost(new InventionUsageCostContext
            {
                EstimatedInputValue = estimatedInputValue,
                Jobs = jobs,
                TotalInventedRuns = totalInventedRuns,
                RequestedRuns = requestedRuns,
                CostIndex = costIndex,
                FactionWarfareMultiplier = factionWarfareMultiplier,
                FacilityCostMultiplier = math.CostMultiplier,
                IndustryTaxPercent = station.IndustryTaxPercent
            }).UsageCost;
        }

        private string GetInventionProductionTypeForBlueprint(BlueprintSearchResult blueprint)
        {
            return _blueprintProductionTypeService.GetInventionProductionType(blueprint);
        }

        private bool ShouldUseInventionCosts(BlueprintSearchResult blueprint)
        {
            if (blueprint == null || blueprint.TechLevel < 2)
            {
                return false;
            }

            int ownedMe;
            int ownedTe;
            return !_database.TryGetOwnedBlueprintEfficiency(blueprint.BlueprintTypeId, out ownedMe, out ownedTe);
        }

        private string GetProductionTypeForBlueprint(BlueprintSearchResult blueprint)
        {
            return _blueprintProductionTypeService.GetProductionType(blueprint);
        }

        private bool ShouldStopReactionDrilldown(BlueprintSearchResult parentBlueprint, BlueprintSearchResult childBlueprint)
        {
            if (parentBlueprint == null || childBlueprint == null || !IsReactionGroup(childBlueprint.GroupId))
            {
                return false;
            }

            var depth = GetReactionDepth();
            if (depth == "Raw")
            {
                return false;
            }

            if (depth == "Advanced")
            {
                return true;
            }

            if (childBlueprint.GroupId == 428 || childBlueprint.GroupId == 974)
            {
                return true;
            }

            var parentName = parentBlueprint.ProductName ?? parentBlueprint.BlueprintName ?? "";
            if ((parentName.IndexOf("Standard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 parentName.IndexOf("Synth", StringComparison.OrdinalIgnoreCase) >= 0) &&
                childBlueprint.GroupId != 712)
            {
                return true;
            }

            return false;
        }

        private string GetReactionDepth()
        {
            var item = reactionDepthBox == null ? null : reactionDepthBox.SelectedItem as ComboBoxItem;
            return item == null ? "Advanced" : item.Content.ToString();
        }

        private static bool IsReactionGroup(int groupId)
        {
            return groupId == 428 || groupId == 429 || groupId == 974 || groupId == 712 || groupId == 4096;
        }

        private double GetAdvancedManufacturingSkillMultiplier(long blueprintTypeId)
        {
            var multiplier = 1.0;
            foreach (var skill in _database.GetRequiredSkills(blueprintTypeId, 1))
            {
                switch (skill.TypeId)
                {
                    case 3398:
                    case 3397:
                    case 3395:
                    case 11444:
                    case 11454:
                    case 11448:
                    case 11453:
                    case 11450:
                    case 11446:
                    case 11433:
                    case 11443:
                    case 11447:
                    case 11452:
                    case 11445:
                    case 11529:
                    case 11451:
                    case 11441:
                    case 11455:
                    case 11449:
                    case 81050:
                    case 3400:
                        multiplier *= 1 - 0.01 * skill.Level;
                        break;
                    case 81896:
                        multiplier *= 1 - 0.02 * skill.Level;
                        break;
                }
            }

            return Math.Max(0, multiplier);
        }

        private void GetDefaultEfficiencyForChildBlueprint(BlueprintSearchResult blueprint, FacilityPreset facilityPreset, DecryptorOption decryptor, out double me, out double te)
        {
            int ownedMe = 0;
            int ownedTe = 0;
            var hasOwnedEfficiency = blueprint != null && _database.TryGetOwnedBlueprintEfficiency(blueprint.BlueprintTypeId, out ownedMe, out ownedTe);
            _blueprintEfficiencyService.GetDefaultChildEfficiency(
                blueprint,
                facilityPreset,
                decryptor,
                hasOwnedEfficiency,
                ownedMe,
                ownedTe,
                out me,
                out te);
        }

        private static long CalculateAdjustedQuantity(long baseQuantity, int runs, double materialMultiplier)
        {
            return BlueprintMaterialMathService.CalculateAdjustedQuantity(baseQuantity, runs, materialMultiplier);
        }

        private static int ReadInt(string text, int defaultValue)
        {
            int value;
            return int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value) ? Math.Max(1, value) : defaultValue;
        }

        private static int ClampSkill(int value)
        {
            return Math.Max(0, Math.Min(5, value));
        }

        private static int ReadSkill(string text, int defaultValue)
        {
            int value;
            return int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value)
                ? ClampSkill(value)
                : ClampSkill(defaultValue);
        }

        private static long ReadLong(string text, long defaultValue)
        {
            long value;
            return long.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value) ? Math.Max(0, value) : defaultValue;
        }

        private static int ReadNonNegativeInt(string text, int defaultValue)
        {
            int value;
            return int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value) ? Math.Max(0, value) : Math.Max(0, defaultValue);
        }

        private static double ReadDouble(string text, double defaultValue)
        {
            double value;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                return value;
            }

            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value) ? value : defaultValue;
        }

        private void FacilityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var preset = facilityList.SelectedItem as FacilityPreset;
            if (preset == null)
            {
                return;
            }

            facilityNameBox.Text = preset.Name;
            facilityIndustrySkillBox.Text = preset.IndustrySkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityAdvancedIndustrySkillBox.Text = preset.AdvancedIndustrySkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityManufacturingImplantBox.Text = preset.ManufacturingImplantPercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilitySccIndustryFeeBox.Text = preset.SccIndustryFeePercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilityAccountingSkillBox.Text = preset.AccountingSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityBrokerRelationsBox.Text = preset.BrokerRelationsSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityBrokerFactionStandingBox.Text = preset.BrokerFactionStanding.ToString("0.##", CultureInfo.CurrentCulture);
            facilityBrokerCorpStandingBox.Text = preset.BrokerCorpStanding.ToString("0.##", CultureInfo.CurrentCulture);
            facilityAlphaTaxBox.Text = preset.AlphaAccountTaxPercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilityBaseSalesTaxBox.Text = preset.BaseSalesTaxPercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilityBaseBrokerFeeBox.Text = preset.BaseBrokerFeePercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilitySccBrokerFeeBox.Text = preset.SccBrokerFeeSurchargePercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilitySpecialBrokerFeeBox.Text = preset.SpecialBrokerFeePercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilityBrokerModeBox.SelectedIndex = Math.Max(0, Math.Min(2, preset.BrokerFeeMode));
            facilityIncludeSalesTaxBox.IsChecked = preset.IncludeSalesTax;
            facilityBuyOrderBrokerFeeBox.IsChecked = preset.IncludeBuyOrderBrokerFee;
            facilityDefaultBpMeBox.Text = preset.DefaultBlueprintMe.ToString(CultureInfo.CurrentCulture);
            facilityDefaultBpTeBox.Text = preset.DefaultBlueprintTe.ToString(CultureInfo.CurrentCulture);
            facilityProductionLinesBox.Text = Math.Max(1, preset.ProductionLines).ToString(CultureInfo.CurrentCulture);
            facilityLaboratoryLinesBox.Text = Math.Max(1, preset.LaboratoryLines).ToString(CultureInfo.CurrentCulture);
            facilitySuggestBuildUnownedBox.IsChecked = preset.SuggestBuildBlueprintsNotOwned;
            facilityBuildWhenMarketShortBox.IsChecked = preset.BuildWhenMarketVolumeShort;
            facilityConvertMineralsToOreBox.IsChecked = preset.ConvertMineralsToOre;
            facilityPreferCompressedOreBox.IsChecked = preset.PreferCompressedOre;
            facilityRefiningSkillBox.Text = preset.RefiningSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityReprocessingSkillBox.Text = preset.ReprocessingSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityOreProcessingSkillBox.Text = preset.OreProcessingSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityReprocessingImplantBox.Text = preset.ReprocessingImplantPercent.ToString("0.##", CultureInfo.CurrentCulture);
            facilityEncryptionSkillBox.Text = preset.EncryptionSkillLevel.ToString(CultureInfo.CurrentCulture);
            facilityDatacoreSkill1Box.Text = preset.DatacoreSkill1Level.ToString(CultureInfo.CurrentCulture);
            facilityDatacoreSkill2Box.Text = preset.DatacoreSkill2Level.ToString(CultureInfo.CurrentCulture);
            facilityScienceSkillBox.Text = preset.ScienceSkillLevel.ToString(CultureInfo.CurrentCulture);
            stationList.ItemsSource = preset.Stations;
            if (preset.Stations.Count == 0)
            {
                preset.Stations.Add(FacilityStation.CreateDefault());
            }
            stationList.SelectedIndex = 0;
        }

        private void StationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var station = stationList.SelectedItem as FacilityStation;
            if (station == null)
            {
                return;
            }

            _loadingFacilitySelection = true;
            try
            {
                SelectRegionAndSystem(station);
                facilityNameDetailBox.Text = station.Name;
                SelectStructure(station);
                SetComboText(facilityProductionTypeBox, station.ProductionType);
                RefreshServiceModuleOptions(station);
                SelectServiceModules(station);
                RefreshRigOptions(station);
                SelectRig(facilityRigPresetBox, station.RigSlot1TypeId);
                SelectRig(facilityRigSlot2Box, station.RigSlot2TypeId);
                SelectRig(facilityRigSlot3Box, station.RigSlot3TypeId);
                facilityMaterialBonusBox.Text = FormatFacilityMath(station);
                facilityIndustryTaxBox.Text = station.IndustryTaxPercent.ToString("0.##", CultureInfo.CurrentCulture);
                facilitySalesFeeBox.Text = station.SalesFeePercent.ToString("0.##", CultureInfo.CurrentCulture);
                facilityFwUpgradeBox.Text = station.FactionWarfareUpgradeLevel.ToString(CultureInfo.CurrentCulture);
            }
            finally
            {
                _loadingFacilitySelection = false;
            }

            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);
            facilityStatus.Text = station.SupportsProduction ? "" : station.ValidationMessage;
        }

        private void NewFacility_Click(object sender, RoutedEventArgs e)
        {
            var preset = new FacilityPreset
            {
                Name = "New preset"
            };
            preset.Stations.Add(FacilityStation.CreateDefault());

            _facilities.Add(preset);
            facilityList.SelectedItem = preset;
            blueprintFacilityBox.SelectedItem = preset;
            _facilityStore.Save(_facilities);
        }

        private void NewStation_Click(object sender, RoutedEventArgs e)
        {
            var preset = facilityList.SelectedItem as FacilityPreset;
            if (preset == null)
            {
                return;
            }

            var station = FacilityStation.CreateDefault();
            station.Name = "New station";
            preset.Stations.Add(station);
            stationList.Items.Refresh();
            stationList.SelectedItem = station;
            _facilityStore.Save(_facilities);
        }

        private void DeleteStation_Click(object sender, RoutedEventArgs e)
        {
            var preset = facilityList.SelectedItem as FacilityPreset;
            var station = stationList.SelectedItem as FacilityStation;
            if (preset == null || station == null)
            {
                return;
            }

            preset.Stations.Remove(station);
            if (preset.Stations.Count == 0)
            {
                preset.Stations.Add(FacilityStation.CreateDefault());
            }

            stationList.Items.Refresh();
            stationList.SelectedIndex = 0;
            _facilityStore.Save(_facilities);
        }

        private void DeleteFacility_Click(object sender, RoutedEventArgs e)
        {
            var preset = facilityList.SelectedItem as FacilityPreset;
            if (preset == null)
            {
                return;
            }

            _facilities.Remove(preset);
            _facilityStore.Save(_facilities);
            if (_facilities.Count > 0)
            {
                facilityList.SelectedIndex = 0;
                blueprintFacilityBox.SelectedIndex = 0;
            }
        }

        private void SaveFacility_Click(object sender, RoutedEventArgs e)
        {
            var preset = facilityList.SelectedItem as FacilityPreset;
            var station = stationList.SelectedItem as FacilityStation;
            if (preset == null || station == null)
            {
                return;
            }

            var structure = facilityTypeBox.SelectedItem as FacilityStructureType;
            preset.Name = facilityNameBox.Text.Trim();
            preset.IndustrySkillLevel = ReadSkill(facilityIndustrySkillBox.Text, 5);
            preset.AdvancedIndustrySkillLevel = ReadSkill(facilityAdvancedIndustrySkillBox.Text, 5);
            preset.ManufacturingImplantPercent = Math.Max(0, ReadDouble(facilityManufacturingImplantBox.Text, 0));
            preset.SccIndustryFeePercent = Math.Max(0, ReadDouble(facilitySccIndustryFeeBox.Text, 4.0));
            preset.AccountingSkillLevel = ReadSkill(facilityAccountingSkillBox.Text, 5);
            preset.BrokerRelationsSkillLevel = ReadSkill(facilityBrokerRelationsBox.Text, 5);
            preset.BrokerFactionStanding = ReadDouble(facilityBrokerFactionStandingBox.Text, 5);
            preset.BrokerCorpStanding = ReadDouble(facilityBrokerCorpStandingBox.Text, 5);
            preset.AlphaAccountTaxPercent = Math.Max(0, ReadDouble(facilityAlphaTaxBox.Text, 0));
            preset.BaseSalesTaxPercent = Math.Max(0, ReadDouble(facilityBaseSalesTaxBox.Text, 4.5));
            preset.BaseBrokerFeePercent = Math.Max(0, ReadDouble(facilityBaseBrokerFeeBox.Text, 3.0));
            preset.SccBrokerFeeSurchargePercent = Math.Max(0, ReadDouble(facilitySccBrokerFeeBox.Text, 0.5));
            preset.SpecialBrokerFeePercent = Math.Max(0, ReadDouble(facilitySpecialBrokerFeeBox.Text, 0));
            preset.BrokerFeeMode = Math.Max(0, Math.Min(2, facilityBrokerModeBox.SelectedIndex));
            preset.IncludeSalesTax = facilityIncludeSalesTaxBox.IsChecked == true;
            preset.IncludeBuyOrderBrokerFee = facilityBuyOrderBrokerFeeBox.IsChecked == true;
            preset.DefaultBlueprintMe = Math.Max(0, ReadNonNegativeInt(facilityDefaultBpMeBox.Text, 10));
            preset.DefaultBlueprintTe = Math.Max(0, ReadNonNegativeInt(facilityDefaultBpTeBox.Text, 20));
            preset.ProductionLines = Math.Max(1, ReadNonNegativeInt(facilityProductionLinesBox.Text, 10));
            preset.LaboratoryLines = Math.Max(1, ReadNonNegativeInt(facilityLaboratoryLinesBox.Text, 10));
            preset.SuggestBuildBlueprintsNotOwned = facilitySuggestBuildUnownedBox.IsChecked == true;
            preset.BuildWhenMarketVolumeShort = facilityBuildWhenMarketShortBox.IsChecked == true;
            preset.ConvertMineralsToOre = facilityConvertMineralsToOreBox.IsChecked == true;
            preset.PreferCompressedOre = facilityPreferCompressedOreBox.IsChecked == true;
            preset.RefiningSkillLevel = ReadSkill(facilityRefiningSkillBox.Text, 5);
            preset.ReprocessingSkillLevel = ReadSkill(facilityReprocessingSkillBox.Text, 5);
            preset.OreProcessingSkillLevel = ReadSkill(facilityOreProcessingSkillBox.Text, 4);
            preset.ReprocessingImplantPercent = Math.Max(0, ReadDouble(facilityReprocessingImplantBox.Text, 0));
            preset.EncryptionSkillLevel = ReadSkill(facilityEncryptionSkillBox.Text, 4);
            preset.DatacoreSkill1Level = ReadSkill(facilityDatacoreSkill1Box.Text, 4);
            preset.DatacoreSkill2Level = ReadSkill(facilityDatacoreSkill2Box.Text, 4);
            preset.ScienceSkillLevel = ReadSkill(facilityScienceSkillBox.Text, 5);
            var selectedSystem = facilitySystemBox.SelectedItem as EveSolarSystem;
            var selectedRegion = facilityRegionBox.SelectedItem as EveRegion;
            if (selectedSystem != null)
            {
                station.SystemName = selectedSystem.Name;
                station.SolarSystemId = selectedSystem.SolarSystemId;
                station.RegionId = selectedSystem.RegionId;
                station.Security = selectedSystem.Security;
            }
            else
            {
                station.SystemName = facilitySystemBox.Text.Trim();
                var foundSystem = _facilityCatalog.FindSystem(station.SystemName);
                if (foundSystem != null)
                {
                    station.SystemName = foundSystem.Name;
                    station.SolarSystemId = foundSystem.SolarSystemId;
                    station.RegionId = foundSystem.RegionId;
                    station.Security = foundSystem.Security;
                }
                else
                {
                    station.RegionId = selectedRegion != null ? selectedRegion.RegionId : station.RegionId;
                    station.Security = _facilityCatalog.GetSystemSecurity(station.SystemName);
                }
            }
            station.Name = facilityNameDetailBox.Text.Trim();
            station.FacilityType = structure != null ? structure.Name : GetComboText(facilityTypeBox);
            station.StructureTypeId = structure != null ? structure.TypeId : 0;
            station.ProductionType = GetComboText(facilityProductionTypeBox);
            station.RigSlot1TypeId = GetSelectedRigId(facilityRigPresetBox);
            station.RigSlot2TypeId = GetSelectedRigId(facilityRigSlot2Box);
            station.RigSlot3TypeId = GetSelectedRigId(facilityRigSlot3Box);
            SaveSelectedServiceModules(station);

            var math = _facilityCatalog.Calculate(station, GetSelectedRigIds(station));
            station.MaterialMultiplier = math.MaterialMultiplier;
            station.TimeMultiplier = math.TimeMultiplier;
            station.CostMultiplier = math.CostMultiplier;
            station.MaterialBonusPercent = math.MaterialBonusPercent;
            station.IndustryTaxPercent = ReadDouble(facilityIndustryTaxBox.Text, 0);
            station.SalesFeePercent = ReadDouble(facilitySalesFeeBox.Text, 0);
            station.FactionWarfareUpgradeLevel = Math.Max(0, Math.Min(5, ReadNonNegativeInt(facilityFwUpgradeBox.Text, 0)));
            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);

            _facilityStore.Save(_facilities);
            facilityList.Items.Refresh();
            stationList.Items.Refresh();
            blueprintFacilityBox.Items.Refresh();
            facilityMaterialBonusBox.Text = FormatFacilityMath(station);
            facilityStatus.Text = station.SupportsProduction
                ? "Saved: " + DateTime.Now.ToString("HH:mm:ss")
                : "Saved with warning: " + station.ValidationMessage;
        }

        private void FacilityRigPresetBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection || facilityMaterialBonusBox == null)
            {
                return;
            }

            var station = stationList == null ? null : stationList.SelectedItem as FacilityStation;
            if (station == null)
            {
                return;
            }

            station.RigSlot1TypeId = GetSelectedRigId(facilityRigPresetBox);
            station.RigSlot2TypeId = GetSelectedRigId(facilityRigSlot2Box);
            station.RigSlot3TypeId = GetSelectedRigId(facilityRigSlot3Box);
            station.Security = GetSelectedSecurity(station);
            var math = _facilityCatalog.Calculate(station, GetSelectedRigIds(station));
            station.MaterialMultiplier = math.MaterialMultiplier;
            station.TimeMultiplier = math.TimeMultiplier;
            station.CostMultiplier = math.CostMultiplier;
            station.MaterialBonusPercent = math.MaterialBonusPercent;
            facilityMaterialBonusBox.Text = FormatFacilityMath(station);
            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);
            facilityStatus.Text = station.SupportsProduction ? "" : station.ValidationMessage;
        }

        private void FacilityServiceModulesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection)
            {
                return;
            }

            var station = stationList == null ? null : stationList.SelectedItem as FacilityStation;
            if (station == null)
            {
                return;
            }

            SaveSelectedServiceModules(station);
            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);
            facilityStatus.Text = station.SupportsProduction ? "" : station.ValidationMessage;
        }

        private void FacilityTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection)
            {
                return;
            }

            var station = stationList == null ? null : stationList.SelectedItem as FacilityStation;
            if (station == null)
            {
                return;
            }

            var structure = facilityTypeBox.SelectedItem as FacilityStructureType;
            if (structure != null)
            {
                station.FacilityType = structure.Name;
                station.StructureTypeId = structure.TypeId;
            }

            RefreshRigOptions(station);
            RefreshServiceModuleOptions(station);
            SelectServiceModules(station);
            SelectRig(facilityRigPresetBox, station.RigSlot1TypeId);
            SelectRig(facilityRigSlot2Box, station.RigSlot2TypeId);
            SelectRig(facilityRigSlot3Box, station.RigSlot3TypeId);
            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);
            facilityStatus.Text = station.SupportsProduction ? "" : station.ValidationMessage;
        }

        private void FacilityProductionTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection)
            {
                return;
            }

            if (facilityTypeBox == null || _facilityCatalog == null)
            {
                return;
            }

            var productionType = GetComboText(facilityProductionTypeBox);
            var station = stationList == null ? null : stationList.SelectedItem as FacilityStation;
            var currentStructureId = (facilityTypeBox.SelectedItem as FacilityStructureType)?.TypeId ?? 0;

            _structureTypes = _facilityCatalog.LoadStructureTypes(productionType, station == null ? 0.5 : GetSelectedSecurity(station), station == null || HasSelectedNpcStationSystem(station));
            facilityTypeBox.ItemsSource = null;
            facilityTypeBox.ItemsSource = _structureTypes;
            facilityTypeBox.SelectedItem = _structureTypes.FirstOrDefault(item => item.TypeId == currentStructureId)
                                           ?? _structureTypes.FirstOrDefault();

            if (station == null)
            {
                return;
            }

            station.ProductionType = productionType;
            var structure = facilityTypeBox.SelectedItem as FacilityStructureType;
            if (structure != null)
            {
                station.FacilityType = structure.Name;
                station.StructureTypeId = structure.TypeId;
            }

            RefreshRigOptions(station);
            RefreshServiceModuleOptions(station);
            SelectServiceModules(station);
            SelectRig(facilityRigPresetBox, station.RigSlot1TypeId);
            SelectRig(facilityRigSlot2Box, station.RigSlot2TypeId);
            SelectRig(facilityRigSlot3Box, station.RigSlot3TypeId);
            station.ValidationMessage = _facilityCatalog.ValidateProduction(station);
            station.SupportsProduction = string.IsNullOrEmpty(station.ValidationMessage);
            facilityStatus.Text = station.SupportsProduction ? "" : station.ValidationMessage;
        }

        private void FacilityRegionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection)
            {
                return;
            }

            var region = facilityRegionBox.SelectedItem as EveRegion;
            if (region == null || facilitySystemBox == null)
            {
                return;
            }

            _systems = _facilityCatalog.LoadSystems(region.RegionId);
            facilitySystemBox.ItemsSource = _systems;
            if (_systems.Count > 0 && facilitySystemBox.SelectedItem == null)
            {
                facilitySystemBox.SelectedIndex = 0;
            }
        }

        private void FacilitySystemBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingFacilitySelection)
            {
                return;
            }

            var station = stationList == null ? null : stationList.SelectedItem as FacilityStation;
            var system = facilitySystemBox.SelectedItem as EveSolarSystem;
            if (station == null || system == null)
            {
                return;
            }

            station.SystemName = system.Name;
            station.SolarSystemId = system.SolarSystemId;
            station.RegionId = system.RegionId;
            station.Security = system.Security;
            RefreshStructureOptions(station);
            RefreshRigOptions(station);
            RefreshServiceModuleOptions(station);
            SelectServiceModules(station);
            facilityMaterialBonusBox.Text = FormatFacilityMath(station);
        }

        private void SelectStructure(FacilityStation station)
        {
            if (_structureTypes == null || !_structureTypes.Any(item => item.TypeId == station.StructureTypeId))
            {
                _structureTypes = _facilityCatalog.LoadStructureTypes(station.ProductionType, GetSelectedSecurity(station), HasSelectedNpcStationSystem(station));
                facilityTypeBox.ItemsSource = null;
                facilityTypeBox.ItemsSource = _structureTypes;
            }

            var structure = _structureTypes.FirstOrDefault(item => item.TypeId == station.StructureTypeId)
                            ?? _structureTypes.FirstOrDefault(item => item.Name == station.FacilityType)
                            ?? _structureTypes.FirstOrDefault();
            facilityTypeBox.SelectedItem = structure;
        }

        private void SelectRegionAndSystem(FacilityStation station)
        {
            if (_regions == null || _regions.Count == 0)
            {
                return;
            }

            var region = _regions.FirstOrDefault(item => item.RegionId == station.RegionId);
            if (region == null && !string.IsNullOrEmpty(station.SystemName))
            {
                var foundSystem = _facilityCatalog.FindSystem(station.SystemName);
                if (foundSystem != null)
                {
                    station.RegionId = foundSystem.RegionId;
                    station.SolarSystemId = foundSystem.SolarSystemId;
                    station.Security = foundSystem.Security;
                    region = _regions.FirstOrDefault(item => item.RegionId == foundSystem.RegionId);
                }
            }

            facilityRegionBox.SelectedItem = region ?? _regions.FirstOrDefault();
            var selectedRegion = facilityRegionBox.SelectedItem as EveRegion;
            if (selectedRegion == null)
            {
                return;
            }

            _systems = _facilityCatalog.LoadSystems(selectedRegion.RegionId);
            facilitySystemBox.ItemsSource = _systems;
            facilitySystemBox.SelectedItem = _systems.FirstOrDefault(item => item.SolarSystemId == station.SolarSystemId)
                                             ?? _systems.FirstOrDefault(item => item.Name == station.SystemName)
                                             ?? _systems.FirstOrDefault();
        }

        private void RefreshRigOptions(FacilityStation station)
        {
            var structure = facilityTypeBox.SelectedItem as FacilityStructureType;
            var security = GetSelectedSecurity(station);
            _rigOptions = _facilityCatalog.LoadRigOptions(structure, security, station == null ? "Manufacturing" : station.ProductionType);
            SetRigItemsSource(_rigOptions);
            var rigSlots = structure == null ? 0 : structure.RigSlots;
            facilityRigPresetBox.IsEnabled = rigSlots >= 1;
            facilityRigSlot2Box.IsEnabled = rigSlots >= 2;
            facilityRigSlot3Box.IsEnabled = rigSlots >= 3;
        }

        private void RefreshStructureOptions(FacilityStation station)
        {
            if (station == null)
            {
                return;
            }

            var currentStructureId = (facilityTypeBox.SelectedItem as FacilityStructureType)?.TypeId ?? station.StructureTypeId;
            _structureTypes = _facilityCatalog.LoadStructureTypes(station.ProductionType, GetSelectedSecurity(station), HasSelectedNpcStationSystem(station));
            facilityTypeBox.ItemsSource = null;
            facilityTypeBox.ItemsSource = _structureTypes;
            facilityTypeBox.SelectedItem = _structureTypes.FirstOrDefault(item => item.TypeId == currentStructureId)
                                           ?? _structureTypes.FirstOrDefault();
        }

        private bool HasSelectedNpcStationSystem(FacilityStation station)
        {
            var selectedSystem = facilitySystemBox == null ? null : facilitySystemBox.SelectedItem as EveSolarSystem;
            if (selectedSystem != null)
            {
                return selectedSystem.NpcStationCount > 0;
            }

            return _facilityCatalog.HasNpcStations(station.SolarSystemId, station.SystemName);
        }

        private void RefreshServiceModuleOptions(FacilityStation station)
        {
            var structure = facilityTypeBox.SelectedItem as FacilityStructureType;
            var security = GetSelectedSecurity(station);
            _serviceModuleOptions = _facilityCatalog.LoadServiceModuleOptions(structure, security);
            facilityServiceModulesList.ItemsSource = _serviceModuleOptions;
            var serviceSlots = structure == null ? 0 : structure.ServiceSlots;
            facilityServiceModulesList.IsEnabled = serviceSlots > 0;
        }

        private void SelectServiceModules(FacilityStation station)
        {
            if (_serviceModuleOptions == null)
            {
                return;
            }

            facilityServiceModulesList.SelectedItems.Clear();
            var selectedIds = GetSelectedServiceModuleIds(station);
            foreach (var module in _serviceModuleOptions.Where(item => selectedIds.Contains(item.TypeId)))
            {
                facilityServiceModulesList.SelectedItems.Add(module);
            }
        }

        private void SaveSelectedServiceModules(FacilityStation station)
        {
            var selected = facilityServiceModulesList.SelectedItems
                .OfType<FacilityServiceModuleOption>()
                .Select(item => item.TypeId)
                .Take(5)
                .ToArray();

            station.ServiceModule1TypeId = selected.Length > 0 ? selected[0] : 0;
            station.ServiceModule2TypeId = selected.Length > 1 ? selected[1] : 0;
            station.ServiceModule3TypeId = selected.Length > 2 ? selected[2] : 0;
            station.ServiceModule4TypeId = selected.Length > 3 ? selected[3] : 0;
            station.ServiceModule5TypeId = selected.Length > 4 ? selected[4] : 0;
        }

        private void SetRigItemsSource(IEnumerable<FacilityRigOption> rigOptions)
        {
            facilityRigPresetBox.ItemsSource = rigOptions;
            facilityRigSlot2Box.ItemsSource = rigOptions;
            facilityRigSlot3Box.ItemsSource = rigOptions;
        }

        private static double GetSelectedSecurity(FacilityStation station)
        {
            return station.Security > 0 ? station.Security : 0.5;
        }

        private void SelectRig(ComboBox comboBox, int typeId)
        {
            if (_rigOptions == null)
            {
                return;
            }

            comboBox.SelectedItem = _rigOptions.FirstOrDefault(item => item.TypeId == typeId) ?? _rigOptions.FirstOrDefault();
        }

        private static int GetSelectedRigId(ComboBox comboBox)
        {
            if (!comboBox.IsEnabled)
            {
                return 0;
            }

            var rig = comboBox.SelectedItem as FacilityRigOption;
            return rig == null ? 0 : rig.TypeId;
        }

        private static int[] GetSelectedRigIds(FacilityStation station)
        {
            return new[] { station.RigSlot1TypeId, station.RigSlot2TypeId, station.RigSlot3TypeId };
        }

        private static int[] GetSelectedServiceModuleIds(FacilityStation station)
        {
            return new[]
            {
                station.ServiceModule1TypeId,
                station.ServiceModule2TypeId,
                station.ServiceModule3TypeId,
                station.ServiceModule4TypeId,
                station.ServiceModule5TypeId
            };
        }

        private string FormatFacilityMath(FacilityStation station)
        {
            var costIndex = _facilityCatalog == null ? 0 : _facilityCatalog.GetSystemCostIndex(station.SolarSystemId, station.ProductionType) * 100;
            if (station.ProductionType == "Reprocessing")
            {
                return string.Format(CultureInfo.CurrentCulture, "Refining yield bonus {0:0.##}% | yield bonus {1:0.####} / T {2:0.####} / Cost {3:0.####} | sec {4:0.0}",
                    station.MaterialBonusPercent,
                    station.MaterialMultiplier,
                    station.TimeMultiplier <= 0 ? 1 : station.TimeMultiplier,
                    station.CostMultiplier <= 0 ? 1 : station.CostMultiplier,
                    station.Security);
            }

            return string.Format(CultureInfo.CurrentCulture, "Base ME {0:0.##}% | M {1:0.####} / T {2:0.####} / Cost {3:0.####} | index {4:0.##}% | sec {5:0.0}",
                station.MaterialBonusPercent,
                station.MaterialMultiplier <= 0 ? 1 : station.MaterialMultiplier,
                station.TimeMultiplier <= 0 ? 1 : station.TimeMultiplier,
                station.CostMultiplier <= 0 ? 1 : station.CostMultiplier,
                costIndex,
                station.Security);
        }
        private static string GetComboText(ComboBox comboBox)
        {
            var item = comboBox.SelectedItem as ComboBoxItem;
            return item != null ? item.Content.ToString() : comboBox.Text;
        }

        private static void SetComboText(ComboBox comboBox, string text)
        {
            foreach (var item in comboBox.Items)
            {
                var comboItem = item as ComboBoxItem;
                if (comboItem != null && comboItem.Content.ToString() == text)
                {
                    comboBox.SelectedItem = comboItem;
                    return;
                }
            }

            comboBox.Text = text;
        }

        private async void RefreshPrices_Click(object sender, RoutedEventArgs e)
        {
            var region = marketRegionBox.SelectedItem as MarketRegion;
            var typeIds = GetRelevantPriceTypeIds();
            if (region == null || typeIds.Count == 0)
            {
                priceStatus.Text = "Р”РѕР±Р°РІСЊС‚Рµ РїСЂРµРґРјРµС‚С‹ РІ РѕС‡РµСЂРµРґСЊ, РїРѕС‚РѕРј РѕР±РЅРѕРІРёС‚Рµ С†РµРЅС‹.";
                return;
            }

            try
            {
                var id = region.StationId ?? region.RegionId;
                refreshPricesButton.IsEnabled = false;
                priceStatus.Text = _priceUpdateStatusService.Format(typeIds.Count, 0, 0, 0, null, true, 0);
                var prices = await Task.Run(() => _market.LoadPrices(id, typeIds, region.StationId.HasValue, progress =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        priceStatus.Text = _priceUpdateStatusService.Format(
                            progress.TotalTypeIds,
                            progress.FreshDownloadedCount,
                            0,
                            progress.MissingCount,
                            progress.LastError,
                            true,
                            progress.ProcessedTypeIds);
                    }));
                }));
                UpsertPriceCache(id, region.Name, prices);
                var missingIds = typeIds.Where(typeId => !prices.ContainsKey(typeId)).ToList();
                var cachedForMissing = GetCachedPrices(id, missingIds);
                var missingCount = typeIds.Count - prices.Count - cachedForMissing.Count;
                priceStatus.Text = _priceUpdateStatusService.Format(typeIds.Count, prices.Count, cachedForMissing.Count, missingCount, null, false, typeIds.Count);
            }
            catch (Exception ex)
            {
                var id = region.StationId ?? region.RegionId;
                var cached = GetCachedPrices(id, typeIds);
                priceStatus.Text = _priceUpdateStatusService.Format(typeIds.Count, 0, cached.Count, typeIds.Count - cached.Count, ex.Message, false, typeIds.Count);
            }
            finally
            {
                refreshPricesButton.IsEnabled = true;
            }
        }

        private void UpsertPriceCache(long regionOrStationId, string locationName, Dictionary<long, MarketPrice> prices)
        {
            if (prices == null || prices.Count == 0)
            {
                return;
            }

            var names = _database.GetTypeNames(prices.Keys);
            _marketPriceCacheService.Upsert(_priceCache, regionOrStationId, locationName, prices, names);
            _priceCacheStore.Save(_priceCache);
            priceCacheGrid.Items.Refresh();
        }

        private Dictionary<long, MarketPrice> LoadPricesWithCache(long regionOrStationId, string locationName, IEnumerable<long> typeIds, bool useStation)
        {
            var ids = typeIds.Distinct().ToList();
            var cached = GetCachedPrices(regionOrStationId, ids);
            if (cached.Count == ids.Count)
            {
                return cached;
            }

            try
            {
                var fresh = _market.LoadPrices(regionOrStationId, ids, useStation);
                UpsertPriceCache(regionOrStationId, locationName, fresh);
                return fresh;
            }
            catch
            {
                return cached;
            }
        }

        private Dictionary<long, MarketPrice> GetCachedPrices(long regionOrStationId, IEnumerable<long> typeIds)
        {
            return _marketPriceCacheService.GetCachedPrices(_priceCache, regionOrStationId, typeIds);
        }

        private Dictionary<long, MarketHistoryStats> GetMarketHistoryStats(IEnumerable<long> typeIds, long regionId, int days)
        {
            var ids = new HashSet<long>(typeIds);
            var result = _database.GetMarketHistoryStats(ids, regionId, days);
            return _marketHistoryCacheService.MergeCachedStats(result, _marketHistoryCache, ids, regionId, days);
        }

        private void UpsertMarketHistoryCache(IEnumerable<MarketHistoryStats> stats)
        {
            _marketHistoryCacheService.Upsert(_marketHistoryCache, stats);
            _marketHistoryCacheStore.Save(_marketHistoryCache);
        }

        private HashSet<long> GetRelevantPriceTypeIds()
        {
            var ids = new HashSet<long>();
            foreach (var blueprint in _blueprints)
            {
                CollectBlueprintTreeTypeIds(blueprint, ids, GetSelectedDecryptor(), new HashSet<long>());
            }

            foreach (var queueItem in _buildQueue)
            {
                CollectBlueprintTreeTypeIds(queueItem.Blueprint, ids, GetDecryptorByTypeId(queueItem.DecryptorTypeId), new HashSet<long>());
            }

            foreach (var project in _projects)
            foreach (var item in project.Items.Where(item => !item.IsCompleted))
            {
                var blueprint = _database.FindBlueprintByProduct(item.ProductTypeId);
                if (blueprint != null)
                {
                    CollectBlueprintTreeTypeIds(blueprint, ids, GetDecryptorByTypeId(item.DecryptorTypeId), new HashSet<long>());
                }
            }

            return ids;
        }

        private void AddBlueprintAndMaterialTypeIds(HashSet<long> ids, long blueprintTypeId, long productTypeId, long decryptorTypeId)
        {
            ids.Add(productTypeId);
            var blueprint = _database.FindBlueprintByProduct(productTypeId);
            if (blueprint != null)
            {
                AddInventionTypeIds(blueprint, ids, GetDecryptorByTypeId(decryptorTypeId));
            }
            foreach (var material in _database.GetManufacturingMaterials(blueprintTypeId))
            {
                ids.Add(material.TypeId);
                AddReprocessingOreTypeIds(material.TypeId, ids);
            }
        }

        private void Navigation_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            blueprintsPage.Visibility = Visibility.Collapsed;
            analysisPage.Visibility = Visibility.Collapsed;
            pricesPage.Visibility = Visibility.Collapsed;
            facilitiesPage.Visibility = Visibility.Collapsed;
            buildQueuePage.Visibility = Visibility.Collapsed;
            projectsPage.Visibility = Visibility.Collapsed;

            if (sender == navBlueprints)
            {
                ShowPage(blueprintsPage, "Р§РµСЂС‚РµР¶Рё", "РїРѕРёСЃРє, СЂР°СЃС‡РµС‚ Рё РґРѕР±Р°РІР»РµРЅРёРµ РІ РѕС‡РµСЂРµРґСЊ");
            }
            else if (sender == navAnalysis)
            {
                ShowPage(analysisPage, "РђРЅР°Р»РёР·", "SVR, РєРѕРЅС‚СЂР°РєС‚С‹, score Рё СЂР°СЃС€РёСЂРµРЅРЅС‹Рµ С„РёР»СЊС‚СЂС‹");
            }
            else if (sender == navPrices)
            {
                ShowPage(pricesPage, "Р¦РµРЅС‹", "Fuzzwork-Р°РіСЂРµРіР°С‚С‹ РїРѕ СЂРµРіРёРѕРЅСѓ РёР»Рё С…Р°Р±Сѓ");
            }
            else if (sender == navFacilities)
            {
                ShowPage(facilitiesPage, "РЎС‚Р°РЅС†РёРё", "РїСЂРѕС„РёР»Рё СЃС‚СЂСѓРєС‚СѓСЂ, СЂРёРіРѕРІ, СЃРёСЃС‚РµРј Рё РЅР°Р»РѕРіРѕРІ");
            }
            else if (sender == navBuildQueue)
            {
                ShowPage(buildQueuePage, "РћС‡РµСЂРµРґСЊ СЃС‚СЂРѕР№РєРё", "РїР»Р°РЅРёСЂСѓРµРјС‹Рµ С‡РµСЂС‚РµР¶Рё РїРµСЂРµРґ Р·Р°РєСѓРїРєРѕР№ Рё РїСЂРѕРµРєС‚Р°РјРё");
            }
            else if (sender == navProjects)
            {
                ShowPage(projectsPage, "РўРµРєСѓС‰РёРµ РїСЂРѕРµРєС‚С‹", "РІРѕР»РЅС‹ РїСЂРѕРёР·РІРѕРґСЃС‚РІР°, РѕСЃС‚Р°С‚РєРё Рё Р·Р°РєСѓРїРєР°");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowBlueprintsMenu_Click(object sender, RoutedEventArgs e)
        {
            navBlueprints.IsChecked = true;
        }

        private void ShowAnalysisMenu_Click(object sender, RoutedEventArgs e)
        {
            navAnalysis.IsChecked = true;
        }

        private void ShowPricesMenu_Click(object sender, RoutedEventArgs e)
        {
            navPrices.IsChecked = true;
        }

        private void ShowFacilitiesMenu_Click(object sender, RoutedEventArgs e)
        {
            navFacilities.IsChecked = true;
        }

        private void ShowQueueMenu_Click(object sender, RoutedEventArgs e)
        {
            navBuildQueue.IsChecked = true;
        }

        private void ShowProjectsMenu_Click(object sender, RoutedEventArgs e)
        {
            navProjects.IsChecked = true;
        }

        private void ShowPage(UIElement page, string title, string subtitle)
        {
            page.Visibility = Visibility.Visible;
            pageTitle.Text = title;
            pageSubtitle.Text = "  " + subtitle;
        }
    }
}
