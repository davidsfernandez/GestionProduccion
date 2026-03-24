# Implementation Plan: Fix Blazor WASM icudt_no_CJK.dat 404/SRI failure

## Phase 1: Investigation and Root Cause Analysis
- [x] Task: Audit `GestionProduccion.csproj` and `GestionProduccion.Client.csproj` for globalization and Blazor configuration.
    - [ ] Check for `InvariantGlobalization` or `BlazorWebAssemblyLoadAllGlobalizationData`.
    - [ ] Check if the project uses specific cultures that require full ICU data.
- [x] Task: Inspect the local `wwwroot/_framework/` content after a local publish.
    - [ ] Run `dotnet publish -c Release` and check if `icudt_no_CJK.dat` is generated.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Investigation' (Protocol in workflow.md)

## Phase 2: Implementation and Fix
- [x] Task: Adjust globalization settings to ensure the resource is either not required or correctly served.
    - [x] If using specific cultures, ensure `BlazorWebAssemblyLoadAllGlobalizationData` is set to `true` or correctly configured.
    - [x] Alternatively, set `InvariantGlobalization` to `true` if culture-specific logic is not needed (unlikely for this project).
- [x] Task: Configure the server/container to correctly handle `.dat` files if missing.
- [x] Task: Conductor - User Manual Verification 'Phase 2: Implementation' (Protocol in workflow.md)

## Phase: Review Fixes
- [x] Task: Apply review suggestions 2ec88f9