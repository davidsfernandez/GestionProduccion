# Implementation Plan: fix_product_visual_fields_20260328

## Phase 1: Preparation & Diagnosis
- [ ] Task: Create work branch `fix/product-visual-fields` from `main`
- [ ] Task: Inspect `Migrations/20260325121917_AddProductVisualFields.cs` to confirm exact column definitions (type, length, nullability)
- [ ] Task: Conductor - User Manual Verification 'Preparation & Diagnosis' (Protocol in workflow.md)

## Phase 2: Automatic Repair Implementation
- [ ] Task: Implement TDD failing test for missing visual columns
    - [ ] Create `GestionProduccion.Tests/Integration/ProductRepairTests.cs`
    - [ ] Write test that verifies `AvailableColors` existence in database after initialization
- [ ] Task: Update `Program.cs` Critical Integrity Repairs (Green Phase)
    - [ ] Add `EnsureColumn("Products", "AvailableColors", "VARCHAR(200) NULL")`
    - [ ] Add `EnsureColumn("Products", "AvailableSizes", "VARCHAR(200) NULL")`
    - [ ] Add `EnsureColumn("Products", "ImageUrl", "VARCHAR(500) NULL")`
- [ ] Task: Verify with Integration Tests
    - [ ] Run `dotnet test` and confirm the new tests pass
    - [ ] Ensure `SKILL.md` compliance (professional English comments)
- [ ] Task: Conductor - User Manual Verification 'Automatic Repair Implementation' (Protocol in workflow.md)

## Phase 3: Docker Reset & Finalization
- [ ] Task: Execute Hard Reset Verification
    - [ ] `docker compose down -v`
    - [ ] `docker compose build --no-cache && docker compose up -d`
- [ ] Task: Merge into `main` and archive track
- [ ] Task: Conductor - User Manual Verification 'Docker Reset & Finalization' (Protocol in workflow.md)
