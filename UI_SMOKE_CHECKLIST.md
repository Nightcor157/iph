# OurIPH Manual UI Smoke Checklist

Use this checklist after the automated Debug build, `OurIPH.Tests.exe`, and `OurIPH.exe --smoke-test` pass. This is an interactive WPF checklist for behavior that the headless smoke test cannot prove. It intentionally avoids EVE SSO, private/corporation contracts, API keys, and mandatory live market access.

## Known Provisional Areas
- Do not treat exact final profit/material-cost numbers for full `Revelation` or `Zirnitra` chains as final legacy parity yet. Structural chain behavior is covered; exact end-to-end numeric parity is still provisional.
- Do not treat full `ConvertToOre` LP ore-plan numeric output as confirmed. Safe mineral fallback and gate/classification behavior are covered; full ore optimization remains a parity TODO.
- Live Fuzzwork/ESI downloads may fail or differ because they depend on current network/live data. Cache fallback and understandable status are the smoke target.
- Authenticated EVE SSO/private/corporation contract workflows are out of scope for this checklist.

## 1. Launch And Baseline
- Close any running `OurIPH.exe` from this workspace output folder.
- Build Debug Any CPU.
- Run `OurIPH/OurIPH/bin/Debug/OurIPH.exe`.
- Success signs:
  - App opens without a crash dialog.
  - Bottom status bar says the local EVEIPH database is connected.
  - Top menu and tabs are visible: Blueprints, Analysis, Prices, Stations, Build Queue, Current Projects.
  - No controls overlap at the default window size.

## 2. Blueprint Search
- Open Blueprints.
- Type `rifter`, press Find.
- Success signs:
  - `Rifter` / `Rifter Blueprint` appears.
  - First visible row is selected.
  - Component Material List shows simple mineral requirements.
  - Raw Material List also shows minerals, not component recursion.
- Type `revelation`, press Find.
- Success signs:
  - `Revelation` rows appear immediately even with recommendation filters enabled.
  - Lower material and skill panels update.
- Type product typeID `19720`, press Find.
- Success signs:
  - `Revelation` is found by product typeID.
- Press All, then Clear List.
- Success signs:
  - Blueprint grid clears.
  - Selected material/raw material/skill/contract review rows clear.
  - Status line reports that the list was cleared.

## 3. Blueprint Fixture Cases
- Search and select `Rifter`.
- Press Estimate with Runs `1`, ME `10`, TE `20`.
- Success signs:
  - Status is OK or explains missing cached prices.
  - Component and Raw grids contain mineral rows such as Tritanium/Pyerite/Mexallon/Isogen.
  - No child component chain appears for the simple T1 case.
- Search and select `Capital Armor Plates`.
- Press Estimate with Runs `1`, ME `10`, TE `20`.
- Success signs:
  - Component list includes capital/component materials.
  - Raw list drills into at least the `Reinforced Carbon Fiber` reaction/formula path when buildable and not forced to buy.
  - Skill grid includes relevant component/reaction build requirements where applicable.
- Search and select `Revelation`.
- Press Estimate with Runs `1`.
- Success signs:
  - Direct material list shows capital components.
  - Raw list expands component-chain materials without hanging.
  - No infinite recursion or duplicate runaway rows.
  - If exact profit/material numbers look suspicious, record them, but remember full final numeric capital-chain parity is still provisional.
- Search and select `Zirnitra`.
- Success signs:
  - Row/details indicate copy-only/BPC-only behavior.
  - ME/TE are forced or displayed as 0/0 for estimate/queue semantics.
  - For 2 queued runs, queue display should show copy count text similar to `BPC 2x1`.

## 4. Facility, Structure, Station, Rigs
- Select `Rifter`, run one estimate with default station/hub.
- Change station/facility/structure or rig settings on Stations/Blueprints where available.
- Re-run Estimate.
- Success signs:
  - Best station/facility text updates or remains explainable.
  - Material/job/total cost or time changes when the selected station/rig bonuses should affect the blueprint.
  - If no station supports the activity, status says no station instead of crashing.

## 5. Price Update And Price Source
- Open Prices.
- Run a normal price update for a small visible set or selected workflow.
- Success signs:
  - Progress/status text is understandable: processed count, fresh/cache/missing/error if applicable.
  - App remains responsive enough to finish or report failure.
- Return to Blueprints and estimate a capital-like item with saved/manual contract samples if available.
- Success signs:
  - Market/Contract source fields show the selected effective source.
  - Contract source/details appear only when samples are accepted by filtering.
  - Missing live prices show a controlled status instead of blank unexplained results.

## 6. Analysis, Top 10, Ranking, Filtering
- Open Analysis.
- Use Estimate on a loaded list, then enable Top with count `10`.
- Success signs:
  - Top filtering is rank/score based after estimation, not just visible row order.
  - Score/reason columns explain why rows are ranked.
  - Profitable queue candidates prefer OK/profitable rows.
- Toggle low-market, limited-source, and rare/officer filters.
- Success signs:
  - Rare/officer/noise rows hide when rare/officer filter is enabled.
  - Limited-source rows hide separately from rare/officer.
  - Filter status reports loaded/shown counts.

## 7. Numeric Input And Readonly Grids
- Try typing letters into Runs, ME, TE, Top count, contract price, facility percent, skill, and stock numeric fields.
- Success signs:
  - Invalid letters are rejected or reset on focus loss.
  - Valid decimal/integer formats still work where expected.
- Double-click computed grid cells: Meta, skills, costs, status, queue ME/TE, project cost/status columns.
- Success signs:
  - No WPF binding exception dialog appears.
  - Only the intended project material owned/bought stock column is editable.

## 8. Build Queue
- Select `Rifter` or `Revelation`, press Add To Queue.
- Success signs:
  - Build Queue tab shows a row with product icon/name, blueprint icon/name, runs, ME/TE, status, profit/SVR fields if estimated.
  - Queue status count increments.
- Add the same item again with same ME/TE/decryptor.
- Success signs:
  - Existing row merges/increments runs instead of duplicating unnecessarily.
- Press Clear Queue from the Blueprint Actions block.
- Success signs:
  - Queue clears without switching pages.
  - Queue status updates in the action block/status bar.

## 9. Project Creation And Project Item Estimate
- Add one or more queue rows to a project.
- Open Current Projects.
- Success signs:
  - Project appears with build items and material rows.
  - Project item estimates show station, material cost, invention/job cost, revenue, profit, ROI, price source, and status when prices are available.
  - Project item status uses copy-only/BPC suffix for copy-only items.
- Recalculate the project.
- Success signs:
  - Costs/status refresh without clearing the project unexpectedly.
  - No crash if some prices are missing.

## 10. Build/Buy/Auto Decisions
- In Current Projects, pick a material that has a buildable blueprint.
- Toggle Build, Buy, and Auto decisions using the available project/material actions.
- Success signs:
  - Build mode creates or keeps child project rows where expected.
  - Buy mode stops child build expansion and moves requirement into buy/material list.
  - Auto clears the explicit decision.
  - Recalculate keeps the selected decision behavior.

## 11. Material Leftovers And Stock Editing
- In Current Projects, edit the owned/bought quantity for a material row.
- Switch pages, then return to Current Projects.
- Success signs:
  - Owned/bought quantity persists.
  - Remaining-to-buy updates.
  - Stage/wave summary updates.
- Mark a wave complete if available.
- Success signs:
  - Item completion and remaining quantity update.
  - No unrelated rows are deleted.

## 12. Persistence After Restart
- Create or update:
  - queue rows;
  - a project;
  - project material stock;
  - facility preset/selection where supported;
  - filter settings.
- Close the app.
- Confirm no hidden workspace `OurIPH.exe` remains.
- Reopen the app.
- Success signs:
  - Saved queue/project/settings reload.
  - No duplicate output folders or locked `bin/Debug/OurIPH.exe`.
  - Database status is still connected.

## 13. Layout And Usability
- Resize to about 1280px wide.
- Resize to a wide monitor.
- Use horizontal scrolling in Blueprint, Analysis, Build Queue, and Project tables.
- Success signs:
  - Product/blueprint identity columns stay readable.
  - Product icons are not clipped.
  - Lower Component/Raw/Skills panels are usable and can be resized with the splitter.
  - No text visibly overlaps nearby controls.

## 14. Notes To Capture During Manual Run
- Record exact scenario, item name, station, runs/ME/TE, selected filters, and whether prices came from cache/live/manual contracts.
- For calculation mismatches, separate:
  - confirmed parity fields from `LEGACY_PARITY_MAP.md`;
  - provisional final capital-chain/ore-plan fields;
  - live price changes.
- Attach screenshots for layout overlap, clipped icons, or confusing status text.
