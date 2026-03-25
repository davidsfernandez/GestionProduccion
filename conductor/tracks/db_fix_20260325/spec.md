# Specification: db_fix_20260325

## Overview
This track addresses database synchronization failures in the Docker environment. Currently, EF Core migration is failing when attempting to drop a foreign key (`FK_ProductionOrderOutputs_ProductionOrderSizes_ProductionOrderS~`) that does not exist in the database, and a missing column `p.CustomerUserId` is causing query failures.

## Functional Requirements
- **Root Cause Analysis:** Investigate why the expected foreign key is missing and why the migrations are out of sync.
- **Foreign Key Resolution:** Modify or fix the migration/schema to handle the missing foreign key during drop operations safely.
- **Column Addition:** Ensure the `CustomerUserId` column exists in the `ProductionOrders` table.
- **Docker Ready:** The system must successfully start with `docker compose up` without schema synchronization failures.
- **Full Schema Sync:** Verify that the database schema matches the EF Core models after the fix.

## Non-Functional Requirements
- **Data Integrity:** Ensure that the fix does not result in data loss in the existing tables.
- **Migration Stability:** Migrations should be idempotent where possible or at least handle common out-of-sync states.

## Acceptance Criteria
- [ ] No `MySqlException: Unknown column 'p.CustomerUserId'` in application logs.
- [ ] No `Can't DROP 'FK_...'; check that column/key exists` in EF Core migration logs.
- [ ] The application starts and reaches "Now listening on: http://[::]:8080".
- [ ] Manual verification that `ProductionOrders` table has `CustomerUserId`.

## Out of Scope
- Adding new features to the CRM or production orders.
- Optimizing database performance beyond schema fixes.
