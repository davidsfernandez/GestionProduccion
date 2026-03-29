# Specification: fix_crm_schema_integrity_20260328

## Overview
This track addresses recurring database schema inconsistencies in the Docker environment where critical CRM tables (`Leads`, `Quotes`, `CustomerProfiles`) are missing despite being present in the EF Core models. It also includes an investigation into why EF Core migrations are failing to apply automatically in Docker.

## Functional Requirements
- **Immediate Schema Repair:** Implement a new repair block in `Program.cs` using raw SQL (`CREATE TABLE IF NOT EXISTS`) to ensure the physical existence of `Leads`, `Quotes`, and `CustomerProfiles` tables.
- **Migration Audit & Verbose Logging:** 
    - Modify `Program.cs` to list and log all pending migrations before calling `MigrateAsync()`.
    - Log any exceptions during the migration process with full details to diagnose why EF Core is stalling.
- **Schema Reconciliation:** Ensure that the manually created tables match the EF Core model definitions exactly (columns, types, foreign keys).

## Non-Functional Requirements
- **Resiliency:** The repair logic must be idempotent and safe to run on every application startup.
- **Standards:** All code, comments, and commit messages must be in professional English per `SKILL.md`.
- **Git Protocol:** Use branch `fix/crm-schema-integrity` and merge to `main` upon completion.

## Acceptance Criteria
- [ ] Application starts in Docker and user can complete a registration (creating a `CustomerProfile`) without `MySqlException`.
- [ ] Logs show exactly which migrations EF Core identifies as pending.
- [ ] Manual verification via `SHOW TABLES;` in the `db` container confirms `Leads`, `Quotes`, and `CustomerProfiles` exist.
- [ ] Technical root cause identified for the EF Core migration failures in Docker.

## Out of Scope
- Migrating existing data from legacy tables.
- Redesigning the CRM entity relationships.
