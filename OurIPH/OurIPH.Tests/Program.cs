using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OurIPH.Models;
using OurIPH.Services;

namespace OurIPH.Tests
{
    internal static class Program
    {
        private static int _assertions;

        private static int Main()
        {
            try
            {
                ContractPricing_SelectsMedianAndRejectsNoise();
                ContractPricing_PrefersContractsForCapitalProducts();
                BlueprintFiltering_ClassifiesLimitedRareCapitalAndReactionProducts();
                BlueprintFiltering_LoadsEditableRuleSettings();
                BlueprintBuildProfile_AppliesTechCapitalReactionAndNoiseToggles();
                BlueprintTypeFilter_ClassifiesBlueprintRowsAndAppliesToggles();
                BlueprintRanking_RanksProfitableLiquidItemsAndFiltersNoise();
                BlueprintRanking_AppliesMarketVolumeAndContractConfidencePenalties();
                BlueprintRanking_FavorsLiquidExamplesOverLowVolumeTraps();
                BlueprintListFilter_AppliesTopLowVolumeAndExplicitSearchRules();
                BlueprintMaterialMath_AdjustsQuantitiesWithRoundingAndMinimums();
                BlueprintManufacturingMath_CalculatesFacilityMultipliersAndInstallationCost();
                BlueprintSkillRequirements_AggregatesBuildChainComponentSkills();
                NumericInputValidation_AcceptsOnlyConfiguredNumberFormats();
                PriceUpdateStatus_FormatsProgressCompletionAndErrors();
                UiSettingsStore_RoundTripsSettingsAndDefaults();
                FacilityPresetStore_RoundTripsAllFieldsAndRepairsDefaultNameMojibake();
                MarketPriceCacheService_UsesLatestCachedPriceAndUpserts();
                MarketHistoryCacheService_MergesFallbacksAndUpserts();
                MarketPriceSelectionService_AppliesModesModifiersAndVolumeFallbacks();
                SalesFeeService_AppliesTaxesBrokerAndStationFees();
                BuildQueueService_MergesEquivalentBlueprintRows();
                BuildQueueService_SelectsProfitableCandidatesInRankOrder();
                BuildProjectDecisionService_SetsUpdatesAndClearsModes();
                UsedByListService_AppendsUniqueNames();
                BlueprintProductionTypeService_ClassifiesProductionTypesAndTiming();
                ProjectMaterialImportService_ParsesClipboardQuantities();
                BlueprintEfficiencyService_AppliesInventionCopyAndChildDefaults();
                BlueprintCopyStatusService_FormatsCopyOnlyRuns();
                BlueprintEstimateApplicationService_AppliesAndResetsEstimateFields();
                BlueprintEstimateCacheService_UsesStableKeysAndCloneIsolation();
                BlueprintEstimateStatusService_FormatsBlueprintAndProjectStatuses();
                ProjectItemEstimateApplicationService_AppliesAndResetsProjectEstimateFields();
                ProjectItemEstimateDisplayService_AppliesSummaryStatusAndPriceDisplay();
                ProjectQueueDisplayService_FormatsJobMaterialStatus();
                BlueprintInventionCostService_AccumulatesMaterialDecryptorAndCopyCosts();
                BlueprintInventionCostService_CalculatesScienceUsageCost();
                BlueprintInventionTimeService_CalculatesCopyAndInventionTimes();
                FacilityActivityStationService_ResolvesActivityStationsAndFallbackCopies();
                OrePlanningService_PlansGreedyOreReplacementDeterministically();
                BlueprintEstimateSelectionService_SelectsBestStationAndDecryptorCandidates();
                BlueprintEstimateTraversalService_CharacterizesRecursiveEstimateEdges();
                BlueprintMaterialEstimateLineService_CharacterizesMaterialPriceLines();
                BlueprintEstimateMaterialTraversalService_CharacterizesMaterialTraversalBranches();
                BlueprintEstimateMaterialOrchestrationService_CreatesBoundariesAndAggregatesResults();
                BlueprintEstimateResultAssembler_AssemblesFinalEstimateSummary();
                DatabaseBackedBlueprintStructuralGolden_LoadRealBlueprintChains();
                DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedLegacyFormulas();
                DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedMaterialCostAndFees();
                DatabaseBackedBlueprintLegacyNumericParity_UsesRecursiveComponentCostHarness();
                DatabaseBackedBlueprintLegacyNumericParity_UsesSurplusExcessAndSellbackMappings();
                DatabaseBackedBlueprintLegacyNumericParity_UsesOreMineralFallbackMappings();
                MaterialBuildBuyDecisionService_SelectsBuildOrBuyCost();
                SalesVolumeRatioService_AppliesLiquidityToBlueprintsAndProjects();
                MarketCacheStores_RoundTripPriceAndHistoryCaches();
                ContractPriceStore_RoundTripsMetadataAndDropsStaleSamples();
                CopyOnlyBlueprintQueue_ShowsBpcRunsAndZeroEfficiency();
                BlueprintInventionMath_CalculatesChanceRunsAndJobs();
                BlueprintProfitability_CalculatesProfitRoiMarginAndIskPerHour();
                EsiPublicContracts_CreatesSamplesOnlyForExactIncludedItemContracts();
                EsiPublicContracts_AllowsExcludedItemsButRejectsInvalidSamples();
                ProjectStock_DistributesOwnedQuantitiesByWaveAndCalculatesInlineEditTotal();
                BuildProjectStore_RoundTripsProjectStockAndDecisions();

                Console.WriteLine("OurIPH.Tests passed: " + _assertions + " assertions");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("OurIPH.Tests failed: " + ex.Message);
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void ContractPricing_SelectsMedianAndRejectsNoise()
        {
            var service = new ContractPricingService();
            var blueprint = Blueprint("Moros", groupName: "Dreadnought", productTypeId: 1001);
            var now = DateTime.Now;
            var samples = new[]
            {
                Sample(1001, "Moros", 1, now),
                Sample(1001, "Moros", 100, now),
                Sample(1001, "Moros", 110, now),
                Sample(1001, "Moros", 120, now),
                Sample(1001, "Moros", 1000, now),
                Sample(1001, "Wrong Item", 115, now),
                Sample(1001, "Moros", 115, now.AddDays(-40)),
                Sample(9999, "Moros", 115, now)
            };

            var result = service.SelectContractPrice(blueprint, samples, maxSampleAgeDays: 30, manualPriceModifierPercent: 10);

            AssertEqual(121, result.UnitPrice, 0.0001, "contract price uses the filtered median plus manual modifier");
            AssertEqual(3, result.AcceptedSamples, "contract pricing accepts only in-band target samples");
            AssertEqual(2, result.RejectedSamples, "contract pricing counts in-target anomalous samples as rejected");
            AssertEqual("Contract", result.Source, "contract pricing result source");
            AssertEqual("High", result.Confidence, "three accepted samples are high confidence");

            var review = service.ReviewSamples(blueprint, samples, maxSampleAgeDays: 30);
            AssertEqual(3, review.AcceptedSamples, "contract review marks accepted samples");
            AssertEqual(2, review.RejectedSamples, "contract review marks rejected anomalous samples");
            AssertTrue(review.Reviews.Any(item => item.ReviewStatus == "Non-target"), "contract review marks non-target samples");
            AssertTrue(review.Reviews.Any(item => item.ReviewStatus == "Stale"), "contract review marks stale samples");
        }

        private static void ContractPricing_PrefersContractsForCapitalProducts()
        {
            var service = new ContractPricingService();
            var blueprint = Blueprint("Charon", groupName: "Freighter", productTypeId: 2001);
            var samples = new[]
            {
                Sample(2001, "Charon", 10_000_000_000, DateTime.Now),
                Sample(2001, "Charon", 10_200_000_000, DateTime.Now),
                Sample(2001, "Charon", 9_800_000_000, DateTime.Now)
            };
            var market = new MarketPrice { SellMin = 9_000_000_000, SellVolume = 50 };

            var result = service.GetEffectiveProductPrice(
                blueprint,
                market,
                samples,
                useContracts: true,
                getMarketUnitPrice: price => price.SellMin,
                getMarketVolume: price => price.SellVolume,
                manualPriceModifierPercent: 0,
                maxSampleAgeDays: 30);

            AssertEqual(10_000_000_000, result.UnitPrice, 0.0001, "capital product uses contract median even when market volume exists");
            AssertEqual("Contract", result.Source, "capital contract result source");
        }

        private static void BlueprintFiltering_ClassifiesLimitedRareCapitalAndReactionProducts()
        {
            var service = new BlueprintFilteringService();

            AssertTrue(service.IsLimitedSourceBlueprint(Blueprint("Navy Module", metaGroupId: 4)), "faction meta groups are limited source");
            AssertTrue(service.IsRareOrNoiseBlueprint(Blueprint("Tobias' Modified Module", groupName: "Officer")), "officer products are rare/noise");
            AssertTrue(service.IsCapitalBlueprint(Blueprint("Thanatos", groupName: "Carrier")), "carrier group is capital");
            AssertTrue(service.IsReactionBlueprint(Blueprint("Fulleride", hasReactionActivity: true)), "reaction activity marks reaction blueprint");
            AssertFalse(service.IsRareOrNoiseBlueprint(Blueprint("T1 Module", groupName: "Module")), "plain T1 module is not rare/noise");
        }

        private static void BlueprintFiltering_LoadsEditableRuleSettings()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directory, "BlueprintFilterRules.xml");
            var rules = BlueprintFilterRules.CreateDefault();
            rules.LimitedMetaGroupIds = new List<int> { 777 };
            rules.LimitedMarketGroupKeywords = new List<string> { "Custom Limited" };
            rules.RareProductKeywords = new List<string> { "Custom Rare" };
            rules.CapitalKeywords = new List<string> { "Custom Capital" };
            rules.ReactionGroupKeywords = new List<string> { "Custom Reaction" };

            var store = new BlueprintFilterRuleStore(filePath);
            store.Save(rules);

            var loadedRules = new BlueprintFilterRuleStore(filePath).Load();
            var service = new BlueprintFilteringService(loadedRules);

            AssertTrue(service.IsLimitedSourceBlueprint(Blueprint("Plain", metaGroupId: 777)), "custom meta group marks limited-source blueprint");
            AssertTrue(service.IsLimitedSourceBlueprint(Blueprint("Plain", marketGroup: "Custom Limited Ships")), "custom market group keyword marks limited-source blueprint");
            AssertTrue(service.IsRareOrNoiseBlueprint(Blueprint("Custom Rare Module")), "custom product keyword marks rare/noise blueprint");
            AssertTrue(service.IsCapitalBlueprint(Blueprint("Plain", groupName: "Custom Capital Hull")), "custom capital keyword marks capital blueprint");
            AssertTrue(service.IsReactionBlueprint(Blueprint("Plain", groupName: "Custom Reaction Chain")), "custom reaction keyword marks reaction blueprint");

            Directory.Delete(directory, recursive: true);
        }

        private static void BlueprintBuildProfile_AppliesTechCapitalReactionAndNoiseToggles()
        {
            var service = new BlueprintBuildProfileService(new BlueprintFilteringService());
            var normal = Blueprint("T1 Module", groupName: "Module");
            normal.TechLevel = 1;
            var tech2 = Blueprint("T2 Module", groupName: "Module");
            tech2.TechLevel = 2;
            var tech3 = Blueprint("T3 Hull", groupName: "Strategic Cruiser");
            tech3.TechLevel = 3;
            var capital = Blueprint("Thanatos", groupName: "Carrier");
            var reaction = Blueprint("Fulleride", groupName: "Composite", hasReactionActivity: true);
            var limited = Blueprint("Navy Module", metaGroupId: 4);
            var rare = Blueprint("Tobias' Modified Module", groupName: "Officer");

            var allAllowed = new BlueprintBuildProfileOptions
            {
                AllowTech2 = true,
                AllowTech3 = true,
                AllowCapital = true,
                AllowReactions = true
            };
            AssertTrue(service.Passes(normal, allAllowed), "normal T1 blueprint passes default build profile");
            AssertTrue(service.Passes(tech2, allAllowed), "T2 blueprint passes when T2 is enabled");
            AssertTrue(service.Passes(tech3, allAllowed), "T3 blueprint passes when T3 is enabled");

            var restrictive = new BlueprintBuildProfileOptions
            {
                AllowTech2 = false,
                AllowTech3 = false,
                AllowCapital = false,
                AllowReactions = false,
                HideLimitedSource = true,
                HideRareOrNoise = true
            };
            AssertFalse(service.Passes(tech2, restrictive), "T2 blueprint is hidden when T2 is disabled");
            AssertFalse(service.Passes(tech3, restrictive), "T3 blueprint is hidden when T3 is disabled");
            AssertFalse(service.Passes(capital, restrictive), "capital blueprint is hidden when capitals are disabled");
            AssertFalse(service.Passes(reaction, restrictive), "reaction blueprint is hidden when reactions are disabled");
            AssertFalse(service.Passes(limited, restrictive), "limited-source blueprint is hidden");
            AssertFalse(service.Passes(rare, restrictive), "rare/officer blueprint is hidden");

            restrictive.HideRareOrNoise = false;
            AssertTrue(service.Passes(rare, restrictive), "rare/officer blueprint can be shown when rare filtering is disabled");
        }

        private static void BlueprintTypeFilter_ClassifiesBlueprintRowsAndAppliesToggles()
        {
            var service = new BlueprintTypeFilterService();
            var ship = Blueprint("Revelation", groupName: "Dreadnought", categoryName: "Ship");
            var ammo = Blueprint("Inferno Cruise Missile", groupName: "Missile");
            var module = Blueprint("Damage Control", groupName: "Module");
            var rig = Blueprint("Capital Trimark Armor Pump", groupName: "Rig");
            var drone = Blueprint("Hammerhead I", groupName: "Combat Drone", categoryName: "Drone");
            var component = Blueprint("Capital Armor Plates", groupName: "Capital Component");
            var structure = Blueprint("Astrahus", groupName: "Citadel", categoryName: "Structure");
            var misc = Blueprint("Station Container", groupName: "Container", categoryName: "Commodity");

            AssertEqual((int)BlueprintTypeCategory.Ship, (int)service.Classify(ship), "ship category classification");
            AssertEqual((int)BlueprintTypeCategory.AmmoCharge, (int)service.Classify(ammo), "ammo/missile classification");
            AssertEqual((int)BlueprintTypeCategory.Module, (int)service.Classify(module), "module classification");
            AssertEqual((int)BlueprintTypeCategory.Rig, (int)service.Classify(rig), "rig classification");
            AssertEqual((int)BlueprintTypeCategory.Drone, (int)service.Classify(drone), "drone classification");
            AssertEqual((int)BlueprintTypeCategory.Component, (int)service.Classify(component), "component classification");
            AssertEqual((int)BlueprintTypeCategory.Structure, (int)service.Classify(structure), "structure classification");
            AssertEqual((int)BlueprintTypeCategory.Misc, (int)service.Classify(misc), "unknown types fall back to misc");

            var onlyShips = new BlueprintTypeFilterOptions { Ships = true };
            AssertTrue(service.Passes(ship, onlyShips), "ship toggle allows ships");
            AssertFalse(service.Passes(module, onlyShips), "ship toggle hides modules");
            AssertFalse(service.Passes(misc, onlyShips), "ship toggle hides misc");

            var onlyMisc = new BlueprintTypeFilterOptions { Misc = true };
            AssertTrue(service.Passes(misc, onlyMisc), "misc toggle allows unclassified rows");
            AssertFalse(service.Passes(component, onlyMisc), "misc toggle does not include known component rows");

            var allEnabled = new BlueprintTypeFilterOptions
            {
                Ships = true,
                AmmoCharges = true,
                Modules = true,
                Rigs = true,
                Drones = true,
                Components = true,
                Structures = true,
                Misc = true
            };
            AssertTrue(service.Passes(module, allEnabled), "all toggles enabled allows module rows");
        }

        private static void BlueprintRanking_RanksProfitableLiquidItemsAndFiltersNoise()
        {
            var service = new BlueprintRankingService(new BlueprintFilteringService());
            var liquid = RankCandidate("Liquid Module", profit: 5_000_000, iskPerHour: 2_000_000, salesVolumeRatio: 5);
            var thin = RankCandidate("Thin Module", profit: 10_000_000, iskPerHour: 1_500_000, salesVolumeRatio: 0.2);
            var rare = RankCandidate("Officer Module", profit: 100_000_000, iskPerHour: 50_000_000, salesVolumeRatio: 10, groupName: "Officer");
            var failed = RankCandidate("Failed Module", profit: 100_000_000, iskPerHour: 50_000_000, salesVolumeRatio: 10);
            failed.EstimateStatus = "Missing prices";

            service.AssignRanks(new[] { thin, rare, failed, liquid });

            AssertEqual(1, liquid.ProfitRank, "liquid item ranks first");
            AssertEqual(2, thin.ProfitRank, "lower-liquidity item still ranks after liquid item");
            AssertEqual(0, rare.ProfitRank, "rare/noise item is not ranked");
            AssertEqual(0, failed.ProfitRank, "non-OK estimate is not ranked");
            AssertTrue(liquid.TopScore > thin.TopScore, "ranked item score follows liquidity-weighted score");

            service.AssignRanks(new[] { rare }, hideRareOrNoise: false);
            AssertEqual(1, rare.ProfitRank, "rare/noise ranking can be enabled by settings");
        }

        private static void BlueprintRanking_AppliesMarketVolumeAndContractConfidencePenalties()
        {
            var service = new BlueprintRankingService(new BlueprintFilteringService());
            var normal = RankCandidate("Normal Module", profit: 10_000, iskPerHour: 1_000, salesVolumeRatio: 2);
            normal.ProductMarketVolume = 100;
            normal.ProducedQuantity = 10;

            var lowVolume = RankCandidate("Low Volume Module", profit: 10_000, iskPerHour: 1_000, salesVolumeRatio: 2);
            lowVolume.ProductMarketVolume = 5;
            lowVolume.ProducedQuantity = 10;

            var oneSampleContract = RankCandidate("Contract Module", profit: 10_000, iskPerHour: 1_000, salesVolumeRatio: 2);
            oneSampleContract.ProductPriceSource = "Contract";
            oneSampleContract.ProductPriceDetails = "Contract median from 1/1 samples";

            var normalScore = service.CalculateScore(normal);
            var lowVolumeScore = service.CalculateScore(lowVolume);
            var oneSampleContractScore = service.CalculateScore(oneSampleContract);

            AssertTrue(lowVolumeScore < normalScore, "low market volume applies ranking penalty");
            AssertTrue(oneSampleContractScore < normalScore, "single-sample contract applies confidence penalty");
        }

        private static void BlueprintRanking_FavorsLiquidExamplesOverLowVolumeTraps()
        {
            var service = new BlueprintRankingService(new BlueprintFilteringService());
            var highVolumeModule = RankCandidate("High Volume Module", profit: 5_000_000, iskPerHour: 2_000_000, salesVolumeRatio: 5);
            highVolumeModule.ProductMarketVolume = 500;
            highVolumeModule.ProducedQuantity = 10;

            var lowVolumeTrap = RankCandidate("Low Volume Trap", profit: 1_000_000_000, iskPerHour: 2_000_000, salesVolumeRatio: 0.05);
            lowVolumeTrap.ProductMarketVolume = 2;
            lowVolumeTrap.ProducedQuantity = 10;

            var contractCapital = RankCandidate("Contract Capital", profit: 500_000_000, iskPerHour: 10_000_000, salesVolumeRatio: 0.30, groupName: "Dreadnought");
            contractCapital.ProductPriceSource = "Contract";
            contractCapital.ProductPriceDetails = "Contract median from 3/3 samples";
            contractCapital.ProductMarketVolume = 1;
            contractCapital.ProducedQuantity = 1;

            service.AssignRanks(new[] { lowVolumeTrap, highVolumeModule, contractCapital });

            AssertTrue(highVolumeModule.TopScore > lowVolumeTrap.TopScore, "liquid module outranks extreme low-volume paper profit trap");
            AssertTrue(contractCapital.TopScore > lowVolumeTrap.TopScore, "contract-priced capital with some liquidity outranks low-volume trap");
            AssertEqual(3, lowVolumeTrap.ProfitRank, "low-volume trap ranks behind liquid examples");
        }

        private static void BlueprintListFilter_AppliesTopLowVolumeAndExplicitSearchRules()
        {
            var service = new BlueprintListFilterService();
            var top = RankCandidate("Top Product", profit: 100, iskPerHour: 100, salesVolumeRatio: 2);
            top.ProfitRank = 1;
            top.ProductTypeId = 1001;
            var second = RankCandidate("Second Product", profit: 100, iskPerHour: 80, salesVolumeRatio: 2);
            second.ProfitRank = 2;
            var lowVolume = RankCandidate("Low Volume Product", profit: 100, iskPerHour: 70, salesVolumeRatio: 1);
            lowVolume.ProfitRank = 3;
            lowVolume.ProductMarketVolume = 1;
            lowVolume.ProducedQuantity = 10;
            var noPrice = RankCandidate("No Price Product", profit: 100, iskPerHour: 60, salesVolumeRatio: 1);
            noPrice.EstimateStatus = "Missing prices";
            var excluded = RankCandidate("Excluded Product", profit: 100, iskPerHour: 50, salesVolumeRatio: 1);
            excluded.ProductTypeId = 9001;

            var options = new BlueprintListFilterOptions
            {
                HideLowVolume = true,
                HideMissingPrices = true,
                TopOnly = true,
                TopCount = 1,
                ExcludedProductIds = new HashSet<long> { excluded.ProductTypeId }
            };

            AssertTrue(service.Passes(top, options), "top ranked item passes top-one filter");
            AssertFalse(service.Passes(second, options), "second ranked item is hidden by top-one filter");
            AssertFalse(service.Passes(lowVolume, options), "low market volume item is hidden");
            AssertFalse(service.Passes(noPrice, options), "missing-price item is hidden");
            AssertFalse(service.Passes(excluded, options), "excluded item is hidden by default");

            options.MatchesExplicitSearch = blueprint => blueprint.ProductTypeId == excluded.ProductTypeId;
            AssertTrue(service.Passes(excluded, options), "explicit search shows direct matches even when normal filters would hide them");
        }

        private static void BlueprintProfitability_CalculatesProfitRoiMarginAndIskPerHour()
        {
            var result = BlueprintProfitabilityService.Calculate(
                materialCost: 700,
                installationCost: 50,
                inventionCost: 50,
                revenue: 1000,
                productionTimeSeconds: 7200);

            AssertEqual(800, result.TotalCost, 0.0001, "profitability total cost includes material, installation, and invention");
            AssertEqual(200, result.Profit, 0.0001, "profitability profit");
            AssertEqual(100, result.IskPerHour, 0.0001, "profitability ISK per hour");
            AssertEqual(20, result.MarginPercent, 0.0001, "profitability margin percent");
            AssertEqual(25, result.ReturnOnInvestmentPercent, 0.0001, "profitability ROI percent");
        }

        private static void BlueprintMaterialMath_AdjustsQuantitiesWithRoundingAndMinimums()
        {
            AssertEqual(9, (int)BlueprintMaterialMathService.CalculateAdjustedQuantity(10, 1, 0.87), "material adjustment rounds up fractional quantities");
            AssertEqual(3, (int)BlueprintMaterialMathService.CalculateAdjustedQuantity(1, 3, 0.50), "material adjustment keeps at least one unit per run");
            AssertEqual(0, (int)BlueprintMaterialMathService.CalculateAdjustedQuantity(10, 0, 1.00), "material adjustment returns zero for zero runs");
            AssertEqual(0, (int)BlueprintMaterialMathService.CalculateAdjustedQuantity(0, 3, 1.00), "material adjustment returns zero for zero base quantity");
            AssertEqual(5, (int)BlueprintMaterialMathService.CalculateAdjustedQuantity(100, 5, -1.00), "negative multipliers do not produce negative material requirements");
        }

        private static void BlueprintManufacturingMath_CalculatesFacilityMultipliersAndInstallationCost()
        {
            var service = new BlueprintManufacturingMathService();

            AssertEqual(0.81, service.CalculateMaterialMultiplier(10, 0.9), 0.0001, "material multiplier combines ME and facility material multiplier");
            AssertEqual(10, service.CalculateFacilityMaterialBonusPercent(0.9), 0.0001, "facility material bonus displays station material savings");
            AssertEqual(0, service.CalculateMaterialMultiplier(120, 0.9), 0.0001, "material multiplier clamps negative values");

            var timeMultiplier = service.CalculateTimeMultiplier(
                timeEfficiency: 20,
                stationTimeMultiplier: 0.8,
                implantPercent: 5,
                industrySkillLevel: 5,
                advancedIndustrySkillLevel: 4,
                advancedManufacturingSkillMultiplier: 0.98);
            AssertEqual(0.41947136, timeMultiplier, 0.000001, "time multiplier combines TE, station, implant, skills, and advanced manufacturing skill");

            var installationCost = service.CalculateInstallationCost(
                estimatedInputValue: 1000000,
                costIndex: 0.05,
                stationCostMultiplier: 0.9,
                factionWarfareMultiplier: 0.8,
                stationTaxRate: 0.0025,
                sccIndustryFeeRate: 0.04,
                alphaAccountTaxRate: 0.01);
            AssertEqual(88500, installationCost, 0.0001, "installation cost combines cost index and tax rates");
        }

        private static void BlueprintSkillRequirements_AggregatesBuildChainComponentSkills()
        {
            var service = new BlueprintSkillRequirementService();
            var hull = Blueprint("Capital Hull", productTypeId: 100);
            hull.BlueprintTypeId = 101;
            var component = Blueprint("Capital Component", productTypeId: 200);
            component.BlueprintTypeId = 201;
            var ram = Blueprint("R.A.M. Tool", productTypeId: 300);
            ram.BlueprintTypeId = 301;

            var ownSkills = new Dictionary<long, SkillRequirement[]>
            {
                { hull.BlueprintTypeId, new[] { Skill(1, "Industry", 1), Skill(2, "Capital Ship Construction", 3) } },
                { component.BlueprintTypeId, new[] { Skill(1, "Industry", 3), Skill(3, "Advanced Component Construction", 4) } },
                { ram.BlueprintTypeId, new[] { Skill(4, "R.A.M. Building", 5) } }
            };
            var materials = new Dictionary<long, MaterialRequirement[]>
            {
                { hull.BlueprintTypeId, new[] { new MaterialRequirement { TypeId = component.ProductTypeId, Name = component.ProductName, Quantity = 2 } } },
                { component.BlueprintTypeId, new[] { new MaterialRequirement { TypeId = ram.ProductTypeId, Name = ram.ProductName, Quantity = 1 } } }
            };
            var blueprintsByProduct = new Dictionary<long, BlueprintSearchResult>
            {
                { component.ProductTypeId, component },
                { ram.ProductTypeId, ram }
            };

            var result = service.BuildRequiredSkills(
                hull,
                blueprint => ownSkills.ContainsKey(blueprint.BlueprintTypeId) ? ownSkills[blueprint.BlueprintTypeId] : Enumerable.Empty<SkillRequirement>(),
                blueprintTypeId => materials.ContainsKey(blueprintTypeId) ? materials[blueprintTypeId] : Enumerable.Empty<MaterialRequirement>(),
                productTypeId => blueprintsByProduct.ContainsKey(productTypeId) ? blueprintsByProduct[productTypeId] : null,
                blueprint => blueprint.ProductTypeId == ram.ProductTypeId,
                (parent, child) => false);

            AssertEqual(3, result.Count, "build-chain skills include root and buildable component skills, but skip always-buy children");
            AssertEqual(3, result.Single(skill => skill.TypeId == 1).Level, "duplicate skills use the maximum required level across the build chain");
            AssertTrue(result.Any(skill => skill.TypeId == 2), "root blueprint skill is included");
            AssertTrue(result.Any(skill => skill.TypeId == 3), "component blueprint skill is included");
            AssertFalse(result.Any(skill => skill.TypeId == 4), "always-buy child blueprint skills are not included");
        }

        private static void NumericInputValidation_AcceptsOnlyConfiguredNumberFormats()
        {
            var service = new NumericInputValidationService();
            var integer = new NumericInputRule { AllowDecimal = false, AllowNegative = false, DefaultText = "1" };
            var signedDecimal = new NumericInputRule { AllowDecimal = true, AllowNegative = true, DefaultText = "0" };
            var unsignedDecimal = new NumericInputRule { AllowDecimal = true, AllowNegative = false, DefaultText = "0" };

            AssertTrue(service.IsValid("123", integer, allowPartial: false), "integer accepts digits");
            AssertFalse(service.IsValid("12a", integer, allowPartial: true), "integer rejects letters");
            AssertFalse(service.IsValid("-1", integer, allowPartial: false), "unsigned integer rejects negative values");
            AssertFalse(service.IsValid("", integer, allowPartial: false), "empty final integer is invalid");
            AssertTrue(service.IsValid("", integer, allowPartial: true), "empty partial integer is allowed while editing");

            AssertTrue(service.IsValid("1.5", unsignedDecimal, allowPartial: false), "decimal accepts invariant decimal point");
            AssertFalse(service.IsValid("-1.5", unsignedDecimal, allowPartial: false), "unsigned decimal rejects negative values");
            AssertTrue(service.IsValid("-1.5", signedDecimal, allowPartial: false), "signed decimal accepts negative values");
            AssertTrue(service.IsValid("-", signedDecimal, allowPartial: true), "signed decimal allows partial minus while editing");
            AssertFalse(service.IsValid("-", signedDecimal, allowPartial: false), "signed decimal rejects lone minus on focus loss");
            AssertFalse(service.IsValid("abc", signedDecimal, allowPartial: true), "decimal rejects letters");
        }

        private static void PriceUpdateStatus_FormatsProgressCompletionAndErrors()
        {
            var service = new PriceUpdateStatusService();
            var timestamp = new DateTime(2026, 5, 11, 12, 34, 56);

            AssertEqual("Обновляю цены: 10/20; fresh 8; cache 0; missing 2",
                service.Format(20, 8, 0, 2, null, inProgress: true, processed: 10, timestamp: timestamp),
                "price progress status");
            AssertEqual("Цены: total 20; fresh 12; cache 5; missing 0; 12:34:56",
                service.Format(20, 12, 5, -3, null, inProgress: false, processed: 20, timestamp: timestamp),
                "price complete status clamps missing and includes timestamp");
            AssertEqual("Цены: total 20; fresh 0; cache 7; missing 13; last error: timeout",
                service.Format(20, 0, 7, 13, "timeout", inProgress: false, processed: 20, timestamp: timestamp),
                "price error status includes last error and omits timestamp");
        }

        private static void UiSettingsStore_RoundTripsSettingsAndDefaults()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directory, "UiSettings.xml");
            var store = new UiSettingsStore(filePath);

            var defaults = store.Load();
            AssertTrue(defaults.AlwaysBuyRam, "missing UI settings defaults to buying R.A.M.");
            AssertTrue(defaults.HideLowVolume, "missing UI settings defaults to hiding low-volume rows");
            AssertEqual("10", defaults.TopBlueprintCount, "missing UI settings defaults Top N count");

            var saved = new UiSettings
            {
                AlwaysBuyRam = false,
                AlwaysBuyFuelBlocks = false,
                ReactionDepth = "Raw",
                MaterialPriceMode = "Buy",
                ProductPriceMode = "Sell",
                Decryptor = "Parity",
                AutoDecryptor = false,
                TypeShips = false,
                TypeAmmo = true,
                TypeModules = false,
                TypeRigs = true,
                TypeDrones = false,
                TypeComponents = true,
                TypeStructures = false,
                TypeMisc = true,
                ProfitableOnly = true,
                HideMissingPrices = false,
                HideLowVolume = false,
                HideLimitedBlueprints = false,
                HideRareNoise = false,
                AllowT2 = false,
                AllowT3 = false,
                AllowCapital = false,
                AllowReactions = false,
                ShowExcludedBlueprints = true,
                HideBySkills = true,
                TopOnly = false,
                TopBlueprintCount = "25",
                MinSvr = "1.5",
                SvrDays = "14",
                PriceTrendFilter = "Up",
                MinSold = "3",
                MinOrders = "4",
                UseContractPrices = false,
                MinIskHour = "1000",
                MinRoi = "12.5",
                MaterialPriceModifier = "2",
                ProductPriceModifier = "-3",
                BlueprintDetailHeight = 333.5
            };

            store.Save(saved);
            var loaded = new UiSettingsStore(filePath).Load();

            AssertFalse(loaded.AlwaysBuyRam, "UI settings persists AlwaysBuyRam");
            AssertFalse(loaded.AutoDecryptor, "UI settings persists AutoDecryptor");
            AssertTrue(loaded.ProfitableOnly, "UI settings persists ProfitableOnly");
            AssertTrue(loaded.ShowExcludedBlueprints, "UI settings persists ShowExcludedBlueprints");
            AssertEqual("Raw", loaded.ReactionDepth, "UI settings persists reaction depth");
            AssertEqual("25", loaded.TopBlueprintCount, "UI settings persists Top N count");
            AssertEqual("Up", loaded.PriceTrendFilter, "UI settings persists trend filter");
            AssertEqual(333.5, loaded.BlueprintDetailHeight, 0.0001, "UI settings persists detail height with invariant culture");

            Directory.Delete(directory, recursive: true);
        }

        private static void FacilityPresetStore_RoundTripsAllFieldsAndRepairsDefaultNameMojibake()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var filePath = Path.Combine(directory, "FacilityPresets.xml");
                var store = new FacilityPresetStore(filePath);
                var preset = new FacilityPreset
                {
                    Name = "Facility fixture",
                    IndustrySkillLevel = 4,
                    AdvancedIndustrySkillLevel = 3,
                    AccountingSkillLevel = 2,
                    BrokerRelationsSkillLevel = 1,
                    BrokerFactionStanding = 6.7,
                    BrokerCorpStanding = 4.3,
                    ManufacturingImplantPercent = 1.5,
                    SccIndustryFeePercent = 3.5,
                    AlphaAccountTaxPercent = 1.2,
                    BaseSalesTaxPercent = 4.4,
                    BaseBrokerFeePercent = 2.9,
                    SccBrokerFeeSurchargePercent = 0.4,
                    SpecialBrokerFeePercent = 0.8,
                    BrokerFeeMode = 2,
                    IncludeSalesTax = false,
                    IncludeBuyOrderBrokerFee = false,
                    DefaultBlueprintMe = 7,
                    DefaultBlueprintTe = 14,
                    ProductionLines = 8,
                    LaboratoryLines = 6,
                    SuggestBuildBlueprintsNotOwned = false,
                    BuildWhenMarketVolumeShort = false,
                    ConvertMineralsToOre = true,
                    PreferCompressedOre = false,
                    RefiningSkillLevel = 4,
                    ReprocessingSkillLevel = 3,
                    OreProcessingSkillLevel = 2,
                    ReprocessingImplantPercent = 1.1,
                    EncryptionSkillLevel = 3,
                    DatacoreSkill1Level = 2,
                    DatacoreSkill2Level = 1,
                    ScienceSkillLevel = 4
                };
                preset.Stations.Add(new FacilityStation
                {
                    Name = "Fixture Azbel",
                    SystemName = "Jita",
                    RegionId = 10000002,
                    SolarSystemId = 30000142,
                    FacilityType = "Azbel",
                    StructureTypeId = 35826,
                    ProductionType = "Capital Manufacturing",
                    RigSlot1TypeId = 37169,
                    RigSlot2TypeId = 43711,
                    RigSlot3TypeId = 43718,
                    ServiceModule1TypeId = 35881,
                    ServiceModule2TypeId = 35878,
                    ServiceModule3TypeId = 35886,
                    ServiceModule4TypeId = 35891,
                    ServiceModule5TypeId = 35899,
                    Security = 0.9,
                    MaterialMultiplier = 0.99,
                    TimeMultiplier = 0.77,
                    CostMultiplier = 0.88,
                    MaterialBonusPercent = 1.23,
                    IndustryTaxPercent = 0.25,
                    SalesFeePercent = 1.5,
                    FactionWarfareUpgradeLevel = 3,
                    SupportsProduction = false,
                    ValidationMessage = "Requires module"
                });

                store.Save(new ObservableCollection<FacilityPreset> { preset });
                var loaded = new FacilityPresetStore(filePath).Load();
                AssertEqual(1, loaded.Count, "facility preset store round-trips preset count");
                var loadedPreset = loaded[0];
                AssertEqual("Facility fixture", loadedPreset.Name, "facility preset name round-trips");
                AssertEqual(4, loadedPreset.IndustrySkillLevel, "facility preset industry skill round-trips");
                AssertEqual(3, loadedPreset.AdvancedIndustrySkillLevel, "facility preset advanced industry skill round-trips");
                AssertEqual(2, loadedPreset.AccountingSkillLevel, "facility preset accounting skill round-trips");
                AssertEqual(1, loadedPreset.BrokerRelationsSkillLevel, "facility preset broker skill round-trips");
                AssertEqual(6.7, loadedPreset.BrokerFactionStanding, 0.0001, "facility preset faction standing round-trips");
                AssertEqual(4.3, loadedPreset.BrokerCorpStanding, 0.0001, "facility preset corp standing round-trips");
                AssertEqual(1.5, loadedPreset.ManufacturingImplantPercent, 0.0001, "facility preset implant round-trips");
                AssertEqual(3.5, loadedPreset.SccIndustryFeePercent, 0.0001, "facility preset SCC fee round-trips");
                AssertEqual(1.2, loadedPreset.AlphaAccountTaxPercent, 0.0001, "facility preset alpha tax round-trips");
                AssertEqual(4.4, loadedPreset.BaseSalesTaxPercent, 0.0001, "facility preset sales tax round-trips");
                AssertEqual(2.9, loadedPreset.BaseBrokerFeePercent, 0.0001, "facility preset broker fee round-trips");
                AssertEqual(0.4, loadedPreset.SccBrokerFeeSurchargePercent, 0.0001, "facility preset broker surcharge round-trips");
                AssertEqual(0.8, loadedPreset.SpecialBrokerFeePercent, 0.0001, "facility preset special broker fee round-trips");
                AssertEqual(2, loadedPreset.BrokerFeeMode, "facility preset broker mode round-trips");
                AssertFalse(loadedPreset.IncludeSalesTax, "facility preset sales tax toggle round-trips");
                AssertFalse(loadedPreset.IncludeBuyOrderBrokerFee, "facility preset buy broker toggle round-trips");
                AssertEqual(7, loadedPreset.DefaultBlueprintMe, "facility preset default ME round-trips");
                AssertEqual(14, loadedPreset.DefaultBlueprintTe, "facility preset default TE round-trips");
                AssertEqual(8, loadedPreset.ProductionLines, "facility preset production lines round-trips");
                AssertEqual(6, loadedPreset.LaboratoryLines, "facility preset lab lines round-trips");
                AssertFalse(loadedPreset.SuggestBuildBlueprintsNotOwned, "facility preset build unowned toggle round-trips");
                AssertFalse(loadedPreset.BuildWhenMarketVolumeShort, "facility preset volume-short toggle round-trips");
                AssertTrue(loadedPreset.ConvertMineralsToOre, "facility preset ore conversion toggle round-trips");
                AssertFalse(loadedPreset.PreferCompressedOre, "facility preset compressed ore toggle round-trips");
                AssertEqual(4, loadedPreset.RefiningSkillLevel, "facility preset refining skill round-trips");
                AssertEqual(3, loadedPreset.ReprocessingSkillLevel, "facility preset reprocessing skill round-trips");
                AssertEqual(2, loadedPreset.OreProcessingSkillLevel, "facility preset ore skill round-trips");
                AssertEqual(1.1, loadedPreset.ReprocessingImplantPercent, 0.0001, "facility preset reprocessing implant round-trips");
                AssertEqual(3, loadedPreset.EncryptionSkillLevel, "facility preset encryption skill round-trips");
                AssertEqual(2, loadedPreset.DatacoreSkill1Level, "facility preset datacore 1 round-trips");
                AssertEqual(1, loadedPreset.DatacoreSkill2Level, "facility preset datacore 2 round-trips");
                AssertEqual(4, loadedPreset.ScienceSkillLevel, "facility preset science skill round-trips");

                AssertEqual(1, loadedPreset.Stations.Count, "facility preset station count round-trips");
                var station = loadedPreset.Stations[0];
                AssertEqual("Fixture Azbel", station.Name, "facility station name round-trips");
                AssertEqual("Jita", station.SystemName, "facility station system round-trips");
                AssertEqual(10000002, (int)station.RegionId, "facility station region round-trips");
                AssertEqual(30000142, (int)station.SolarSystemId, "facility station system id round-trips");
                AssertEqual("Azbel", station.FacilityType, "facility station type round-trips");
                AssertEqual(35826, station.StructureTypeId, "facility station structure type round-trips");
                AssertEqual("Capital Manufacturing", station.ProductionType, "facility station production type round-trips");
                AssertEqual(37169, station.RigSlot1TypeId, "facility station rig 1 round-trips");
                AssertEqual(43711, station.RigSlot2TypeId, "facility station rig 2 round-trips");
                AssertEqual(43718, station.RigSlot3TypeId, "facility station rig 3 round-trips");
                AssertEqual(35881, station.ServiceModule1TypeId, "facility station module 1 round-trips");
                AssertEqual(35878, station.ServiceModule2TypeId, "facility station module 2 round-trips");
                AssertEqual(35886, station.ServiceModule3TypeId, "facility station module 3 round-trips");
                AssertEqual(35891, station.ServiceModule4TypeId, "facility station module 4 round-trips");
                AssertEqual(35899, station.ServiceModule5TypeId, "facility station module 5 round-trips");
                AssertEqual(0.9, station.Security, 0.0001, "facility station security round-trips");
                AssertEqual(0.99, station.MaterialMultiplier, 0.0001, "facility station material multiplier round-trips");
                AssertEqual(0.77, station.TimeMultiplier, 0.0001, "facility station time multiplier round-trips");
                AssertEqual(0.88, station.CostMultiplier, 0.0001, "facility station cost multiplier round-trips");
                AssertEqual(1.23, station.MaterialBonusPercent, 0.0001, "facility station material bonus round-trips");
                AssertEqual(0.25, station.IndustryTaxPercent, 0.0001, "facility station industry tax round-trips");
                AssertEqual(1.5, station.SalesFeePercent, 0.0001, "facility station sales fee round-trips");
                AssertEqual(3, station.FactionWarfareUpgradeLevel, "facility station FW level round-trips");
                AssertFalse(station.SupportsProduction, "facility station validation state round-trips");
                AssertEqual("Requires module", station.ValidationMessage, "facility station validation message round-trips");

                loadedPreset.Name = "Updated fixture";
                station.SystemName = "Amarr";
                station.SolarSystemId = 30002187;
                station.RegionId = 10000043;
                station.SupportsProduction = true;
                station.ValidationMessage = "";
                loadedPreset.ConvertMineralsToOre = false;
                store.Save(loaded);
                var reloaded = new FacilityPresetStore(filePath).Load()[0];
                AssertEqual("Updated fixture", reloaded.Name, "facility preset update persists");
                AssertEqual("Amarr", reloaded.Stations[0].SystemName, "facility station system update persists");
                AssertEqual(30002187, (int)reloaded.Stations[0].SolarSystemId, "facility station system id update persists");
                AssertFalse(reloaded.ConvertMineralsToOre, "facility preset ore toggle update persists");
                AssertTrue(reloaded.Stations[0].SupportsProduction, "facility station support update persists");

                var expectedPresetName = "\u041d\u043e\u0432\u044b\u0439 \u043f\u0440\u0435\u0441\u0435\u0442";
                var expectedStationName = "\u041d\u043e\u0432\u0430\u044f \u0441\u0442\u0430\u043d\u0446\u0438\u044f";
                var expectedWarning = "\u0414\u043b\u044f Manufacturing \u043d\u0443\u0436\u0435\u043d service module";
                var mojibakePath = Path.Combine(directory, "FacilityPresets-Mojibake.xml");
                var mojibakeXml =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<FacilityPresets>" +
                    "<Preset Name=\"" + ToCp1251Mojibake(expectedPresetName) + "\">" +
                    "<Stations>" +
                    "<Station Name=\"" + ToCp1251Mojibake(expectedStationName) + "\" System=\"Jita\" RegionId=\"10000002\" SolarSystemId=\"30000142\" FacilityType=\"NPC Station\" StructureTypeId=\"0\" ProductionType=\"Manufacturing\" SupportsProduction=\"false\" ValidationMessage=\"" + ToCp1251Mojibake(expectedWarning) + "\" />" +
                    "</Stations>" +
                    "</Preset>" +
                    "</FacilityPresets>";
                File.WriteAllText(mojibakePath, mojibakeXml, Encoding.UTF8);
                var repaired = new FacilityPresetStore(mojibakePath).Load()[0];
                AssertEqual(expectedPresetName, repaired.Name, "facility preset store repairs mojibake preset names");
                AssertEqual(expectedStationName, repaired.Stations[0].Name, "facility preset store repairs mojibake station names");
                AssertEqual(expectedWarning, repaired.Stations[0].ValidationMessage, "facility preset store repairs mojibake validation messages");
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        private static string ToCp1251Mojibake(string value)
        {
            return Encoding.GetEncoding(1251).GetString(Encoding.UTF8.GetBytes(value));
        }

        private static void MarketPriceCacheService_UsesLatestCachedPriceAndUpserts()
        {
            var service = new MarketPriceCacheService();
            var cache = new ObservableCollection<MarketPriceCacheEntry>
            {
                new MarketPriceCacheEntry { RegionOrStationId = 1, TypeId = 34, SellMin = 5, BuyMax = 4, SellVolume = 10, BuyVolume = 8, UpdatedAt = new DateTime(2026, 5, 1) },
                new MarketPriceCacheEntry { RegionOrStationId = 1, TypeId = 34, SellMin = 7, BuyMax = 6, SellVolume = 12, BuyVolume = 9, UpdatedAt = new DateTime(2026, 5, 2) },
                new MarketPriceCacheEntry { RegionOrStationId = 2, TypeId = 34, SellMin = 99, UpdatedAt = new DateTime(2026, 5, 3) }
            };

            var cached = service.GetCachedPrices(cache, 1, new[] { 34L, 35L });

            AssertEqual(1, cached.Count, "market price cache filters by location and requested type IDs");
            AssertEqual(7, cached[34].SellMin, 0.0001, "market price cache uses latest cached entry");
            AssertEqual(12, (int)cached[34].SellVolume, "market price cache keeps latest sell volume");

            service.Upsert(
                cache,
                1,
                "The Forge",
                new Dictionary<long, MarketPrice>
                {
                    { 34, new MarketPrice { TypeId = 34, SellMin = 8, BuyMax = 7, SellVolume = 20, BuyVolume = 15 } },
                    { 35, new MarketPrice { TypeId = 35, SellMin = 3, BuyMax = 2, SellVolume = 5, BuyVolume = 4 } }
                },
                new Dictionary<long, string> { { 34, "Tritanium" } },
                new DateTime(2026, 5, 11, 10, 0, 0));

            AssertEqual(4, cache.Count, "market price cache upsert updates existing row and adds new rows");
            AssertEqual("Tritanium", cache.First(item => item.RegionOrStationId == 1 && item.TypeId == 34).TypeName, "market price cache applies known type names");
            AssertEqual("", cache.First(item => item.RegionOrStationId == 1 && item.TypeId == 35).TypeName, "market price cache tolerates missing type names");
            AssertEqual(8, cache.First(item => item.RegionOrStationId == 1 && item.TypeId == 34).SellMin, 0.0001, "market price cache updates sell price");
        }

        private static void MarketHistoryCacheService_MergesFallbacksAndUpserts()
        {
            var service = new MarketHistoryCacheService();
            var cache = new ObservableCollection<MarketHistoryStats>
            {
                new MarketHistoryStats { TypeId = 34, RegionId = 1, Days = 7, AverageDailyVolume = 10, TotalVolume = 70, TotalOrders = 2, PriceTrend = 1, UpdatedAt = new DateTime(2026, 5, 1) },
                new MarketHistoryStats { TypeId = 34, RegionId = 1, Days = 7, AverageDailyVolume = 12, TotalVolume = 84, TotalOrders = 3, PriceTrend = 2, UpdatedAt = new DateTime(2026, 5, 2) },
                new MarketHistoryStats { TypeId = 35, RegionId = 2, Days = 7, AverageDailyVolume = 99, UpdatedAt = new DateTime(2026, 5, 3) }
            };
            var fresh = new Dictionary<long, MarketHistoryStats>
            {
                { 34, new MarketHistoryStats { TypeId = 34, RegionId = 1, Days = 7, AverageDailyVolume = 0 } },
                { 36, new MarketHistoryStats { TypeId = 36, RegionId = 1, Days = 7, AverageDailyVolume = 5 } }
            };

            var merged = service.MergeCachedStats(fresh, cache, new[] { 34L, 35L, 36L }, 1, 7);

            AssertEqual(2, merged.Count, "market history cache merge keeps fresh rows and valid cached fallbacks");
            AssertEqual(12, merged[34].AverageDailyVolume, 0.0001, "market history cache replaces empty fresh stats with newest cached stats");
            AssertEqual(5, merged[36].AverageDailyVolume, 0.0001, "market history cache keeps valid fresh stats");
            AssertFalse(merged.ContainsKey(35), "market history cache ignores other regions");

            service.Upsert(
                cache,
                new[]
                {
                    new MarketHistoryStats { TypeId = 34, RegionId = 1, Days = 7, AverageDailyVolume = 20, TotalVolume = 140, TotalOrders = 4, PriceTrend = 3 },
                    new MarketHistoryStats { TypeId = 37, RegionId = 1, Days = 7, AverageDailyVolume = 2, TotalVolume = 14, TotalOrders = 1, PriceTrend = -1, UpdatedAt = new DateTime(2026, 5, 4) }
                },
                new DateTime(2026, 5, 11, 10, 0, 0));

            AssertEqual(4, cache.Count, "market history cache upsert updates existing rows and adds new rows");
            AssertEqual(20, cache.First(item => item.TypeId == 34 && item.RegionId == 1 && item.Days == 7).AverageDailyVolume, 0.0001, "market history cache updates existing volume");
            AssertTrue(cache.First(item => item.TypeId == 34 && item.RegionId == 1 && item.Days == 7).UpdatedAt == new DateTime(2026, 5, 11, 10, 0, 0), "market history cache fills missing timestamp");
            AssertTrue(cache.First(item => item.TypeId == 37).UpdatedAt == new DateTime(2026, 5, 4), "market history cache preserves provided timestamp");
        }

        private static void MarketPriceSelectionService_AppliesModesModifiersAndVolumeFallbacks()
        {
            var service = new MarketPriceSelectionService();
            var price = new MarketPrice { SellMin = 100, BuyMax = 80, SellVolume = 0, BuyVolume = 12 };

            AssertEqual(110, service.ApplyModifier(100, 10), 0.0001, "market price selection applies positive modifiers");
            AssertEqual(0, service.ApplyModifier(100, -150), 0.0001, "market price selection clamps negative modified values");
            AssertEqual(0, service.ApplyModifier(0, 10), 0.0001, "market price selection keeps zero prices at zero");
            AssertEqual(105, service.GetUnitPrice(price, "Min Sell", 5), 0.0001, "market price selection uses sell price by default");
            AssertEqual(72, service.GetUnitPrice(price, "Max Buy", -10), 0.0001, "market price selection uses buy price for Max Buy");
            AssertEqual(0, (int)service.GetStrictVolume(price, "Min Sell"), "strict market volume uses sell volume without fallback");
            AssertEqual(12, (int)service.GetProductVolume(price, "Min Sell"), "product market volume falls back to buy volume when sell volume is empty");
            AssertEqual(12, (int)service.GetProductVolume(price, "Max Buy"), "product market volume uses buy volume for Max Buy");
        }

        private static void SalesFeeService_AppliesTaxesBrokerAndStationFees()
        {
            var service = new SalesFeeService();
            var preset = new FacilityPreset
            {
                IncludeSalesTax = true,
                AccountingSkillLevel = 5,
                BaseSalesTaxPercent = 4.5,
                BrokerFeeMode = 1,
                BrokerRelationsSkillLevel = 5,
                BaseBrokerFeePercent = 3.0,
                BrokerFactionStanding = 5,
                BrokerCorpStanding = 5
            };

            AssertEqual(20.25, service.GetSalesTax(1000, preset), 0.0001, "sales fee service applies accounting skill to sales tax");
            AssertEqual(100, service.GetSalesBrokerFee(1000, preset), 0.0001, "sales fee service applies minimum broker fee");
            AssertEqual(879.75, service.ApplySalesTaxesAndFees(1000, preset, new FacilityStation { SalesFeePercent = 10 }), 0.0001, "sales fee service excludes station fee when sales tax is included");

            preset.IncludeSalesTax = false;
            preset.BrokerFeeMode = 2;
            preset.SpecialBrokerFeePercent = 1;
            preset.SccBrokerFeeSurchargePercent = 0.5;
            AssertEqual(150, service.GetSalesBrokerFee(10000, preset), 0.0001, "sales fee service supports explicit special broker fee mode");
            AssertEqual(9750, service.ApplySalesTaxesAndFees(10000, preset, new FacilityStation { SalesFeePercent = 1 }), 0.0001, "sales fee service applies station sales fee when tax is excluded");
            AssertEqual(0, service.ApplySalesTaxesAndFees(-1, preset, null), 0.0001, "sales fee service clamps non-positive gross value");
        }

        private static void BuildQueueService_MergesEquivalentBlueprintRows()
        {
            var service = new BuildQueueService();
            var queue = new ObservableCollection<BuildQueueItem>();
            var blueprint = Blueprint("Revelation", productTypeId: 19720);
            var updatedBlueprint = Blueprint("Revelation", productTypeId: 19720);
            updatedBlueprint.Profit = 123;

            var added = service.AddOrMerge(queue, new BuildQueueItem
            {
                Blueprint = blueprint,
                Runs = 2,
                MaterialEfficiency = 10,
                TimeEfficiency = 20,
                DecryptorTypeId = 0
            });

            var merged = service.AddOrMerge(queue, new BuildQueueItem
            {
                Blueprint = updatedBlueprint,
                Runs = 0,
                MaterialEfficiency = 10,
                TimeEfficiency = 20,
                DecryptorTypeId = 0
            });

            var secondAdded = service.AddOrMerge(queue, new BuildQueueItem
            {
                Blueprint = blueprint,
                Runs = 1,
                MaterialEfficiency = 8,
                TimeEfficiency = 20,
                DecryptorTypeId = 0
            });

            AssertTrue(added, "build queue service reports new rows");
            AssertFalse(merged, "build queue service reports merged rows");
            AssertTrue(secondAdded, "build queue service keeps different ME rows separate");
            AssertEqual(2, queue.Count, "build queue service merges only equivalent queue keys");
            AssertEqual(3, queue[0].Runs, "build queue service adds at least one run when merging");
            AssertEqual(123, queue[0].Blueprint.Profit, 0.0001, "build queue service refreshes merged blueprint reference");
            AssertEqual("Очередь стройки: 2", service.FormatStatus(queue.Count), "build queue service formats queue status");
        }

        private static void BuildQueueService_SelectsProfitableCandidatesInRankOrder()
        {
            var service = new BuildQueueService();
            var rankedSecond = Blueprint("Rank 2", productTypeId: 2);
            rankedSecond.Profit = 100;
            rankedSecond.ProfitRank = 2;
            rankedSecond.SvrTimesIskPerHour = 1;
            rankedSecond.EstimateStatus = "OK";

            var rankedFirst = Blueprint("Rank 1", productTypeId: 1);
            rankedFirst.Profit = 10;
            rankedFirst.ProfitRank = 1;
            rankedFirst.SvrTimesIskPerHour = 1;
            rankedFirst.EstimateStatus = "ok - estimated";

            var unrankedHighScore = Blueprint("Unranked high score", productTypeId: 3);
            unrankedHighScore.Profit = 500;
            unrankedHighScore.SvrTimesIskPerHour = 1000;
            unrankedHighScore.EstimateStatus = "OK";

            var unrankedLowScore = Blueprint("Unranked low score", productTypeId: 4);
            unrankedLowScore.Profit = 900;
            unrankedLowScore.SvrTimesIskPerHour = 1;
            unrankedLowScore.EstimateStatus = "OK";

            var loss = Blueprint("Loss", productTypeId: 5);
            loss.Profit = -1;
            loss.EstimateStatus = "OK";

            var notEstimated = Blueprint("Not estimated", productTypeId: 6);
            notEstimated.Profit = 1000;
            notEstimated.EstimateStatus = "";

            var selected = service.SelectProfitableCandidates(
                new[] { unrankedLowScore, rankedSecond, loss, unrankedHighScore, notEstimated, rankedFirst },
                3);

            AssertEqual(3, selected.Count, "build queue service limits profitable candidates");
            AssertEqual(1, (int)selected[0].ProductTypeId, "build queue service prioritizes lower positive rank first");
            AssertEqual(2, (int)selected[1].ProductTypeId, "build queue service keeps ranked rows before unranked rows");
            AssertEqual(3, (int)selected[2].ProductTypeId, "build queue service orders unranked rows by SVR times ISK/hour");
        }

        private static void BuildProjectDecisionService_SetsUpdatesAndClearsModes()
        {
            var service = new BuildProjectDecisionService();
            var project = new BuildProject();

            AssertEqual("", service.GetBuildBuyMode(project, 100), "project decision service defaults to auto mode");

            service.SetBuildBuyMode(project, 100, "Buy");
            AssertEqual("Buy", service.GetBuildBuyMode(project, 100), "project decision service saves buy mode");
            AssertEqual(1, project.BuildBuyDecisions.Count, "project decision service adds one decision row");

            service.SetBuildBuyMode(project, 100, "Build");
            AssertEqual("Build", service.GetBuildBuyMode(project, 100), "project decision service updates existing decision row");
            AssertEqual(1, project.BuildBuyDecisions.Count, "project decision service does not duplicate decision rows");

            service.SetBuildBuyMode(project, 100, "Auto");
            AssertEqual("", service.GetBuildBuyMode(project, 100), "project decision service clears auto mode");
            AssertEqual(0, project.BuildBuyDecisions.Count, "project decision service removes auto decision rows");

            service.SetBuildBuyMode(project, 100, "Buy");
            service.SetBuildBuyMode(project, 100, "");
            AssertEqual(0, project.BuildBuyDecisions.Count, "project decision service clears blank mode");

            service.SetBuildBuyMode(project, 0, "Buy");
            AssertEqual(0, project.BuildBuyDecisions.Count, "project decision service ignores invalid type IDs");
        }

        private static void UsedByListService_AppendsUniqueNames()
        {
            var service = new UsedByListService();

            AssertEqual("", service.Add(null, null), "used-by list tolerates empty inputs");
            AssertEqual("Revelation", service.Add("", "Revelation"), "used-by list starts with first name");
            AssertEqual("Revelation, Moros", service.Add("Revelation", "Moros"), "used-by list appends new names");
            AssertEqual("Revelation, Moros", service.Add("Revelation, Moros", "Moros"), "used-by list avoids duplicate names");
            AssertEqual("Revelation", service.Add("Revelation", ""), "used-by list ignores blank names");
        }

        private static void BlueprintProductionTypeService_ClassifiesProductionTypesAndTiming()
        {
            var service = new BlueprintProductionTypeService();
            var reaction = Blueprint("Fulleride", groupName: "Composite", hasReactionActivity: true);
            var super = Blueprint("Avatar", groupName: "Titan");
            super.GroupId = 30;
            var capitalComponent = Blueprint("Capital Armor Plates", groupName: "Capital Component");
            capitalComponent.GroupId = 873;
            var subsystem = Blueprint("Subsystem", categoryName: "Subsystem");
            subsystem.CategoryId = 32;
            var capitalByName = Blueprint("Capital Ship", groupName: "Capital Industrial", categoryName: "Ship");

            AssertEqual("Manufacturing", service.GetProductionType(null), "production type defaults to manufacturing");
            AssertEqual("Reactions", service.GetProductionType(reaction), "production type classifies reaction activity");
            AssertEqual("Supercapital Manufacturing", service.GetProductionType(super), "production type classifies supercapital groups");
            AssertEqual("Capital Components", service.GetProductionType(capitalComponent), "production type classifies capital component groups");
            AssertEqual("Subsystem Manufacturing", service.GetProductionType(subsystem), "production type classifies subsystem category");
            AssertEqual("Capital Manufacturing", service.GetProductionType(capitalByName), "production type classifies capital ships by names");
            AssertEqual("T3 Invention", service.GetInventionProductionType(subsystem), "invention production type classifies T3 categories");
            AssertEqual(1, service.GetBuildWave("Reactions"), "build wave puts reactions first");
            AssertEqual(2, service.GetBuildWave("Capital Components"), "build wave puts components second");
            AssertEqual(3, service.GetBuildWave("Manufacturing"), "build wave puts final manufacturing third");
            AssertEqual(1.0, service.GetFactionWarfareCostMultiplier(0), 0.0001, "FW multiplier defaults to one");
            AssertEqual(0.5, service.GetFactionWarfareCostMultiplier(5), 0.0001, "FW multiplier clamps at level five");
            AssertEqual(8, service.GetParallelProductionTime(new[] { 5.0, 4.0, 4.0, 3.0 }, 2), 0.0001, "parallel production time balances jobs across lines");
            AssertEqual(300, service.CalculateBlueprintProductionTime(100, 7, 1.5, new FacilityPreset { ProductionLines = 4 }), 0.0001, "production time batches runs by available lines");
        }

        private static void ProjectMaterialImportService_ParsesClipboardQuantities()
        {
            var service = new ProjectMaterialImportService();
            var materials = new[]
            {
                new BuildProjectMaterial { TypeId = 34, Name = "Tritanium" },
                new BuildProjectMaterial { TypeId = 35, Name = "Capital Armor Plates" },
                new BuildProjectMaterial { TypeId = 36, Name = "Capital Armor" }
            };
            var text = "Tritanium\t1 234\r\n"
                       + "Capital Armor Plates 2,500\n"
                       + "capital armor 3.000\n"
                       + "Unknown Material 999\n"
                       + "Tritanium\tquantity\t5";

            var imported = service.ImportBoughtQuantities(text, materials);

            AssertEqual(3, imported.Count, "project material import parses only known materials");
            AssertEqual(1239, (int)imported[34], "project material import aggregates repeated exact-name rows");
            AssertEqual(2500, (int)imported[35], "project material import prefers longer material names for prefix matches");
            AssertEqual(3000, (int)imported[36], "project material import parses case-insensitive prefix matches");

            long quantity;
            AssertTrue(service.TryParseQuantity("1\u00A0234,56", out quantity), "project material import parses grouped quantity text");
            AssertEqual(123456, (int)quantity, "project material import strips separators to match existing clipboard behavior");
            AssertFalse(service.TryParseQuantity("none", out quantity), "project material import rejects non-numeric text");
        }

        private static void BlueprintEfficiencyService_AppliesInventionCopyAndChildDefaults()
        {
            var service = new BlueprintEfficiencyService();
            var decryptor = new DecryptorOption { MaterialEfficiencyModifier = 1, TimeEfficiencyModifier = 2 };
            var tech2 = Blueprint("T2 Module");
            tech2.TechLevel = 2;
            var copyOnly = Blueprint("Zirnitra", groupName: "Dreadnought");
            copyOnly.IsCopyOnlyBlueprint = true;
            var reaction = Blueprint("Fulleride", hasReactionActivity: true);
            var t1 = Blueprint("T1 Module");
            t1.TechLevel = 1;

            double me;
            double te;
            service.GetEffectiveEfficiency(tech2, 10, 20, decryptor, useInventionCosts: true, out me, out te);
            AssertEqual(3, me, 0.0001, "blueprint efficiency service applies invention base ME plus decryptor");
            AssertEqual(6, te, 0.0001, "blueprint efficiency service applies invention base TE plus decryptor");

            service.GetEffectiveEfficiency(copyOnly, 10, 20, decryptor, useInventionCosts: false, out me, out te);
            AssertEqual(0, me, 0.0001, "blueprint efficiency service forces copy-only ME to zero");
            AssertEqual(0, te, 0.0001, "blueprint efficiency service forces copy-only TE to zero");

            service.GetDefaultChildEfficiency(reaction, null, decryptor, false, 0, 0, out me, out te);
            AssertEqual(0, me, 0.0001, "blueprint efficiency service gives reaction children zero ME");
            AssertEqual(0, te, 0.0001, "blueprint efficiency service gives reaction children zero TE");

            service.GetDefaultChildEfficiency(t1, new FacilityPreset { DefaultBlueprintMe = 9, DefaultBlueprintTe = 18 }, decryptor, true, 7, 14, out me, out te);
            AssertEqual(7, me, 0.0001, "blueprint efficiency service prefers owned child ME");
            AssertEqual(14, te, 0.0001, "blueprint efficiency service prefers owned child TE");

            service.GetDefaultChildEfficiency(tech2, null, decryptor, false, 0, 0, out me, out te);
            AssertEqual(3, me, 0.0001, "blueprint efficiency service defaults T2 child ME from invention");
            AssertEqual(6, te, 0.0001, "blueprint efficiency service defaults T2 child TE from invention");

            service.GetDefaultChildEfficiency(t1, new FacilityPreset { DefaultBlueprintMe = 9, DefaultBlueprintTe = 18 }, null, false, 0, 0, out me, out te);
            AssertEqual(9, me, 0.0001, "blueprint efficiency service uses facility default child ME for T1");
            AssertEqual(18, te, 0.0001, "blueprint efficiency service uses facility default child TE for T1");
        }

        private static void SalesVolumeRatioService_AppliesLiquidityToBlueprintsAndProjects()
        {
            var service = new SalesVolumeRatioService();
            var history = new Dictionary<long, MarketHistoryStats>
            {
                { 100, new MarketHistoryStats { TypeId = 100, AverageDailyVolume = 24, TotalVolume = 240, TotalOrders = 12, PriceTrend = 1.5 } },
                { 200, new MarketHistoryStats { TypeId = 200, AverageDailyVolume = 0, TotalVolume = 10, TotalOrders = 0, PriceTrend = -1 } }
            };
            var blueprint = Blueprint("Liquid", productTypeId: 100);
            blueprint.IskPerHour = 3600;

            service.ApplyToBlueprint(blueprint, history, producedQuantity: 2, productionTimeSeconds: 3600);

            AssertEqual(0.5, blueprint.SalesVolumeRatio, 0.0001, "SVR service calculates blueprint sales volume ratio");
            AssertEqual(1800, blueprint.SvrTimesIskPerHour, 0.0001, "SVR service multiplies blueprint SVR by ISK/hour");
            AssertEqual(20, blueprint.AverageItemsPerOrder, 0.0001, "SVR service calculates average items per order");
            AssertEqual(1.5, blueprint.PriceTrend, 0.0001, "SVR service copies price trend");

            var projectItem = new BuildProjectItem
            {
                ProductTypeId = 100,
                Runs = 2,
                PortionSize = 1,
                ProductionTimeSeconds = 3600,
                Profit = 7200
            };
            service.ApplyToProjectItem(projectItem, history);

            AssertEqual(0.5, projectItem.SalesVolumeRatio, 0.0001, "SVR service calculates project item sales volume ratio");
            AssertEqual(3600, projectItem.SvrTimesIskPerHour, 0.0001, "SVR service calculates project item SVR times ISK/hour from profit/time");

            var dry = Blueprint("Dry", productTypeId: 200);
            dry.SalesVolumeRatio = 9;
            dry.SvrTimesIskPerHour = 9;
            service.ApplyToBlueprint(dry, history, producedQuantity: 1, productionTimeSeconds: 100);
            AssertEqual(0, dry.SalesVolumeRatio, 0.0001, "SVR service resets rows with zero average daily volume");
            AssertEqual(0, dry.SvrTimesIskPerHour, 0.0001, "SVR service clears score when no market volume exists");
        }

        private static void MarketCacheStores_RoundTripPriceAndHistoryCaches()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            var pricePath = Path.Combine(directory, "MarketPriceCache.xml");
            var historyPath = Path.Combine(directory, "MarketHistoryCache.xml");

            var priceStore = new MarketPriceCacheStore(pricePath);
            priceStore.Save(new ObservableCollection<MarketPriceCacheEntry>
            {
                new MarketPriceCacheEntry
                {
                    RegionOrStationId = 60003760,
                    LocationName = "Jita 4-4",
                    TypeId = 34,
                    TypeName = "Tritanium",
                    SellMin = 5.5,
                    BuyMax = 4.5,
                    SellVolume = 100,
                    BuyVolume = 90,
                    UpdatedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc)
                }
            });

            var prices = new MarketPriceCacheStore(pricePath).Load();
            AssertEqual(1, prices.Count, "market price cache store reloads one row");
            AssertEqual(60003760, (int)prices[0].RegionOrStationId, "market price cache store reloads location id");
            AssertEqual("Tritanium", prices[0].TypeName, "market price cache store reloads type name");
            AssertEqual(5.5, prices[0].SellMin, 0.0001, "market price cache store reloads sell price");
            AssertEqual(100, (int)prices[0].SellVolume, "market price cache store reloads sell volume");

            var historyStore = new MarketHistoryCacheStore(historyPath);
            historyStore.Save(new ObservableCollection<MarketHistoryStats>
            {
                new MarketHistoryStats
                {
                    TypeId = 34,
                    RegionId = 10000002,
                    Days = 7,
                    AverageDailyVolume = 42.5,
                    TotalVolume = 300,
                    TotalOrders = 12,
                    PriceTrend = -0.25,
                    UpdatedAt = new DateTime(2026, 5, 11, 11, 0, 0, DateTimeKind.Utc)
                }
            });

            var history = new MarketHistoryCacheStore(historyPath).Load();
            AssertEqual(1, history.Count, "market history cache store reloads one row");
            AssertEqual(34, (int)history[0].TypeId, "market history cache store reloads type id");
            AssertEqual(7, history[0].Days, "market history cache store reloads days");
            AssertEqual(42.5, history[0].AverageDailyVolume, 0.0001, "market history cache store reloads average volume");
            AssertEqual(-0.25, history[0].PriceTrend, 0.0001, "market history cache store reloads trend");

            Directory.Delete(directory, recursive: true);
        }

        private static void ContractPriceStore_RoundTripsMetadataAndDropsStaleSamples()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directory, "ContractPrices.xml");
            var store = new ContractPriceStore(filePath);
            store.Save(new ObservableCollection<ContractPriceSample>
            {
                new ContractPriceSample
                {
                    TypeId = 19720,
                    TypeName = "Revelation",
                    Price = 4_000_000_000,
                    ObservedAt = DateTime.Now.AddDays(-1),
                    ContractId = 123,
                    Source = "ESI Public",
                    LocationId = 60003760,
                    Quantity = 2,
                    ItemCount = 1,
                    Title = "Revelation x2"
                },
                new ContractPriceSample
                {
                    TypeId = 19720,
                    TypeName = "Old Revelation",
                    Price = 1,
                    ObservedAt = DateTime.Now.AddDays(-121)
                },
                new ContractPriceSample
                {
                    TypeId = 0,
                    TypeName = "Invalid",
                    Price = 1,
                    ObservedAt = DateTime.Now
                }
            });

            var loaded = new ContractPriceStore(filePath).Load();

            AssertEqual(1, loaded.Count, "contract price store keeps only valid fresh samples");
            AssertEqual(19720, (int)loaded[0].TypeId, "contract price store reloads type id");
            AssertEqual("Revelation", loaded[0].TypeName, "contract price store reloads type name");
            AssertEqual(4_000_000_000, loaded[0].Price, 0.0001, "contract price store reloads price");
            AssertEqual(123, (int)loaded[0].ContractId, "contract price store reloads contract id");
            AssertEqual("ESI Public", loaded[0].Source, "contract price store reloads source");
            AssertEqual(60003760, (int)loaded[0].LocationId, "contract price store reloads location id");
            AssertEqual(2, (int)loaded[0].Quantity, "contract price store reloads quantity");
            AssertEqual(1, loaded[0].ItemCount, "contract price store reloads item count");
            AssertEqual("Revelation x2", loaded[0].Title, "contract price store reloads title");

            Directory.Delete(directory, recursive: true);
        }

        private static void CopyOnlyBlueprintQueue_ShowsBpcRunsAndZeroEfficiency()
        {
            var blueprint = Blueprint("Zirnitra", groupName: "Dreadnought");
            blueprint.IsCopyOnlyBlueprint = true;
            blueprint.MaxProductionLimit = 1;

            var queueItem = new BuildQueueItem
            {
                Blueprint = blueprint,
                Runs = 2,
                MaterialEfficiency = 10,
                TimeEfficiency = 20
            };

            AssertEqual("BPC 2x1 | ME 0 / TE 0", queueItem.EfficiencyText.Replace("\u00A0", " "), "copy-only queue rows show one-run BPC usage and zero ME/TE");
            AssertTrue(blueprint.BlueprintAvailabilityText.Contains("BPC only"), "copy-only blueprint exposes BPC-only availability text");
        }

        private static void BlueprintCopyStatusService_FormatsCopyOnlyRuns()
        {
            var service = new BlueprintCopyStatusService();
            var normal = Blueprint("Revelation", groupName: "Dreadnought");
            var copyOnly = Blueprint("Zirnitra", groupName: "Dreadnought");
            copyOnly.IsCopyOnlyBlueprint = true;
            copyOnly.MaxProductionLimit = 1;

            AssertEqual("OK", service.AddCopyStatus(normal, 3, "OK"), "normal blueprints keep estimate status unchanged");
            AssertEqual("OK | BPC only ME/TE 0/0, 3x1 run", service.AddCopyStatus(copyOnly, 3, "OK"), "copy-only blueprints show needed BPC copies");

            copyOnly.MaxProductionLimit = 10;
            AssertEqual("OK | BPC only ME/TE 0/0, 2x10 run", service.AddCopyStatus(copyOnly, 11, "OK"), "copy-only blueprints round up copies by max production limit");

            copyOnly.MaxProductionLimit = 0;
            AssertEqual("OK | BPC only ME/TE 0/0, 1x1 run", service.AddCopyStatus(copyOnly, 0, "OK"), "copy-only status clamps invalid runs and max limit");
        }

        private static void BlueprintEstimateApplicationService_AppliesAndResetsEstimateFields()
        {
            var service = new BlueprintEstimateApplicationService();
            var blueprint = Blueprint("Revelation", groupName: "Dreadnought");
            blueprint.ProducedQuantity = 9;
            blueprint.ProductMarketVolume = 500;
            blueprint.ContractUnitPrice = 123;
            blueprint.ProductPriceSource = "Contract";
            blueprint.SalesVolumeRatio = 2;
            blueprint.SvrTimesIskPerHour = 3;
            blueprint.TotalItemsSold = 4;
            blueprint.TotalOrdersFilled = 5;
            blueprint.AverageItemsPerOrder = 6;
            blueprint.PriceTrend = 0.5;
            blueprint.CurrentBuyOrders = 7;
            blueprint.CurrentSellOrders = 8;

            service.Apply(
                blueprint,
                new BlueprintEstimate
                {
                    MaterialCost = 100,
                    InstallationCost = 20,
                    InventionCost = 30,
                    FacilityMaterialBonusPercent = 1.5,
                    CostIndexPercent = 2.5,
                    ProductionTimeSeconds = 3600,
                    InventionChancePercent = 45,
                    InventionJobs = 3,
                    InventedRunsPerSuccess = 10,
                    InventionMaterialsCost = 11,
                    InventionCopyCost = 12,
                    InventionJobUsageCost = 13,
                    InventionStationName = "Science Station",
                    InventionStationSystem = "Jita",
                    CopyStationName = "Copy Station",
                    CopyStationSystem = "Perimeter",
                    BuildMaterialLines = 4,
                    BuyMaterialLines = 2
                },
                totalCost: 150,
                revenue: 250,
                status: "OK + invention",
                station: new FacilityStation { Name = "Azbel", SystemName = "Jita" });

            AssertEqual(100, blueprint.MaterialCost, 0.0001, "estimate application copies material cost");
            AssertEqual(20, blueprint.InstallationCost, 0.0001, "estimate application copies installation cost");
            AssertEqual(30, blueprint.InventionCost, 0.0001, "estimate application copies invention cost");
            AssertEqual(150, blueprint.TotalCost, 0.0001, "estimate application applies total cost");
            AssertEqual(250, blueprint.Revenue, 0.0001, "estimate application applies revenue");
            AssertEqual(100, blueprint.Profit, 0.0001, "estimate application recalculates profit");
            AssertEqual(100, blueprint.IskPerHour, 0.0001, "estimate application calculates isk per hour");
            AssertEqual(66.6666, blueprint.ReturnOnInvestmentPercent, 0.001, "estimate application calculates ROI");
            AssertEqual(1.5, blueprint.FacilityMaterialBonusPercent, 0.0001, "estimate application copies facility material bonus");
            AssertEqual(2.5, blueprint.CostIndexPercent, 0.0001, "estimate application copies cost index");
            AssertEqual(45, blueprint.InventionChancePercent, 0.0001, "estimate application copies invention chance");
            AssertEqual(3, blueprint.InventionJobs, "estimate application copies invention jobs");
            AssertEqual(10, blueprint.InventedRunsPerSuccess, "estimate application copies invented runs");
            AssertEqual("Science Station", blueprint.InventionStationName, "estimate application copies invention station");
            AssertEqual("Copy Station", blueprint.CopyStationName, "estimate application copies copy station");
            AssertEqual(4, blueprint.BuildMaterialLines, "estimate application copies build material line count");
            AssertEqual(2, blueprint.BuyMaterialLines, "estimate application copies buy material line count");
            AssertEqual("OK + invention", blueprint.EstimateStatus, "estimate application copies status");
            AssertEqual("Azbel", blueprint.BestFacilityName, "estimate application copies facility name");
            AssertEqual("Jita", blueprint.BestFacilitySystem, "estimate application copies facility system");
            AssertEqual(0, (int)blueprint.ProducedQuantity, "estimate application clears produced quantity until caller sets it");
            AssertEqual(0, (int)blueprint.ProductMarketVolume, "estimate application clears product market volume until caller sets it");
            AssertEqual(0, blueprint.ContractUnitPrice, 0.0001, "estimate application clears contract price until caller sets it");
            AssertEqual("", blueprint.ProductPriceSource, "estimate application clears price source until caller sets it");
            AssertEqual(0, blueprint.SalesVolumeRatio, 0.0001, "estimate application clears stale SVR");
            AssertEqual(0, (int)blueprint.CurrentSellOrders, "estimate application clears stale order counts");

            service.Apply(
                blueprint,
                new BlueprintEstimate
                {
                    MaterialCost = 500,
                    InstallationCost = 50,
                    InventionCost = 25,
                    ProductionTimeSeconds = 1800,
                    BuildMaterialLines = 7,
                    BuyMaterialLines = 8
                },
                totalCost: 575,
                revenue: 1000,
                productPriceResult: new ContractPriceResult
                {
                    UnitPrice = 1000,
                    ContractUnitPrice = 950,
                    Source = "Contract",
                    Detail = "Contract median from 3/3 samples"
                },
                producedQuantity: 2,
                productMarketVolume: 42,
                status: "OK",
                station: new FacilityStation { Name = "Sotiyo", SystemName = "Amarr" });

            AssertEqual(575, blueprint.TotalCost, 0.0001, "estimate application display overload applies total cost");
            AssertEqual(1000, blueprint.Revenue, 0.0001, "estimate application display overload applies revenue");
            AssertEqual(425, blueprint.Profit, 0.0001, "estimate application display overload recalculates profit");
            AssertEqual(850, blueprint.IskPerHour, 0.0001, "estimate application display overload recalculates isk per hour");
            AssertEqual(2, (int)blueprint.ProducedQuantity, "estimate application display overload copies produced quantity");
            AssertEqual(42, (int)blueprint.ProductMarketVolume, "estimate application display overload copies product market volume");
            AssertEqual(950, blueprint.ContractUnitPrice, 0.0001, "estimate application display overload copies contract unit price");
            AssertEqual("Contract", blueprint.ProductPriceSource, "estimate application display overload copies product price source");
            AssertEqual("Contract median from 3/3 samples", blueprint.ProductPriceDetails, "estimate application display overload copies product price details");
            AssertEqual(7, blueprint.BuildMaterialLines, "estimate application display overload keeps build line count");
            AssertEqual(8, blueprint.BuyMaterialLines, "estimate application display overload keeps buy line count");
            AssertEqual("Sotiyo", blueprint.BestFacilityName, "estimate application display overload copies facility name");
        }

        private static void ProjectItemEstimateApplicationService_AppliesAndResetsProjectEstimateFields()
        {
            var service = new ProjectItemEstimateApplicationService();
            var item = new BuildProjectItem
            {
                ProductName = "Revelation",
                Runs = 2,
                PortionSize = 1,
                MaterialCost = 999,
                Profit = 999,
                SalesVolumeRatio = 9,
                TotalItemsSold = 9
            };

            service.ResetUnavailable(item, "No station");
            AssertEqual(0, item.MaterialCost, 0.0001, "project estimate reset clears material cost");
            AssertEqual(0, item.Profit, 0.0001, "project estimate reset clears profit");
            AssertEqual(0, item.SalesVolumeRatio, 0.0001, "project estimate reset clears stale SVR");
            AssertEqual(0, (int)item.TotalItemsSold, "project estimate reset clears stale history count");
            AssertEqual("No station", item.EstimateStatus, "project estimate reset applies status");

            service.Apply(
                item,
                new BlueprintEstimate
                {
                    MaterialCost = 100,
                    InstallationCost = 20,
                    InventionCost = 30,
                    ProductionTimeSeconds = 7200,
                    InventionChancePercent = 48,
                    InventionJobs = 4,
                    InventedRunsPerSuccess = 10,
                    InventionMaterialsCost = 11,
                    InventionCopyCost = 12,
                    InventionJobUsageCost = 13,
                    InventionStationName = "Science Station",
                    CopyStationName = "Copy Station",
                    BuildMaterialLines = 5,
                    BuyMaterialLines = 3
                },
                revenue: 350,
                productPriceResult: new ContractPriceResult
                {
                    ContractUnitPrice = 175,
                    Source = "Contract",
                    Detail = "median"
                },
                productMarketVolume: 42,
                status: "OK",
                station: new FacilityStation { Name = "Azbel", SystemName = "Jita" });

            AssertEqual("Azbel", item.BestStationName, "project estimate application copies best station");
            AssertEqual(100, item.MaterialCost, 0.0001, "project estimate application copies material cost");
            AssertEqual(20, item.InstallationCost, 0.0001, "project estimate application copies installation cost");
            AssertEqual(30, item.InventionCost, 0.0001, "project estimate application copies invention cost");
            AssertEqual(350, item.Revenue, 0.0001, "project estimate application copies revenue");
            AssertEqual(200, item.Profit, 0.0001, "project estimate application calculates profit");
            AssertEqual(133.3333, item.ReturnOnInvestmentPercent, 0.001, "project estimate application calculates ROI");
            AssertEqual(175, item.ContractUnitPrice, 0.0001, "project estimate application copies contract price");
            AssertEqual("Contract", item.ProductPriceSource, "project estimate application copies price source");
            AssertEqual("median", item.ProductPriceDetails, "project estimate application copies price detail");
            AssertEqual(42, (int)item.ProductMarketVolume, "project estimate application copies market volume");
            AssertEqual(5, item.BuildMaterialLines, "project estimate application copies build line count");
            AssertEqual(3, item.BuyMaterialLines, "project estimate application copies buy line count");
            AssertEqual("OK", item.EstimateStatus, "project estimate application copies status");
        }

        private static void ProjectItemEstimateDisplayService_AppliesSummaryStatusAndPriceDisplay()
        {
            var service = new ProjectItemEstimateDisplayService();
            var item = new BuildProjectItem
            {
                ProductName = "Zirnitra",
                Runs = 2,
                PortionSize = 3
            };
            var blueprint = Blueprint("Zirnitra", groupName: "Dreadnought");
            blueprint.IsCopyOnlyBlueprint = true;
            blueprint.MaxProductionLimit = 1;

            service.Apply(
                item,
                blueprint,
                new BlueprintEstimate
                {
                    MaterialCost = 100,
                    InstallationCost = 20,
                    InventionCost = 30,
                    ProductionTimeSeconds = 3600,
                    BuildMaterialLines = 6,
                    BuyMaterialLines = 4
                },
                new ContractPriceResult
                {
                    UnitPrice = 100,
                    ContractUnitPrice = 95,
                    Source = "Contract",
                    Detail = "median"
                },
                productMarketVolume: 42,
                facilityPreset: new FacilityPreset { IncludeSalesTax = false, BrokerFeeMode = 0 },
                station: new FacilityStation { Name = "Azbel", SystemName = "Jita" },
                hasPriceCache: true);

            AssertEqual(600, item.Revenue, 0.0001, "project display service calculates net revenue from unit price and total quantity");
            AssertEqual(450, item.Profit, 0.0001, "project display service applies profitability through application service");
            AssertEqual(300, item.ReturnOnInvestmentPercent, 0.0001, "project display service applies ROI through application service");
            AssertEqual(95, item.ContractUnitPrice, 0.0001, "project display service copies contract unit price");
            AssertEqual("Contract", item.ProductPriceSource, "project display service copies price source");
            AssertEqual("median", item.ProductPriceDetails, "project display service copies price details");
            AssertEqual(42, (int)item.ProductMarketVolume, "project display service copies product market volume");
            AssertEqual(6, item.BuildMaterialLines, "project display service copies build line count");
            AssertEqual(4, item.BuyMaterialLines, "project display service copies buy line count");
            AssertEqual("OK + invention | BPC only ME/TE 0/0, 2x1 run", item.EstimateStatus, "project display service appends copy-only project status");
            AssertEqual("Azbel", item.BestStationName, "project display service copies station name");
        }

        private static void ProjectQueueDisplayService_FormatsJobMaterialStatus()
        {
            var service = new ProjectQueueDisplayService();
            var job = new BuildProjectItem { Wave = 2, ProductName = "Revelation" };
            var materials = new[]
            {
                new BuildProjectMaterial { Wave = 2, UsedBy = "revelation", Name = "Tritanium", Quantity = 100, OwnedQuantity = 25 },
                new BuildProjectMaterial { Wave = 2, UsedBy = "Revelation", Name = "Megacyte", Quantity = 10, OwnedQuantity = 0 },
                new BuildProjectMaterial { Wave = 2, UsedBy = "Revelation", Name = "Zydrine", Quantity = 20, OwnedQuantity = 0 },
                new BuildProjectMaterial { Wave = 2, UsedBy = "Revelation", Name = "Mexallon", Quantity = 5, OwnedQuantity = 0 },
                new BuildProjectMaterial { Wave = 1, UsedBy = "Revelation", Name = "Ignored wave", Quantity = 999, OwnedQuantity = 0 },
                new BuildProjectMaterial { Wave = 2, UsedBy = "Other", Name = "Ignored item", Quantity = 999, OwnedQuantity = 0 }
            };

            AssertEqual("Need: Tritanium 75; Zydrine 20; Megacyte 10; +1",
                service.GetJobMaterialStatus(job, materials),
                "project queue display service formats top material blockers for a job");

            AssertEqual("Ready",
                service.GetJobMaterialStatus(job, new[]
                {
                    new BuildProjectMaterial { Wave = 2, UsedBy = "Revelation", Name = "Tritanium", Quantity = 100, OwnedQuantity = 100 }
                }),
                "project queue display service reports ready when no materials remain");
            AssertEqual("", service.GetJobMaterialStatus(null, materials), "project queue display service ignores missing jobs");
        }

        private static void BlueprintEstimateCacheService_UsesStableKeysAndCloneIsolation()
        {
            var service = new BlueprintEstimateCacheService();
            var blueprint = Blueprint("Revelation", groupName: "Dreadnought");
            blueprint.BlueprintTypeId = 19721;
            var decryptor = new DecryptorOption { TypeId = 34201 };
            var station = new FacilityStation { Name = "Jita IV - Moon 4" };

            var key = service.CreateKey(blueprint, 7, 10.1256, 20.9876, decryptor, station);
            AssertEqual("19721:7:10.126:20.988:34201:Jita IV - Moon 4", key, "estimate cache key is invariant and rounded like the legacy inline key");

            var cache = new Dictionary<string, BlueprintEstimate>();
            var original = new BlueprintEstimate { MaterialCost = 100, InventionCost = 20 };
            service.Store(cache, key, original);
            original.MaterialCost = 999;

            BlueprintEstimate cached;
            AssertTrue(service.TryGet(cache, key, out cached), "estimate cache returns stored value");
            AssertEqual(100, cached.MaterialCost, 0.0001, "estimate cache stores a clone");
            cached.MaterialCost = 555;

            BlueprintEstimate cachedAgain;
            AssertTrue(service.TryGet(cache, key, out cachedAgain), "estimate cache returns stored value again");
            AssertEqual(100, cachedAgain.MaterialCost, 0.0001, "estimate cache returns a fresh clone");
        }

        private static void BlueprintEstimateStatusService_FormatsBlueprintAndProjectStatuses()
        {
            var service = new BlueprintEstimateStatusService();

            AssertEqual("OK", service.GetBlueprintStatus(new BlueprintEstimate(), 0), "blueprint estimate status is OK without missing prices or invention");
            AssertEqual("OK + invention", service.GetBlueprintStatus(new BlueprintEstimate { InventionCost = 10 }, 0), "blueprint estimate status shows invention cost");
            AssertEqual("Нет цен: 3", service.GetBlueprintStatus(new BlueprintEstimate { InventionCost = 10 }, 3), "blueprint estimate status prioritizes missing prices over invention");
            AssertEqual("Нет данных invention", service.GetBlueprintStatus(new BlueprintEstimate { InventionMissing = true }, 3), "blueprint estimate status prioritizes missing invention data");

            AssertEqual("Нет кеша цен", service.GetProjectItemStatus(new BlueprintEstimate(), false), "project estimate status reports missing price cache");
            AssertEqual("OK + invention", service.GetProjectItemStatus(new BlueprintEstimate { InventionCost = 25 }, true), "project estimate status shows invention cost when cache exists");
            AssertEqual("Нет данных invention", service.GetProjectItemStatus(new BlueprintEstimate { InventionMissing = true }, false), "project estimate status prioritizes missing invention data");
        }

        private static void BlueprintInventionMath_CalculatesChanceRunsAndJobs()
        {
            var facility = new FacilityPreset
            {
                EncryptionSkillLevel = 5,
                DatacoreSkill1Level = 5,
                DatacoreSkill2Level = 5
            };
            var decryptor = new DecryptorOption
            {
                ProbabilityModifier = 1.2,
                RunModifier = 2
            };
            var invention = new InventionInfo
            {
                Probability = 0.4,
                RunsPerSuccess = 1,
                MaxProductionLimit = 10
            };

            var chance = BlueprintInventionMathService.CalculateChance(0.4, decryptor, facility);
            AssertEqual(0.699999, chance, 0.0001, "invention chance includes science skills and decryptor probability");

            var plan = BlueprintInventionMathService.CreatePlan(
                new BlueprintSearchResult { ProductName = "T2 Module", BlueprintName = "T2 Module Blueprint", TechLevel = 2 },
                invention,
                manufacturingRuns: 25,
                facilityPreset: facility,
                decryptor: decryptor);

            AssertEqual(12, plan.RunsPerSuccess, "T2 invention uses max production limit plus decryptor run modifier");
            AssertEqual(3, plan.SuccessfulJobsNeeded, "invention plan calculates successful jobs needed from requested runs");
            AssertEqual(5, plan.Jobs, "invention plan inflates jobs by invention chance");

            var t3Plan = BlueprintInventionMathService.CreatePlan(
                new BlueprintSearchResult { ProductName = "T3 Hull", BlueprintName = "T3 Hull Blueprint", TechLevel = 3 },
                invention,
                manufacturingRuns: 5,
                facilityPreset: facility,
                decryptor: null);

            AssertEqual(1, t3Plan.RunsPerSuccess, "T3 invention keeps invention runs per success instead of T2 max production limit");
        }

        private static void BlueprintInventionCostService_AccumulatesMaterialDecryptorAndCopyCosts()
        {
            var service = new BlueprintInventionCostService();
            var invention = new InventionInfo
            {
                SourceBlueprintTypeId = 100,
                Materials =
                {
                    new MaterialRequirement { TypeId = 200, Quantity = 2 },
                    new MaterialRequirement { TypeId = 201, Quantity = 3 }
                },
                CopyMaterials =
                {
                    new MaterialRequirement { TypeId = 300, Quantity = 4 }
                }
            };
            var plan = new InventionPlan
            {
                Jobs = 5,
                SuccessfulJobsNeeded = 2,
                RunsPerSuccess = 10
            };
            var prices = new Dictionary<long, double>
            {
                { 100, 1000 },
                { 200, 10 },
                { 201, 20 },
                { 300, 5 },
                { 400, 50 }
            };

            var result = service.CalculateMaterialCosts(new InventionCostContext
            {
                Invention = invention,
                Plan = plan,
                ManufacturingRuns = 10,
                TechLevel = 2,
                DecryptorTypeId = 400,
                GetUnitPrice = typeId => prices.TryGetValue(typeId, out var price) ? price : 0
            });

            AssertEqual(5000, result.SourceCost, 0.0001, "invention cost includes source blueprint cost per job");
            AssertEqual(400, result.MaterialCost, 0.0001, "invention cost includes datacore/material costs per job");
            AssertEqual(250, result.DecryptorCost, 0.0001, "invention cost includes decryptor cost per job");
            AssertEqual(50, result.CopyMaterialCost, 0.0001, "invention copy material cost is prorated by invented runs");
            AssertEqual(5700, result.TotalCost, 0.0001, "invention total cost sums material buckets");
            AssertFalse(result.MissingPrice, "all available invention prices do not mark missing");

            prices.Remove(201);
            var missingResult = service.CalculateMaterialCosts(new InventionCostContext
            {
                Invention = invention,
                Plan = plan,
                ManufacturingRuns = 10,
                TechLevel = 2,
                DecryptorTypeId = 400,
                GetUnitPrice = typeId => prices.TryGetValue(typeId, out var price) ? price : 0
            });
            AssertTrue(missingResult.MissingPrice, "missing invention material price marks missing");

            prices.Remove(100);
            var t3MissingSource = service.CalculateMaterialCosts(new InventionCostContext
            {
                Invention = invention,
                Plan = plan,
                ManufacturingRuns = 10,
                TechLevel = 3,
                DecryptorTypeId = 0,
                GetUnitPrice = typeId => prices.TryGetValue(typeId, out var price) ? price : 0
            });
            AssertTrue(t3MissingSource.MissingPrice, "missing T3 source blueprint price marks missing");
            AssertEqual(0, t3MissingSource.CopyMaterialCost, 0.0001, "T3 invention skips copy material costs");
        }

        private static void BlueprintInventionCostService_CalculatesScienceUsageCost()
        {
            var service = new BlueprintInventionCostService();
            var result = service.CalculateUsageCost(new InventionUsageCostContext
            {
                EstimatedInputValue = 1000000,
                Jobs = 5,
                TotalInventedRuns = 20,
                RequestedRuns = 10,
                CostIndex = 0.05,
                FactionWarfareMultiplier = 0.8,
                FacilityCostMultiplier = 0.9,
                IndustryTaxPercent = 0.25
            });

            AssertEqual(720, result.JobGrossCost, 0.0001, "science usage gross job cost uses EIV, cost index, FW, and facility multiplier");
            AssertEqual(1.8, result.JobTax, 0.0001, "science usage tax uses station industry tax");
            AssertEqual(1804.5, result.UsageCost, 0.0001, "science usage cost is prorated by jobs and requested runs");

            var zero = service.CalculateUsageCost(new InventionUsageCostContext
            {
                EstimatedInputValue = 1000000,
                Jobs = 0,
                TotalInventedRuns = 20,
                RequestedRuns = 10
            });
            AssertEqual(0, zero.UsageCost, 0.0001, "science usage cost ignores invalid job counts");
        }

        private static void BlueprintInventionTimeService_CalculatesCopyAndInventionTimes()
        {
            var service = new BlueprintInventionTimeService();
            var invention = new InventionInfo
            {
                BaseCopyTime = 1000,
                BaseInventionTime = 500
            };
            var plan = new InventionPlan
            {
                Jobs = 5
            };

            var result = service.CalculateScienceTimes(new InventionTimeContext
            {
                Invention = invention,
                Plan = plan,
                TechLevel = 2,
                AdvancedIndustrySkillLevel = 4,
                ScienceSkillLevel = 5,
                LaboratoryLines = 2,
                CopyTimeMultiplier = 0.8,
                InventionTimeMultiplier = 0.9
            });

            AssertEqual(2640, result.CopyTimeSeconds, 0.0001, "copy time includes science, advanced industry, facility time, and job count");
            AssertEqual(1188, result.InventionTimeSeconds, 0.0001, "invention time packs jobs into laboratory sessions");
            AssertEqual(3828, result.ExtraTimeSeconds, 0.0001, "invention extra time sums copy and invention time");

            var t3Result = service.CalculateScienceTimes(new InventionTimeContext
            {
                Invention = invention,
                Plan = plan,
                TechLevel = 3,
                AdvancedIndustrySkillLevel = 4,
                ScienceSkillLevel = 5,
                LaboratoryLines = 0,
                CopyTimeMultiplier = 0.8,
                InventionTimeMultiplier = 0.9
            });

            AssertEqual(0, t3Result.CopyTimeSeconds, 0.0001, "T3 invention skips copy time");
            AssertEqual(1980, t3Result.InventionTimeSeconds, 0.0001, "laboratory lines clamp to at least one session lane");
        }

        private static void FacilityActivityStationService_ResolvesActivityStationsAndFallbackCopies()
        {
            var service = new FacilityActivityStationService();
            var inventionStation = new FacilityStation
            {
                Name = "Science Azbel",
                SystemName = "Jita",
                ProductionType = "Invention",
                SupportsProduction = true,
                MaterialBonusPercent = 2
            };
            var manufacturingStation = new FacilityStation
            {
                Name = "Manufacturing Azbel",
                SystemName = "Perimeter",
                ProductionType = "Manufacturing",
                SupportsProduction = true,
                RegionId = 10000002,
                SolarSystemId = 30000142,
                RigSlot1TypeId = 123,
                IndustryTaxPercent = 0.25,
                TimeMultiplier = 0.8,
                CostMultiplier = 0.9,
                FactionWarfareUpgradeLevel = 3
            };
            var preset = new FacilityPreset();
            preset.Stations.Add(manufacturingStation);
            preset.Stations.Add(inventionStation);

            var resolved = service.ResolveActivityStation(preset, manufacturingStation, "Invention");
            AssertEqual("Science Azbel", resolved.Name, "activity station resolver prefers matching preset station");
            AssertEqual("Invention", resolved.ProductionType, "activity station resolver keeps matching production type");

            var fallback = service.ResolveActivityStation(new FacilityPreset(), manufacturingStation, "Copying");
            AssertEqual("Manufacturing Azbel", fallback.Name, "activity station resolver copies fallback station name");
            AssertEqual("Copying", fallback.ProductionType, "activity station resolver rewrites fallback production type");
            AssertEqual(123, fallback.RigSlot1TypeId, "activity station resolver preserves rig selection on fallback copy");
            AssertEqual(0.25, fallback.IndustryTaxPercent, 0.0001, "activity station resolver preserves tax on fallback copy");
            AssertTrue(!object.ReferenceEquals(manufacturingStation, fallback), "fallback activity station is a copy, not the original instance");

            manufacturingStation.SupportsProduction = false;
            AssertTrue(service.ResolveActivityStation(new FacilityPreset(), manufacturingStation, "Copying") == null, "unsupported fallback station is not used");
        }

        private static void OrePlanningService_PlansGreedyOreReplacementDeterministically()
        {
            var service = new OrePlanningService();
            var oreOutputs = new Dictionary<long, List<ReprocessingOption>>
            {
                {
                    1001,
                    new List<ReprocessingOption>
                    {
                        new ReprocessingOption { OreTypeId = 1001, OreName = "Mixed Ore", UnitsToReprocess = 10, OreVolume = 1.5, MineralTypeId = 34, MineralQuantity = 50 },
                        new ReprocessingOption { OreTypeId = 1001, OreName = "Mixed Ore", UnitsToReprocess = 10, OreVolume = 1.5, MineralTypeId = 35, MineralQuantity = 10 }
                    }
                },
                {
                    1002,
                    new List<ReprocessingOption>
                    {
                        new ReprocessingOption { OreTypeId = 1002, OreName = "Pyerite Ore", UnitsToReprocess = 5, OreVolume = 2.0, MineralTypeId = 35, MineralQuantity = 60 }
                    }
                }
            };
            var orePrices = new Dictionary<long, double> { { 1001, 100 }, { 1002, 80 } };
            var candidates = service.BuildCandidates(
                oreOutputs,
                new[] { 34L, 35L },
                option => 1.0,
                option => orePrices[option.OreTypeId]);

            AssertEqual(2, candidates.Count, "ore planning builds one candidate per ore type");
            AssertEqual(50, (int)candidates.Single(item => item.OreTypeId == 1001).OutputByMineral[34], "ore candidate includes required mineral output");
            AssertEqual(37, (int)service.GetReprocessedMineralOutput(new ReprocessingOption { MineralQuantity = 50 }, 0.75), "ore planning floors reprocessed output by yield");

            var remaining = new Dictionary<long, long> { { 34, 100 }, { 35, 60 } };
            var plan = service.PlanOreForMinerals(
                remaining,
                candidates,
                new Dictionary<long, double> { { 34, 5 }, { 35, 10 } });

            AssertEqual(2, plan.Count, "ore planning covers all required minerals with two ore choices");
            AssertEqual(1002, (int)plan[0].Candidate.OreTypeId, "ore planning first chooses the best cost-per-covered-value ore");
            AssertEqual(5, (int)plan[0].Quantity, "ore planning rounds selected ore to reprocessing units");
            AssertEqual(60, (int)plan[0].ProducedByMineral[35], "ore planning records produced mineral output by type");
            AssertEqual(1001, (int)plan[1].Candidate.OreTypeId, "ore planning then covers remaining Tritanium with mixed ore");
            AssertEqual(20, (int)plan[1].Quantity, "ore planning multiplies batch count by ore reprocessing units");
            AssertEqual(0, (int)remaining[34], "ore planning clears remaining Tritanium requirement");
            AssertEqual(0, (int)remaining[35], "ore planning clears remaining Pyerite requirement");

            var cost = service.CalculatePurchaseCost(
                new Dictionary<long, long> { { 34, 100 }, { 35, 60 } },
                oreOutputs,
                new[] { 34L, 35L },
                option => 1.0,
                option => orePrices[option.OreTypeId],
                new Dictionary<long, double> { { 34, 5 }, { 35, 10 } });
            AssertEqual(2400, cost, 0.0001, "ore planning purchase cost sums selected ore quantities at ore unit price");

            var missingPriceCost = service.CalculatePurchaseCost(
                new Dictionary<long, long> { { 34, 100 } },
                oreOutputs,
                new[] { 34L },
                option => 1.0,
                option => option.OreTypeId == 1001 ? 0 : 80,
                new Dictionary<long, double> { { 34, 5 } });
            AssertEqual(0, missingPriceCost, 0.0001, "ore planning purchase cost stays controlled when planned ore price is missing");
        }

        private static void BlueprintEstimateSelectionService_SelectsBestStationAndDecryptorCandidates()
        {
            var service = new BlueprintEstimateSelectionService();
            var preset = new FacilityPreset();
            preset.Stations.Add(new FacilityStation { Name = "Manufacturing", ProductionType = "Manufacturing", SupportsProduction = true });
            preset.Stations.Add(new FacilityStation { Name = "Invention", ProductionType = "Invention", SupportsProduction = true });
            preset.Stations.Add(new FacilityStation { Name = "Offline Copy", ProductionType = "Copying", SupportsProduction = false });
            AssertEqual("Invention", service.GetSupportedStationCandidates(preset, "Invention").Single().Name, "station candidate selection prefers matching supported production type");
            AssertEqual(2, service.GetSupportedStationCandidates(preset, "Reactions").Count, "station candidate selection falls back to all supported stations");

            var expensiveFast = new FacilityStation { Name = "Fast" };
            var cheapSlow = new FacilityStation { Name = "Cheap Slow" };
            var cheapFast = new FacilityStation { Name = "Cheap Fast" };

            var station = service.SelectCheapestStation(new[]
            {
                new BlueprintStationEstimateCandidate
                {
                    Station = expensiveFast,
                    Estimate = new BlueprintEstimate { MaterialCost = 100, InventionCost = 10, InstallationCost = 5, ProductionTimeSeconds = 10 }
                },
                new BlueprintStationEstimateCandidate
                {
                    Station = cheapSlow,
                    Estimate = new BlueprintEstimate { MaterialCost = 90, InventionCost = 5, InstallationCost = 5, ProductionTimeSeconds = 50 }
                },
                new BlueprintStationEstimateCandidate
                {
                    Station = cheapFast,
                    Estimate = new BlueprintEstimate { MaterialCost = 90, InventionCost = 5, InstallationCost = 5, ProductionTimeSeconds = 20 }
                }
            });

            AssertEqual("Cheap Fast", station.Name, "station selector chooses lowest total cost and then faster production time");

            var fallback = new DecryptorOption { TypeId = 0, Name = "None" };
            var parity = new DecryptorOption { TypeId = 1, Name = "Parity" };
            var accelerant = new DecryptorOption { TypeId = 2, Name = "Accelerant" };
            var decryptor = service.SelectBestDecryptor(new[]
            {
                new BlueprintDecryptorEstimateCandidate { Decryptor = parity, Profit = 100 },
                new BlueprintDecryptorEstimateCandidate { Decryptor = accelerant, Profit = 250 }
            }, fallback);

            AssertEqual("Accelerant", decryptor.Name, "decryptor selector chooses highest profit");
            AssertEqual("None", service.SelectBestDecryptor(new BlueprintDecryptorEstimateCandidate[0], fallback).Name, "decryptor selector falls back when no candidates are profitable/evaluable");
        }

        private static void BlueprintEstimateTraversalService_CharacterizesRecursiveEstimateEdges()
        {
            var service = new BlueprintEstimateTraversalService();
            var parent = Blueprint("Capital Hull", groupName: "Capital", productTypeId: 1000);
            var component = Blueprint("Capital Component", groupName: "Capital Component", productTypeId: 2000);
            component.PortionSize = 10;
            var material = new MaterialRequirement { TypeId = component.ProductTypeId, Name = component.ProductName, Quantity = 21 };
            var state = new BlueprintEstimateRecursionState();

            AssertTrue(service.ShouldLookupChildBlueprint(state, material.TypeId), "simple T1/material line can attempt child lookup when not in recursion path");
            state.Path.Add(material.TypeId);
            AssertFalse(service.ShouldLookupChildBlueprint(state, material.TypeId), "cycle/path guard suppresses child lookup for material already in path");
            state.Path.Clear();

            var request = service.CreateChildRequest(parent, component, material, adjustedQuantity: 21, materialEfficiency: 10, timeEfficiency: 20, decryptor: null);
            AssertEqual(3, request.ChildRuns, "child component request rounds required quantity up by child portion size");
            AssertEqual(21, (int)request.RequiredQuantity, "child component request preserves required quantity");
            AssertEqual("Capital Hull", request.ParentBlueprint.ProductName, "child component request keeps parent blueprint");
            AssertEqual("Capital Component", request.ChildBlueprint.ProductName, "child component request keeps child blueprint");

            var copyOnly = Blueprint("Zirnitra", groupName: "Dreadnought", productTypeId: 3000);
            copyOnly.IsCopyOnlyBlueprint = true;
            copyOnly.MaxProductionLimit = 1;
            copyOnly.PortionSize = 1;
            var copyRequest = service.CreateChildRequest(parent, copyOnly, new MaterialRequirement { TypeId = 3000, Quantity = 2 }, 2, 0, 0, null);
            AssertEqual(2, copyRequest.ChildRuns, "copy-only BPC child request still follows current portion-size run calculation");

            service.EnterParent(state, parent);
            AssertTrue(state.Path.Contains(parent.ProductTypeId), "entering recursion records parent product type id");
            service.ExitParent(state, parent);
            AssertFalse(state.Path.Contains(parent.ProductTypeId), "leaving recursion removes parent product type id");

            var noSurplus = service.ApplySurplusOffset(100, component, childRuns: 2, requiredQuantity: 20, getProductUnitPrice: _ => 5, applySalesTaxesAndFees: value => value);
            AssertEqual(100, noSurplus, 0.0001, "surplus offset keeps child cost when produced quantity exactly matches requirement");

            var missingPrice = service.ApplySurplusOffset(100, component, childRuns: 3, requiredQuantity: 21, getProductUnitPrice: _ => 0, applySalesTaxesAndFees: value => value);
            AssertEqual(100, missingPrice, 0.0001, "missing surplus price keeps current child cost");

            var offset = service.ApplySurplusOffset(100, component, childRuns: 3, requiredQuantity: 21, getProductUnitPrice: _ => 5, applySalesTaxesAndFees: value => value - 5);
            AssertEqual(60, offset, 0.0001, "surplus offset subtracts net sale value for excess child output");

            var clamped = service.ApplySurplusOffset(10, component, childRuns: 3, requiredQuantity: 21, getProductUnitPrice: _ => 5, applySalesTaxesAndFees: value => value);
            AssertEqual(0, clamped, 0.0001, "surplus offset clamps child cost at zero");

            var sellbackDisabled = service.ApplySurplusOffset(100, component, childRuns: 3, requiredQuantity: 21, getProductUnitPrice: _ => 5, applySalesTaxesAndFees: value => value, applySurplusSellback: false);
            AssertEqual(100, sellbackDisabled, 0.0001, "surplus offset honors legacy sell-excess disabled behavior");
        }

        private static void BlueprintMaterialEstimateLineService_CharacterizesMaterialPriceLines()
        {
            var service = new BlueprintMaterialEstimateLineService();
            var material = new MaterialRequirement { TypeId = 34, Name = "Tritanium", Quantity = 100 };
            var price = new MarketPrice { SellMin = 5, SellVolume = 150 };

            var simple = service.CreateLine(material, runs: 2, materialMultiplier: 0.9, price: price,
                buildWhenMarketVolumeShort: true,
                getUnitPrice: item => item.SellMin,
                getAvailableVolume: item => item.SellVolume);

            AssertEqual(180, (int)simple.AdjustedQuantity, "material line uses current adjusted quantity rounding");
            AssertEqual(5, simple.UnitPrice, 0.0001, "material line reads unit price through delegate");
            AssertEqual(900, simple.BuyCost, 0.0001, "material line calculates buy cost from adjusted quantity and unit price");
            AssertTrue(simple.MarketVolumeShort, "material line marks market short when available volume is below adjusted quantity");

            var missingPrice = service.CreateLine(material, runs: 2, materialMultiplier: 0.9, price: null,
                buildWhenMarketVolumeShort: true,
                getUnitPrice: item => item.SellMin,
                getAvailableVolume: item => item.SellVolume);
            AssertEqual(0, missingPrice.UnitPrice, 0.0001, "missing price/cache produces zero material unit price");
            AssertEqual(0, missingPrice.BuyCost, 0.0001, "missing price/cache produces zero buy cost");
            AssertFalse(missingPrice.MarketVolumeShort, "missing price/cache does not mark market volume short");

            var noShort = service.CreateLine(material, runs: 2, materialMultiplier: 0.9, price: price,
                buildWhenMarketVolumeShort: false,
                getUnitPrice: item => item.SellMin,
                getAvailableVolume: item => item.SellVolume);
            AssertFalse(noShort.MarketVolumeShort, "material line respects disabled market-volume-short build policy");
        }

        private static void BlueprintEstimateMaterialTraversalService_CharacterizesMaterialTraversalBranches()
        {
            var service = new BlueprintEstimateMaterialTraversalService();
            var parent = Blueprint("Capital Hull", groupName: "Capital", productTypeId: 1000);
            var component = Blueprint("Capital Component", groupName: "Capital Component", productTypeId: 2000);
            component.PortionSize = 10;
            var facility = new FacilityPreset { BuildWhenMarketVolumeShort = true };
            var station = new FacilityStation { Name = "Azbel", SupportsProduction = true };

            var simple = service.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = parent,
                    Materials = new[] { new MaterialRequirement { TypeId = 10, Name = "T1 Material", Quantity = 100 } },
                    FacilityPreset = facility,
                    Prices = new Dictionary<long, MarketPrice> { { 10, new MarketPrice { SellMin = 5, SellVolume = 1000 } } },
                    AdjustedPrices = new Dictionary<long, double> { { 10, 2 } },
                    Runs = 2,
                    MaterialMultiplier = 0.9,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ => null,
                    IsMineral = _ => false,
                    GetMaterialUnitPrice = price => price.SellMin,
                    GetAvailableMarketVolume = price => price.SellVolume
                });

            AssertEqual(900, simple.MaterialCost, 0.0001, "material traversal buys simple T1 material with current adjusted quantity");
            AssertEqual(400, simple.EstimatedInputValue, 0.0001, "material traversal accumulates adjusted-price EIV for simple material");
            AssertEqual(0, simple.BuildMaterialLines, "material traversal has no build line for simple buy material");
            AssertEqual(1, simple.BuyMaterialLines, "material traversal records a buy line for simple material");

            var child = service.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = parent,
                    Materials = new[] { new MaterialRequirement { TypeId = component.ProductTypeId, Name = component.ProductName, Quantity = 21 } },
                    FacilityPreset = facility,
                    Prices = new Dictionary<long, MarketPrice>
                    {
                        { component.ProductTypeId, new MarketPrice { SellMin = 1000, SellVolume = 5 } }
                    },
                    AdjustedPrices = new Dictionary<long, double>(),
                    Runs = 1,
                    MaterialMultiplier = 1,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = typeId => typeId == component.ProductTypeId ? component : null,
                    IsMineral = _ => false,
                    ShouldAlwaysBuy = _ => false,
                    ShouldStopReactionDrilldown = (_, __) => false,
                    GetDefaultChildEfficiency = (_, __, ___) => Tuple.Create(10.0, 20.0),
                    GetCheapestChildStation = _ => station,
                    CalculateChildEstimate = (_, __) => new BlueprintEstimate { MaterialCost = 500, ProductionTimeSeconds = 42 },
                    GetMaterialUnitPrice = price => price.SellMin,
                    GetProductUnitPrice = price => 5,
                    GetAvailableMarketVolume = price => price.SellVolume,
                    ApplySalesTaxesAndFees = (gross, _, __) => gross
                });

            AssertEqual(455, child.MaterialCost, 0.0001, "material traversal applies surplus offset before build/buy decision");
            AssertEqual(1, child.BuildMaterialLines, "material traversal builds child component when adjusted child cost beats buy cost");
            AssertEqual(0, child.BuyMaterialLines, "material traversal does not count a buy line for built child component");
            AssertEqual(42, child.ComponentProductionTimes.Single(), 0.0001, "material traversal carries child production time");
            AssertTrue(child.MaterialLines[0].MarketVolumeShort, "material traversal preserves market-volume-short flag from material line");
            AssertEqual(3, child.MaterialLines[0].ChildRequest.ChildRuns, "material traversal creates child request with current child run rounding");

            var cycleCalls = 0;
            var cycleState = new BlueprintEstimateRecursionState();
            cycleState.Path.Add(component.ProductTypeId);
            var cycle = service.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = parent,
                    Materials = new[] { new MaterialRequirement { TypeId = component.ProductTypeId, Name = component.ProductName, Quantity = 21 } },
                    FacilityPreset = facility,
                    Prices = new Dictionary<long, MarketPrice> { { component.ProductTypeId, new MarketPrice { SellMin = 7, SellVolume = 1000 } } },
                    Runs = 1,
                    MaterialMultiplier = 1,
                    RecursionState = cycleState
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ =>
                    {
                        cycleCalls++;
                        return component;
                    },
                    IsMineral = _ => false,
                    GetMaterialUnitPrice = price => price.SellMin,
                    GetAvailableMarketVolume = price => price.SellVolume
                });

            AssertEqual(0, cycleCalls, "material traversal honors path guard before child blueprint lookup");
            AssertEqual(147, cycle.MaterialCost, 0.0001, "cycle-guarded material traversal falls back to buy cost");

            var mineral = service.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = parent,
                    Materials = new[] { new MaterialRequirement { TypeId = 34, Name = "Tritanium", Quantity = 100 } },
                    Prices = new Dictionary<long, MarketPrice>(),
                    AdjustedPrices = new Dictionary<long, double> { { 34, 1 } },
                    Runs = 2,
                    MaterialMultiplier = 0.9,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ => null,
                    IsMineral = typeId => typeId == 34,
                    GetMaterialUnitPrice = price => price.SellMin,
                    GetAvailableMarketVolume = price => price.SellVolume
                });

            AssertEqual(0, mineral.MaterialCost, 0.0001, "mineral fallback leaves mineral purchase cost for caller ore/mineral pricing");
            AssertEqual(180, (int)mineral.MineralBuyQuantities[34], "mineral fallback aggregates adjusted mineral quantity");
            AssertEqual(200, mineral.EstimatedInputValue, 0.0001, "mineral fallback preserves adjusted-price EIV contribution");

            var missingPrice = service.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = parent,
                    Materials = new[] { new MaterialRequirement { TypeId = 999, Name = "Missing Price", Quantity = 5 } },
                    Prices = new Dictionary<long, MarketPrice>(),
                    Runs = 1,
                    MaterialMultiplier = 1,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ => null,
                    IsMineral = _ => false,
                    GetMaterialUnitPrice = price => price.SellMin,
                    GetAvailableMarketVolume = price => price.SellVolume
                });
            AssertEqual(0, missingPrice.MaterialCost, 0.0001, "missing price/cache material traversal preserves zero buy cost behavior");
            AssertEqual(1, missingPrice.BuyMaterialLines, "missing price/cache material traversal still records a buy line");
        }

        private static void BlueprintEstimateMaterialOrchestrationService_CreatesBoundariesAndAggregatesResults()
        {
            var service = new BlueprintEstimateMaterialOrchestrationService();
            var blueprint = Blueprint("Rifter", productTypeId: 587);
            var station = new FacilityStation { Name = "Fixture Station", SupportsProduction = true };
            var preset = new FacilityPreset { BuildWhenMarketVolumeShort = true };
            var prices = new Dictionary<long, MarketPrice> { { 34, new MarketPrice { TypeId = 34, SellMin = 5, SellVolume = 100 } } };
            var adjustedPrices = new Dictionary<long, double> { { 34, 2 } };
            var state = new BlueprintEstimateRecursionState();
            var materials = new[] { new MaterialRequirement { TypeId = 34, Name = "Tritanium", Quantity = 100 } };

            var context = service.CreateContext(
                blueprint,
                materials,
                station,
                preset,
                prices,
                adjustedPrices,
                runs: 2,
                materialEfficiency: 10,
                timeEfficiency: 20,
                materialMultiplier: 0.9,
                decryptor: null,
                recursionState: state);
            AssertEqual("Rifter", context.Blueprint.ProductName, "estimate material orchestration context keeps blueprint");
            AssertEqual("Fixture Station", context.Station.Name, "estimate material orchestration context keeps station");
            AssertEqual(2, context.Runs, "estimate material orchestration context keeps runs");
            AssertEqual(0.9, context.MaterialMultiplier, 0.0001, "estimate material orchestration context keeps material multiplier");
            AssertTrue(object.ReferenceEquals(state, context.RecursionState), "estimate material orchestration context keeps recursion state boundary");

            var dependencies = service.CreateDependencies(
                findBlueprintByProduct: _ => null,
                isMineral: typeId => typeId == 34,
                shouldAlwaysBuy: _ => false,
                shouldStopReactionDrilldown: (_, __) => false,
                getDefaultChildEfficiency: (_, __, ___) => Tuple.Create(0.0, 0.0),
                getCheapestChildStation: _ => station,
                calculateChildEstimate: (_, __) => null,
                getMaterialUnitPrice: price => price == null ? 0 : price.SellMin,
                getProductUnitPrice: price => price == null ? 0 : price.SellMin,
                getAvailableMarketVolume: price => price == null ? 0 : price.SellVolume,
                applySalesTaxesAndFees: (value, _, __) => value);
            AssertTrue(dependencies.IsMineral(34), "estimate material orchestration dependencies keep mineral delegate");
            AssertEqual(5, dependencies.GetMaterialUnitPrice(prices[34]), 0.0001, "estimate material orchestration dependencies keep price delegate");

            var result = service.TraverseMaterials(context, dependencies, mineralQuantities =>
            {
                long quantity;
                return mineralQuantities.TryGetValue(34, out quantity) ? quantity * 5 : 0;
            });

            AssertEqual(900, result.MaterialCost, 0.0001, "estimate material orchestration adds mineral purchase cost after traversal");
            AssertEqual(400, result.EstimatedInputValue, 0.0001, "estimate material orchestration preserves traversal EIV");
            AssertEqual(1, result.BuyMaterialLines, "estimate material orchestration preserves buy line count");
            AssertEqual(0, result.BuildMaterialLines, "estimate material orchestration preserves build line count");
            AssertEqual(180, (int)result.TraversalResult.MineralBuyQuantities[34], "estimate material orchestration exposes traversal mineral quantities");
        }

        private static void BlueprintEstimateResultAssembler_AssemblesFinalEstimateSummary()
        {
            var service = new BlueprintEstimateResultAssembler();
            var blueprint = Blueprint("Fixture Hull", productTypeId: 7000);
            blueprint.BaseProductionTime = 100;
            var plan = new InventionPlan
            {
                Chance = 0.4,
                Jobs = 3,
                RunsPerSuccess = 10,
                SourceCost = 11,
                MaterialCost = 22,
                DecryptorCost = 33,
                CopyMaterialCost = 44,
                InventionUsageCost = 55,
                CopyUsageCost = 66,
                InventionStationName = "Invention Station",
                InventionStationSystem = "Jita",
                CopyStationName = "Copy Station",
                CopyStationSystem = "Perimeter"
            };

            var estimate = service.Assemble(new BlueprintEstimateAssemblyContext
            {
                Blueprint = blueprint,
                FacilityPreset = new FacilityPreset { ProductionLines = 2 },
                InventionPlan = plan,
                ComponentProductionTimes = new[] { 90.0, 20.0, 10.0 },
                Runs = 3,
                TimeMultiplier = 1.5,
                InventionTimeSeconds = 7,
                MaterialCost = 1000,
                InstallationCost = 200,
                FacilityMaterialBonusPercent = 2.5,
                CostIndexPercent = 3.5,
                InventionCost = 123,
                InventionMissing = true,
                BuildMaterialLines = 4,
                BuyMaterialLines = 5
            });

            AssertEqual(1000, estimate.MaterialCost, 0.0001, "estimate assembler keeps material cost");
            AssertEqual(200, estimate.InstallationCost, 0.0001, "estimate assembler keeps installation cost");
            AssertEqual(2.5, estimate.FacilityMaterialBonusPercent, 0.0001, "estimate assembler keeps facility material bonus");
            AssertEqual(3.5, estimate.CostIndexPercent, 0.0001, "estimate assembler keeps cost index percent");
            AssertEqual(397, estimate.ProductionTimeSeconds, 0.0001, "estimate assembler combines root production, invention time, and parallel component time");
            AssertEqual(123, estimate.InventionCost, 0.0001, "estimate assembler keeps invention cost");
            AssertTrue(estimate.InventionMissing, "estimate assembler keeps invention missing flag");
            AssertEqual(40, estimate.InventionChancePercent, 0.0001, "estimate assembler formats invention chance percent");
            AssertEqual(3, estimate.InventionJobs, "estimate assembler keeps invention jobs");
            AssertEqual(10, estimate.InventedRunsPerSuccess, "estimate assembler keeps invented runs per success");
            AssertEqual(66, estimate.InventionMaterialsCost, 0.0001, "estimate assembler sums source, material, and decryptor invention costs");
            AssertEqual(44, estimate.InventionCopyCost, 0.0001, "estimate assembler keeps copy material cost");
            AssertEqual(121, estimate.InventionJobUsageCost, 0.0001, "estimate assembler sums invention and copy usage costs");
            AssertEqual("Invention Station", estimate.InventionStationName, "estimate assembler keeps invention station");
            AssertEqual("Perimeter", estimate.CopyStationSystem, "estimate assembler keeps copy station system");
            AssertEqual(4, estimate.BuildMaterialLines, "estimate assembler keeps build line count");
            AssertEqual(5, estimate.BuyMaterialLines, "estimate assembler keeps buy line count");
        }

        private static void DatabaseBackedBlueprintStructuralGolden_LoadRealBlueprintChains()
        {
            var databasePath = AppPaths.EveIphDatabasePath;
            AssertTrue(File.Exists(databasePath), "local EVE IPH sqlite database exists for database-backed golden fixtures");

            var database = new EveDatabaseService(databasePath);
            AssertTrue(database.IsDatabaseAvailable(), "local EVE IPH sqlite database has ALL_BLUEPRINTS_FACT");

            var rifter = database.SearchBlueprints("Rifter", 10).FirstOrDefault(bp => bp.ProductName == "Rifter");
            var capitalArmorPlates = database.SearchBlueprints("Capital Armor Plates", 10).FirstOrDefault(bp => bp.ProductName == "Capital Armor Plates");
            var revelation = database.SearchBlueprints("Revelation", 10).FirstOrDefault(bp => bp.ProductName == "Revelation");
            var zirnitra = database.SearchBlueprints("Zirnitra", 10).FirstOrDefault(bp => bp.ProductName == "Zirnitra");

            AssertTrue(rifter != null, "database fixture loads simple T1 Rifter blueprint");
            AssertTrue(capitalArmorPlates != null, "database fixture loads component-chain Capital Armor Plates blueprint");
            AssertTrue(revelation != null, "database fixture loads capital-like Revelation blueprint");
            AssertTrue(zirnitra != null, "database fixture loads copy-only/special Zirnitra blueprint");

            AssertEqual("Rifter Blueprint", rifter.BlueprintName, "Rifter fixture blueprint name");
            AssertEqual("Capital Armor Plates Blueprint", capitalArmorPlates.BlueprintName, "Capital Armor Plates fixture blueprint name");
            AssertEqual("Revelation Blueprint", revelation.BlueprintName, "Revelation fixture blueprint name");
            AssertEqual("Zirnitra Blueprint", zirnitra.BlueprintName, "Zirnitra fixture blueprint name");
            AssertFalse(rifter.IsCopyOnlyBlueprint, "Rifter is BPO-capable in local database fixture");
            AssertTrue(zirnitra.IsCopyOnlyBlueprint, "Zirnitra is represented as copy-only/BPC-only in local database fixture");

            var rifterMaterials = database.GetManufacturingMaterials(rifter.BlueprintTypeId).ToList();
            var capitalArmorPlateMaterials = database.GetManufacturingMaterials(capitalArmorPlates.BlueprintTypeId).ToList();
            var revelationMaterials = database.GetManufacturingMaterials(revelation.BlueprintTypeId).ToList();
            var zirnitraMaterials = database.GetManufacturingMaterials(zirnitra.BlueprintTypeId).ToList();

            AssertEqual(4, rifterMaterials.Count, "Rifter fixture has four direct mineral material lines");
            AssertMaterial(rifterMaterials, 34, "Tritanium", 32000, "Rifter mineral fixture");
            AssertMaterial(rifterMaterials, 35, "Pyerite", 6000, "Rifter mineral fixture");
            AssertMaterial(rifterMaterials, 36, "Mexallon", 2500, "Rifter mineral fixture");
            AssertMaterial(rifterMaterials, 37, "Isogen", 500, "Rifter mineral fixture");

            AssertEqual(9, capitalArmorPlateMaterials.Count, "Capital Armor Plates fixture has real component/reaction-adjacent materials");
            AssertMaterial(capitalArmorPlateMaterials, 57457, "Reinforced Carbon Fiber", 100, "Capital Armor Plates child-chain fixture");
            AssertTrue(database.FindBlueprintByProduct(57457) != null, "Capital Armor Plates fixture resolves Reinforced Carbon Fiber child reaction formula");

            AssertEqual(19, revelationMaterials.Count, "Revelation fixture has 19 direct material lines");
            AssertMaterial(revelationMaterials, 21017, "Capital Armor Plates", 4, "Revelation capital component fixture");
            AssertMaterial(revelationMaterials, 21039, "Capital Siege Array", 15, "Revelation capital component fixture");
            AssertEqual(19, revelationMaterials.Count(material => database.FindBlueprintByProduct(material.TypeId) != null), "Revelation fixture resolves child blueprints for all direct material lines");

            AssertEqual(19, zirnitraMaterials.Count, "Zirnitra fixture has 19 direct material lines");
            AssertMaterial(zirnitraMaterials, 53037, "Capital Absorption Thruster Array", 3, "Zirnitra Triglavian component fixture");
            AssertTrue(database.FindBlueprintByProduct(53037) != null, "Zirnitra fixture resolves Capital Absorption Thruster Array child blueprint");

            var traversalService = new BlueprintEstimateMaterialTraversalService();
            var rifterTraversal = traversalService.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = rifter,
                    Materials = rifterMaterials,
                    Prices = new Dictionary<long, MarketPrice>(),
                    AdjustedPrices = rifterMaterials.ToDictionary(material => material.TypeId, material => 1.0),
                    Runs = 1,
                    MaterialEfficiency = 0,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    GetMaterialUnitPrice = _ => 0,
                    GetProductUnitPrice = _ => 0,
                    GetAvailableMarketVolume = _ => 0,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });

            AssertEqual(4, rifterTraversal.BuyMaterialLines, "Rifter mineral fixture traverses as buy/mineral fallback lines without child recursion");
            AssertEqual(0, rifterTraversal.BuildMaterialLines, "Rifter mineral fixture does not create build lines");
            AssertEqual(32000, (int)rifterTraversal.MineralBuyQuantities[34], "Rifter mineral fallback keeps Tritanium quantity");
            AssertEqual(41000, rifterTraversal.EstimatedInputValue, 0.0001, "Rifter provisional EIV fixture sums adjusted one-ISK mineral quantities");

            var childRequests = new List<ChildBlueprintEstimateRequest>();
            var childEstimateCosts = new Dictionary<long, double>();
            var revelationTraversal = traversalService.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = revelation,
                    Materials = revelationMaterials,
                    FacilityPreset = new FacilityPreset { BuildWhenMarketVolumeShort = true },
                    Prices = revelationMaterials.ToDictionary(material => material.TypeId, material => new MarketPrice { TypeId = material.TypeId, SellMin = 1000, SellVolume = Math.Max(1, material.Quantity - 1) }),
                    AdjustedPrices = revelationMaterials.ToDictionary(material => material.TypeId, material => 10.0),
                    Runs = 1,
                    MaterialEfficiency = 0,
                    TimeEfficiency = 0,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    ShouldAlwaysBuy = _ => false,
                    ShouldStopReactionDrilldown = (_, __) => false,
                    GetDefaultChildEfficiency = (_, __, ___) => Tuple.Create(0.0, 0.0),
                    GetCheapestChildStation = _ => new FacilityStation { Name = "Fixture Station", SupportsProduction = true },
                    CalculateChildEstimate = (request, _) =>
                    {
                        childRequests.Add(request);
                        childEstimateCosts[request.Material.TypeId] = request.ChildRuns * 10.0;
                        return new BlueprintEstimate { MaterialCost = request.ChildRuns * 10.0, ProductionTimeSeconds = request.ChildRuns };
                    },
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetProductUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });

            AssertEqual(19, childRequests.Count, "Revelation database fixture creates child estimate requests for all direct materials");
            AssertEqual(19, revelationTraversal.BuildMaterialLines, "Revelation database fixture deterministically selects build branch with cheaper child estimates");
            AssertEqual(0, revelationTraversal.BuyMaterialLines, "Revelation database fixture avoids buy branch when child build is cheaper");
            AssertTrue(revelationTraversal.MaterialLines.Any(line => line.Material.TypeId == 21017 && line.MarketVolumeShort), "Revelation fixture flags short market volume on component material line");
            AssertTrue(childEstimateCosts.ContainsKey(21017), "Revelation fixture calculated child estimate for Capital Armor Plates");

            var guardedState = new BlueprintEstimateRecursionState();
            guardedState.Path.Add(21017);
            var guardedRequests = new List<ChildBlueprintEstimateRequest>();
            var guardedTraversal = traversalService.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = revelation,
                    Materials = revelationMaterials,
                    FacilityPreset = new FacilityPreset { BuildWhenMarketVolumeShort = true },
                    Prices = revelationMaterials.ToDictionary(material => material.TypeId, material => new MarketPrice { TypeId = material.TypeId, SellMin = 1000, SellVolume = material.Quantity }),
                    AdjustedPrices = revelationMaterials.ToDictionary(material => material.TypeId, material => 10.0),
                    Runs = 1,
                    MaterialEfficiency = 0,
                    MaterialMultiplier = 1.0,
                    RecursionState = guardedState
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    ShouldAlwaysBuy = _ => false,
                    ShouldStopReactionDrilldown = (_, __) => false,
                    GetDefaultChildEfficiency = (_, __, ___) => Tuple.Create(0.0, 0.0),
                    GetCheapestChildStation = _ => new FacilityStation { Name = "Fixture Station", SupportsProduction = true },
                    CalculateChildEstimate = (request, _) =>
                    {
                        guardedRequests.Add(request);
                        return new BlueprintEstimate { MaterialCost = 1, ProductionTimeSeconds = 1 };
                    },
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetProductUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });

            AssertEqual(18, guardedRequests.Count, "Revelation database fixture path guard suppresses the already-visited child component only");
            AssertEqual(1, guardedTraversal.BuyMaterialLines, "Revelation database fixture keeps cycle-guarded component as a buy fallback");

            var missingPriceTraversal = traversalService.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = capitalArmorPlates,
                    Materials = capitalArmorPlateMaterials,
                    Prices = new Dictionary<long, MarketPrice>(),
                    AdjustedPrices = new Dictionary<long, double>(),
                    Runs = 1,
                    MaterialEfficiency = 0,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    ShouldAlwaysBuy = _ => true,
                    GetMaterialUnitPrice = _ => 0,
                    GetProductUnitPrice = _ => 0,
                    GetAvailableMarketVolume = _ => 0,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });

            AssertEqual(0, missingPriceTraversal.MaterialCost, 0.0001, "database fixture missing-price traversal remains controlled at zero material cost");
            AssertTrue(missingPriceTraversal.BuyMaterialLines > 0, "database fixture missing-price traversal still records buy/mineral fallback lines");
        }

        private static void DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedLegacyFormulas()
        {
            var database = new EveDatabaseService(AppPaths.EveIphDatabasePath);
            var rifter = database.SearchBlueprints("Rifter", 10).First(bp => bp.ProductName == "Rifter");
            var capitalArmorPlates = database.SearchBlueprints("Capital Armor Plates", 10).First(bp => bp.ProductName == "Capital Armor Plates");
            var revelation = database.SearchBlueprints("Revelation", 10).First(bp => bp.ProductName == "Revelation");

            var rifterMaterials = database.GetManufacturingMaterials(rifter.BlueprintTypeId).ToList();
            var capitalArmorPlateMaterials = database.GetManufacturingMaterials(capitalArmorPlates.BlueprintTypeId).ToList();
            var revelationMaterials = database.GetManufacturingMaterials(revelation.BlueprintTypeId).ToList();

            // Legacy source: Blueprint.vb BuildItem, CurrentMatQuantity = Max(runs, Ceiling(Round(runs * baseQty * SetBPMaterialModifier(...), 2))).
            var rifterMe10Station98 = CalculateLegacyAdjustedMaterialQuantities(rifterMaterials, runs: 1, materialEfficiency: 10, facilityMaterialMultiplier: 0.98);
            AssertEqual(28224, (int)rifterMe10Station98[34], "legacy numeric parity Rifter Tritanium ME10 facility 0.98");
            AssertEqual(5292, (int)rifterMe10Station98[35], "legacy numeric parity Rifter Pyerite ME10 facility 0.98");
            AssertEqual(2205, (int)rifterMe10Station98[36], "legacy numeric parity Rifter Mexallon ME10 facility 0.98");
            AssertEqual(441, (int)rifterMe10Station98[37], "legacy numeric parity Rifter Isogen ME10 facility 0.98");

            var capMe10Station100 = CalculateLegacyAdjustedMaterialQuantities(capitalArmorPlateMaterials, runs: 1, materialEfficiency: 10, facilityMaterialMultiplier: 1.0);
            AssertEqual(40500, (int)capMe10Station100[34], "legacy numeric parity Capital Armor Plates Tritanium ME10");
            AssertEqual(141750, (int)capMe10Station100[35], "legacy numeric parity Capital Armor Plates Pyerite ME10");
            AssertEqual(40500, (int)capMe10Station100[36], "legacy numeric parity Capital Armor Plates Mexallon ME10");
            AssertEqual(10800, (int)capMe10Station100[37], "legacy numeric parity Capital Armor Plates Isogen ME10");
            AssertEqual(1080, (int)capMe10Station100[38], "legacy numeric parity Capital Armor Plates Nocxium ME10");
            AssertEqual(554, (int)capMe10Station100[39], "legacy numeric parity Capital Armor Plates Zydrine ME10");
            AssertEqual(278, (int)capMe10Station100[40], "legacy numeric parity Capital Armor Plates Megacyte ME10");
            AssertEqual(5, (int)capMe10Station100[2870], "legacy numeric parity Capital Armor Plates Organic Mortar Applicators ME10 rounds up from 4.5");
            AssertEqual(90, (int)capMe10Station100[57457], "legacy numeric parity Capital Armor Plates Reinforced Carbon Fiber ME10");

            var rifterServiceQuantities = rifterMaterials.ToDictionary(
                material => material.TypeId,
                material => BlueprintMaterialMathService.CalculateAdjustedQuantity(material.Quantity, 1, (1 - 10 / 100.0) * 0.98));
            AssertEqual((int)rifterMe10Station98[34], (int)rifterServiceQuantities[34], "OurIPH material math matches legacy Rifter Tritanium numeric formula");
            AssertEqual((int)rifterMe10Station98[35], (int)rifterServiceQuantities[35], "OurIPH material math matches legacy Rifter Pyerite numeric formula");
            AssertEqual((int)rifterMe10Station98[36], (int)rifterServiceQuantities[36], "OurIPH material math matches legacy Rifter Mexallon numeric formula");
            AssertEqual((int)rifterMe10Station98[37], (int)rifterServiceQuantities[37], "OurIPH material math matches legacy Rifter Isogen numeric formula");

            // Legacy source: Blueprint.vb SetProductionTime and SetBPTimeModifier with no extra advanced-manufacturing item skill multiplier.
            var manufacturingMath = new BlueprintManufacturingMathService();
            var productionType = new BlueprintProductionTypeService();
            var timeMultiplier = manufacturingMath.CalculateTimeMultiplier(
                timeEfficiency: 20,
                stationTimeMultiplier: 0.90,
                implantPercent: 0,
                industrySkillLevel: 5,
                advancedIndustrySkillLevel: 5,
                advancedManufacturingSkillMultiplier: 1.0);
            var rifterProductionTime = productionType.CalculateBlueprintProductionTime(
                rifter.BaseProductionTime,
                runs: 1,
                timeMultiplier: timeMultiplier,
                facilityPreset: new FacilityPreset { ProductionLines = 1 });
            AssertEqual(2937.6, rifterProductionTime, 0.0001, "legacy numeric parity Rifter production time TE20 facility 0.90 Industry V Advanced Industry V");

            var revelationCapitalArmorLine = revelationMaterials.First(material => material.TypeId == 21017);
            var adjustedRevelationCapitalArmor = BlueprintMaterialMathService.CalculateAdjustedQuantity(
                revelationCapitalArmorLine.Quantity,
                runs: 1,
                materialMultiplier: 1.0);
            var childBlueprint = database.FindBlueprintByProduct(revelationCapitalArmorLine.TypeId);
            AssertTrue(childBlueprint != null, "legacy numeric parity Revelation fragment resolves Capital Armor Plates child blueprint");
            AssertEqual(4, (int)adjustedRevelationCapitalArmor, "legacy numeric parity Revelation fragment adjusted Capital Armor Plates quantity ME0");
            AssertEqual(4, (int)Math.Ceiling(adjustedRevelationCapitalArmor / (double)childBlueprint.PortionSize), "legacy numeric parity Revelation fragment child runs from component portion size");
        }

        private static void DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedMaterialCostAndFees()
        {
            var database = new EveDatabaseService(AppPaths.EveIphDatabasePath);
            var rifter = database.SearchBlueprints("Rifter", 10).First(bp => bp.ProductName == "Rifter");
            var capitalArmorPlates = database.SearchBlueprints("Capital Armor Plates", 10).First(bp => bp.ProductName == "Capital Armor Plates");

            var rifterMaterials = database.GetManufacturingMaterials(rifter.BlueprintTypeId).ToList();
            var capitalArmorPlateMaterials = database.GetManufacturingMaterials(capitalArmorPlates.BlueprintTypeId).ToList();

            var rifterMe10Station98 = CalculateLegacyAdjustedMaterialQuantities(rifterMaterials, runs: 1, materialEfficiency: 10, facilityMaterialMultiplier: 0.98);
            var rifterUnitPrices = new Dictionary<long, double>
            {
                { 34, 5 },
                { 35, 8 },
                { 36, 12 },
                { 37, 20 }
            };

            // Legacy source: Material.vb SetTotalCostVolume and Materials.vb InsertMaterial sum Quantity * CostPerItem.
            var legacyRifterDirectMaterialCost = CalculateLegacyMaterialTotalCost(rifterMe10Station98, rifterUnitPrices);
            AssertEqual(218736, legacyRifterDirectMaterialCost, 0.0001, "legacy numeric parity Rifter deterministic direct material cost");

            var rifterTraversal = TraverseLegacyDirectBuyFixture(
                rifter,
                rifterMaterials,
                rifterUnitPrices,
                adjustedPrices: new Dictionary<long, double>
                {
                    { 34, 2 },
                    { 35, 3 },
                    { 36, 4 },
                    { 37, 5 }
                },
                runs: 1,
                materialEfficiency: 10,
                facilityMaterialMultiplier: 0.98);
            AssertEqual(legacyRifterDirectMaterialCost, rifterTraversal.MaterialCost, 0.0001, "OurIPH traversal direct-buy fixture matches legacy Rifter material cost formula");
            AssertEqual(94500, rifterTraversal.EstimatedInputValue, 0.0001, "legacy numeric parity Rifter EIV uses unmodified per-run base quantities and adjusted prices");

            // Legacy source: Blueprint.vb SetManufacturingCostsAndFees, FacilityUsage = CLng(EIV * runs) * (CostIndex bonuses + FacilityTax + SCC + Alpha).
            var manufacturingMath = new BlueprintManufacturingMathService();
            var rifterInstallationCost = manufacturingMath.CalculateInstallationCost(
                rifterTraversal.EstimatedInputValue,
                costIndex: 0.03,
                stationCostMultiplier: 1.0,
                factionWarfareMultiplier: 1.0,
                stationTaxRate: 0.01,
                sccIndustryFeeRate: 0.04,
                alphaAccountTaxRate: 0);
            AssertEqual(7560, rifterInstallationCost, 0.0001, "legacy numeric parity Rifter manufacturing usage cost from confirmed EIV formula");

            var capMe10Station100 = CalculateLegacyAdjustedMaterialQuantities(capitalArmorPlateMaterials, runs: 1, materialEfficiency: 10, facilityMaterialMultiplier: 1.0);
            var capUnitPrices = capitalArmorPlateMaterials.ToDictionary(material => material.TypeId, _ => 10.0);
            var legacyCapDirectMaterialCost = CalculateLegacyMaterialTotalCost(capMe10Station100, capUnitPrices);
            AssertEqual(2355570, legacyCapDirectMaterialCost, 0.0001, "legacy numeric parity Capital Armor Plates deterministic direct material cost");

            var capTraversal = TraverseLegacyDirectBuyFixture(
                capitalArmorPlates,
                capitalArmorPlateMaterials,
                capUnitPrices,
                adjustedPrices: new Dictionary<long, double>(),
                runs: 1,
                materialEfficiency: 10,
                facilityMaterialMultiplier: 1.0);
            AssertEqual(legacyCapDirectMaterialCost, capTraversal.MaterialCost, 0.0001, "OurIPH traversal direct-buy fixture matches legacy Capital Armor Plates material cost formula");
        }

        private static void DatabaseBackedBlueprintLegacyNumericParity_UsesRecursiveComponentCostHarness()
        {
            var database = new EveDatabaseService(AppPaths.EveIphDatabasePath);
            var capitalArmorPlates = database.SearchBlueprints("Capital Armor Plates", 10).First(bp => bp.ProductName == "Capital Armor Plates");
            var reinforcedCarbonFiber = database.SearchBlueprints("Reinforced Carbon Fiber", 10).First(bp => bp.ProductName == "Reinforced Carbon Fiber");
            var revelation = database.SearchBlueprints("Revelation", 10).First(bp => bp.ProductName == "Revelation");

            var capMaterials = database.GetManufacturingMaterials(capitalArmorPlates.BlueprintTypeId).ToList();
            var reinforcedCarbonFiberMaterials = database.GetManufacturingMaterials(reinforcedCarbonFiber.BlueprintTypeId).ToList();
            var revelationMaterials = database.GetManufacturingMaterials(revelation.BlueprintTypeId).ToList();
            var revelationCapitalArmorLine = revelationMaterials.First(material => material.TypeId == capitalArmorPlates.ProductTypeId);

            var unitPrices = new Dictionary<long, double>();
            foreach (var material in capMaterials)
            {
                unitPrices[material.TypeId] = 10;
            }

            foreach (var material in reinforcedCarbonFiberMaterials)
            {
                unitPrices[material.TypeId] = 10;
            }

            unitPrices[reinforcedCarbonFiber.ProductTypeId] = 100000;
            unitPrices[capitalArmorPlates.ProductTypeId] = 10000000;

            var adjustedPrices = unitPrices.Keys.ToDictionary(typeId => typeId, _ => 1.0);
            const double costIndex = 0.03;
            const double stationTax = 0.01;
            const double sccFee = 0.04;

            // Legacy source: Blueprint.vb BuildItem recursively builds child blueprints, then inserts child GetTotalComponentCost as the parent component line cost.
            var capRecursive = CalculateLegacyRecursiveComponentCost(
                database,
                capitalArmorPlates,
                runs: 1,
                materialEfficiency: 10,
                facilityMaterialMultiplier: 1.0,
                maxBuildDepth: 1,
                unitPrices: unitPrices,
                adjustedPrices: adjustedPrices,
                costIndex: costIndex,
                stationTaxRate: stationTax,
                sccIndustryFeeRate: sccFee);
            AssertEqual(1, capRecursive.ChildBuilds, "recursive legacy harness builds Reinforced Carbon Fiber child for Capital Armor Plates");
            AssertEqual(8, capRecursive.BuyLines, "recursive legacy harness buys the non-buildable Capital Armor Plates leaf materials");
            AssertEqual(2358712.08, capRecursive.MaterialCost, 0.0001, "legacy recursive parity Capital Armor Plates material cost with built Reinforced Carbon Fiber");
            AssertEqual(20938.24, capRecursive.InstallationCost, 0.0001, "legacy recursive parity Capital Armor Plates root usage cost remains separately visible");
            AssertEqual(2379650.32, capRecursive.TotalBuildCost, 0.0001, "legacy recursive parity Capital Armor Plates total build cost with root usage");

            var capTraversal = TraverseLegacyRecursiveFixture(
                database,
                capitalArmorPlates,
                capMaterials,
                unitPrices,
                adjustedPrices,
                runs: 1,
                materialEfficiency: 10,
                facilityMaterialMultiplier: 1.0,
                childBuildDepth: 0,
                costIndex: costIndex,
                stationTaxRate: stationTax,
                sccIndustryFeeRate: sccFee);
            AssertEqual(capRecursive.MaterialCost, capTraversal.MaterialCost, 0.0001, "OurIPH traversal matches legacy recursive Capital Armor Plates material cost harness");
            AssertEqual(1, capTraversal.BuildMaterialLines, "OurIPH traversal builds one child line in Capital Armor Plates recursive harness");
            AssertEqual(8, capTraversal.BuyMaterialLines, "OurIPH traversal buys eight leaf lines in Capital Armor Plates recursive harness");

            var cycleGuard = CalculateLegacyRecursiveComponentCost(
                database,
                capitalArmorPlates,
                runs: 1,
                materialEfficiency: 10,
                facilityMaterialMultiplier: 1.0,
                maxBuildDepth: 1,
                unitPrices: unitPrices,
                adjustedPrices: adjustedPrices,
                costIndex: 0,
                stationTaxRate: 0,
                sccIndustryFeeRate: 0,
                initialPath: new HashSet<long> { reinforcedCarbonFiber.ProductTypeId });
            AssertEqual(0, cycleGuard.ChildBuilds, "recursive legacy harness path guard suppresses a previsited child component");
            AssertEqual(11354670, cycleGuard.MaterialCost, 0.0001, "recursive legacy harness cycle guard falls back to component buy cost");

            var capForRevelationFragment = CalculateLegacyRecursiveComponentCost(
                database,
                capitalArmorPlates,
                runs: 4,
                materialEfficiency: 0,
                facilityMaterialMultiplier: 1.0,
                maxBuildDepth: 1,
                unitPrices: unitPrices,
                adjustedPrices: adjustedPrices,
                costIndex: costIndex,
                stationTaxRate: stationTax,
                sccIndustryFeeRate: sccFee);
            AssertEqual(10556957.12, capForRevelationFragment.TotalBuildCost, 0.0001, "legacy recursive parity Revelation fragment builds four Capital Armor Plates including child usage");

            var revelationRequests = new List<ChildBlueprintEstimateRequest>();
            var revelationFragmentTraversal = new BlueprintEstimateMaterialTraversalService().Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = revelation,
                    Materials = new[] { revelationCapitalArmorLine },
                    Prices = new Dictionary<long, MarketPrice>
                    {
                        { capitalArmorPlates.ProductTypeId, new MarketPrice { TypeId = capitalArmorPlates.ProductTypeId, SellMin = unitPrices[capitalArmorPlates.ProductTypeId], SellVolume = 100 } }
                    },
                    AdjustedPrices = new Dictionary<long, double> { { capitalArmorPlates.ProductTypeId, 1.0 } },
                    Runs = 1,
                    MaterialEfficiency = 0,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = _ => false,
                    ShouldAlwaysBuy = _ => false,
                    ShouldStopReactionDrilldown = (_, __) => false,
                    GetDefaultChildEfficiency = (_, __, ___) => Tuple.Create(0.0, 0.0),
                    GetCheapestChildStation = _ => new FacilityStation { Name = "Fixture Station", SupportsProduction = true },
                    CalculateChildEstimate = (request, _) =>
                    {
                        revelationRequests.Add(request);
                        var result = CalculateLegacyRecursiveComponentCost(
                            database,
                            request.ChildBlueprint,
                            request.ChildRuns,
                            request.MaterialEfficiency,
                            facilityMaterialMultiplier: 1.0,
                            maxBuildDepth: 1,
                            unitPrices: unitPrices,
                            adjustedPrices: adjustedPrices,
                            costIndex: costIndex,
                            stationTaxRate: stationTax,
                            sccIndustryFeeRate: sccFee);
                        return new BlueprintEstimate
                        {
                            MaterialCost = result.MaterialCost,
                            InstallationCost = result.InstallationCost
                        };
                    },
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetProductUnitPrice = _ => 0,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });

            AssertEqual(1, revelationRequests.Count, "Revelation recursive fragment creates one Capital Armor Plates child request");
            AssertEqual(4, revelationRequests[0].ChildRuns, "Revelation recursive fragment propagates four Capital Armor Plates child runs");
            AssertEqual(capForRevelationFragment.TotalBuildCost, revelationFragmentTraversal.MaterialCost, 0.0001, "OurIPH traversal matches legacy recursive Revelation Capital Armor Plates fragment");
        }

        private static void DatabaseBackedBlueprintLegacyNumericParity_UsesSurplusExcessAndSellbackMappings()
        {
            // Legacy source: Blueprint.vb GetAdjustedQuantity returns remaining required quantity after checking ExcessBuildItems.
            var noExcess = ApplyLegacyGetAdjustedQuantity(requiredQuantity: 100, excessQuantity: 0);
            AssertEqual(100, (int)noExcess.RequiredAfterLookup, "legacy excess mapping leaves requirement unchanged without leftovers");
            AssertEqual(0, (int)noExcess.ExcessAfterLookup, "legacy excess mapping keeps no leftover when no excess exists");
            AssertEqual(0, (int)noExcess.UsedFromExcessAtLookup, "legacy excess mapping records no used quantity without leftovers");

            var partialExcess = ApplyLegacyGetAdjustedQuantity(requiredQuantity: 100, excessQuantity: 40);
            AssertEqual(60, (int)partialExcess.RequiredAfterLookup, "legacy excess mapping subtracts partial leftover from required quantity");
            AssertEqual(40, (int)partialExcess.ExcessAfterLookup, "legacy excess mapping does not mutate partial excess until UseExcessMaterials");
            AssertEqual(60, (int)partialExcess.UsedFromExcessAtLookup, "legacy excess mapping records legacy UsedExcessMaterials quantity from remaining requirement in partial branch");

            var partialAfterUse = ApplyLegacyUseExcessMaterials(excessQuantity: partialExcess.ExcessAfterLookup, materialQuantity: 40);
            AssertEqual(0, (int)partialAfterUse.ExcessAfterUse, "legacy UseExcessMaterials consumes the partial leftover after child build quantity is known");
            AssertEqual(0, (int)partialAfterUse.UsedFromExcessAfterUse, "legacy UseExcessMaterials stores the cloned post-consumption excess quantity");

            var fullExcess = ApplyLegacyGetAdjustedQuantity(requiredQuantity: 100, excessQuantity: 120);
            AssertEqual(0, (int)fullExcess.RequiredAfterLookup, "legacy excess mapping skips build when leftover fully covers requirement");
            AssertEqual(20, (int)fullExcess.ExcessAfterLookup, "legacy excess mapping leaves unused leftover after full coverage");
            AssertEqual(100, (int)fullExcess.UsedFromExcessAtLookup, "legacy excess mapping records consumed quantity when leftover fully covers requirement");

            // Legacy source: Blueprint.vb AdjustSellExcessValue sums ExcessMaterials.GetTotalCost, applies AdjustPriceforTaxesandFees, then clamps below zero to zero.
            var sellDisabled = CalculateLegacySellExcessAmount(grossExcessValue: 1000, sellExcessItems: false, preset: null);
            AssertEqual(0, sellDisabled, 0.0001, "legacy sellback returns zero when SellExcessItems is disabled");

            var noTaxNoBroker = CalculateLegacySellExcessAmount(
                grossExcessValue: 1000,
                sellExcessItems: true,
                preset: new FacilityPreset { IncludeSalesTax = false, BrokerFeeMode = 0 });
            AssertEqual(1000, noTaxNoBroker, 0.0001, "legacy sellback keeps gross excess value when taxes and broker are disabled");

            var taxed = CalculateLegacySellExcessAmount(
                grossExcessValue: 1000,
                sellExcessItems: true,
                preset: new FacilityPreset
                {
                    IncludeSalesTax = true,
                    AccountingSkillLevel = 5,
                    BaseSalesTaxPercent = 4.5,
                    BrokerFeeMode = 2,
                    SpecialBrokerFeePercent = 10,
                    SccBrokerFeeSurchargePercent = 0
                });
            AssertEqual(879.75, taxed, 0.0001, "legacy sellback subtracts sales tax and broker fee before applying surplus credit");

            var clamped = CalculateLegacySellExcessAmount(
                grossExcessValue: 50,
                sellExcessItems: true,
                preset: new FacilityPreset
                {
                    IncludeSalesTax = false,
                    BrokerFeeMode = 2,
                    SpecialBrokerFeePercent = 500,
                    SccBrokerFeeSurchargePercent = 0
                });
            AssertEqual(0, clamped, 0.0001, "legacy sellback clamps negative net excess value to zero");

            var service = new BlueprintEstimateTraversalService();
            var component = Blueprint("Fixture Component", productTypeId: 9000);
            component.PortionSize = 10;
            AssertEqual(100, service.ApplySurplusOffset(100, component, 3, 21, _ => 5, value => value, applySurplusSellback: false), 0.0001, "OurIPH surplus service can represent legacy SellExcessItems false");
            AssertEqual(60, service.ApplySurplusOffset(100, component, 3, 21, _ => 5, value => value - 5), 0.0001, "OurIPH surplus service matches legacy sellback-enabled net value subtraction");
            AssertEqual(100, service.ApplySurplusOffset(100, component, 3, 21, _ => 5, _ => 0), 0.0001, "OurIPH surplus service keeps cost when taxes/fees consume the sellback value");
        }

        private static void DatabaseBackedBlueprintLegacyNumericParity_UsesOreMineralFallbackMappings()
        {
            var database = new EveDatabaseService(AppPaths.EveIphDatabasePath);
            var rifter = database.SearchBlueprints("Rifter", 10).First(bp => bp.ProductName == "Rifter");
            var capitalArmorPlates = database.SearchBlueprints("Capital Armor Plates", 10).First(bp => bp.ProductName == "Capital Armor Plates");
            var rifterMaterials = database.GetManufacturingMaterials(rifter.BlueprintTypeId).ToList();
            var capitalArmorPlateMaterials = database.GetManufacturingMaterials(capitalArmorPlates.BlueprintTypeId).ToList();

            // Legacy source: Blueprint.vb only calls ConvertToOre after BuildItem when ReprocessingFacility exists and ConvertToOre is true.
            AssertFalse(ShouldLegacyConvertMineralsToOre(hasReprocessingFacility: false, convertToOre: false), "legacy ConvertToOre gate stays disabled without a reprocessing facility");
            AssertFalse(ShouldLegacyConvertMineralsToOre(hasReprocessingFacility: false, convertToOre: true), "legacy ConvertToOre gate requires a reprocessing facility even when the flag is true");
            AssertFalse(ShouldLegacyConvertMineralsToOre(hasReprocessingFacility: true, convertToOre: false), "legacy ConvertToOre gate honors the facility ConvertToOre flag");
            AssertTrue(ShouldLegacyConvertMineralsToOre(hasReprocessingFacility: true, convertToOre: true), "legacy ConvertToOre gate enables only when both facility and flag are present");

            // Legacy source: Blueprint.vb AddMaterial skips minerals only when IgnoreMinerals is true; otherwise minerals remain raw materials until optional ConvertToOre runs.
            AssertTrue(database.IsMineral(34), "legacy mineral classification maps Tritanium to inventory group 18");
            AssertTrue(database.IsMineral(35), "legacy mineral classification maps Pyerite to inventory group 18");
            AssertFalse(database.IsMineral(2870), "legacy mineral classification keeps Organic Mortar Applicators outside mineral fallback");
            AssertFalse(database.IsMineral(57457), "legacy mineral classification keeps Reinforced Carbon Fiber outside mineral fallback");

            var traversal = new BlueprintEstimateMaterialTraversalService();
            var rifterTraversal = traversal.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = rifter,
                    Materials = rifterMaterials,
                    Prices = new Dictionary<long, MarketPrice>(),
                    AdjustedPrices = rifterMaterials.ToDictionary(material => material.TypeId, material => 1.0),
                    Runs = 1,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume
                });

            AssertEqual(0, rifterTraversal.MaterialCost, 0.0001, "legacy ore fallback disabled path leaves mineral purchase pricing to the caller");
            AssertEqual(4, rifterTraversal.BuyMaterialLines, "legacy ore fallback disabled path records each direct mineral as a buy/mineral line");
            AssertEqual(0, rifterTraversal.BuildMaterialLines, "legacy ore fallback disabled path does not recurse into mineral child blueprints");
            AssertEqual(32000, (int)rifterTraversal.MineralBuyQuantities[34], "legacy ore fallback disabled path keeps Tritanium as raw mineral quantity");
            AssertEqual(6000, (int)rifterTraversal.MineralBuyQuantities[35], "legacy ore fallback disabled path keeps Pyerite as raw mineral quantity");
            AssertEqual(2500, (int)rifterTraversal.MineralBuyQuantities[36], "legacy ore fallback disabled path keeps Mexallon as raw mineral quantity");
            AssertEqual(500, (int)rifterTraversal.MineralBuyQuantities[37], "legacy ore fallback disabled path keeps Isogen as raw mineral quantity");
            AssertTrue(rifterTraversal.MaterialLines.All(line => line.IsMineralFallback), "legacy ore fallback disabled path marks all Rifter direct minerals as mineral fallback lines");

            var organicMortar = capitalArmorPlateMaterials.First(material => material.TypeId == 2870);
            var organicTraversal = traversal.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = capitalArmorPlates,
                    Materials = new[] { organicMortar },
                    Prices = new Dictionary<long, MarketPrice> { { organicMortar.TypeId, new MarketPrice { TypeId = organicMortar.TypeId, SellMin = 12, SellVolume = 100 } } },
                    AdjustedPrices = new Dictionary<long, double> { { organicMortar.TypeId, 3 } },
                    Runs = 1,
                    MaterialMultiplier = 0.9,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = database.IsMineral,
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume
                });

            AssertEqual(60, organicTraversal.MaterialCost, 0.0001, "legacy no-child non-mineral fallback buys adjusted Organic Mortar Applicators quantity");
            AssertEqual(1, organicTraversal.BuyMaterialLines, "legacy no-child non-mineral fallback records a buy line");
            AssertEqual(0, organicTraversal.MineralBuyQuantities.Count, "legacy no-child non-mineral fallback does not enter mineral fallback bucket");

            var missingChildTraversal = traversal.Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = Blueprint("Fixture Parent", productTypeId: 900000),
                    Materials = new[] { new MaterialRequirement { TypeId = 999999, Name = "Fixture Missing Child", Quantity = 7 } },
                    Prices = new Dictionary<long, MarketPrice> { { 999999, new MarketPrice { TypeId = 999999, SellMin = 3.5, SellVolume = 10 } } },
                    Runs = 1,
                    MaterialMultiplier = 1.0,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ => null,
                    IsMineral = _ => false,
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume
                });
            AssertEqual(24.5, missingChildTraversal.MaterialCost, 0.0001, "legacy missing child blueprint path falls back to direct material buy cost");
            AssertEqual(1, missingChildTraversal.BuyMaterialLines, "legacy missing child blueprint path records a buy line");

            var compressedTritaniumOptions = database.GetReprocessingOptionsForMineral(34, compressedOnly: true);
            var allTritaniumOptions = database.GetReprocessingOptionsForMineral(34, compressedOnly: false);
            AssertTrue(compressedTritaniumOptions.Count > 0, "database-backed ConvertToOre fixture exposes compressed ore candidates for Tritanium");
            AssertTrue(allTritaniumOptions.Count >= compressedTritaniumOptions.Count, "database-backed ConvertToOre fixture exposes at least compressed candidates when compression is not required");
            AssertTrue(compressedTritaniumOptions.All(option => option.IsCompressed), "database-backed ConvertToOre compressed-only fixture filters ore candidates");
            var oreOutputs = database.GetReprocessingOutputsForOres(new[] { compressedTritaniumOptions.First().OreTypeId });
            AssertTrue(oreOutputs.Any(output => output.MineralTypeId == 34), "database-backed ConvertToOre fixture resolves ore output back to Tritanium");

            var orePlanner = new OrePlanningService();
            var oreOutputLookup = oreOutputs
                .GroupBy(output => output.OreTypeId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var realCandidates = orePlanner.BuildCandidates(
                oreOutputLookup,
                new[] { 34L },
                _ => 1.0,
                _ => 2.0);
            AssertEqual(1, realCandidates.Count, "database-backed ore planner builds a candidate from real reprocessing rows");
            AssertTrue(realCandidates[0].OutputByMineral.ContainsKey(34), "database-backed ore planner keeps required Tritanium output");
            var requiredTritanium = realCandidates[0].OutputByMineral[34] + 1;
            var realRemaining = new Dictionary<long, long> { { 34, requiredTritanium } };
            var realPlan = orePlanner.PlanOreForMinerals(
                realRemaining,
                realCandidates,
                new Dictionary<long, double> { { 34, 5 } });
            AssertEqual(1, realPlan.Count, "database-backed ore planner creates a real local ore plan");
            AssertEqual(0, (int)realRemaining[34], "database-backed ore planner covers the real local Tritanium requirement");
            AssertEqual(realCandidates[0].UnitsToReprocess * 2, (int)realPlan[0].Quantity, "database-backed ore planner rounds real local ore to reprocessing batches");
            AssertEqual(realPlan[0].Quantity * 2, orePlanner.CalculatePurchaseCost(
                new Dictionary<long, long> { { 34, requiredTritanium } },
                oreOutputLookup,
                new[] { 34L },
                _ => 1.0,
                _ => 2.0,
                new Dictionary<long, double> { { 34, 5 } }), 0.0001, "database-backed ore planner purchase cost uses planned real local ore quantity and unit price");
        }

        private static void MaterialBuildBuyDecisionService_SelectsBuildOrBuyCost()
        {
            var service = new MaterialBuildBuyDecisionService();

            var cheaperBuild = service.Decide(new MaterialBuildBuyDecisionContext
            {
                BuyCost = 100,
                BuildCost = 80
            });
            AssertTrue(cheaperBuild.ShouldBuild, "build/buy decision builds when child build cost is cheaper");
            AssertEqual(80, cheaperBuild.SelectedCost, 0.0001, "build/buy decision uses build cost when building");

            var marketShortBuild = service.Decide(new MaterialBuildBuyDecisionContext
            {
                BuyCost = 50,
                BuildCost = 80,
                MarketVolumeShort = true
            });
            AssertTrue(marketShortBuild.ShouldBuild, "build/buy decision builds when market volume is short and a build path exists");
            AssertEqual(80, marketShortBuild.SelectedCost, 0.0001, "build/buy decision preserves build cost when market is short");

            var noBuyPrice = service.Decide(new MaterialBuildBuyDecisionContext
            {
                BuyCost = 0,
                BuildCost = 120
            });
            AssertTrue(noBuyPrice.ShouldBuild, "build/buy decision builds when no buy price exists");

            var buy = service.Decide(new MaterialBuildBuyDecisionContext
            {
                BuyCost = 50,
                BuildCost = double.MaxValue,
                MarketVolumeShort = true
            });
            AssertFalse(buy.ShouldBuild, "build/buy decision buys when no build path exists");
            AssertEqual(50, buy.SelectedCost, 0.0001, "build/buy decision uses buy cost when buying");
        }

        private static void EsiPublicContracts_CreatesSamplesOnlyForExactIncludedItemContracts()
        {
            var service = new EsiPublicContractService();
            var target = Blueprint("Moros", productTypeId: 1001);
            var contract = new PublicContractSummary
            {
                ContractId = 77,
                Price = 20_000_000_000,
                StartLocationId = 60003760,
                Title = "Moros"
            };

            var sample = service.TryCreateSample(target, contract, new[]
            {
                new PublicContractItem { TypeId = 1001, Quantity = 2, IsIncluded = true }
            });

            AssertTrue(sample != null, "exact public item contract creates a sample");
            AssertEqual(10_000_000_000, sample.Price, 0.0001, "contract sample unit price divides by included target quantity");
            AssertEqual(77, (int)sample.ContractId, "contract sample persists ESI contract id");

            var packageSample = service.TryCreateSample(target, contract, new[]
            {
                new PublicContractItem { TypeId = 1001, Quantity = 1, IsIncluded = true },
                new PublicContractItem { TypeId = 34, Quantity = 1, IsIncluded = true }
            });

            AssertTrue(packageSample == null, "multi-type public contracts are rejected as bait/package contracts");
        }

        private static void EsiPublicContracts_AllowsExcludedItemsButRejectsInvalidSamples()
        {
            var service = new EsiPublicContractService();
            var target = Blueprint("Moros", productTypeId: 1001);
            var contract = new PublicContractSummary
            {
                ContractId = 78,
                Price = 30_000_000_000,
                StartLocationId = 60003760,
                Title = "Moros with return item"
            };

            var sample = service.TryCreateSample(target, contract, new[]
            {
                new PublicContractItem { TypeId = 1001, Quantity = 1, IsIncluded = true },
                new PublicContractItem { TypeId = 34, Quantity = 1, IsIncluded = false }
            });

            AssertTrue(sample != null, "excluded contract items do not make an exact included-item contract look like bait");
            AssertEqual(1, (int)sample.Quantity, "contract sample quantity counts included target items only");
            AssertEqual(1, sample.ItemCount, "contract sample item count tracks included item rows only");

            var invalid = service.TryCreateSample(target, contract, new[]
            {
                new PublicContractItem { TypeId = 1001, Quantity = 0, IsIncluded = true }
            });

            AssertTrue(invalid == null, "zero-quantity included items are rejected");
        }

        private static void ProjectStock_DistributesOwnedQuantitiesByWaveAndCalculatesInlineEditTotal()
        {
            var lines = new[]
            {
                new BuildProjectMaterial { TypeId = 34, Name = "Tritanium", Wave = 2, Quantity = 40, UnitPrice = 2 },
                new BuildProjectMaterial { TypeId = 34, Name = "Tritanium", Wave = 1, Quantity = 60, UnitPrice = 2 },
                new BuildProjectMaterial { TypeId = 35, Name = "Pyerite", Wave = 1, Quantity = 25, UnitPrice = 4 }
            };

            ProjectStockService.DistributeByWave(lines, new Dictionary<long, long>
            {
                { 34, 75 },
                { 35, 10 }
            });

            AssertEqual(60, (int)lines[1].OwnedQuantity, "stock distribution fills earlier waves first");
            AssertEqual(15, (int)lines[0].OwnedQuantity, "stock distribution carries remaining stock into later wave");
            AssertEqual(50, lines[0].TotalCost, 0.0001, "stock distribution updates remaining material cost");
            AssertEqual(10, (int)lines[2].OwnedQuantity, "stock distribution handles separate type IDs");
            AssertEqual(90, (int)ProjectStockService.GetTotalOwnedThroughLine(lines[0], 30), "inline edit converts line-owned quantity to total stock through that wave");
        }

        private static void BuildProjectStore_RoundTripsProjectStockAndDecisions()
        {
            var directory = Path.Combine(Path.GetTempPath(), "OurIPH.Tests", Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directory, "BuildProjects.xml");
            var store = new BuildProjectStore(filePath);
            var projects = new ObservableCollection<BuildProject>
            {
                new BuildProject
                {
                    Name = "Persistence smoke",
                    CreatedAt = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Local),
                    Items =
                    {
                        new BuildProjectItem
                        {
                            BlueprintTypeId = 100,
                            ProductTypeId = 200,
                            BlueprintName = "Rifter Blueprint",
                            ProductName = "Rifter",
                            GroupName = "Frigate",
                            ProductionType = "Manufacturing",
                            IsRootItem = true,
                            Wave = 1,
                            Runs = 3,
                            PortionSize = 1,
                            RequiredQuantity = 3,
                            ProductPriceSource = "Contract",
                            ProductPriceDetails = "Contract median from 3/3 samples",
                            ContractUnitPrice = 500000,
                            EstimateStatus = "OK"
                        }
                    },
                    Stock =
                    {
                        new BuildProjectStock { TypeId = 34, OwnedQuantity = 1250 }
                    },
                    BuildBuyDecisions =
                    {
                        new BuildProjectBuildBuyDecision { TypeId = 200, Mode = "Build" }
                    }
                }
            };

            store.Save(projects);
            var loaded = new BuildProjectStore(filePath).Load();

            AssertEqual(1, loaded.Count, "project store reloads saved project count");
            AssertEqual("Persistence smoke", loaded[0].Name, "project store reloads project name");
            AssertEqual(1250, (int)loaded[0].Stock[0].OwnedQuantity, "project store reloads material stock");
            AssertEqual("Build", loaded[0].BuildBuyDecisions[0].Mode, "project store reloads build/buy decision");
            AssertEqual("Contract", loaded[0].Items[0].ProductPriceSource, "project store reloads product price source");

            loaded[0].Stock[0].OwnedQuantity = 2000;
            store.Save(loaded);
            var reloaded = new BuildProjectStore(filePath).Load();

            AssertEqual(2000, (int)reloaded[0].Stock[0].OwnedQuantity, "project stock edits persist across reload");

            Directory.Delete(directory, recursive: true);
        }

        private static BlueprintSearchResult Blueprint(
            string productName,
            string groupName = null,
            string categoryName = null,
            string marketGroup = null,
            long productTypeId = 1,
            int metaGroupId = 0,
            bool hasReactionActivity = false)
        {
            return new BlueprintSearchResult
            {
                ProductTypeId = productTypeId,
                ProductName = productName,
                BlueprintName = productName + " Blueprint",
                GroupName = groupName ?? "",
                CategoryName = categoryName ?? "",
                ItemMarketGroup = marketGroup ?? "",
                MetaGroupId = metaGroupId,
                HasReactionActivity = hasReactionActivity
            };
        }

        private static BlueprintSearchResult RankCandidate(
            string productName,
            double profit,
            double iskPerHour,
            double salesVolumeRatio,
            string groupName = "Module")
        {
            return new BlueprintSearchResult
            {
                ProductName = productName,
                BlueprintName = productName + " Blueprint",
                GroupName = groupName,
                Profit = profit,
                IskPerHour = iskPerHour,
                SalesVolumeRatio = salesVolumeRatio,
                ReturnOnInvestmentPercent = 25,
                EstimateStatus = "OK",
                ProductPriceSource = "Market",
                ProductMarketVolume = 100,
                ProducedQuantity = 10
            };
        }

        private static ContractPriceSample Sample(long typeId, string typeName, double price, DateTime observedAt)
        {
            return new ContractPriceSample
            {
                TypeId = typeId,
                TypeName = typeName,
                Price = price,
                ObservedAt = observedAt
            };
        }

        private static SkillRequirement Skill(long typeId, string name, int level)
        {
            return new SkillRequirement
            {
                TypeId = typeId,
                Name = name,
                Level = level
            };
        }

        private static void AssertMaterial(IEnumerable<MaterialRequirement> materials, long typeId, string name, long quantity, string message)
        {
            var material = materials.FirstOrDefault(item => item.TypeId == typeId);
            AssertTrue(material != null, message + " contains " + name);
            AssertEqual(name, material.Name, message + " material name for type " + typeId);
            AssertEqual((int)quantity, (int)material.Quantity, message + " material quantity for " + name);
        }

        private static Dictionary<long, long> CalculateLegacyAdjustedMaterialQuantities(
            IEnumerable<MaterialRequirement> materials,
            int runs,
            double materialEfficiency,
            double facilityMaterialMultiplier)
        {
            var materialModifier = (1 - (materialEfficiency / 100.0)) * facilityMaterialMultiplier;
            return materials.ToDictionary(
                material => material.TypeId,
                material => Math.Max(runs, (long)Math.Ceiling(Math.Round(runs * material.Quantity * materialModifier, 2))));
        }

        private static double CalculateLegacyMaterialTotalCost(
            IReadOnlyDictionary<long, long> adjustedQuantities,
            IReadOnlyDictionary<long, double> unitPrices)
        {
            return adjustedQuantities.Sum(item =>
            {
                double unitPrice;
                return unitPrices.TryGetValue(item.Key, out unitPrice) ? item.Value * unitPrice : 0;
            });
        }

        private static BlueprintMaterialTraversalResult TraverseLegacyDirectBuyFixture(
            BlueprintSearchResult blueprint,
            IEnumerable<MaterialRequirement> materials,
            IReadOnlyDictionary<long, double> unitPrices,
            IReadOnlyDictionary<long, double> adjustedPrices,
            int runs,
            double materialEfficiency,
            double facilityMaterialMultiplier)
        {
            var prices = unitPrices.ToDictionary(
                item => item.Key,
                item => new MarketPrice
                {
                    TypeId = item.Key,
                    SellMin = item.Value,
                    SellVolume = long.MaxValue
                });

            return new BlueprintEstimateMaterialTraversalService().Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = blueprint,
                    Materials = materials.ToList(),
                    Prices = prices,
                    AdjustedPrices = adjustedPrices.ToDictionary(item => item.Key, item => item.Value),
                    Runs = runs,
                    MaterialEfficiency = materialEfficiency,
                    MaterialMultiplier = (1 - (materialEfficiency / 100.0)) * facilityMaterialMultiplier,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = _ => null,
                    IsMineral = _ => false,
                    ShouldAlwaysBuy = _ => true,
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetProductUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });
        }

        private static BlueprintMaterialTraversalResult TraverseLegacyRecursiveFixture(
            EveDatabaseService database,
            BlueprintSearchResult blueprint,
            IEnumerable<MaterialRequirement> materials,
            IReadOnlyDictionary<long, double> unitPrices,
            IReadOnlyDictionary<long, double> adjustedPrices,
            int runs,
            double materialEfficiency,
            double facilityMaterialMultiplier,
            int childBuildDepth,
            double costIndex,
            double stationTaxRate,
            double sccIndustryFeeRate)
        {
            var prices = unitPrices.ToDictionary(
                item => item.Key,
                item => new MarketPrice
                {
                    TypeId = item.Key,
                    SellMin = item.Value,
                    SellVolume = long.MaxValue
                });

            return new BlueprintEstimateMaterialTraversalService().Traverse(
                new BlueprintEstimateContext
                {
                    Blueprint = blueprint,
                    Materials = materials.ToList(),
                    Prices = prices,
                    AdjustedPrices = adjustedPrices.ToDictionary(item => item.Key, item => item.Value),
                    Runs = runs,
                    MaterialEfficiency = materialEfficiency,
                    MaterialMultiplier = (1 - (materialEfficiency / 100.0)) * facilityMaterialMultiplier,
                    RecursionState = new BlueprintEstimateRecursionState()
                },
                new BlueprintEstimateDependencies
                {
                    FindBlueprintByProduct = database.FindBlueprintByProduct,
                    IsMineral = _ => false,
                    ShouldAlwaysBuy = _ => false,
                    ShouldStopReactionDrilldown = (_, __) => false,
                    GetDefaultChildEfficiency = (_, __, ___) => Tuple.Create(0.0, 0.0),
                    GetCheapestChildStation = _ => new FacilityStation { Name = "Fixture Station", SupportsProduction = true },
                    CalculateChildEstimate = (request, _) =>
                    {
                        var result = CalculateLegacyRecursiveComponentCost(
                            database,
                            request.ChildBlueprint,
                            request.ChildRuns,
                            request.MaterialEfficiency,
                            facilityMaterialMultiplier: 1.0,
                            maxBuildDepth: childBuildDepth,
                            unitPrices: unitPrices,
                            adjustedPrices: adjustedPrices,
                            costIndex: costIndex,
                            stationTaxRate: stationTaxRate,
                            sccIndustryFeeRate: sccIndustryFeeRate);
                        return new BlueprintEstimate
                        {
                            MaterialCost = result.MaterialCost,
                            InstallationCost = result.InstallationCost
                        };
                    },
                    GetMaterialUnitPrice = price => price == null ? 0 : price.SellMin,
                    GetProductUnitPrice = _ => 0,
                    GetAvailableMarketVolume = price => price == null ? 0 : price.SellVolume,
                    ApplySalesTaxesAndFees = (value, _, __) => value
                });
        }

        private static LegacyRecursiveCostResult CalculateLegacyRecursiveComponentCost(
            EveDatabaseService database,
            BlueprintSearchResult blueprint,
            int runs,
            double materialEfficiency,
            double facilityMaterialMultiplier,
            int maxBuildDepth,
            IReadOnlyDictionary<long, double> unitPrices,
            IReadOnlyDictionary<long, double> adjustedPrices,
            double costIndex,
            double stationTaxRate,
            double sccIndustryFeeRate,
            ISet<long> initialPath = null)
        {
            var path = initialPath == null ? new HashSet<long>() : new HashSet<long>(initialPath);
            return CalculateLegacyRecursiveComponentCostCore(
                database,
                blueprint,
                runs,
                materialEfficiency,
                facilityMaterialMultiplier,
                maxBuildDepth,
                unitPrices,
                adjustedPrices,
                costIndex,
                stationTaxRate,
                sccIndustryFeeRate,
                path);
        }

        private static LegacyRecursiveCostResult CalculateLegacyRecursiveComponentCostCore(
            EveDatabaseService database,
            BlueprintSearchResult blueprint,
            int runs,
            double materialEfficiency,
            double facilityMaterialMultiplier,
            int maxBuildDepth,
            IReadOnlyDictionary<long, double> unitPrices,
            IReadOnlyDictionary<long, double> adjustedPrices,
            double costIndex,
            double stationTaxRate,
            double sccIndustryFeeRate,
            HashSet<long> path)
        {
            var result = new LegacyRecursiveCostResult();
            if (database == null || blueprint == null || runs <= 0)
            {
                return result;
            }

            path.Add(blueprint.ProductTypeId);
            var materials = database.GetManufacturingMaterials(blueprint.BlueprintTypeId).ToList();
            var adjustedQuantities = CalculateLegacyAdjustedMaterialQuantities(materials, runs, materialEfficiency, facilityMaterialMultiplier);

            foreach (var material in materials)
            {
                var adjustedQuantity = adjustedQuantities[material.TypeId];
                result.EstimatedInputValue += material.Quantity * runs * GetDictionaryValue(adjustedPrices, material.TypeId);

                var childBlueprint = database.FindBlueprintByProduct(material.TypeId);
                if (childBlueprint != null)
                {
                    result.ChildLookups++;
                }

                if (childBlueprint != null && maxBuildDepth > 0 && !path.Contains(childBlueprint.ProductTypeId))
                {
                    var childRuns = (int)Math.Ceiling(adjustedQuantity / (double)Math.Max(1, childBlueprint.PortionSize));
                    var childResult = CalculateLegacyRecursiveComponentCostCore(
                        database,
                        childBlueprint,
                        childRuns,
                        materialEfficiency: 0,
                        facilityMaterialMultiplier: 1.0,
                        maxBuildDepth: maxBuildDepth - 1,
                        unitPrices: unitPrices,
                        adjustedPrices: adjustedPrices,
                        costIndex: costIndex,
                        stationTaxRate: stationTaxRate,
                        sccIndustryFeeRate: sccIndustryFeeRate,
                        path: path);

                    var buyComparisonCost = GetDictionaryValue(unitPrices, material.TypeId) * Math.Max(1, childBlueprint.PortionSize) * childRuns;
                    var shouldBuild = buyComparisonCost <= 0 || buyComparisonCost > childResult.TotalBuildCost;
                    if (shouldBuild)
                    {
                        result.MaterialCost += childResult.TotalBuildCost;
                        result.ChildBuilds++;
                        result.ChildRuns += childRuns;
                        result.ChildBuildCost += childResult.TotalBuildCost;
                        continue;
                    }
                }

                var buyCost = adjustedQuantity * GetDictionaryValue(unitPrices, material.TypeId);
                result.MaterialCost += buyCost;
                result.DirectBuyCost += buyCost;
                result.BuyLines++;
            }

            result.InstallationCost = new BlueprintManufacturingMathService().CalculateInstallationCost(
                Math.Round(result.EstimatedInputValue, 0),
                costIndex,
                stationCostMultiplier: 1.0,
                factionWarfareMultiplier: 1.0,
                stationTaxRate: stationTaxRate,
                sccIndustryFeeRate: sccIndustryFeeRate,
                alphaAccountTaxRate: 0);
            path.Remove(blueprint.ProductTypeId);
            return result;
        }

        private static double GetDictionaryValue(IReadOnlyDictionary<long, double> values, long typeId)
        {
            double value;
            return values != null && values.TryGetValue(typeId, out value) ? value : 0;
        }

        private static LegacyExcessLookupResult ApplyLegacyGetAdjustedQuantity(long requiredQuantity, long excessQuantity)
        {
            var result = new LegacyExcessLookupResult
            {
                RequiredAfterLookup = Math.Max(0, requiredQuantity),
                ExcessAfterLookup = Math.Max(0, excessQuantity)
            };

            if (requiredQuantity <= 0 || excessQuantity <= 0)
            {
                return result;
            }

            if (requiredQuantity > excessQuantity)
            {
                result.RequiredAfterLookup = requiredQuantity - excessQuantity;
                result.ExcessAfterLookup = excessQuantity;
                result.UsedFromExcessAtLookup = result.RequiredAfterLookup;
            }
            else
            {
                result.RequiredAfterLookup = 0;
                result.ExcessAfterLookup = excessQuantity - requiredQuantity;
                result.UsedFromExcessAtLookup = requiredQuantity;
            }

            return result;
        }

        private static LegacyExcessUseResult ApplyLegacyUseExcessMaterials(long excessQuantity, long materialQuantity)
        {
            var remaining = Math.Max(0, excessQuantity - Math.Max(0, materialQuantity));
            return new LegacyExcessUseResult
            {
                ExcessAfterUse = remaining,
                UsedFromExcessAfterUse = remaining
            };
        }

        private static double CalculateLegacySellExcessAmount(double grossExcessValue, bool sellExcessItems, FacilityPreset preset)
        {
            if (!sellExcessItems || grossExcessValue <= 0)
            {
                return 0;
            }

            return Math.Max(0, new SalesFeeService().ApplySalesTaxesAndFees(grossExcessValue, preset, null));
        }

        private static bool ShouldLegacyConvertMineralsToOre(bool hasReprocessingFacility, bool convertToOre)
        {
            return hasReprocessingFacility && convertToOre;
        }

        private sealed class LegacyRecursiveCostResult
        {
            public double MaterialCost { get; set; }
            public double InstallationCost { get; set; }
            public double EstimatedInputValue { get; set; }
            public double DirectBuyCost { get; set; }
            public double ChildBuildCost { get; set; }
            public int ChildLookups { get; set; }
            public int ChildBuilds { get; set; }
            public int ChildRuns { get; set; }
            public int BuyLines { get; set; }

            public double TotalBuildCost
            {
                get { return MaterialCost + InstallationCost; }
            }
        }

        private sealed class LegacyExcessLookupResult
        {
            public long RequiredAfterLookup { get; set; }
            public long ExcessAfterLookup { get; set; }
            public long UsedFromExcessAtLookup { get; set; }
        }

        private sealed class LegacyExcessUseResult
        {
            public long ExcessAfterUse { get; set; }
            public long UsedFromExcessAfterUse { get; set; }
        }

        private static void AssertTrue(bool condition, string message)
        {
            _assertions++;
            if (!condition)
            {
                throw new InvalidOperationException("Assertion failed: " + message);
            }
        }

        private static void AssertFalse(bool condition, string message)
        {
            AssertTrue(!condition, message);
        }

        private static void AssertEqual(int expected, int actual, string message)
        {
            _assertions++;
            if (expected != actual)
            {
                throw new InvalidOperationException(string.Format("{0}. Expected {1}, actual {2}.", message, expected, actual));
            }
        }

        private static void AssertEqual(string expected, string actual, string message)
        {
            _assertions++;
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(string.Format("{0}. Expected '{1}', actual '{2}'.", message, expected, actual));
            }
        }

        private static void AssertEqual(double expected, double actual, double tolerance, string message)
        {
            _assertions++;
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new InvalidOperationException(string.Format("{0}. Expected {1}, actual {2}.", message, expected, actual));
            }
        }
    }
}
