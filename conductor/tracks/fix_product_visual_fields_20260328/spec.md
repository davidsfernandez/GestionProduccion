# Specification: fix_product_visual_fields_20260328

## Overview
This track addresses a `MySqlException: Unknown column 'p.AvailableColors'` occurring during application runtime in the Docker environment. While the seeding crash was previously mitigated, runtime queries to the `Products` table still fail because visual fields (`AvailableColors`, `AvailableSizes`, `ImageUrl`) are missing from the physical database schema despite being present in the EF Core models.

## Functional Requirements
- **Automatic Schema Repair:** Add `AvailableColors`, `AvailableSizes`, and `ImageUrl` to the `Critical Integrity Repairs` block in `Program.cs`. This ensures the columns are created via direct SQL if EF Core migrations are inconsistent.
- **EF Core Audit:** Investigate the `__EFMigrationsHistory` table in Docker to understand why the `AddProductVisualFields` migration is considered "applied" when the DDL changes are missing.
- **Docker Stabilization:** Perform a hard reset of the Docker environment (volumes and images) to verify the fix in a clean state.

## Non-Functional Requirements
- **Language Standards:** All code, comments, and commit messages must be in professional English per `SKILL.md`.
- **Git Protocol:** Work in a dedicated branch `fix/product-visual-fields` and merge to `main`.
- **Resiliency:** The repair block should be idempotent (safe to run multiple times).

## Acceptance Criteria
- [ ] Application starts and user can login without `Unknown column 'p.AvailableColors'` crash.
- [ ] Manual verification via `DESCRIBE Products;` in the `db` container shows all three columns exist.
- [ ] Technical explanation provided for the migration desync root cause.

## Out of Scope
- Adding new features to the products module.
- Modifying the frontend UI for these fields.
