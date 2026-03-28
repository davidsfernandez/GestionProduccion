# Implementation Plan: fix_db_sync_issue_20260328

## Phase 1: Diagnosis & Fix Coordination
- [ ] Task: Investigate Startup Sequence
    - [ ] Read `Program.cs` to understand the current DB synchronization and seeding flow.
    - [ ] Read `DbInitializer.cs` to identify the "Critical Integrity Repairs" and "Backfilling" logic that is failing.
- [ ] Task: Create Failing Test
    - [ ] Create `GestionProduccion.Tests/Integration/SchemaSyncIntegrationTests.cs`.
    - [ ] Write a test that simulates the Docker startup sequence (Migrate -> Seed) and captures the `MySqlException`.
- [ ] Task: Conductor - User Manual Verification 'Diagnosis & Fix Coordination' (Protocol in workflow.md)

## Phase 2: Implementation & Code Fix
- [ ] Task: Refactor Startup Sequence in C#
    - [ ] Modify `Program.cs` to ensure `await context.Database.MigrateAsync()` is completed and flushed before any seeding logic.
    - [ ] Refactor `DbInitializer.SeedAsync` to include a pre-check for required columns (like `CustomerUserId`) before executing queries that depend on them.
- [ ] Task: Improve Migration Observability
    - [ ] Add logging to list pending migrations before applying them.
    - [ ] Ensure the "Schema already updated" warning doesn't trigger if there are actually pending DDL changes.
- [ ] Task: Verify with Red Phase Test
    - [ ] Run the new integration test and confirm it now passes.
- [ ] Task: Conductor - User Manual Verification 'Implementation & Code Fix' (Protocol in workflow.md)

## Phase 3: Docker Verification
- [ ] Task: Clean Rebuild and Deployment
    - [ ] Execute `docker compose down -v` to clear all state.
    - [ ] Execute `docker compose build --no-cache && docker compose up -d`.
- [ ] Task: Validation of Container Health
    - [ ] Monitor logs for successful "SYSTEM: ALL STABILIZATION TASKS COMPLETED".
    - [ ] Verify the existence of the column manually in the database container using `docker exec`.
- [ ] Task: Conductor - User Manual Verification 'Docker Verification' (Protocol in workflow.md)
