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
                    CompanyName = "Minha Fábrica",
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

            // 3. Ensure Admin user exists or reset its password for testing
            var adminEmail = "admin@serona.com";
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            var hashedPw = BCrypt.Net.BCrypt.HashPassword("Admin123!");

            if (adminUser == null)
            {
                logger.LogInformation("Seeding default admin user...");
                await context.Users.AddAsync(new User
                {
                    FullName = "Administrador Serona",
                    Email = adminEmail,
                    PasswordHash = hashedPw,
                    Role = UserRole.Administrator,
                    IsActive = true,
                    AvatarUrl = "/img/avatars/avatar.jpg",
                    ExternalId = Guid.NewGuid()
                });
            }
            else
            {
                logger.LogInformation("Resetting admin user password for testing...");
                adminUser.PasswordHash = hashedPw;
                context.Users.Update(adminUser);
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
