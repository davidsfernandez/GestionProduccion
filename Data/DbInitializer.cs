/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Checking if database needs seeding...");

            // 1. Ensure any default configuration exists
            if (!await context.SystemConfigurations.AnyAsync())
            {
                logger.LogInformation("Seeding default system configuration...");
                await context.SystemConfigurations.AddAsync(new SystemConfiguration
                {
                    Key = "MainConfig",
                    CompanyName = "Minha FÃ¡brica",
                    CompanyTaxId = "00.000.000/0001-00",
                    LogoBase64 = "",
                    DailyFixedCost = 500.00m,
                    OperationalHourlyCost = 45.00m
                });
            }

            // 2. Ensure default Bonus Rules exist (Fase 2: 105)
            if (!await context.BonusRules.AnyAsync())
            {
                logger.LogInformation("Seeding default bonus rules...");
                await context.BonusRules.AddAsync(new BonusRule
                {
                    Name = "Standard Production Bonus",
                    ProductivityPercentage = 95.0,
                    DeadlineBonusPercentage = 2.0m,
                    DefectLimitPercentage = 2.0m,
                    DelayPenaltyPercentage = 5.0m,
                    BonusAmount = 150.00m,
                    IsActive = true,
                    IsAtomicMode = true, // Enabled by default per Igor's new policy
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            
            // 3. Backfill ProductionOrderOutputs for existing orders (Transparent Migration)
            await BackfillProductionOutputsAsync(context, logger);

            // 4. Backfill Product and ProductionOrder bonuses
            await BackfillBonusesAsync(context, logger);

            logger.LogInformation("Database seeding and data migration completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private static async Task BackfillProductionOutputsAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("DATA MIGRATION: Checking for orders that need production output backfilling...");

        // Safety check: verify column existence before querying to prevent crash if migrations are partially applied.
        // We only perform this check on relational providers (MySQL, SQLite, etc.)
        bool columnExists = true;
        if (context.Database.IsRelational())
        {
            try 
            {
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'ProductionOrders' AND COLUMN_NAME = 'CustomerUserId' AND TABLE_SCHEMA = DATABASE()";
                columnExists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Schema verification failed: {Msg}. Proceeding with caution.", ex.Message);
                columnExists = true; 
            }
        }

        if (!columnExists)
        {
            logger.LogWarning("DATA MIGRATION: Skipping backfill as CustomerUserId column is not yet present in the database.");
            return;
        }

        // Get all orders that don't have ANY output records yet
        var ordersNeedingBackfill = await context.ProductionOrders
            .Include(o => o.Sizes)
            .Where(o => !context.ProductionOrderOutputs.Any(poo => poo.ProductionOrderId == o.Id))
            .ToListAsync();

        if (!ordersNeedingBackfill.Any())
        {
            logger.LogInformation("DATA MIGRATION: No orders need backfilling.");
            return;
        }

        logger.LogInformation("DATA MIGRATION: Found {Count} orders to migrate. Reconstructing history...", ordersNeedingBackfill.Count);

        var stages = Enum.GetValues<ProductionStage>();
        int totalCreated = 0;

        foreach (var order in ordersNeedingBackfill)
        {
            // Determine which stages this order has already "passed" or is "currently in"
            // For backfilling, we consider completed stages
            foreach (var stage in stages)
            {
                bool stageCompleted = false;

                if (order.CurrentStatus == ProductionStatus.Completed)
                {
                    stageCompleted = true; // All stages are done
                }
                else if ((int)order.CurrentStage > (int)stage)
                {
                    stageCompleted = true; // Order has moved past this stage
                }

                if (stageCompleted)
                {
                    foreach (var size in order.Sizes)
                    {
                        var output = new ProductionOrderOutput
                        {
                            ProductionOrderId = order.Id,
                            ProductionOrderSizeId = size.Id,
                            Stage = stage,
                            Quantity = size.Quantity,
                            UserId = order.UserId ?? 1, // Fallback to system admin if no user assigned
                            CreatedAt = order.CompletedAt ?? order.CreatedAt,
                            Note = "Auto-generated during system update v1.2"
                        };
                        await context.ProductionOrderOutputs.AddAsync(output);
                        totalCreated++;
                    }
                }
            }
        }

        if (totalCreated > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("DATA MIGRATION: Successfully created {Count} production output records. History restored.", totalCreated);
        }
    }

    private static async Task BackfillBonusesAsync(AppDbContext context, ILogger logger)
    {
        // Safety check: verify column existence before querying to prevent crash in out-of-sync schemas (e.g. Docker)
        bool visualColumnsExist = true;
        if (context.Database.IsRelational())
        {
            try 
            {
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                
                // We check for one of the missing columns as a proxy for the entire migration set
                cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'AvailableColors' AND TABLE_SCHEMA = DATABASE()";
                visualColumnsExist = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Product schema verification failed: {Msg}. Proceeding with caution.", ex.Message);
                visualColumnsExist = true; 
            }
        }

        if (!visualColumnsExist)
        {
            logger.LogWarning("DATA MIGRATION: Skipping bonus backfill as product visual columns are not yet present in the database.");
            return;
        }

        // Set default bonus for products that have 0 (Legacy products)
        var products = await context.Products.Where(p => p.DefaultBonusPerPiece == 0).ToListAsync();
        if (products.Any())
        {
            foreach (var p in products) p.DefaultBonusPerPiece = 1.50m;
            await context.SaveChangesAsync();
            logger.LogInformation("DATA MIGRATION: Set default bonus (1.50) for {Count} legacy products.", products.Count);
        }

        // Set applied bonus for existing orders based on their product (The Snapshot)
        var orders = await context.ProductionOrders.Where(o => o.AppliedBonusPerPiece == 0).ToListAsync();
        if (orders.Any())
        {
            foreach (var o in orders)
            {
                var product = await context.Products.FindAsync(o.ProductId);
                o.AppliedBonusPerPiece = product?.DefaultBonusPerPiece ?? 1.50m;
            }
            await context.SaveChangesAsync();
            logger.LogInformation("DATA MIGRATION: Snapshotted applied bonus for {Count} existing orders.", orders.Count);
        }
    }
}


