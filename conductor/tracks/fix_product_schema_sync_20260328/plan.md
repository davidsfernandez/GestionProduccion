# Implementation Plan: fix_product_schema_sync_20260328

## Phase 1: Diagnosis & Branching
- [ ] Task: Create work branch `fix/product-schema-sync` from `main`
- [ ] Task: Analyze migration `20260325121917_AddProductVisualFields.cs` and snapshot state
- [ ] Task: Inspect `DbInitializer.cs` to identify all potentially problematic queries in `BackfillBonusesAsync`
- [ ] Task: Conductor - User Manual Verification 'Diagnosis & Branching' (Protocol in workflow.md)

## Phase 2: Implementation & TDD
- [ ] Task: Create Failing Test for Seeder Resiliency
    - [ ] Create `GestionProduccion.Tests/Integration/ProductSchemaResiliencyTests.cs`
    - [ ] Write a test that mocks a database missing visual columns and verifies `SeedAsync` skips them without crashing
- [ ] Task: Refactor `DbInitializer.BackfillBonusesAsync` (Green Phase)
    - [ ] Implement `context.Database.IsRelational()` and `information_schema` checks for `AvailableColors`, `AvailableSizes`, and `ImageUrl`
    - [ ] Ensure logic falls back gracefully if columns are not found
- [ ] Task: Verify Coverage and Quality
    - [ ] Run `dotnet test` to confirm the fix and ensure no regressions in schema synchronization
    - [ ] Perform self-review for `SKILL.md` compliance (English only, professional comments)
- [ ] Task: Conductor - User Manual Verification 'Implementation & TDD' (Protocol in workflow.md)

## Phase 3: Finalization & Sync
- [ ] Task: Merge `fix/product-schema-sync` into `main`
- [ ] Task: Update `tracks.md` and archive the track locally
- [ ] Task: Conductor - User Manual Verification 'Finalization & Sync' (Protocol in workflow.md)
