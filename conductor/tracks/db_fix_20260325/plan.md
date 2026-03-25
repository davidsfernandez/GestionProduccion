# Implementation Plan: db_fix_20260325

## Phase 1: Research and Diagnosis
- [ ] Task: Analyze migration history and schema state
    - [ ] List all migrations in the project
    - [ ] Inspect the `20260304210025_AddProductionOrderOutput.cs` migration (likely source of FK issue)
    - [ ] Check `ProductionOrder.cs` model for `CustomerUserId`
- [ ] Task: Compare EF Core models against current DB schema to find other missing columns
    - [ ] Run a schema comparison (e.g., using `dotnet ef migrations script` or manual inspection)
- [ ] Task: Conductor - User Manual Verification 'Research and Diagnosis' (Protocol in workflow.md)

## Phase 2: Database Schema Fix
- [ ] Task: Create failing test that attempts to access `CustomerUserId` on `ProductionOrders`
    - [ ] Create `ProductionOrderSchemaTests.cs`
    - [ ] Write test attempting to query by `CustomerUserId`
- [ ] Task: Fix Migration / Create new migration to reconcile schema
    - [ ] Handle the missing FK drop safely (e.g., using a raw SQL check in migration if needed, or fixing the migration order)
    - [ ] Add `CustomerUserId` to `ProductionOrder` entity and generate migration
- [ ] Task: Run migration locally and verify success
    - [ ] Run `dotnet ef database update`
    - [ ] Verify test passes
- [ ] Task: Conductor - User Manual Verification 'Database Schema Fix' (Protocol in workflow.md)

## Phase 3: Docker Verification
- [ ] Task: Verify fix in Docker environment
    - [ ] Rebuild Docker containers: `docker compose build --no-cache`
    - [ ] Start system: `docker compose up -d`
    - [ ] Monitor logs for `SYSTEM: Synchronizing database schema...` success
- [ ] Task: Final Targeted Verification
    - [ ] Check `db` container for column existence: `DESCRIBE ProductionOrders;`
- [ ] Task: Conductor - User Manual Verification 'Docker Verification' (Protocol in workflow.md)
