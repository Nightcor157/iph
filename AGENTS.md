# AGENTS.md

## Mission
Build the new `OurIPH` WPF client into a working replacement for the old EVE IPH manufacturing workflow. Preserve EVE item names, region names, station names, and game terms in their original form.

## Persistent Memory
Before every large task, read these files from the project root:
- `OUR_IPH_PLAN.md` for the global roadmap and project intent.
- `IMPLEMENTATION_NOTES.md` for current implementation status and decisions.
- `TODO_NEXT.md` for immediate tasks, known issues, and technical debt.

After every meaningful implementation stage, update `IMPLEMENTATION_NOTES.md` and `TODO_NEXT.md` so work can continue after chat context compaction.

## Autonomous Work Rules
- Work autonomously until the task is genuinely complete or a true blocker appears.
- Do not stop after analysis or after one isolated step.
- Make code changes when needed, then build and fix compile errors.
- Prefer small, scoped changes that fit the existing `Models`, `Services`, `Stores`, and WPF structure.
- Do not rewrite the project from scratch.
- Port legacy VB.NET logic from `EVE-IPH-master` carefully and verify behavior against local data where possible.
- Do not revert unrelated user changes.

## Active Scope Rule
- Choose exactly one lane per pass: GUI bugfix/workflow stabilization, parity fixture, refactor/extraction, or docs cleanup.
- Do not mix lanes in the same pass unless a tiny documentation update is needed to record the completed work.
- If GUI/manual verification is required but interactive WPF access is unavailable, do not substitute unrelated refactor or parity work. Document the blocker and provide exact manual verification steps instead.
- Treat full `ConvertToOre.vb` LP numeric parity and full `CalculateBlueprintEstimate` extraction as separate explicit lanes, not default follow-up work.

## Build And Verification
Use Visual Studio MSBuild directly when needed:

Before running any `OurIPH` build, close running `OurIPH.exe` instances from this workspace output folders first. This avoids locked `bin/Debug/OurIPH.exe` files and prevents extra verification output folders from piling up. After smoke checks, make sure no hidden verification `OurIPH.exe` process is left running.

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "OurIPH\OurIPH.sln" /p:Configuration=Debug /p:Platform="Any CPU" /m /v:minimal
```

Legacy reference build command:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "EVE-IPH-master\EVE Isk per Hour.sln" /p:Configuration=Debug /p:Platform=x86 /m /v:minimal
```

If tests are added later, run them after each relevant change. If there are no tests, run build and a smoke check where possible.

## Definition Of Done
- `OurIPH` builds without errors.
- Manufacturing environment setup works and affects calculations.
- Price update works with cache and understandable UI status.
- Project build setup, material stock, and build/buy decisions persist.
- Contract pricing works for appropriate categories such as capital ships.
- Scam/anomalous contract filtering and rare/officer filtering are applied.
- Top variants are ranked with profit, SVR/liquidity, and filtering rules.
- `IMPLEMENTATION_NOTES.md` and `TODO_NEXT.md` describe what changed and what remains.
