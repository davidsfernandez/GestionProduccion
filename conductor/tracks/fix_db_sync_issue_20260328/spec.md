# Specification: fix_db_sync_issue_20260328

## Overview
This track addresses a critical failure during the Docker deployment where the application cannot start because it attempts to seed data into a schema that is missing the `CustomerUserId` column in the `ProductionOrders` table. Despite logs suggesting migrations are handled, the physical database remains out of sync with the EF Core model.

## Functional Requirements
- **Migration Enforcement:** Modify the application startup sequence to guarantee that `context.Database.MigrateAsync()` completes successfully before any data seeding or backfilling logic is invoked.
- **Dependency Investigation:** Identify why the `LinkProductionOrderToCustomer` migration is being skipped or failing silently in the Docker environment.
- **Coordination Fix:** Correct the logic in `Program.cs` and `DbInitializer.cs` to prevent the "Schema already updated via migrations" warning from bypassing necessary DDL changes.
- **Docker Validation:** Ensure the fix works in a clean `docker compose down -v` scenario.

## Non-Functional Requirements
- **Reliability:** Startup should be deterministic; seeding must never run on an incomplete schema.
- **Observability:** Improve logging during the migration phase to clearly state which migrations are being applied.

## Acceptance Criteria
- [ ] The `app` container starts and reaches the "Now listening on: http://[::]:8080" state without throwing `MySqlException`.
- [ ] Logs confirm that the `LinkProductionOrderToCustomer` migration has been applied.
- [ ] The `ProductionOrders` table in the `db` container contains the `CustomerUserId` column (verified via `DESCRIBE`).
- [ ] No manual SQL intervention is required after `docker compose up`.

## Out of Scope
- Adding new fields to other entities.
- Modifying the business logic of the backfilling process itself.
