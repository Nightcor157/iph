# Local Technical Debt Audit

Date: 2026-05-17

Scope: local-only work that does not require EVE SSO, live market data, public ESI/Fuzzwork availability, or manual interactive UI decisions.

## Active Code TODO/FIXME Scan
- `OurIPH/OurIPH`: no `TODO`, `FIXME`, `HACK`, or `XXX` markers found outside `bin`/`obj`.
- `OurIPH/OurIPH.Tests`: no active source TODO/FIXME markers found outside `bin`/`obj`.
- Legacy `EVE-IPH-master` still contains historical TODO comments. Treat those as parity-reference clues only; do not copy them into the new client as unresolved work without validating the surrounding VB.NET behavior.

## MainWindow.xaml.cs Hotspots
- `OurIPH/OurIPH/MainWindow.xaml.cs` is still about 4,635 lines and remains the main integration hotspot.
- It still owns about 60 `_Click` handlers, which is acceptable for WPF event wiring but too much for calculation/business behavior.
- Roughly 30 price/material/calculation helper methods still live in the window. Several are now thinner wrappers over services, and explicit context/result/dependency models plus delegate-backed material traversal exist for the next `CalculateBlueprintEstimate` extraction, but full database-backed recursive estimate ownership remains the largest local calculation hotspot.

## Already Extracted And Tested
- Contract pricing and contract sample review: `ContractPricingService`, `EsiPublicContractService`.
- Blueprint filtering/ranking/list decisions: `BlueprintFilteringService`, `BlueprintListFilterService`, `BlueprintTypeFilterService`, `BlueprintBuildProfileService`, `BlueprintRankingService`, `BlueprintEstimateSelectionService`, `BlueprintEstimateTraversalService`, `MaterialBuildBuyDecisionService`.
- Blueprint math slices: `BlueprintEfficiencyService`, `BlueprintEstimateApplicationService`, `BlueprintEstimateCacheService`, `BlueprintEstimateMaterialOrchestrationService`, `BlueprintEstimateMaterialTraversalService`, `BlueprintEstimateResultAssembler`, `BlueprintEstimateStatusService`, `BlueprintManufacturingMathService`, `BlueprintMaterialEstimateLineService`, `BlueprintMaterialMathService`, `BlueprintInventionCostService`, `BlueprintInventionMathService`, `BlueprintInventionTimeService`, `BlueprintProfitabilityService`, `BlueprintProductionTypeService`, `BlueprintSkillRequirementService`, `OrePlanningService`.
- Database-backed blueprint fixture coverage: `OurIPH.Tests` loads real local DB cases for Rifter, Capital Armor Plates, Revelation, and Zirnitra, and verifies material lines, child blueprint resolution, copy-only state, traversal branch behavior, path guard behavior, and controlled missing-price behavior.
- Numeric legacy parity coverage: `OurIPH.Tests` now confirms direct material quantity math for Rifter and Capital Armor Plates, deterministic direct material total cost for Rifter and Capital Armor Plates, a bounded recursive component-cost harness for Capital Armor Plates -> Reinforced Carbon Fiber, a small Revelation -> Capital Armor Plates recursive fragment, surplus/excess/sellback behavior for small confirmed cases, ore/mineral fallback gate/classification behavior for safe local cases, a simple Rifter EIV/manufacturing usage fee fragment, a simple Rifter production-time fragment, and a small Revelation child-run fragment against formulas read from legacy `Blueprint.vb`, `ConvertToOre.vb`, `Material.vb`, `Materials.vb`, and `Globals.vb`. The extracted `OrePlanningService` has deterministic unit coverage plus a real local reprocessing-row structural fixture, but not full legacy LP numeric parity.
- Project/queue stock, import, estimate application/display, and decision logic: `ProjectStockService`, `ProjectItemEstimateApplicationService`, `ProjectItemEstimateDisplayService`, `ProjectMaterialImportService`, `ProjectQueueDisplayService`, `BuildProjectDecisionService`, `BuildQueueService`, `UsedByListService`, `BuildProjectStore`.
- Market/cache/fee/liquidity helpers: `MarketPriceCacheService`, `MarketHistoryCacheService`, `MarketPriceSelectionService`, `SalesFeeService`, `SalesVolumeRatioService`.
- UI-adjacent pure helpers: `BlueprintCopyStatusService`, `FacilityActivityStationService`, `NumericInputValidationService`, `PriceUpdateStatusService`, `UiSettingsStore`.
- Facilities/station preset persistence is now covered by `FacilityPresetStore_RoundTripsAllFieldsAndRepairsDefaultNameMojibake`, including all preset/station XML fields, repeated save/load edits, and UTF-8 mojibake repair for legacy-corrupted default names/messages.

## Next Local Extraction Candidates
- A small `CalculateBlueprintEstimate` glue extraction is now complete: material traversal orchestration and final estimate object assembly are service-owned. Blueprint estimate display-ready product fields are service-owned through `BlueprintEstimateApplicationService`, and project item display-ready revenue/status/price-source mapping is service-owned through `ProjectItemEstimateDisplayService`. Do not continue into full recursive estimate ownership until database-backed numeric parity coverage is deeper around the remaining risky branches. Ore/mineral fallback is mapped for safe gate/classification/fallback cases, and the current greedy ore-planning helper is now extracted into `OrePlanningService`; the next safest parity work is mapping the full `ConvertToOre.vb GetOresfromMinerals` LP ore plan with deterministic local `REPROCESSING` fixtures, or combining surplus plus ore fallback into a bounded recursive end-to-end fixture.
- Full recursive `CalculateBlueprintEstimate` transfer remains higher risk until database-backed golden tests cover child blueprint lookup, mineral/ore fallback, surplus offsets, invention/decryptor integration, capital/component chains, and final material/profit numbers.

## CalculateBlueprintEstimate Decomposition
- Cache/setup: estimate cache key/clone lookup, facility math, material/time multipliers, facility bonus, industry fees.
- Material line pricing: adjusted quantity, price delegates, buy cost, market-volume-short detection.
- Recursion guard: path lookup suppression, parent enter/exit around recursive child estimate.
- Delegate-backed material traversal: adjusted material iteration, child blueprint lookup delegate calls, child estimate delegate calls, surplus offset application, mineral quantity aggregation, adjusted-price EIV contribution, market-volume-short propagation, and build/buy decision branching are now in `BlueprintEstimateMaterialTraversalService`.
- Material orchestration boundary: `BlueprintEstimateMaterialOrchestrationService` owns context/dependency object creation, calls the traversal service, and aggregates mineral purchase cost through a delegate.
- Child blueprint lookup: database product-to-blueprint lookup, always-buy checks, and reaction drilldown stops are still supplied by `MainWindow.xaml.cs` as delegates.
- Child request: child ME/TE defaults, child run count from adjusted quantity and portion size, and cheapest station lookup are now routed through service/delegate boundaries.
- Child result: recursive estimate call, child build cost, component production time, and surplus output sale offset are handled by the traversal service with the actual recursive call delegated back to the window.
- Mineral/ore fallback: mineral quantity aggregation and adjusted-price EIV contribution are extracted; legacy `ConvertToOre` gating/classification is documented and tested; current greedy ore planning now lives in `OrePlanningService`; full legacy LP-equivalent planning remains the next risky parity target.
- Build/buy decision: choose recursive build cost vs market buy cost is extracted.
- Invention/decryptor/station: invention material/time/usage costs, science activity stations, decryptor effects.
- Final assembly: `BlueprintEstimateResultAssembler` owns the final `BlueprintEstimate` object assembly for production time, invention breakdown, and build/buy line counts. `BlueprintEstimateApplicationService` owns Blueprint row summary/profit/stale reset and display-ready product price/source mapping. Installation cost calculation and cache store remain in `MainWindow.xaml.cs`.
- Price source orchestration around `LoadPricesWithCache`, `GetRelevantPriceTypeIds`, and effective product/material price helpers.
- Project copy/export text generation for EVE buy lists, hauling plans, wave jobs, and project plans.
- Facility/station UI selection mapping for rigs, service modules, security, and station keys. The current WPF selection-loading guard fixes one mutation class, but the remaining UI-owned mapping would still benefit from a small view-model/application boundary after manual Facilities testing confirms the workflow shape.
- UI settings persistence currently lives in the window; it can become a small `UiSettingsStore` once the setting surface stabilizes.

## Verification Policy
- Before every build, close workspace `OurIPH.exe` instances.
- For local-only passes, run normal Debug MSBuild, `OurIPH.Tests.exe`, and `OurIPH.exe --smoke-test`.
- Use `UI_SMOKE_CHECKLIST.md` for interactive WPF checks that automated smoke cannot prove. The checklist now separates UI success criteria from known provisional areas such as full capital-chain final numeric parity and full `ConvertToOre` LP ore-plan parity.
