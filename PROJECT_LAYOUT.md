# PROJECT_LAYOUT.md

## Source Folders
- `OurIPH/OurIPH.sln` is the active C# WPF solution.
- `OurIPH/OurIPH` contains the WPF client source.
- `OurIPH/OurIPH.Tests` contains the no-NuGet console regression harness.
- `EVE-IPH-master` is the legacy VB.NET WinForms reference source. Use it for parity checks, not as the active client.
- `UI_SMOKE_CHECKLIST.md` contains the manual WPF smoke checklist for workflows that cannot be fully verified by `--smoke-test`; it now includes fixture workflows, expected success signs, and provisional parity caveats for the next interactive pass.
- `LOCAL_TECH_DEBT_AUDIT.md` tracks local-only refactor/test/documentation debt discovered from source scans.
- `LEGACY_PARITY_MAP.md` maps legacy VB.NET calculation entry points to current OurIPH services and database-backed fixture coverage.

## Service Map
- Data access and runtime paths: `EveDatabaseService`, `AppPaths`, `AppLogger`.
- Settings and persistence stores: `BuildQueueStore`, `BuildProjectStore`, `CharacterSkillStore`, `ContractPriceStore`, `ExcludedBlueprintStore`, `FacilityPresetStore`, `MarketHistoryCacheStore`, `MarketPriceCacheStore`, `BlueprintFilterRuleStore`.
- Blueprint decision services: `BlueprintFilteringService`, `BlueprintListFilterService`, `BlueprintTypeFilterService`, `BlueprintBuildProfileService`, `BlueprintRankingService`, `BlueprintEstimateSelectionService`, `BlueprintEstimateTraversalService`, `MaterialBuildBuyDecisionService`.
- Blueprint calculation services: `BlueprintEfficiencyService`, `BlueprintEstimateApplicationService`, `BlueprintEstimateCacheService`, `BlueprintEstimateMaterialOrchestrationService`, `BlueprintEstimateMaterialTraversalService`, `BlueprintEstimateResultAssembler`, `BlueprintEstimateStatusService`, `BlueprintManufacturingMathService`, `BlueprintMaterialEstimateLineService`, `BlueprintMaterialMathService`, `BlueprintInventionCostService`, `BlueprintInventionMathService`, `BlueprintInventionTimeService`, `BlueprintProfitabilityService`, `BlueprintProductionTypeService`, `BlueprintSkillRequirementService`, `OrePlanningService`, `ProjectStockService`.
- Project and build queue services: `BuildProjectDecisionService`, `BuildQueueService`, `ProjectItemEstimateApplicationService`, `ProjectItemEstimateDisplayService`, `ProjectMaterialImportService`, `ProjectQueueDisplayService`, `UsedByListService`.
- Market, cache, and contract services: `FuzzworkMarketService`, `EsiMarketHistoryService`, `EsiMarketOrderService`, `EsiPublicContractService`, `ContractPricingService`, `MarketPriceCacheService`, `MarketHistoryCacheService`, `MarketPriceSelectionService`, `SalesFeeService`, `SalesVolumeRatioService`, `TimeoutWebClient`.
- UI-adjacent pure helpers: `BlueprintCopyStatusService`, `FacilityActivityStationService`, `NumericInputValidationService`, `PriceUpdateStatusService`, `UiSettingsStore`.
- `MainWindow.xaml.cs` should increasingly act as WPF glue: event handlers, binding refresh, selection mapping, and calls into the services above.
- Facilities/station preset workflow currently uses `FacilityCatalogService` for local DB-backed region/system/security/cost-index, structure, rig, and service-module data, and `FacilityPresetStore` for UTF-8 XML persistence. `MainWindow.xaml.cs` still owns the WPF selection handlers, but guarded loading prevents preset fields from being overwritten while the form is populated.

## Runtime And Settings Data
- `LatestFiles/EVEIPH DB.sqlite` is the preferred local EVE IPH database source.
- `Root Directory/EVEIPH DB.sqlite` is the fallback legacy/runtime database source.
- `AppPaths.EveIphDatabasePath` copies the database to `%LOCALAPPDATA%\OurIPH\Data\EVEIPH DB.sqlite` when the source path is UNC, because `System.Data.SQLite` can fail against UNC/WSL paths.
- `OurIPH.Tests` now uses the same database path for database-backed golden fixtures covering Rifter, Capital Armor Plates, Revelation, and Zirnitra chain structure, plus numeric legacy parity fragments for material quantity, deterministic direct material cost, bounded recursive component cost, surplus/excess/sellback behavior, ore/mineral fallback gate/classification behavior, simple production time, EIV, and manufacturing usage formulas. It also has characterization coverage for the extracted material orchestration, final estimate assembly, blueprint estimate display/price-source mapping, project item display/price-source mapping, project job material status boundaries, and the current greedy ore-planning helper, including a real local reprocessing-row structural check.
- `OurIPH.Tests` also contains a facility preset persistence regression that round-trips all preset/station XML fields, verifies edited values survive reload/save/reload, and verifies legacy-style mojibake default names/messages are repaired on load.
- `OurIPH/Settings` stores editable XML settings used by the WPF client, including:
  - `UiSettings.xml`
  - `BuildQueue.xml`
  - `BuildProjects.xml`
  - `FacilityPresets.xml`
  - `MarketPriceCache.xml`
  - `BlueprintFilterRules.xml`

## Generated Files
- `OurIPH/OurIPH/bin` and `OurIPH/OurIPH/obj` are build artifacts.
- `OurIPH/OurIPH.Tests/bin` and `OurIPH/OurIPH.Tests/obj` are test build artifacts.
- Runtime logs are written beside the built WPF executable:
  - `OurIPH-runtime.log`
  - `OurIPH-startup.log`
  - `OurIPH-crash.log`

## Verification Commands
Run from the repository root:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "OurIPH\OurIPH.sln" /p:Configuration=Debug /p:Platform="Any CPU" /m /v:minimal
& "OurIPH\OurIPH.Tests\bin\Debug\OurIPH.Tests.exe"
$p = Start-Process -FilePath (Resolve-Path "OurIPH\OurIPH\bin\Debug\OurIPH.exe") -ArgumentList "--smoke-test" -WindowStyle Hidden -Wait -PassThru; $p.ExitCode
```

Directly invoking the WPF `WinExe` from PowerShell can return before the process exits; use `Start-Process -Wait -PassThru` for smoke checks.

For interactive verification, use `UI_SMOKE_CHECKLIST.md` after the build and automated smoke commands pass.
