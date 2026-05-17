# Our IPH - start plan

## Current source

- Source repo: EVEIPH/EVE-IPH, downloaded as `EVE-IPH-master`.
- Related repos downloaded locally:
  - `LatestFiles` - release binaries, installer, current `EVEIPH DB.sqlite`, patch notes, DLLs.
  - `EVE-IPH-Deployment-Program` - deployment/build tooling for release database and exported files.
  - `EVE-SDE-Database-Builder` - older SDE import/build tool.
  - `eveiph.github.io` - project website.
- Main solution: `EVE-IPH-master/EVE Isk per Hour.sln`.
- Main app: VB.NET Windows Forms, .NET Framework 4.6.1, x86.
- Entry point: `My Project/Application.Designer.vb` starts `frmMain`.
- Main project: `EVE Isk per Hour.vbproj`.
- Major dependencies: System.Data.SQLite, Newtonsoft.Json, LpSolveDotNet.

## First target

Build a smaller "Our IPH" version focused on the core workflow:

1. Choose or search blueprint/item.
2. Load/update market prices.
3. Calculate manufacturing cost, profit, and ISK/hour.
4. Show a compact result table with only the important columns.
5. Save local settings without requiring the full legacy interface.

## Shopping List Direction

Goal: turn the shopping list into a current-build tracker, not only a buy/export list.

- Keep the Blueprint tab workflow as the main workflow.
- Keep item names, locations, and EVE-specific names in their original form.
- Add a current build checklist that shows what remains to build, what can be removed as completed, and what excess output is expected from blueprint/reaction portion sizes.
- First implemented step: the Shopping List build grid now shows `Циклы`, `Будет`, and `Остаток` for built items, so portion-size leftovers are visible.
- MVP implemented: Shopping List now has `Добавить в проект` and `Текущие проекты` buttons.
  - Projects are saved as snapshots in `Settings/CurrentProjects.xml`.
  - The `Текущие проекты` window shows project tabs for итоговые items, build items, and buy items.
  - Build rows show cycles, produced amount, and excess output.
  - Selected rows can be deleted from a project as they are completed.

## Fixed issues

- Startup crash in `frmMain.DisplayESIStatusMessages()` when the local SQLite database does not contain `ESI_STATUS_ITEMS` or `ESI_ENDPOINT_ROUTE_TO_SCOPE`.
  - Cause: local builds did not include the release database from `LatestFiles`.
  - Runtime data fix: copied `LatestFiles/EVEIPH DB.sqlite` into `Root Directory/EVEIPH DB.sqlite`.
  - Code fix: `DisplayESIStatusMessages()` now skips ESI status messages if either status table is missing.
- Official EVE IPH update prompts are disabled for local Our IPH builds.
  - Cause: the upstream updater compares local files to the official release XML and can replace local changes.
  - Code fix: `CheckForUpdates()` exits early when `DisableOfficialUpdates` is enabled.
- ESI `429 Too Many Requests` errors no longer open a modal dialog for every failed price request.
  - Cause: threaded price updates can hit ESI rate limits and each worker attempted to show its own error dialog.
  - Code fix: `ESIErrorProcessor.ProcessWebException()` cancels threaded updates quietly for HTTP 429.
- Update Prices now defaults to Fuzzworks instead of CCP/ESI.
  - Cause: CCP/ESI bulk market order updates can hit rate limits during normal use.
  - Code fix: `DefaultUseESIData` is set to `DataSource.Fuzzworks`, and saved CCP settings are migrated to Fuzzworks on load.
- Fuzzworks price downloads are less aggressive.
  - Cause: updating nearly every selected item at once can timeout even against Fuzzworks.
  - Code fix: Fuzzworks batches are capped at 30 type IDs per request, requests pause briefly between batches, and JSON download timeout is 30 seconds with limited retries.
- Russian UI localization has started.
  - Current scope: main window menus, main tabs, status bar, Update Prices controls, and visible price table headers.
  - Non-goal: item names, region names, solar system names, and price type values remain in their original EVE/database form.

## Recommended approach

Start by stabilizing the existing app, then carve out a simplified UI:

1. Get the original solution building locally in Visual Studio 2019/2022 with .NET Framework 4.6.1 targeting pack.
2. Keep the calculation/data classes at first, especially blueprint, material, market price, SQLite, and ESI code.
3. Add a new simplified form instead of editing `frmMain` directly.
4. Route only the minimum manufacturing workflow into the new form.
5. Fix bugs as we encounter them, with small reproducible cases.

## Known setup notes

- Visual Studio 2022 Community is installed.
- MSBuild is available at `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`.
- Git is installed at `C:\Program Files\Git\cmd\git.exe`.
- The current PowerShell session may not have the updated `PATH`; use full paths if needed.
- The repository ZIP is available locally as `EVE-IPH-master.zip`.
- The extracted source is available in `EVE-IPH-master`.
- The latest release database is available in `LatestFiles/EVEIPH DB.sqlite`.
- `LatestFiles/EVEIPH DB.sqlite` contains the startup tables `ESI_STATUS_ITEMS` and `ESI_ENDPOINT_ROUTE_TO_SCOPE`.
- A copy of that database has been placed in `Root Directory/EVEIPH DB.sqlite` so the locally built app can start.
- Original solution build command:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "EVE Isk per Hour.sln" /p:Configuration=Debug /p:Platform=x86 /m /v:minimal
```

- The original solution builds successfully after allowing writes to the parent `Root Directory`.

## Questions to decide next

- Keep VB.NET WinForms, or migrate to a newer stack such as C#/.NET WinForms/WPF?
- Which feature should be first: manufacturing calculator, mining/reprocessing, market prices, or characters/ESI login?
- Which bugs are most important to fix first?
