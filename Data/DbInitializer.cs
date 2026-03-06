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
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
            
            // 3. Backfill ProductionOrderOutputs for existing orders (Transparent Migration)
            await BackfillProductionOutputsAsync(context, logger);

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
}


