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
            if (!await context.SystemConfigurations.AnyAsync(sc => sc.Key == "CompanyLogo"))
            {
                logger.LogInformation("Seeding default system configuration...");
                await context.SystemConfigurations.AddAsync(new SystemConfiguration
                {
                    Key = "CompanyLogo",
                    Value = "",
                    Description = "Base64 Company Logo",
                    DailyFixedCost = 500.00m,
                    OperationalHourlyCost = 45.00m
                });
            }

            // 2. Ensure at least one Administrator exists (Fase 2: 102, 103, 104)
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Administrator))
            {
                logger.LogInformation("Seeding default administrator...");
                var admin = new User
                {
                    FullName = "System Administrator",
                    Email = "admin@local.host",
                    Role = UserRole.Administrator,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    IsActive = true,
                    AvatarUrl = "/img/avatars/avatar.jpg"
                };
                await context.Users.AddAsync(admin);
            }

            // 3. Ensure default Bonus Rules exist (Fase 2: 105)
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
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
