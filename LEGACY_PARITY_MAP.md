# Legacy Parity Map

Date: 2026-05-17

Purpose: track where production/calculation behavior comes from in legacy VB.NET EVE IPH, where the matching OurIPH logic now lives, and which pieces are covered by local database-backed fixtures. This file is not a claim that current OurIPH numbers are fully correct; it separates safety-net characterization from legacy/database parity work.

## Local Data Sources
- Preferred fixture database: `LatestFiles/EVEIPH DB.sqlite`.
- Fallback fixture database: `Root Directory/EVEIPH DB.sqlite`.
- OurIPH loader: `OurIPH/OurIPH/Services/AppPaths.cs` selects the database, and `EveDatabaseService` reads `ALL_BLUEPRINTS_FACT`, `ALL_BLUEPRINT_MATERIALS_FACT`, `INVENTORY_TYPES`, `INVENTORY_GROUPS`, `INDUSTRY_ACTIVITY_PRODUCTS`, and related price/static tables.

## Database-Backed Fixture Cases
- Simple T1 blueprint: `Rifter` / `Rifter Blueprint`.
  - Local fixture confirms 4 direct mineral material lines: Tritanium 32,000; Pyerite 6,000; Mexallon 2,500; Isogen 500.
  - Child chain expectation: no child blueprint recursion; minerals aggregate into mineral fallback quantities.
  - Numeric parity now confirms legacy adjusted material quantities for ME 10, facility material multiplier 0.98: Tritanium 28,224; Pyerite 5,292; Mexallon 2,205; Isogen 441.
  - Numeric parity now confirms deterministic direct material total cost with fixture unit prices 5/8/12/20: 218,736 ISK from legacy `Material.GetTotalCost`.
  - Numeric parity now confirms a deterministic EIV/manufacturing usage fragment: EIV 94,500 ISK and usage cost 7,560 ISK with cost index 0.03, station tax 0.01, SCC 0.04, no alpha tax.
  - Numeric parity now confirms legacy production time for TE 20, facility time multiplier 0.90, Industry V, Advanced Industry V, no implant: 2,937.6 seconds from base time 6,000.
- Component-chain blueprint: `Capital Armor Plates` / `Capital Armor Plates Blueprint`.
  - Local fixture confirms 9 direct material lines.
  - Child chain expectation: `Reinforced Carbon Fiber` resolves to `Reinforced Carbon Fiber Reaction Formula`.
  - Numeric parity now confirms legacy adjusted material quantities for ME 10, facility material multiplier 1.0: Tritanium 40,500; Pyerite 141,750; Mexallon 40,500; Isogen 10,800; Nocxium 1,080; Zydrine 554; Megacyte 278; Organic Mortar Applicators 5; Reinforced Carbon Fiber 90.
  - Numeric parity now confirms deterministic direct material total cost with fixture 10 ISK/unit prices: 2,355,570 ISK from legacy `Material.GetTotalCost`.
  - Bounded recursive parity now confirms building `Reinforced Carbon Fiber` for one child run when deterministic buy cost is higher than child build cost. Confirmed recursive material cost: 2,358,712.08 ISK; separate root usage cost: 20,938.24 ISK; total build cost: 2,379,650.32 ISK.
  - Bounded path-guard parity now confirms a previsited `Reinforced Carbon Fiber` child is not recursed into and falls back to deterministic buy cost: 11,354,670 ISK.
- Capital-like/component-chain blueprint: `Revelation` / `Revelation Blueprint`.
  - Local fixture confirms 19 direct material lines.
  - Child chain expectation: all 19 direct material lines resolve to child blueprints in the local database, including `Capital Armor Plates` and `Capital Siege Array`.
  - Traversal fixture confirms deterministic child request creation, market-volume-short propagation, cheaper-build branch selection, and path guard fallback for a previsited component.
  - Numeric parity now confirms a small safe child-run fragment: `Capital Armor Plates` direct quantity 4 at ME 0/facility 1.0 produces adjusted quantity 4 and child runs 4 because the child blueprint portion size is 1.
  - Bounded recursive parity now confirms the `Revelation` -> `Capital Armor Plates` fragment: four child runs produce a deterministic child total build cost of 10,556,957.12 ISK, including the child `Capital Armor Plates` usage and its built `Reinforced Carbon Fiber` child.
- Copy-only/special blueprint: `Zirnitra` / `Zirnitra Blueprint`.
  - Local fixture confirms copy-only/BPC-only state through missing blueprint market group, 19 direct material lines, and Triglavian capital components such as `Capital Absorption Thruster Array`.

## Legacy To OurIPH Mapping
| Area | Legacy VB.NET source | OurIPH source | Current coverage |
| --- | --- | --- | --- |
| Blueprint static data load | `Blueprint.vb` constructor around blueprint/material SQL; `EVEBlueprints.vb` blueprint scan/import paths | `EveDatabaseService.SearchBlueprints`, `FindBlueprintByProduct`, `GetManufacturingMaterials`, `GetInventionInfo`, `LoadDecryptors` | Database-backed fixtures load Rifter, Capital Armor Plates, Revelation, Zirnitra and assert material/child-chain structure. |
| Material calculation and recursive component traversal | `Blueprint.vb` `BuildItems`, `BuildItem`, `GetAdjustedQuantity`, `UseExcessMaterials`, `SetBuiltItem`; `Material.vb` `SetTotalCostVolume`; `Materials.vb` `InsertMaterial` | `MainWindow.CalculateBlueprintEstimate` plus `BlueprintEstimateContext`, `BlueprintEstimateDependencies`, `BlueprintEstimateTraversalService`, `BlueprintMaterialEstimateLineService`, `BlueprintEstimateMaterialTraversalService`, `BlueprintEstimateMaterialOrchestrationService`, `BlueprintEstimateResultAssembler` | Characterization tests cover delegate traversal behavior, material orchestration boundary creation/aggregation, and final estimate assembly; database fixtures cover real child-chain resolution and path guard behavior. Numeric parity now covers direct adjusted material quantities, deterministic direct material total cost, bounded Capital Armor Plates -> Reinforced Carbon Fiber recursive cost, path guard fallback, and a Revelation -> Capital Armor Plates recursive fragment. Full end-to-end final estimate parity is still provisional. |
| Build/buy decision | `Blueprint.vb` `GetBuildFlag`, `BuildifNotEnoughItemsonMarket`, `ManualBuildBuyValue` | `MaterialBuildBuyDecisionService`, `BlueprintEstimateMaterialTraversalService`, `MainWindow.ShouldAlwaysBuy`, `MainWindow.ShouldStopReactionDrilldown` | Unit tests cover cheaper build, buy fallback, market-volume-short forcing, and manual-ish deterministic branches. Database fixture covers Revelation build branch with deterministic fake prices/costs. |
| ME/TE and facility modifiers | `Blueprint.vb` `SetBPMaterialModifier`, `SetBPTimeModifier`, `SetProductionTime`, `SetManufacturingCostsAndFees`, `GetMETEforBP`; `ManufacturingFacility.vb` facility selection/bonus methods | `BlueprintEfficiencyService`, `BlueprintManufacturingMathService`, `BlueprintProductionTypeService`, `FacilityCatalogService`, `FacilityActivityStationService`, `BlueprintEstimateSelectionService` | Service tests cover copy-only 0/0, T1 defaults, T2 invention defaults, reaction defaults, station/decryptor selection, manufacturing multipliers and usage math. Numeric parity now covers Rifter production-time and EIV/manufacturing usage fragments. Full legacy numeric parity still needs database-backed end-to-end expected values. |
| Invention/copy costs and times | `Blueprint.vb` `InventBlueprint`, `InventREBlueprint`, `SetCopyUsage`, `SetInventionUsage`, `SetInventionChance`, `SetInventionTime`, `GetCopyTime` | `BlueprintInventionMathService`, `BlueprintInventionCostService`, `BlueprintInventionTimeService`, `FacilityActivityStationService` | Service tests cover deterministic invention planning, datacore/decryptor/copy buckets, usage cost formula, copy/invention time formulas. Real legacy parity for T2/T3 examples remains provisional. |
| Profit/material cost assembly | `Blueprint.vb` getters such as `GetTotalRawCost`, `GetTotalBuildCost`, `GetTotalComponentCost`, `GetTotalRawProfit`, `GetTotalComponentProfit`, `GetTotalIskperHourRaw`, `GetTotalIskperHourComponents`; `frmMain.vb` manufacturing compare paths | `BlueprintProfitabilityService`, `BlueprintEstimateApplicationService`, `ProjectItemEstimateApplicationService`, `ProjectItemEstimateDisplayService`, `BlueprintEstimateResultAssembler`, `SalesFeeService`, `SalesVolumeRatioService`, remaining recursive orchestration inside `MainWindow.CalculateBlueprintEstimate` | Unit tests cover pure profitability, fee math, blueprint/project estimate application, product price/source display mapping, SVR/liquidity, project row revenue/status mapping, and final estimate assembly. Database-backed numeric final estimate parity is not yet locked. |
| Shopping/current project material rollup | `ShoppingList.vb` `InsertShoppingItem`, `GetUpdatedQuantity`, `GetNewMatQuantity`, `GetFullBuildMaterialList`, `GetFullBuyList`; `CurrentProjects.vb` `CreateProjectFromShoppingList`, `GetBuildDependencies`, `CalculateBuildWave` | `BuildQueueService`, `BuildProjectDecisionService`, `ProjectStockService`, `ProjectMaterialImportService`, `UsedByListService`, `BuildProjectStore` | Unit/store tests cover queue merge, stock distribution, clipboard material import, used-by strings, project persistence. Interactive project workflow remains manual UI checklist work. |

## Facilities / Station Presets Legacy Mapping
- Legacy facility state:
  - Legacy source: `ManufacturingFacility.vb` `IndustryFacility` fields and `Copy` method.
  - Behavior: legacy stores facility id/name, region/system names and ids, solar-system security, cost index, include-cost/time/usage flags, activity cost, facility tax, material/time/cost multipliers, FW upgrade level, ore/ice/moon refine rates, and `ConvertToOre`. Saved facility equality also compares these fields.
  - OurIPH correspondence: `FacilityPreset` and `FacilityStation` persisted by `FacilityPresetStore`, with station name/system ids/security, structure type, production type, rig slots, service modules, multipliers, material bonus, industry/sales tax, FW level, support flag, and validation message. Preset-level settings cover skills, taxes/broker settings, default ME/TE/runs, build/buy/ore-conversion flags, and reprocessing/invention skills.
  - Status: local XML round-trip is now regression-tested for all OurIPH preset/station fields. Legacy has additional per-activity include-cost/time/usage and detailed refine-rate fields that are not yet modeled one-for-one in the WPF preset.
- Automatic system/region/index lookup:
  - Legacy source: `ManufacturingFacility.vb` `LoadFacilityRegions`, `GetSystemsSQL`, `LoadFacilitySystems`, `LoadFacilities`, and selected facility loading around the `INDUSTRY_SYSTEMS_COST_INDICIES` joins.
  - Behavior: region/system lists are populated from local DB; system combo text includes cost index when available; choosing a facility loads region id, system id, security, and cost index. Cost index is read from `INDUSTRY_SYSTEMS_COST_INDICIES` by solar system and activity, falling back to zero.
  - OurIPH correspondence: `FacilityCatalogService.LoadRegions`, `LoadSystems`, `FindSystem`, `HasNpcStations`, `GetSystemCostIndex`, and `MainWindow.SelectRegionAndSystem` / `SaveFacility_Click`.
  - Status: local DB system/security/id lookup is implemented. This pass fixed manual system save to resolve `SolarSystemId`, `RegionId`, and security through `FindSystem`, so users do not have to re-enter system metadata. Cost index remains read-only/displayed from the local DB/cache; refreshing current ESI indices is still live-data work.
- Public structures and ESI refresh:
  - Legacy source: `ESI.vb` `UpdateIndustrySystemsCostIndex`, `UpdatePublicStructureswithMarkets`; `ProgramSettings.vb` `LoadESISystemCostIndiciesDataonStartup`, `LoadESIPublicStructuresonStartup`, `SaveFacilitiesbyChar`, and `ShareSavedFacilities`.
  - Behavior: legacy could refresh system cost indices and public structures from ESI and cache them in local tables/settings, optionally shared across characters.
  - OurIPH correspondence: static/local DB facility catalog plus XML presets; no authenticated/private structure refresh exists.
  - Status: local/cached static data is used; live ESI cost-index/public-structure refresh and private structure services remain external/live-data blockers.
- Facility modifiers, rigs, and service modules:
  - Legacy source: `ManufacturingFacility.vb` `RefreshMMTMCMBonuses`, `GetFacilityBonusMulitiplier`, `GetStructureModifier`, `GetInstalledModules`, `GetRigBonuses`, and validation/UI event loading guards such as `LoadingRegions`, `LoadingSystems`, and `LoadingFacilities`.
  - Behavior: structure/service/rig options are filtered by activity, security, fitting rules, and available local data. Loading flags prevent combo-box population from overwriting the selected facility while the UI is being filled.
  - OurIPH correspondence: `FacilityCatalogService.LoadStructureTypes`, `LoadRigOptions`, `LoadServiceModuleOptions`, `Calculate`, `ValidateProduction`, and `MainWindow` facility selection handlers.
  - Status: this pass added a WPF `_loadingFacilitySelection` guard so loading an existing preset no longer mutates the station through selection-change handlers. Validation messages for facilities are now readable ASCII and default new preset/station names no longer use mojibake.

## Surplus / Leftovers / Sellback Legacy Mapping
- Excess/leftover lookup:
  - Legacy source: `Blueprint.vb GetAdjustedQuantity`.
  - Formula/behavior: if no matching excess material exists, required quantity is unchanged. If `required > excess`, returned required quantity is `required - excess`; the excess list is not mutated until `UseExcessMaterials` later knows the built quantity. If `required <= excess`, returned required quantity is `0`, the excess list is reduced to `excess - required`, and the build path is skipped for that line.
  - OurIPH correspondence: project stock offsets use `ProjectStockService`; recursive overbuild/excess is represented at the estimate boundary by `BlueprintEstimateTraversalService.ApplySurplusOffset` plus the bounded test harness.
  - Status: confirmed for narrow test cases. Open question: full recursive propagation of `UsedExcessMaterials` into project/build-list UI remains provisional.
- Partial-excess quirk:
  - Legacy source: `Blueprint.vb GetAdjustedQuantity`.
  - Behavior: in the `required > excess` branch, legacy records `UsedExcessMaterials` with the remaining required quantity (`required - excess`) before `UseExcessMaterials` later consumes the actual excess list. This is source-confirmed and preserved as documented behavior in tests, but it should be treated carefully before copying into production UI semantics.
  - Status: confirmed as source behavior, economic impact still provisional outside the narrow harness.
- Excess consumption after build quantity:
  - Legacy source: `Blueprint.vb UseExcessMaterials`.
  - Formula/behavior: exits when `MaterialQuantity <= 0`; otherwise subtracts `MaterialQuantity` from the matching excess material and reinserts only a positive remainder. When a saved excess snapshot is supplied, non-target excess entries are restored if recursive drilldown consumed them but the parent is later bought instead.
  - OurIPH correspondence: no full production equivalent yet for recursive excess list mutation; project material stock uses `ProjectStockService`.
  - Status: confirmed for no/partial/full small cases; broader recursive restoration remains provisional.
- Sellback value:
  - Legacy source: `Blueprint.vb AdjustSellExcessValue`; tax/broker helper in `Globals.vb AdjustPriceforTaxesandFees`.
  - Formula/behavior: when `SellExcessItems = False`, sellback credit is zero. When true, sum `ExcessMaterials.GetTotalMaterialsCost`, subtract sales tax if enabled and broker fee according to broker settings, then clamp negative net value to zero.
  - OurIPH correspondence: `SalesFeeService.ApplySalesTaxesAndFees`; `BlueprintEstimateTraversalService.ApplySurplusOffset`.
  - Status: confirmed for disabled, no-tax/no-broker, taxed/brokered, and clamp-to-zero small cases.
- Surplus child output offset:
  - Legacy source: child overbuild is produced via `BuildQuantity = Ceiling(required / childPortionSize)` and excess materials are inserted into `BPExcessMaterials`/`ExcessMaterials`; sellback then affects `SetPriceData` and `GetBuildFlag` only when sellback is enabled.
  - OurIPH correspondence: `BlueprintEstimateTraversalService.ApplySurplusOffset` now has an explicit `applySurplusSellback` flag with default `true`, preserving current behavior while allowing legacy `SellExcessItems = False` parity tests.
  - Status: narrow sellback-enabled and sellback-disabled behavior is confirmed. Full end-to-end recursive excess list accounting with user settings remains provisional.

## ConvertToOre / Ore-Mineral Fallback Legacy Mapping
- ConvertToOre call-site gate:
  - Legacy source: `Blueprint.vb` after `BuildItem`/`SetPriceData`.
  - Formula/behavior: `ConvertToOre.GetOresfromMinerals` is called only when `ReprocessingFacility IsNot Nothing` and `ReprocessingFacility.ConvertToOre = True`. Otherwise raw minerals remain in `RawMaterials`; mineral skipping is controlled separately by the `IgnoreMinerals` flag in `AddMaterial`.
  - OurIPH correspondence: `BlueprintEstimateMaterialTraversalService` aggregates mineral fallback quantities; `MainWindow.GetMineralPurchaseCost`, `GetOrePlanPurchaseCost`, `ConvertProjectMineralsToOre`, and `ConvertProjectMineralsToOreByWave` handle ore/mineral replacement cost and project material replacement when enabled.
  - Status: confirmed for gate behavior and disabled-conversion fallback in `DatabaseBackedBlueprintLegacyNumericParity_UsesOreMineralFallbackMappings`.
- Mineral classification:
  - Legacy source: `Blueprint.vb AddMaterial`, where group id `18` is the mineral group for `IgnoreMinerals`; mineral rows otherwise flow into raw material processing.
  - OurIPH correspondence: `EveDatabaseService.IsMineral`, which reads `INVENTORY_TYPES.groupID = 18`.
  - Status: confirmed for Tritanium/Pyerite as minerals and Organic Mortar Applicators/Reinforced Carbon Fiber as non-minerals.
- Disabled ConvertToOre / raw mineral fallback:
  - Legacy source: `Blueprint.vb BuildItem` raw material insertion plus the post-build ConvertToOre gate.
  - Behavior: a simple mineral-only blueprint such as Rifter keeps mineral required quantities as raw material requirements when no ore conversion is active.
  - OurIPH correspondence: `BlueprintEstimateMaterialTraversalService` sets `IsMineralFallback`, increments buy/mineral lines, stores `MineralBuyQuantities`, leaves direct mineral `MaterialCost` at zero for later mineral/ore purchase-cost handling, and still contributes adjusted-price EIV.
  - Status: confirmed for Rifter quantities: Tritanium 32,000; Pyerite 6,000; Mexallon 2,500; Isogen 500.
- Non-mineral/no-child fallback:
  - Legacy source: `Blueprint.vb BuildItem` and `AddMaterial`; when a material has no buildable child blueprint path, it is inserted/bought as a material line.
  - OurIPH correspondence: `BlueprintEstimateMaterialTraversalService` buy branch through `BlueprintMaterialEstimateLineService` and `MaterialBuildBuyDecisionService`.
  - Status: confirmed for `Organic Mortar Applicators` in `Capital Armor Plates` and a synthetic missing-child fixture.
- Full ConvertToOre optimization:
  - Legacy source: `ConvertToOre.vb GetOresfromMinerals`; `ReprocessingPlant.Reprocess`.
  - Formula/behavior: builds candidate ore/ice/moon-ore rows from `ConversionToOreSettings.SelectedOres`, compression choices, variants, ignored refined items, prices, volume, and refinery output. It refines each candidate, builds an `LpSolve` model with `GE` constraints for required refined items, minimizes by `Refine Price`, `Ore/Ice Price`, or `Ore/Ice Volume`, rounds selected outputs up to refine-unit batches, removes converted minerals from the material list, inserts ore material lines, calculates returned excess minerals, and accumulates reprocessing usage fees.
  - OurIPH correspondence: current project/material code now uses `OrePlanningService` for the greedy ore candidate construction, batch planning, and purchase-cost helper behind `GetOrePlanPurchaseCost` and `ConvertProjectMineralsToOreByWave`, plus `EveDatabaseService.GetReprocessingOptionsForMineral` / `GetReprocessingOutputsForOres`; there is not yet an LP-equivalent service.
  - Status: structural local data availability is confirmed for Tritanium compressed ore candidates and reprocessing outputs. The extracted greedy planner has deterministic unit coverage and a real local reprocessing-row structural check, but exact numeric LP ore-plan parity remains provisional and should not be treated as confirmed until a deterministic legacy-derived fixture is added.

## Confirmed Formula/Behavior Slices
- Material quantity adjustment is confirmed against `Blueprint.vb BuildItem`: `Max(runs, Ceiling(Round(runs * baseQuantity * ((1 - ME/100) * facilityMaterialMultiplier), 2)))`. Covered by database-backed numeric parity for Rifter and Capital Armor Plates.
- Direct material total cost is confirmed against `Material.vb SetTotalCostVolume` and `Materials.vb InsertMaterial`: adjusted `Quantity * CostPerItem`, summed across inserted material lines. Covered by deterministic database-backed numeric parity for Rifter and Capital Armor Plates.
- Manufacturing usage fee is confirmed against `Blueprint.vb SetManufacturingCostsAndFees`: `CLng(EIV * runs) * ((CostIndex * CostMultiplier * FWMultiplier) + FacilityTax + SCCIndustryFee + AlphaTax)`. Covered by a deterministic Rifter EIV/usage fragment.
- Bounded recursive component cost is confirmed against `Blueprint.vb BuildItem` / `GetBuildFlag` / `SetPriceData` for the narrow cases listed above: child runs are `Ceiling(adjustedRequiredQuantity / childPortionSize)`, deterministic build is selected when `(unitBuyPrice * childPortionSize * childRuns) > childTotalBuildCost`, and child `TotalComponentCost` is inserted as the parent component line cost. Surplus sellback is disabled in the harness.
- Surplus/excess/sellback behavior is confirmed for narrow cases against `Blueprint.vb GetAdjustedQuantity`, `UseExcessMaterials`, `AdjustSellExcessValue`, and `Globals.vb AdjustPriceforTaxesandFees`: no/partial/full leftover handling, sellback disabled, tax/broker deduction, and clamp-to-zero.
- ConvertToOre/mineral fallback gate and disabled-conversion behavior are confirmed for narrow cases against `Blueprint.vb`, `ConvertToOre.vb`, and inventory group 18 classification: Rifter direct minerals remain raw mineral fallback quantities when conversion is not active, while non-mineral/no-child materials stay direct buy lines.
- Production time for a simple manufacturing job is confirmed against `Blueprint.vb SetProductionTime` / `SetBPTimeModifier`: `baseProductionTime * (1 - TE/100) * facilityTimeMultiplier * implantMultiplier * (1 - IndustrySkill*0.04) * (1 - AdvancedIndustrySkill*0.03) * sessions`, where sessions are derived from runs, production lines, and blueprint count. Covered by a Rifter numeric parity fragment.
- Facility material/time multipliers and the confirmed installation-cost formula are covered in `BlueprintManufacturingMathService`.
- Sales tax, broker fee, station fee, and net revenue are covered in `SalesFeeService`.
- Build/buy branch selection is covered in `MaterialBuildBuyDecisionService`.
- Recursion path guard, child request run rounding, and surplus offset are covered in `BlueprintEstimateTraversalService`.
- Delegate-backed material traversal is covered with both mocked characterization and real database-backed chain fixtures. The material orchestration, final estimate assembly, estimate display mapping, and greedy ore-planning services are covered by characterization tests that preserve context/dependency pass-through, mineral fallback aggregation, production time composition, invention breakdown mapping, build/buy line counts, profitability recalculation, produced quantity/market volume, project row revenue/status mapping, contract-vs-market source display fields, and deterministic ore candidate/plan/cost behavior.

## Provisional Or Not Yet Confirmed Against Legacy
- Exact final numeric material cost/profit/ISK-hour for real capital chains such as Revelation and Zirnitra.
- Exact legacy numeric LP handling for ore conversion when prices are missing, ore purchase is preferable, or `MinimizeOn`/compression/ignored refined items change selected ore quantities.
- Exact recursive component final cost parity for broader Capital Armor Plates/Revelation chains after combining surplus sellback, ore/mineral fallback, owned BP ME/TE, and build/buy user overrides.
- Exact ore/mineral fallback parity inside recursive chains beyond the current gate/classification/fallback fixtures.
- Exact T2/T3 invention parity for datacores, decryptors, copy materials, copy-only source blueprints, and science facility choices.
- Full facility/rig/service-module parity for structures and reactions.
- Live market and contract pricing parity, because these require current external data or saved user samples.

## Manual Or Live Verification Needed
- Interactive WPF workflows listed in `UI_SMOKE_CHECKLIST.md`; the checklist now calls out which fixture workflows are UI smoke checks and which final numeric areas remain provisional.
- EVE SSO/private/corporation contract ingestion.
- Live market/SVR/ranking tuning against current ESI/Fuzzwork data.
- Final numeric parity against a running legacy EVE IPH UI for selected examples, if exact legacy output capture becomes available.
