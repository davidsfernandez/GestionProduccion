# Implementation Plan: fix_crm_schema_integrity_20260328

## Phase 1: Branching & Technical Audit
- [ ] Task: Create work branch `fix/crm-schema-integrity` from `main`
- [ ] Task: Extract physical SQL definitions for `Leads`, `Quotes`, and `CustomerProfiles` from existing migrations
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Audit' (Protocol in workflow.md)

## Phase 2: Implementation of Schema Repairs & Logging
- [ ] Task: Enhance `Program.cs` Migration Logging
    - [ ] Log pending migrations list before execution
    - [ ] Add detailed exception logging for `MigrateAsync()`
- [ ] Task: Implement CRM Table Repairs in `Program.cs` (Green Phase)
    - [ ] Add `CREATE TABLE IF NOT EXISTS` logic for `Leads`
    - [ ] Add `CREATE TABLE IF NOT EXISTS` logic for `Quotes`
    - [ ] Add `CREATE TABLE IF NOT EXISTS` logic for `CustomerProfiles`
- [ ] Task: Create and Run Integrity Tests
    - [ ] Create `GestionProduccion.Tests/Integration/CRMSchemaIntegrityTests.cs`
    - [ ] Verify that tables are accessible via LINQ after initialization
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Repairs' (Protocol in workflow.md)

## Phase 3: Docker Validation & Sync
- [ ] Task: Hard Reset Docker Verification
    - [ ] `docker compose down -v`
    - [ ] `docker compose up -d`
    - [ ] Verify logs show no `Table doesn't exist` errors during customer registration
- [ ] Task: Merge `fix/crm-schema-integrity` into `main` and archive track
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Final' (Protocol in workflow.md)
