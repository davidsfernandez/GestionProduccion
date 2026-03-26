# Implementation Plan: db_fix_20260325

## Phase 1: Research and Diagnosis
- [x] Task: Analyze migration history and schema state
    - [x] List all migrations in the project
    - [x] Inspect the `20260304210025_AddProductionOrderOutput.cs` migration (likely source of FK issue)
    - [x] Check `ProductionOrder.cs` model for `CustomerUserId`
- [x] Task: Compare EF Core models against current DB schema to find other missing columns
    - [x] Run a schema comparison (e.g., using `dotnet ef migrations script` or manual inspection)
- [x] Task: Conductor - User Manual Verification 'Research and Diagnosis' (Protocol in workflow.md)

## Phase 2: Database Schema Fix
- [x] Task: Create failing test that attempts to access `CustomerUserId` on `ProductionOrders`
    - [x] Create `ProductionOrderSchemaTests.cs`
    - [x] Write test attempting to query by `CustomerUserId`
- [x] Task: Fix Migration / Create new migration to reconcile schema
    - [x] Handle the missing FK drop safely (e.g., using a raw SQL check in migration if needed, or fixing the migration order)
    - [x] Add `CustomerUserId` to `ProductionOrder` entity and generate migration
- [x] Task: Run migration locally and verify success
    - [x] Run `dotnet ef database update`
    - [x] Verify test passes
- [x] Task: Conductor - User Manual Verification 'Database Schema Fix' (Protocol in workflow.md)

## Phase 3: Docker Verification
- [x] Task: Verify fix in Docker environment
    - [x] Rebuild Docker containers: `docker compose build --no-cache`
    - [x] Start system: `docker compose up -d`
    - [x] Monitor logs for `SYSTEM: Synchronizing database schema...` success
- [x] Task: Final Targeted Verification
    - [x] Check `db` container for column existence: `DESCRIBE ProductionOrders;`
- [x] Task: Conductor - User Manual Verification 'Docker Verification' (Protocol in workflow.md)
