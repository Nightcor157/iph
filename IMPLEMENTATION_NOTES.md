# IMPLEMENTATION_NOTES.md

## Current State
- Project root is `/home/ilya/projects/is-per-our-https-github-com`.
- Legacy source is `EVE-IPH-master`: VB.NET WinForms, .NET Framework 4.6.1, x86.
- New client is `OurIPH`: C# WPF, .NET Framework 4.8, solution `OurIPH/OurIPH.sln`.
- Runtime data is read from `LatestFiles/EVEIPH DB.sqlite` first, then `Root Directory/EVEIPH DB.sqlite`.
- When the app is launched from a WSL UNC path, `AppPaths` now copies the SQLite DB into `%LOCALAPPDATA%\OurIPH\Data\EVEIPH DB.sqlite` and uses that local copy because `System.Data.SQLite` cannot reliably open the DB through `\\wsl.localhost\...`.
- `OurIPH` builds successfully with MSBuild 17 / VS 2022.
- `OurIPH.exe --smoke-test` now constructs `MainWindow`, logs startup/shutdown, exits with code 0, and does not leave a crash log.
- Windows verification now runs from this session: MSBuild builds `OurIPH.sln`, `OurIPH.Tests.exe` passes, and WPF smoke is checked with `Start-Process -Wait -PassThru` so the real process exit code is observed.

## Implemented In OurIPH
- Main WPF shell with pages for Blueprints, Prices, Facilities, Build Queue, and Projects.
- SQLite access through `EveDatabaseService` for blueprint search, materials, skills, invention data, decryptors, adjusted prices, and market history.
- Facility setup through `FacilityCatalogService` and `FacilityPresetStore`.
- Market price loading through `FuzzworkMarketService` and ESI history/order services.
- XML stores for build queue, build projects, market price cache, market history cache, facility presets, excluded blueprints, character skills, and manual contract price samples.
- Blueprint estimation includes material cost, installation cost, invention cost, station bonuses, taxes/fees, production time, ROI, ISK/hour, SVR/liquidity, and build tree expansion.
- Project tracking includes build/buy decisions, project materials, stages/waves, owned/bought quantities, clipboard import/export, and project persistence.

## Implemented In 2026-05-10 Pass 1
- Rewrote `AGENTS.md` as UTF-8 persistent agent instructions.
- Created `IMPLEMENTATION_NOTES.md` and `TODO_NEXT.md` for durable project memory.
- Added `ContractPricingService`.
  - Uses manual contract samples already stored by `ContractPriceStore`.
  - Filters non-target samples by product type/name.
  - Rejects anomalous/scam-like sample prices outside a median-based band.
  - Chooses median accepted contract price instead of a simple average.
  - Prefers contract prices for capital-like products and for no-volume markets when contract confidence is high.
- Added `BlueprintFilteringService`.
  - Centralizes capital/reaction/limited-source detection.
  - Adds rare/officer/noise filtering for top recommendations.
  - Top ranking skips rare/officer/noisy items when the existing limited-source filter is active.
- Wired contract pricing into blueprint estimates and project item estimates.
  - Blueprint grid has contract price details and top-reason text.
  - Project grid shows product price source and contract price.
  - `BuildProjectItem` and `BuildProjectStore` persist contract price/source/details.
- Hardened `FuzzworkMarketService`, `EsiMarketHistoryService`, and `EsiMarketOrderService` with timeouts and limited retries.

## Implemented In 2026-05-10 Pass 2
- Added `AppLogger` and expanded lifecycle logging.
  - Runtime log: `OurIPH-runtime.log`.
  - Startup log: `OurIPH-startup.log`.
  - Crash log: `OurIPH-crash.log`.
  - Logs startup args, MainWindow construction, shutdown, dispatcher exceptions, AppDomain exceptions, and unobserved task exceptions.
- Added `--smoke-test` startup mode.
  - Constructs `MainWindow` without showing the GUI.
  - Runs startup DB/settings initialization.
  - Closes and exits explicitly with code 0 on success.
- Found and fixed the GUI/harness startup failure.
  - Cause: SQLite could not open the DB over the WSL UNC path.
  - Fix: `AppPaths.EveIphDatabasePath` now returns a local runtime DB copy for UNC sources.
- Improved project material stock UI.
  - Selecting a material fills the owned quantity box.
  - Shows selected material status: needed, owned, remaining, and required-through-wave.
  - Added quick actions for marking stock up to prior wave or current wave.
- Added `BlueprintRankingService`.
  - Moves top ranking logic out of `MainWindow.xaml.cs`.
  - Computes `TopScore` using ISK/hour, liquidity/SVR, profit, ROI, price source confidence, trend, and low-market-volume penalty.
  - `BlueprintSearchResult` now exposes `TopScoreText` and a richer `TopReasonText`.

## Implemented In 2026-05-10 Pass 3
- Added `OurIPH.Tests`, a small no-NuGet console test harness included in `OurIPH.sln`.
  - Covers `ContractPricingService` median/anomaly behavior and capital contract preference.
  - Covers `BlueprintFilteringService` limited-source, rare/officer/noise, capital, and reaction classification.
  - Covers `BlueprintRankingService` rank filtering plus liquidity, volume, and contract-confidence penalties.
- Added `BlueprintProfitabilityService`.
  - Centralizes basic profitability math: total cost, profit, ISK/hour, margin, and ROI.
  - `MainWindow` now uses it for blueprint and project item profitability fields.
  - `OurIPH.Tests` includes a focused profitability arithmetic test.
- Improved Prices page progress/status.
  - `FuzzworkMarketService.LoadPrices` now has an optional progress callback while preserving the old call signature.
  - Price update status shows total, processed, fresh downloaded, cache hits for missing fresh prices, missing count, and last error when fallback is used.
  - The price status text wraps so long error/progress messages remain readable.

## Implemented In 2026-05-10 Pass 4
- Added public ESI contract ingestion for selected blueprint products.
  - New `EsiPublicContractService` reads public contract summaries from `/contracts/public/{region_id}/` and item lists from `/contracts/public/items/{contract_id}/`.
  - The service requires no auth and only imports `item_exchange` contracts with exact included item lists containing one distinct typeID.
  - This gives stronger bait/package filtering for public contracts before they become `ContractPriceSample` rows.
  - `ContractPriceSample` and `ContractPriceStore` now persist richer metadata: contract ID, source, location ID, quantity, item count, and title.
  - The Blueprints page has an `ESI контракты` button for importing exact public samples for the selected product in the selected region.

## Implemented In 2026-05-10 Pass 5
- Rare/officer filtering is no longer controlled indirectly by the old limited-source checkbox.
  - `UiSettings.xml` persists a dedicated `HideRareNoise` flag.
  - The Blueprints page exposes separate controls for `Скрыть limited-source` and `Скрыть rare/officer`.
  - `BlueprintRankingService.AssignRanks` now accepts `hideRareOrNoise` so top ranking follows the UI setting.
- Project material stock editing is easier.
  - The Projects material grid `Куплено` column is editable inline.
  - Editing a wave row persists stock as prior-wave owned quantity plus the value entered for that row.
  - Clipboard import and the side stock editor now share the same stock update helper.

## Implemented In 2026-05-10 Pass 6
- Added `ProjectStockService`.
  - Moves project material stock distribution by wave out of `MainWindow.xaml.cs`.
  - Provides the inline-edit conversion from row-owned quantity to persisted total owned-through-wave quantity.
  - `OurIPH.Tests` covers wave-first stock distribution, remaining cost recalculation, and inline-edit total conversion.
- Added shared `TimeoutWebClient`.
  - Removed duplicate nested timeout web-client classes from Fuzzwork and ESI market/contract services.
- The blueprint grid now includes `Score детали`.
  - Shows score inputs from existing fields: SVR, profit, ROI, trend, and price source.
- Added contract sample review for the selected blueprint.
  - New `ContractPriceSampleReview` model backs a `Contract samples` grid under the blueprint details area.
- `ContractPricingService.ReviewSamples` classifies selected-product samples as accepted, rejected, stale, or non-target using the same median-band logic as pricing.
- The review status line shows median, accepted count, and rejected count.
- `OurIPH.Tests` covers review status classification.

## Implemented In 2026-05-10 Pass 7
- Fixed `OurIPH.Tests.csproj` platform coverage so the solution builds under the documented `/p:Platform="Any CPU"` command when MSBuild normalizes the project platform to `AnyCPU`.
- Verified the full solution in Windows MSBuild and ran the console test harness successfully.
- Added a stricter WPF smoke-check procedure using `Start-Process -Wait -PassThru`; direct PowerShell invocation of a WPF `WinExe` can return before the process exits.
- Added testable path injection to `BuildProjectStore`.
- Added a project persistence regression test that saves a project, reloads it, changes material stock, saves again, and verifies the stock edit survives another reload.
- Added editable blueprint filtering rule settings.
  - New `BlueprintFilterRules` model stores limited-source meta groups, limited/rare keyword rules, capital keywords, and reaction keywords.
  - New `BlueprintFilterRuleStore` persists those rules to `OurIPH/Settings/BlueprintFilterRules.xml` and creates default rules on first startup.
  - `BlueprintFilteringService` now consumes the loaded rules instead of hard-coded keyword arrays.
  - `OurIPH.Tests` verifies custom persisted filtering rules affect limited-source, rare/noise, capital, and reaction classification.
- Tuned `BlueprintRankingService` to reduce low-volume paper-profit traps.
  - Replaced linear SVR weighting with a capped liquidity multiplier.
  - Capped profit contribution relative to ISK/hour so huge one-off profits do not dominate the top list.
  - Added explicit market coverage penalties for no-volume, low-coverage, and thin-market products, while keeping contract-priced products viable.
  - `OurIPH.Tests` now covers a high-volume module, low-volume trap, and contract-priced capital example.
- Expanded public ESI contract ingestion from selected-product import to visible top/capital/reaction scans.
  - Added an `ESI топ` button on the Blueprints page.
  - It scans the selected region for visible top-ranked, capital, and reaction candidates, using the same exact included-item filtering and contract-id dedupe as selected-product import.
  - Imported samples are applied back to matching blueprints and persisted through `ContractPriceStore`.
  - `OurIPH.Tests` now covers excluded return items and invalid zero-quantity public contract samples.
- Extracted material adjustment arithmetic.
  - Added `BlueprintMaterialMathService.CalculateAdjustedQuantity`.
  - `MainWindow.xaml.cs` delegates the existing calculation wrapper to the service.
  - `OurIPH.Tests` covers rounding up, minimum one unit per run, zero runs/base quantity, and negative multipliers.
- Extracted deterministic invention planning arithmetic.
  - Added public `InventionPlan` model.
  - Added `BlueprintInventionMathService` for invention chance, invented runs per success, successful jobs needed, and expected invention job count.
  - `MainWindow.xaml.cs` now delegates its existing invention plan/chance wrappers to that service.
  - `OurIPH.Tests` covers skill/decryptor chance math, T2 max production limit behavior, decryptor run modifiers, probability-inflated job count, and T3 runs-per-success behavior.
- Made low-volume hiding the conservative recommendation default.
  - `hideLowVolumeBox` defaults to checked in XAML.
  - `LoadUiSettings` now defaults missing `HideLowVolume` to true.
  - Reset filter now turns low-volume hiding back on.
  - Checked-in `UiSettings.xml` has `HideLowVolume="true"`, so top recommendations suppress thin/no-volume items unless explicitly requested.
- Added `PROJECT_LAYOUT.md` documenting active source folders, upstream legacy snapshots, settings/runtime data, generated files, logs, and the verified build/test/smoke commands.

## Implemented In 2026-05-10 Pass 8
- Improved Blueprint UI usability after the "load all blueprints" workflow.
  - Added an explicit `Очистить список` button next to `Все чертежи`.
  - Clearing the list also clears selected blueprint materials, skills, and contract sample review rows.
  - Added a small blueprint list status text that reports loaded/cleared state.
- Made dense WPF tables easier to read.
  - Enabled explicit `AutoGenerateColumns="False"` on the main grids.
  - Enabled user column resizing and horizontal/vertical scrollbars on blueprint, price, queue, and project grids.
  - Added minimum column widths so dense result tables scroll instead of crushing columns into each other.
- Started moving the Blueprint page back toward the legacy EVE IPH workflow.
  - Added a separate `Анализ` navigation page.
  - Moved SVR filters, contract controls, contract sample review, score/ranking columns, market depth, trend, and order-count columns out of the main Blueprint table and into `Анализ`.
  - The main Blueprint table now focuses on the legacy-style work surface: product, blueprint, group, tech/meta/skills, portion, material/invention/job costs, total, revenue, profit, ROI, ISK/hour, facility, and status.
  - `Анализ` has its own table bound to the same blueprint collection for score, contract, SVR/liquidity, trend, market risk, and order details.
- Added a compact-density pass to make the WPF shell feel closer to legacy WinForms.
  - Reduced global button, textbox, and combobox padding.
  - Reduced base font size and page header size.
  - Reduced default DataGrid row height.
  - Tightened the Blueprint top panel spacing and simplified some checkbox labels.
  - Reduced selected-material and skill grid row/icon sizes.
  - Widened the `Анализ` contracts group and switched its action buttons to a wrapping layout so `ESI топ` is no longer clipped.

## Implemented In 2026-05-10 Pass 9
- Continued moving the Blueprint page toward legacy EVE IPH clarity without copying the old UI exactly.
  - Added a real `Blueprint Type / Build profile` filter block with working type toggles for ships, ammo/charges, modules, rigs, drones, components, structures, and misc.
  - Type toggles are wired into `BlueprintPassesFilter`, persist in `UiSettings.xml`, and are restored by reset-filter.
  - The filter uses existing blueprint category/group/market-group data rather than cosmetic placeholder controls.
  - Added a right-side selected-blueprint totals panel for production time, material/invention/job costs, total cost, revenue, profit, ROI, ISK/hour, status, and best facility.
  - Wrapped the selected blueprint material and skill tables in labeled group boxes so the lower detail area reads more like the old component/skill work surface.

## Implemented In 2026-05-10 Pass 10
- Tightened the Blueprint screen after live screenshot review.
  - Reduced the left navigation width and stopped the database status from wrapping vertically inside the nav area.
  - Fixed the sidebar docking so navigation stays at the top and database status stays at the bottom.
  - Buttons no longer stretch vertically inside grid cells, reducing the oversized search/action controls.
  - Widened the right filter/action column and reduced the main content margin to give dense grids and filters more usable horizontal room.
  - Top-row Blueprint groups now align to their content height instead of visually filling the tallest filter row.

## Implemented In 2026-05-10 Pass 11
- Moved the WPF shell closer to the legacy EVE IPH window structure.
  - Replaced the left navigation sidebar with a top menu row and legacy-style tab strip.
  - Kept the existing navigation controls and handlers, so pages still switch through the established `Navigation_Checked` flow.
  - Moved database/list status visibility into a bottom status bar, matching the old status-line layout.
  - Hid the modern page title/subtitle header so the first visible work area starts immediately under the legacy tabs.
- Moved selected-blueprint totals from a tall right-side column into the upper-right calculation block.
  - The main blueprint result table now uses the full center width again.
  - The totals block shows time, material/invention/job cost, total, revenue, profit, ROI, ISK/hour, and status near the top controls.
- Made the main blueprint result grid denser.
  - Default DataGrid row height is now 24.
  - Product and blueprint icons in the main grid were reduced from 40px to compact 22px images.
- Split the lower Blueprint detail area closer to the old EVE IPH material layout.
  - Added a separate `Raw Material List` grid beside the direct component material list.
  - Raw material rows are computed recursively from available child blueprints instead of duplicating the component list.
  - The raw expansion respects selected runs/ME/facility material multiplier, default child blueprint efficiency, `ShouldAlwaysBuy`, and reaction drilldown depth.
  - Skills remain visible as a third compact lower panel.
- Tightened the 1280px Blueprint layout after screenshot review.
  - Rebalanced top panel columns so blueprint search keeps usable width and facility/station fields no longer collapse to a single character.
  - Reduced the search action button columns.
  - Changed the main result `Продукт` column from star sizing to a fixed width so product names do not wrap into letter fragments when many result columns are present.
- Fixed follow-up usability issues from live Blueprint screenshot review.
  - Increased the lower Blueprint detail panel from 160px to 230px.
  - Restored the main result grid row height to 34px and product/blueprint icons to 28px so icons are not clipped.
  - Widened the main `Продукт` column and added product text trimming.
  - The upper-right selected totals block now shows selected product, blueprint, and group even before an estimate has populated cost/profit fields.
  - Added `Очистить очередь` to the Blueprint page Actions block; it reuses `ClearQueue_Click`, persists the empty queue, and reports status in the status bar.
- Continued the adaptive Blueprint layout pass.
  - Replaced the fixed lower detail height with a resizable row and `GridSplitter`; default lower height is now 280px with a 220px minimum.
  - Added a `Очистить очередь` action directly to the Blueprint Actions block so users can clear build queue without switching tabs.
  - Build queue status is now tracked in `queueStatusText`, shown on the queue page and echoed in the bottom status bar.
  - Queue status updates after startup load, add/merge, and clear operations.

## Legacy Logic Already Represented
- Blueprint/material lookup from IPH SQLite facts.
- Manufacturing/reaction material expansion.
- Portion-size-aware build quantities and excess output concepts.
- Fuzzwork/ESI market price sources and cache ideas.
- Facility bonuses, rigs, service modules, system cost index, taxes, and fees are partially represented in C#.
- Current-project style workflow from legacy Shopping List has been reimagined as `BuildProject` and project pages.

## Known Problems And Blockers
- `MainWindow.xaml.cs` is still very large and mixes UI event handlers, business calculations, persistence orchestration, network calls, and clipboard logic, though ranking, stock math, profitability, contract pricing, and several business rules are now in services.
- Automated tests now exist for selected pure services and pass in the Windows-capable session. They do not yet cover all UI workflows or full legacy parity.
- Contract pricing now supports manual samples and selected-product public ESI ingestion; broad regional auto-ingestion and authenticated private/corporation contracts are not implemented.
- Contract pricing now supports manual samples, selected-product public ESI ingestion, and visible top/capital/reaction public ESI scans. Authenticated private/corporation contracts still require EVE SSO/API credentials.
- Rare/officer filtering has a dedicated persisted UI setting and the keyword/meta rules are now editable through `OurIPH/Settings/BlueprintFilterRules.xml`; there is not yet an in-app rules editor.
- Build artifacts and runtime data are stored beside source; repository boundaries are unclear because the root directory is not a single git repository.

## Architecture Decisions
- Continue improving the current WPF/C# client instead of rewriting from scratch.
- Keep EVE names and market terms in English/database form; localize only application UI labels/status text as needed.
- Add focused services for missing business rules instead of making `MainWindow.xaml.cs` larger when practical.
- Keep XML stores for current local settings until the data model stabilizes.
- Prefer deterministic filtering/ranking rules that can later be unit-tested.
- Treat manual contract samples as the first contract pricing source; add ESI contract ingestion later behind the same `ContractPricingService` API.
- Use a local runtime copy for SQLite DBs when the source path is a WSL/UNC path.

## Screens And Services Requiring Work
- Prices page: progress now shows total/fresh/cache/missing/error; cancellation and richer per-batch UI are still pending.
- Facilities page: verify selected structures/rigs/modules/systems affect all estimates and project recalculation with targeted examples.
- Projects page: inline material stock editing exists; persistence still needs a real restart verification.
- Blueprints page: tune top score weights against real manufacturing examples.
- `ContractPricingService`: selected-product public ESI samples and accepted/rejected review UI exist; broad scans and auth sources are still pending.
- `BlueprintFilteringService`: limited-source and rare/officer toggles are persisted; keyword/meta rules are persisted in `BlueprintFilterRules.xml`, but an in-app editor is still pending.

## Verification
- MSBuild `OurIPH/OurIPH.sln` passed after AppLogger/smoke/AppPaths changes.
- `OurIPH.exe --smoke-test` initially reproduced startup failure: SQLite could not open DB via WSL UNC path.
- MSBuild passed after fixing `AppPaths` local DB copy.
- `OurIPH.exe --smoke-test` passed with exit code 0 and no crash log after `AppPaths` fix.
- MSBuild passed after project material UI changes.
- MSBuild passed after `BlueprintRankingService` extraction and top score changes.
- Final `OurIPH.exe --smoke-test` passed with exit code 0 and no crash log.
- 2026-05-10 Pass 3 verification blocker: current shell cannot execute Windows MSBuild (`powershell.exe` fails with `Exec format error`) and has no local `dotnet`, `msbuild`, `xbuild`, `mono`, `csc`, or `mcs`. Static checks confirmed new project/file references are present, but full build, `OurIPH.Tests.exe`, and `OurIPH.exe --smoke-test` must be run from a Windows/Visual Studio-capable session.
- Public ESI endpoint smoke from this shell passed with `curl` for The Forge (`10000002`): contract summaries and contract item lists responded without auth.
- MSBuild was retried after Pass 5 and is still blocked by the same Windows interop `Exec format error`.
- MSBuild was retried after Pass 6 and remains blocked by the same Windows interop `Exec format error`.
- MSBuild was retried after contract review UI changes and remains blocked by the same Windows interop `Exec format error`.
- 2026-05-10 Pass 7: MSBuild `OurIPH/OurIPH.sln` passed with `/p:Configuration=Debug /p:Platform="Any CPU"`.
- 2026-05-10 Pass 7: `OurIPH.Tests.exe` passed with 66 assertions.
- 2026-05-10 Pass 7: `OurIPH.exe --smoke-test` passed with real process `ExitCode=0` when launched via `Start-Process -Wait -PassThru`; runtime log shows `MainWindow constructed successfully` and application exit code 0.
- 2026-05-10 Pass 8: MSBuild `OurIPH/OurIPH.sln` passed after Blueprint UI clear/scroll changes.
- 2026-05-10 Pass 8: `OurIPH.Tests.exe` passed with 66 assertions.
- 2026-05-10 Pass 8: `OurIPH.exe --smoke-test` passed with real process `ExitCode=0`.
- 2026-05-10 Pass 8 legacy UI shift: normal Debug MSBuild was blocked because a live `OurIPH.exe` process was holding `OurIPH/OurIPH/bin/Debug/OurIPH.exe`; verification succeeded by building to `bin/CodexVerify`, running `OurIPH.Tests.exe` with 66 assertions, and running WPF smoke with real process `ExitCode=0`.
- 2026-05-10 Pass 8 compact-density UI pass: verified by building to `bin/CodexVerify`, running `OurIPH.Tests.exe` with 66 assertions, and running WPF smoke with real process `ExitCode=0`.
- 2026-05-10 Pass 9 Blueprint type/UI clarity pass: verified by building to `bin/CodexVerify`, running `OurIPH.Tests.exe` with 66 assertions, and running WPF smoke with real process `ExitCode=0`.
- 2026-05-10 Pass 10 layout-density pass: first rebuild was blocked by a leftover hidden smoke process locking `bin/CodexVerify/OurIPH.exe`; the Codex-owned process was stopped, then verification passed by building to `bin/CodexVerify2`, running `OurIPH.Tests.exe` with 66 assertions, and running WPF smoke with real process `ExitCode=0`.
- User cleanup note: extra `bin/CodexVerify*` folders were removed by the user; future builds should close workspace `OurIPH.exe` processes first and prefer the normal Debug output path.
- 2026-05-10 Pass 11 legacy shell/table pass: verified with normal Debug MSBuild after closing workspace `OurIPH.exe`; `OurIPH.Tests.exe` passed with 66 assertions, WPF smoke passed with real process `ExitCode=0`, and no hidden `OurIPH.exe` remained afterward.
- 2026-05-10 Pass 11 raw-material split: verified again with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 11 1280px layout fix: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, and WPF smoke `ExitCode=0`.
- 2026-05-10 Pass 11 follow-up UI fixes: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 11 adaptive lower panel/queue status pass: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, and WPF smoke `ExitCode=0`.
- 2026-05-10 Pass 11 queue/table polish pass: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 11 lower-panel polish pass: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 11 functional menu pass: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 11 frozen identity columns pass: verified with normal Debug MSBuild, `OurIPH.Tests.exe` 66 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.
- 2026-05-10 Pass 12 numeric input and copy-only BPC pass: added shared numeric TextBox input/paste validation, loaded blueprint market group and max production limit from SQLite, marks non-market-copy-only blueprints such as Zirnitra as BPC-only, forces non-invention copy-only ME/TE to 0/0, shows BPC copy/run text in Blueprint and Build Queue grids, and updates project recalculation to keep copy-only project items at 0/0. Verified with normal Debug MSBuild, `OurIPH.Tests.exe` 68 assertions, WPF smoke `ExitCode=0`, no hidden `OurIPH.exe` afterward, and a direct Zirnitra database check reporting `CopyOnly=True`, `MaxRuns=1`.
- 2026-05-10 Pass 12 DataGrid edit crash fix: made readonly result/review/cache/queue/project summary grids explicitly `IsReadOnly=True` so WPF no longer attempts TwoWay edits against computed model properties such as `MetaGroupText`; project material rows now cancel editing for every column except the intended `Куплено` stock column. Rechecked legacy `Blueprint.GetMETEforBP`: old logic looks up owned ME/TE first, otherwise uses T2/T3 base invention efficiency, default BPO ME/TE for normal non-reaction blueprints, and 0/0 for reactions. Verified with normal Debug MSBuild, `OurIPH.Tests.exe` 68 assertions, WPF smoke `ExitCode=0`, and no hidden `OurIPH.exe` afterward.

## Implemented In 2026-05-11 Pass 16
- Closed the focused Blueprint Type filter technical-debt checklist.
- Added `BlueprintTypeFilterOptions` and `BlueprintTypeFilterService`.
- Moved Ships/Ammo/Modules/Rigs/Drones/Components/Structures/Misc classification out of `MainWindow.xaml.cs` while preserving checkbox behavior.
- Added regression coverage for representative category classification, misc fallback, all-enabled behavior, and selective toggle filtering.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 93 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 17
- Closed the focused Blueprint Build Profile filter technical-debt checklist.
- Added `BlueprintBuildProfileOptions` and `BlueprintBuildProfileService`.
- Moved T2/T3/capital/reaction/limited-source/rare-or-noise checks out of `MainWindow.xaml.cs`; the window now only maps checkbox state into options and the service performs the decision.
- Added regression coverage for disabling T2, T3, capitals, reactions, limited-source rows, rare/officer rows, and re-enabling rare/officer visibility.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 103 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 18
- Added `UI_SMOKE_CHECKLIST.md` as the manual interactive WPF verification checklist for workflows that cannot be proven by `--smoke-test`.
- The checklist covers local-only scenarios: Blueprint search/typeID lookup, list clearing, filters/top behavior, estimate details, copy-only Zirnitra behavior, build-chain skills, queue clearing, project stock editing, table layout, and DataGrid read-only stability.
- Updated `PROJECT_LAYOUT.md` to document the checklist and when to use it.
- Added a TODO pointer so future manual passes use the checklist instead of scattered notes.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 103 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 19
- Closed the focused Numeric input validation extraction checklist.
- Added `NumericInputRule` and `NumericInputValidationService`, moving the pure number-format validation out of `MainWindow.xaml.cs`.
- WPF-specific textbox event handling remains in `MainWindow`, but validation is now testable and reusable.
- Added regression coverage for integer fields, decimal fields, signed/unsigned rules, partial edit input, final focus-loss validation, and letter rejection.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 114 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 20
- Closed the focused Price update status extraction checklist.
- Added `PriceUpdateStatusService` and moved price progress/completion/error text formatting out of `MainWindow.xaml.cs`.
- Kept the WPF refresh workflow in the window, but status formatting is now deterministic and covered by tests, including timestamp injection for completed updates.
- Added regression coverage for in-progress status, completed status with cached/missing counts, missing-count clamping, and last-error display.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 117 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 21
- Added `LOCAL_TECH_DEBT_AUDIT.md` for local-only debt that can be worked without SSO/API/live market data/manual UI access.
- Re-scanned active `OurIPH` code for `TODO`, `FIXME`, `HACK`, and `XXX`; no active source markers were found outside build artifacts.
- Documented current `MainWindow.xaml.cs` hotspots: about 6,055 lines, about 60 click handlers, and remaining price/material/calculation helpers that are good extraction targets.
- Expanded `PROJECT_LAYOUT.md` with a service map and an explicit goal for `MainWindow.xaml.cs` to keep shrinking toward WPF glue.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 117 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 22
- Closed the focused UI settings store extraction checklist.
- Added `UiSettings` and `UiSettingsStore` so XML loading/saving for `OurIPH/Settings/UiSettings.xml` no longer lives directly in `MainWindow.xaml.cs`.
- Added `AppPaths.GetSettingsPath(...)` as the shared settings-file path helper.
- `MainWindow.xaml.cs` now maps controls to/from `UiSettings` and delegates the file persistence to the store.
- Added regression coverage for missing-file defaults and XML round-trip persistence across filter, price, contract, and lower-detail-height settings.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 128 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 23
- Closed the focused settings-path consolidation checklist.
- Added `AppPaths.SettingsDirectory` and updated `AppPaths.GetSettingsPath(...)` to create and reuse the shared `OurIPH/Settings` folder.
- Updated the default constructors for build queue, projects, blueprint filter rules, character skills, contract prices, excluded blueprints, facilities, market history cache, and market price cache stores to use the shared settings path helper.
- Verified no remaining direct `Path.Combine(AppPaths.WorkspaceRoot, "OurIPH", "Settings", ...)` duplication remains in active services.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 128 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 24
- Closed the focused market price cache service extraction checklist.
- Added `MarketPriceCacheService` for latest-cache lookup and price upsert behavior.
- `MainWindow.xaml.cs` now delegates cached-price lookup and cache-entry mutation to the service, while still handling database names, persistence, and grid refresh.
- Added regression coverage for selecting the newest cached price by type/location, ignoring other locations, updating existing rows, adding new rows, preserving missing names safely, and applying known type names.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 135 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 25
- Closed the focused market history cache service extraction checklist.
- Added `MarketHistoryCacheService` for merging database history stats with cached fallback stats and for upserting history cache rows.
- `MainWindow.xaml.cs` now delegates market-history cache merge/mutation to the service, while still handling the database query and store persistence.
- Added regression coverage for newest cached fallback selection, region/day filtering, preserving valid fresh stats, updating existing rows, adding new rows, and timestamp fallback behavior.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 143 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 26
- Closed the focused market price selection extraction checklist.
- Added `MarketPriceSelectionService` for price modifier application, Min Sell/Max Buy unit-price choice, strict material volume, and product-volume fallback behavior.
- `MainWindow.xaml.cs` now delegates product unit price, product market volume, material market volume, and material price modifier math to the service.
- Added regression coverage for positive/negative modifiers, zero prices, Min Sell vs Max Buy price choice, strict sell-volume behavior, and product sell-volume fallback to buy volume.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 151 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 27
- Closed the focused sales fee service extraction checklist.
- Added `SalesFeeService` for sales tax, broker fee, station sales fee, and net revenue calculations.
- `MainWindow.xaml.cs` now delegates sales-tax/broker-fee math to the service for project estimates, blueprint estimates, decryptor selection, material surplus offsets, and buy-order broker fee handling.
- Added regression coverage for accounting skill tax reduction, broker minimum fee, explicit special broker fee mode, station fee behavior when tax is excluded, and non-positive gross-value clamping.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 157 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 28
- Refreshed `LOCAL_TECH_DEBT_AUDIT.md` after the latest extraction passes.
- Updated the current `MainWindow.xaml.cs` audit count to about 5,898 lines, about 60 click handlers, and about 31 remaining price/material/calculation helper methods.
- Updated `PROJECT_LAYOUT.md` service map with the new market cache, market price selection, sales fee, and UI settings services.
- Re-scanned active `OurIPH` source for `TODO`, `FIXME`, `HACK`, and `XXX`; no active source markers were found outside build artifacts.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 157 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 29
- Closed the focused build queue merge service extraction checklist.
- Added `BuildQueueService` for queue item merge-key behavior and queue status text formatting.
- `MainWindow.xaml.cs` now delegates add-or-merge queue rules and status text formatting to the service, while keeping WPF navigation, persistence, and grid refresh in the window.
- Added regression coverage for new-row detection, equivalent-row merging, different-efficiency row separation, minimum one-run merge increments, refreshed blueprint references, and queue status formatting.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 164 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 30
- Extended `BuildQueueService` with profitable candidate selection for the Blueprint page `Прибыльные в очередь` workflow.
- `MainWindow.xaml.cs` now delegates the candidate filtering/sorting rule to the service and only handles visible rows, item creation, queue insertion, and navigation.
- Added regression coverage for positive-profit/OK-status filtering, ranked rows before unranked rows, rank ordering, unranked SVR*ISK/hour ordering, and max-count limiting.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 168 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 31
- Closed the focused build project decision service extraction checklist.
- Added `BuildProjectDecisionService` for project material/item Build/Buy/Auto mode decisions.
- `MainWindow.xaml.cs` now delegates Build/Buy mode lookup and mutation to the service, while keeping WPF commands and project refresh/persistence in the window.
- Added regression coverage for default Auto behavior, setting Buy, updating to Build, avoiding duplicate decision rows, clearing with Auto, clearing with blank mode, and ignoring invalid type IDs.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 177 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 32
- Closed the focused project material used-by list extraction checklist.
- Added `UsedByListService` for appending unique product names into project material `UsedBy` strings.
- `MainWindow.xaml.cs` now delegates `UsedBy` aggregation to the service while keeping project material row construction in the window.
- Added regression coverage for empty inputs, first-name insertion, appending new names, avoiding duplicates, and ignoring blank names.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 182 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 33
- Closed the focused blueprint production type service extraction checklist.
- Added `BlueprintProductionTypeService` for production type classification, T3 invention classification, build-wave mapping, faction warfare cost multiplier, parallel job time packing, and production-time batching by available lines.
- `MainWindow.xaml.cs` now delegates these production-type/timing helpers to the service, keeping the larger estimate orchestration in the window for now.
- Added regression coverage for reaction, supercapital, capital component, subsystem, and capital-by-name classification; T3 invention routing; build waves; faction warfare multipliers; parallel production time; and production-line batching.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 196 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 34
- Refreshed `LOCAL_TECH_DEBT_AUDIT.md` and `PROJECT_LAYOUT.md` after the latest queue/project/production service extractions.
- Updated the current `MainWindow.xaml.cs` audit count to about 5,707 lines and documented that several remaining helper methods are now thin service wrappers.
- Updated the service map with `BlueprintProductionTypeService`, `BuildProjectDecisionService`, `BuildQueueService`, and `UsedByListService`.
- Re-scanned active `OurIPH` source for `TODO`, `FIXME`, `HACK`, and `XXX`; no active source markers were found outside build artifacts.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 196 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 35
- Closed the focused project material clipboard import extraction checklist.
- Added `ProjectMaterialImportService` for parsing bought/owned material quantities from clipboard text.
- `MainWindow.xaml.cs` now delegates clipboard text parsing and material-name matching to the service, while keeping WPF clipboard access and project stock mutation in the window.
- Added regression coverage for tab-separated exact names, case-insensitive prefix matching, longest material-name preference, repeated-row aggregation, unknown material rejection, and quantity parsing with spaces/NBSP/commas/dots.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 203 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 36
- Closed the focused blueprint efficiency service extraction checklist.
- Added `BlueprintEfficiencyService` for effective ME/TE and default child blueprint efficiency behavior.
- `MainWindow.xaml.cs` now delegates invention ME/TE, copy-only ME/TE, reaction child defaults, owned child efficiency, T2 child invention defaults, and T1 facility defaults to the service while still supplying the owned-blueprint lookup from `EveDatabaseService`.
- Added regression coverage for invention base 2/4 plus decryptor modifiers, copy-only 0/0, reaction child 0/0, owned child efficiency preference, T2 invention defaults, and T1 facility defaults.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 215 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 37
- Closed the focused sales volume ratio service extraction checklist.
- Added `SalesVolumeRatioService` for applying SVR/liquidity metrics to both blueprint rows and project items.
- Removed duplicated SVR application logic from `MainWindow.xaml.cs`; the window now delegates Total sold/orders, average items per order, trend, SVR, and SVR*ISK/hour calculations to the service.
- Added regression coverage for blueprint SVR, blueprint SVR*ISK/hour, project-item SVR from profit/time, average items per order, trend copying, and reset behavior when market volume is zero.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 223 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 38
- Closed the focused market cache store round-trip test checklist.
- Added path-injection constructors to `MarketPriceCacheStore` and `MarketHistoryCacheStore` so cache persistence can be tested without touching real workspace settings.
- Hardened both cache stores to create the target directory on save.
- Added regression coverage for XML round-trip persistence of market price cache rows and market history cache rows.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 233 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 39
- Closed the focused contract price store round-trip test checklist.
- Added path-injection constructor support to `ContractPriceStore` so contract sample persistence can be tested without touching real workspace settings.
- Hardened `ContractPriceStore.Save` to create the target directory for injected paths.
- Added regression coverage for XML round-trip persistence of contract metadata: type, price, observed time, contract ID, source, location, quantity, item count, and title.
- Added coverage that stale samples beyond the 120-day persistence cutoff and invalid samples are dropped on save/load.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 243 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 40
- Refreshed `LOCAL_TECH_DEBT_AUDIT.md` and `PROJECT_LAYOUT.md` after the latest efficiency/import/SVR/store passes.
- Updated the current `MainWindow.xaml.cs` audit count to about 5,532 lines.
- Updated the service map with `BlueprintEfficiencyService`, `ProjectMaterialImportService`, and `SalesVolumeRatioService`.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 243 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 41
- Closed the focused local extraction pass around Blueprint/Project estimate orchestration without touching external ESI/Fuzzwork/manual UI paths.
- Moved the `BlueprintEstimate` DTO out of `MainWindow.xaml.cs` into `Models/BlueprintEstimate.cs`.
- Added `BlueprintEstimateApplicationService` and `ProjectItemEstimateApplicationService` so applying estimate results, profitability fields, stale liquidity resets, and project item estimate fields no longer live directly in the window.
- Added `BlueprintCopyStatusService`, `BlueprintEstimateStatusService`, `BlueprintEstimateCacheService`, and `BlueprintManufacturingMathService` for BPC-only status text, estimate status priority, recursive estimate cache keys/cloning, and manufacturing multiplier/installation-cost formulas.
- `MainWindow.xaml.cs` is down to about 5,432 lines; the remaining risky core is recursive material build/buy traversal, best station/decryptor search, and invention cost accumulation.
- Added regression coverage for copy-only status formatting, estimate/project estimate application, recursive estimate cache clone isolation, status priority, and manufacturing math.
- Verification after each extraction: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 309 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 42
- Continued the local extraction pass around recursive build/buy, best station/decryptor orchestration, and invention cost accumulation without using external access.
- Added explicit context/result models for invention material costs, invention science times, science usage fees, and material build/buy decisions.
- Added `BlueprintInventionCostService` for source/datacore/decryptor/copy material cost accumulation and science usage cost formula.
- Added `BlueprintInventionTimeService` for copy and invention time accumulation.
- Added `FacilityActivityStationService` for resolving Invention/Copying activity stations and fallback station copies.
- Extended `BlueprintEstimateSelectionService` for supported station candidate filtering, cheapest station selection, and best decryptor candidate selection.
- Added `MaterialBuildBuyDecisionService` for the recursive material build-vs-buy decision rule.
- `MainWindow.xaml.cs` is down to about 5,344 lines. The remaining high-risk extraction is the full recursive `CalculateBlueprintEstimate` traversal with database child blueprint lookup, surplus offsets, mineral/ore fallback, and cycle/path handling.
- Added regression coverage for invention cost buckets, usage fee math, science time math, activity station fallback, station/decryptor selection, and material build/buy decisions.
- Verification after each extraction: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 347 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 43
- Prepared the safe migration foundation for `CalculateBlueprintEstimate` without moving the full recursive method.
- Added explicit future-transfer models: `BlueprintEstimateContext`, `BlueprintEstimateDependencies`, `BlueprintEstimateRecursionState`, `BlueprintEstimateResult`, `MaterialEstimateLine`, and `ChildBlueprintEstimateRequest`.
- Added `BlueprintEstimateTraversalService` for the first safe recursive sub-block: path/cycle guard, parent enter/exit, child request creation, and surplus offset application.
- Added `BlueprintMaterialEstimateLineService` for the price delegate wrapper around a material line: adjusted quantity, unit price, buy cost, and market-volume-short detection.
- Wired those two services into the existing `CalculateBlueprintEstimate` while preserving the recursive orchestration in `MainWindow.xaml.cs`.
- Added characterization/golden tests for simple material lines, missing price/cache behavior, market-volume-short behavior, child component request runs, capital/component-like chains, copy-only child run behavior, path/cycle guard, surplus offsets, and surplus clamp-to-zero behavior.
- Current decomposition of `CalculateBlueprintEstimate`:
  - setup and cache lookup: cache key, facility/station math, skill/time/material multipliers;
  - material line pricing: adjusted quantity, unit price, buy cost, market volume short;
  - recursion and child lookup: path/cycle guard, database child blueprint lookup, always-buy/reaction-drilldown stops;
  - child estimate request: child efficiency, child runs, cheapest station, recursive estimate call;
  - child result handling: build cost, component production time, surplus sale offset;
  - mineral/ore fallback: mineral buy quantity aggregation and ore purchase cost fallback;
  - build/buy decision: choose child build cost vs market buy cost;
  - invention/decryptor/station integration: invention material/time/usage cost, activity station resolution, decryptor effects;
  - final estimate assembly: installation cost, production time, line counts, invention breakdown, cache store.
- `MainWindow.xaml.cs` is about 5,356 lines after adding the explicit foundation and wiring; the method itself is better prepared for the next extraction even though the total file grew slightly due to service fields and context wiring.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 368 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 44
- Closed the delegate-backed material traversal extraction pass without moving the full `CalculateBlueprintEstimate` method.
- Added `BlueprintMaterialTraversalResult` and `BlueprintEstimateMaterialTraversalService` for material traversal orchestration around adjusted material lines, surplus offsets, child blueprint delegate calls, mineral fallback aggregation, market-volume-short detection, and build/buy decisions.
- Extended `BlueprintEstimateContext` and `BlueprintEstimateDependencies` with explicit traversal dependencies for material multiplier, child efficiency lookup, cheapest child station lookup, recursive child estimate calculation, and market-volume lookup.
- Wired `CalculateBlueprintEstimate` through the new traversal service using delegates back to the existing database/UI-adjacent methods, keeping WPF state and full recursive estimate ownership in `MainWindow.xaml.cs` for now.
- Added characterization coverage for buy-only traversal, child component recursion via delegate, surplus-before-decision behavior, missing-price fallback, market-volume-short behavior, mineral fallback aggregation, cycle/path guard suppression, and buy-vs-build branch selection.
- `MainWindow.xaml.cs` is down to about 5,319 lines after removing the embedded material traversal loop. The remaining risky extraction tail is full database-backed recursive estimate ownership, final estimate assembly, and broader golden tests over real capital/component chains.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 385 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 45
- Paused further `CalculateBlueprintEstimate` extraction and added the requested legacy/database-backed golden test foundation first.
- Added `LEGACY_PARITY_MAP.md` to map legacy VB.NET production/calculation entry points to the current OurIPH services and to separate confirmed service behavior from provisional numeric parity.
- Added database-backed fixture coverage in `OurIPH.Tests` using the real local `LatestFiles/EVEIPH DB.sqlite` through `EveDatabaseService`.
- Fixture cases now cover:
  - `Rifter` as a simple T1/mineral blueprint with four direct mineral lines.
  - `Capital Armor Plates` as a component-chain blueprint with `Reinforced Carbon Fiber` resolving to a child reaction formula.
  - `Revelation` as a capital-like chain with 19 direct material lines and 19 child blueprint resolutions.
  - `Zirnitra` as copy-only/BPC-only special blueprint with Triglavian capital components.
- Added deterministic database-backed traversal checks for mineral fallback aggregation, child request creation, path/cycle guard suppression, market-volume-short propagation, cheaper-build branch selection, and controlled missing-price behavior.
- Numeric final estimate/profit parity is explicitly documented as provisional until exact legacy outputs are captured or replicated; current tests lock real database structure and traversal behavior, not live market truth.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 441 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-11 Pass 46
- Added the first numeric legacy parity layer without doing any new `CalculateBlueprintEstimate` extraction.
- Split the database-backed test naming into structural golden coverage and numeric legacy parity coverage.
- Added `DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedLegacyFormulas` in `OurIPH.Tests`.
- Confirmed numeric expected values from legacy `Blueprint.vb`, not from current OurIPH final estimates:
  - Direct material quantity formula from `BuildItem`: `Max(runs, Ceiling(Round(runs * baseQuantity * ((1 - ME/100) * facilityMaterialMultiplier), 2)))`.
  - Simple production time formula from `SetProductionTime` / `SetBPTimeModifier`.
- Added database-backed numeric assertions for:
  - `Rifter` ME 10 + facility material multiplier 0.98 adjusted mineral quantities.
  - `Capital Armor Plates` ME 10 adjusted material quantities, including rounding up `Organic Mortar Applicators` from 4.5 to 5 and `Zydrine` from 553.5 to 554.
  - `Rifter` TE 20 + facility time multiplier 0.90 + Industry V + Advanced Industry V production time of 2,937.6 seconds.
  - A small `Revelation` child-run fragment: 4 `Capital Armor Plates` required at ME 0/facility 1.0 maps to 4 child runs because portion size is 1.
- Updated `LEGACY_PARITY_MAP.md` to mark these fields as confirmed numeric parity and keep final recursive material cost/profit/ISK-hour as provisional.
- Verification: normal Debug MSBuild passed and `OurIPH.Tests.exe` passed with 462 assertions. Final WPF smoke for this pass is recorded after documentation updates.

## Implemented In 2026-05-12 Pass 47
- Continued numeric legacy parity only; no further `CalculateBlueprintEstimate` extraction and no UI behavior changes.
- Re-read legacy `Material.vb`, `Materials.vb`, and `Blueprint.vb SetManufacturingCostsAndFees` before adding new expected numbers.
- Added `DatabaseBackedBlueprintLegacyNumericParity_UsesConfirmedMaterialCostAndFees` in `OurIPH.Tests`.
- Confirmed direct material total formula from legacy `Material.SetTotalCostVolume` / `Materials.InsertMaterial`: `TotalCost = CostPerItem * Quantity`, summed by inserted material lines.
- Confirmed manufacturing usage formula from legacy `Blueprint.vb SetManufacturingCostsAndFees`: `CLng(EIV * runs) * ((CostIndex * CostMultiplier * FWMultiplier) + FacilityTax + SCCIndustryFee + AlphaTax)`.
- Added deterministic database-backed numeric assertions for:
  - `Rifter` ME 10 + facility material multiplier 0.98 direct material cost: 218,736 ISK with fixture prices.
  - `Rifter` EIV: 94,500 ISK from unmodified per-run base quantities and deterministic adjusted prices.
  - `Rifter` manufacturing usage cost: 7,560 ISK with confirmed EIV/rate inputs.
  - `Capital Armor Plates` ME 10 direct material cost: 2,355,570 ISK with deterministic 10 ISK/unit fixture prices.
- The new fixture also compares those legacy-derived numbers against `BlueprintEstimateMaterialTraversalService` in a forced direct-buy boundary, keeping recursive component final cost parity provisional.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 468 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 48
- Added a narrow recursive component-cost legacy parity harness without moving `CalculateBlueprintEstimate` further and without UI changes.
- Re-read the legacy recursive path in `Blueprint.vb BuildItem`, especially child blueprint lookup, child run propagation, `GetBuildFlag`, child `GetTotalComponentCost` insertion, and `SetPriceData` total-cost assembly.
- Added `DatabaseBackedBlueprintLegacyNumericParity_UsesRecursiveComponentCostHarness` in `OurIPH.Tests`.
- The harness is test-only and bounded: it uses the confirmed legacy formulas for adjusted quantity, direct material total cost, child runs, build-vs-buy comparison, and manufacturing usage fee. It intentionally excludes surplus sellback and ore/mineral fallback until those legacy branches are mapped.
- Confirmed database-backed recursive fragments:
  - `Capital Armor Plates` ME 10 builds `Reinforced Carbon Fiber` for one child run when the deterministic component buy price is higher than child build cost.
  - `Capital Armor Plates` recursive material cost with built `Reinforced Carbon Fiber`: 2,358,712.08 ISK.
  - `Capital Armor Plates` separate root usage cost: 20,938.24 ISK; total build cost: 2,379,650.32 ISK.
  - Path guard suppresses a previsited `Reinforced Carbon Fiber` child and falls back to deterministic buy cost: 11,354,670 ISK.
  - Small `Revelation` fragment builds four `Capital Armor Plates` and confirms total child build cost: 10,556,957.12 ISK.
- Compared the legacy-derived recursive expected values against `BlueprintEstimateMaterialTraversalService` through delegates, keeping expected values independent from the production traversal.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 482 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 49
- Mapped legacy surplus/excess/sellback behavior without moving `CalculateBlueprintEstimate` further and without UI changes.
- Re-read legacy `Blueprint.vb` paths:
  - `GetAdjustedQuantity` for excess/leftover lookup before material processing.
  - `UseExcessMaterials` for consuming the excess list after child build quantity is known.
  - `AdjustSellExcessValue` for sellback value calculation.
  - `SetPriceData` and `GetBuildFlag` for where sellback affects total cost and build/buy comparison.
- Added `DatabaseBackedBlueprintLegacyNumericParity_UsesSurplusExcessAndSellbackMappings` in `OurIPH.Tests`.
- Added narrow parity coverage for:
  - no leftover: requirement stays unchanged;
  - partial leftover: required quantity is reduced and the excess list is consumed later by `UseExcessMaterials`;
  - full leftover: build is skipped and the unused remainder stays in excess;
  - sellback disabled: sellback credit is zero;
  - sellback enabled: gross excess value is reduced by sales tax/broker fee and clamped to zero if fees exceed gross value.
- Added an explicit `applySurplusSellback` flag to `BlueprintEstimateTraversalService.ApplySurplusOffset` with default `true`, so current behavior is preserved while tests can represent legacy `SellExcessItems = False`.
- Documented the legacy quirk that the partial `GetAdjustedQuantity` branch records `UsedExcessMaterials` quantity from the remaining requirement before `UseExcessMaterials` consumes the actual excess list.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 501 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 50
- Mapped legacy `ConvertToOre` / ore-mineral fallback behavior without moving `CalculateBlueprintEstimate` further and without UI changes.
- Re-read legacy `ConvertToOre.vb` and `Blueprint.vb` call sites:
  - `Blueprint.vb` only calls `ConvertToOre.GetOresfromMinerals` after `BuildItem` when a reprocessing facility exists and `ReprocessingFacility.ConvertToOre = True`.
  - When conversion is not enabled, minerals remain normal raw material lines; legacy skips minerals only through the separate `IgnoreMinerals` flag.
  - `ConvertToOre.vb` builds selected ore candidates from user conversion settings, refines each candidate through `ReprocessingPlant`, then uses `LpSolve` to minimize by refine price, ore/ice price, or ore/ice volume.
  - Converted minerals are removed from the material list, ore lines are inserted, refined output is used to compute returned excess, and reprocessing usage fees are accumulated.
- Added `DatabaseBackedBlueprintLegacyNumericParity_UsesOreMineralFallbackMappings` in `OurIPH.Tests`.
- Added narrow database-backed coverage for:
  - legacy `ConvertToOre` gating by reprocessing facility plus flag;
  - mineral classification through inventory group 18;
  - Rifter raw mineral fallback quantities when ore conversion is disabled;
  - non-mineral/no-child direct buy fallback for `Organic Mortar Applicators`;
  - synthetic missing-child direct buy fallback;
  - local reprocessing candidate/output availability for Tritanium, including compressed-only filtering.
- Full numeric parity for the LP ore plan is intentionally still provisional because it depends on selected ores, compression settings, ignored refined items, refinery skills/rates, `MinimizeOn`, and lpsolve rounding.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 526 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 51
- Resumed cautious `CalculateBlueprintEstimate` extraction after the recursive component, surplus/sellback, and ore/mineral fallback parity-mapping passes.
- Did not move the full method, did not change UI behavior, and did not touch full `ConvertToOre` LP numeric parity.
- Added `BlueprintEstimateMaterialOrchestrationService` as a small service boundary around:
  - explicit `BlueprintEstimateContext` creation;
  - explicit `BlueprintEstimateDependencies` creation;
  - invocation of the existing delegate-backed `BlueprintEstimateMaterialTraversalService`;
  - material traversal result aggregation with mineral purchase cost supplied through a delegate.
- Added `BlueprintEstimateResultAssembler` and `BlueprintEstimateAssemblyContext` to move the final `BlueprintEstimate` object assembly out of `MainWindow.xaml.cs`.
- `MainWindow.CalculateBlueprintEstimate` now keeps the high-risk recursive database/UI-adjacent delegates in the window, but no longer directly owns the material traversal service call or the final estimate object initializer.
- Removed now-unused `MainWindow` service fields for the lower-level traversal/line/build-buy services that are owned by the new orchestration service.
- Added tests:
  - `BlueprintEstimateMaterialOrchestrationService_CreatesBoundariesAndAggregatesResults`;
  - `BlueprintEstimateResultAssembler_AssemblesFinalEstimateSummary`.
- Existing database-backed structural and legacy numeric parity tests continue to pass, including Rifter, Capital Armor Plates, Revelation fragment, surplus/sellback, and ore/mineral fallback gate coverage.
- `MainWindow.xaml.cs` is now about 5,303 lines.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 555 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 52
- Continued a cautious local extraction pass focused on UI-independent estimate summary/display mapping, without moving `CalculateBlueprintEstimate` wholesale and without touching full `ConvertToOre` LP numeric parity.
- Extended `BlueprintEstimateApplicationService` with an overload that applies display-ready product result fields for blueprint rows:
  - produced quantity;
  - product market volume;
  - contract unit price;
  - product price source;
  - product price details.
- Preserved the existing overload behavior that clears stale product price/source/liquidity fields until the caller supplies fresh display data.
- Removed the old `MainWindow.xaml.cs` `SetEstimate` wrappers, which duplicated estimate DTO creation and manually passed summary/profit/status fields through a long parameter list.
- `EstimateBlueprints` now calls `BlueprintEstimateApplicationService.Apply` directly for both unavailable estimates and normal successful estimates.
- Added regression coverage in `BlueprintEstimateApplicationService_AppliesAndResetsEstimateFields` for the new display overload, including total/revenue/profit/ISK-hour recalculation, contract-vs-market source display fields, produced quantity, market volume, build/buy line counts, and facility display fields.
- Existing structural golden and legacy numeric parity tests continue to pass; no formulas were changed.
- `MainWindow.xaml.cs` is now about 5,254 lines.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 567 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 53
- Continued the symmetric cautious extraction pass for Project item estimate display/summary mapping without moving `CalculateBlueprintEstimate` and without touching full `ConvertToOre` LP numeric parity.
- Added `ProjectItemEstimateDisplayService` as a UI-independent project-row display boundary around:
  - net revenue calculation from effective unit price, total project quantity, sales taxes/fees, facility preset, and station;
  - project estimate status formatting through `BlueprintEstimateStatusService`;
  - copy-only/BPC status suffix formatting through `BlueprintCopyStatusService`;
  - final project row application through `ProjectItemEstimateApplicationService`.
- `ApplyProjectItemEstimates` in `MainWindow.xaml.cs` now delegates project item revenue/status/price-source display mapping to the new service and no longer assembles those display fields inline.
- Added `ProjectQueueDisplayService` for project job material blocker status text used by the project job export workflow. This keeps the WPF handler responsible for clipboard/UI state while moving row-status formatting out of the window.
- Added tests:
  - `ProjectItemEstimateDisplayService_AppliesSummaryStatusAndPriceDisplay`;
  - `ProjectQueueDisplayService_FormatsJobMaterialStatus`.
- Existing database-backed structural and legacy numeric parity tests continue to pass; no formulas or UI behavior were intentionally changed.
- `MainWindow.xaml.cs` is now about 4,635 lines.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 581 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 54
- Paused extraction/refactor work and prepared the project for the next manual interactive WPF UI pass.
- Reworked `UI_SMOKE_CHECKLIST.md` into a scenario-based manual checklist with actions, expected success signs, and known provisional zones.
- The checklist now covers:
  - app launch/database status;
  - Blueprint search by name and product typeID;
  - Rifter, Capital Armor Plates, Revelation, and Zirnitra fixture workflows;
  - station/structure/rig influence checks;
  - price update status and market/contract source display;
  - Analysis/Top 10/ranking/filtering, including rare/officer and limited-source filtering;
  - numeric input validation and readonly DataGrid protections;
  - build queue add/merge/clear;
  - project creation, project item estimates, build/buy/auto decisions, material stock editing, and persistence after restart;
  - 1280px/wide-monitor layout checks.
- Added explicit checklist notes that full final numeric capital-chain parity and full `ConvertToOre` LP ore-plan parity are still provisional and should not be confused with UI smoke failures.
- Updated `TODO_NEXT.md` to clearly separate manual UI checks, external/live-data blockers, and remaining local extraction/parity tasks.
- No production code or calculation formulas were changed in this pass.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 581 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-12 Pass 55
- Focused on Facilities / Station preset persistence and UX; did not continue `CalculateBlueprintEstimate` extraction, did not touch full `ConvertToOre` LP numeric parity, and did not change calculation formulas.
- Fixed preset loading/saving safety:
  - `FacilityPresetStore` now supports a testable explicit file path and creates its target directory before saving.
  - Existing mojibake strings in loaded preset/station names and validation messages are repaired when they match the legacy UTF-8-as-CP1251 corruption pattern.
  - New default preset/station names in the Facilities UI are readable ASCII (`New preset`, `New station`), and the no-rig option is now `No rig`.
- Fixed a real persistence/UX bug in `SaveFacility_Click`:
  - invalid/incomplete station configurations are saved with a warning instead of being discarded before `_facilityStore.Save`;
  - manual system text is resolved through local DB `FindSystem`, filling `SystemName`, `SolarSystemId`, `RegionId`, and security when available.
- Added a WPF loading guard around Facilities selection population so `SelectionChanged` handlers no longer overwrite the current station while an existing preset is being displayed.
- Cleaned facility validation messages in `FacilityCatalogService` so broken Cyrillic/mojibake no longer appears in station preset warnings.
- Added regression coverage:
  - `FacilityPresetStore_RoundTripsAllFieldsAndRepairsDefaultNameMojibake` round-trips all preset/station fields;
  - verifies edits survive a second save/load;
  - verifies broken default Cyrillic names/messages are repaired on load.
- Studied legacy facility behavior in `ManufacturingFacility.vb`, `ESI.vb`, and `ProgramSettings.vb`:
  - legacy auto-loaded region/system/facility lists from the local DB;
  - cost indices came from `INDUSTRY_SYSTEMS_COST_INDICIES` and could be refreshed via ESI;
  - public structures could be refreshed via ESI;
  - UI loading guards prevented combo population from mutating selected facility state.
- Documented the Facilities / Station Presets legacy mapping in `LEGACY_PARITY_MAP.md`.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 649 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-17 Pass 56
- Continued with the next safe local TODO around `ConvertToOre` / ore planning, without moving `CalculateBlueprintEstimate`, without changing UI workflow, and without claiming full legacy LP numeric parity.
- Added `OrePlanningService` as a production-safe boundary for the current greedy ore replacement helper:
  - builds ore candidates from local reprocessing outputs and required mineral type IDs;
  - floors reprocessed output by supplied facility/yield delegate;
  - chooses greedy batches by cost per covered mineral value;
  - records produced minerals by mineral type;
  - calculates selected ore purchase cost and returns zero when a planned ore has no price.
- Routed `ConvertProjectMineralsToOreByWave` and `GetOrePlanPurchaseCost` through `OrePlanningService`.
- Removed the old ore-planning helper methods and nested ore-plan classes from `MainWindow.xaml.cs`; the window still owns WPF/cache/database wiring, selected region prices, and facility yield calculation.
- Added `OrePlanningService_PlansGreedyOreReplacementDeterministically` to cover candidate construction, yield flooring, greedy selection order, ore batch rounding, remaining-requirement clearing, purchase cost, and missing-price behavior.
- Extended the database-backed ore/mineral fallback fixture so a real local compressed-ore reprocessing row flows through `OrePlanningService`, rounds to reprocessing batches, clears the Tritanium requirement, and calculates controlled purchase cost.
- Updated `LEGACY_PARITY_MAP.md`, `PROJECT_LAYOUT.md`, `LOCAL_TECH_DEBT_AUDIT.md`, and `TODO_NEXT.md` to state clearly that this is current greedy planner coverage, while full legacy `ConvertToOre.vb GetOresfromMinerals` LP numeric parity remains provisional.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 668 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Implemented In 2026-05-17 Pass 57
- Corrected the active development direction back to GUI recovery and core workflow stabilization.
- Updated `TODO_NEXT.md` so the active sprint is explicitly GUI/workflow stabilization: automated baseline first, manual UI smoke preparation next, concrete GUI/workflow bug fixes only, no `CalculateBlueprintEstimate` extraction, no full `ConvertToOre` LP parity, and no speculative features/columns.
- Added an `AGENTS.md` active scope rule: choose one lane per pass and do not substitute unrelated refactor/parity work when manual GUI verification is unavailable.
- Automated baseline passed before GUI inspection.
- Interactive WPF/manual UI access was not available from this Codex session, so the manual checklist was not claimed as passed.
- Static GUI workflow inspection found a concrete UX bug cluster: several status bar, page subtitle, project workflow, queue, and status-service strings still displayed UTF-8 mojibake instead of readable Russian.
- Fixed that display/status cluster in `MainWindow.xaml.cs` and `BlueprintEstimateStatusService` without changing calculation formulas, UI layout, or extraction boundaries.
- Updated `OurIPH.Tests` status-service expectations so readable strings such as `Нет цен`, `Нет данных invention`, and `Нет кеша цен` are locked by tests.
- Full legacy `ConvertToOre.vb` LP numeric parity and full final capital-chain numeric parity remain provisional/backlog; neither was advanced in this pass.
- Verification: normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 668 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.

## Last Updated
- 2026-05-10: Added console service tests, extracted profitability, stock math, material adjustment math, and invention planning math, improved price update progress, added public ESI contract sample ingestion and review UI, split rare/officer filtering settings, added inline project stock editing, shared timeout web client, score breakdown UI, fixed Windows test-project build configuration, verified build/tests/smoke, added project stock persistence regression coverage, moved blueprint filtering rules into editable XML settings, tuned ranking against liquid/low-volume/capital examples, added broad public ESI scan support for visible top/capital/reaction candidates, made low-volume hiding the default recommendation posture, documented project layout/runtime settings policy, added Blueprint UI clear/scroll usability fixes, and continued shifting the Blueprint UI back toward the legacy EVE IPH layout with a top menu/tab shell, bottom status line, compact result table, upper-right totals, direct/raw material grids, advanced analytics isolated on a separate page, working blueprint type filters on the main workflow, persistent lower detail height, visible queue status on the Blueprint actions block, compact queue icons, queue status updates after remove/clear/create-project actions, wrapped lower skill actions, lower-table tooltips for long item names, best-facility display in the selected totals block, functional top menu commands for navigation, estimate, price refresh, queue clearing, project creation, and exit, frozen product/blueprint identity columns in the main, analysis, and queue tables, numeric input validation for calculation/settings fields, copy-only BPC handling for Zirnitra-style blueprints, and readonly table protections to prevent WPF edit-mode crashes on computed columns.
- 2026-05-10 Pass 13: Fixed explicit Blueprint search parity. The Blueprints page now treats typed search as a direct lookup: matching product/blueprint names or typeIDs stay visible even when recommendation filters are enabled, the first visible result is selected and scrolled into view, and the status line reports loaded vs shown counts. SQLite search is explicitly case-insensitive and supports product/blueprint typeID lookup. Direct verification: `Revelation`/`revelation` returned Revelation and Revelation Navy Issue, `19720` returned Revelation, and `22444` returned Sleipnir. Normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 68 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.
- 2026-05-10 Pass 14: Build-chain component skills now feed selected Blueprint skill rows and skill filtering, matching the legacy pattern where component build skills are merged into the required skill lists. `BlueprintSkillRequirementService` aggregates root blueprint skills plus buildable component blueprint skills, keeps the highest required level for duplicates, and respects always-buy/reaction-drilldown stops. Normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 73 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.
- 2026-05-10 Pass 15: Closed the focused Blueprint list-filter technical-debt checklist. Added `BlueprintListFilterOptions` and `BlueprintListFilterService` so the main list filtering rules are no longer embedded directly in `MainWindow.xaml.cs`; the window now builds options from UI controls and delegates type/build/skill checks to existing methods. The Blueprint status line now reports loaded/shown counts after filter changes and explains active Top N state as score-based after `Оценить`. Added regression coverage for Top N, low market volume, missing prices, excluded items, and explicit-search bypass. Normal Debug MSBuild passed, `OurIPH.Tests.exe` passed with 79 assertions, WPF smoke passed with `ExitCode=0`, and no hidden workspace `OurIPH.exe` remained.
