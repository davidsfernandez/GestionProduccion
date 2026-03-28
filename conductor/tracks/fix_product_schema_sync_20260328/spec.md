# Specification: fix_product_schema_sync_20260328

## Overview
This track addresses a `MySqlException` occurring during database seeding in the Docker environment. The application fails with `Unknown column 'p.AvailableColors' in 'field list'` when executing `BackfillBonusesAsync`. This indicates that the `Products` table schema is out of sync with the EF Core models, specifically missing visual fields (`AvailableColors`, `AvailableSizes`, `ImageUrl`).

## Functional Requirements
- **Root Cause Analysis:** Investigate the `__EFMigrationsHistory` table and the `20260325121917_AddProductVisualFields.cs` migration file to determine why these fields are missing in the Docker database.
- **Seeder Resiliency:** Refactor `DbInitializer.BackfillBonusesAsync` to include a schema verification check (using `information_schema`) before querying fields that might not exist in an out-of-sync database.
- **Migration Validation:** Ensure that any pending migrations are logged clearly during the startup sequence to improve observability of the synchronization process.

## Non-Functional Requirements
- **Code Standards:** All new code, comments, and documentation must be in professional English per `SKILL.md`.
- **Git Hygiene:** Work must be performed in a dedicated branch and merged to `main` after verification.
- **Privacy:** Configuration files and `.md` files must not be staged for the remote repository.

## Acceptance Criteria
- [ ] Application starts in Docker without `MySqlException: Unknown column 'p.AvailableColors'`.
- [ ] `BackfillBonusesAsync` executes successfully or skips gracefully if columns are missing.
- [ ] Technical summary explaining why the migration was skipped in Docker.
- [ ] Database schema in the `db` container contains the expected `Products` columns.

## Out of Scope
- Adding these columns manually to the `Program.cs` repair block (user requested to rely on EF migrations for now).
- Modifying product business logic.
