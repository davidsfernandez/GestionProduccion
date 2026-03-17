/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 */

using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.GestionProduccion.Domain.Interfaces; // For IQARepository if in sub-namespace
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GestionProduccion.Tests.Integration
{
    public class AtomicBonusIntegrationTests : BaseIntegrationTest
    {
        public AtomicBonusIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        private IServiceProvider GetTestServiceProvider(AppDbContext db)
        {
            var services = new ServiceCollection();
            
            // Repositories
            services.AddSingleton(db);
            services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
            services.AddScoped<IProductionOrderOutputRepository, ProductionOrderOutputRepository>();
            services.AddScoped<IBonusRuleRepository, BonusRuleRepository>();
            services.AddScoped<ISewingTeamRepository, SewingTeamRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            
            // Services
            services.AddScoped<BonusCalculationService>();
            services.AddScoped<QAService>(); // Required by Bonus Service
            
            // Mocks
            services.AddSingleton(new Mock<IQARepository>().Object);
            services.AddSingleton(new Mock<ILocalFileStorageService>().Object);
            services.AddSingleton(new Mock<ILogger<BonusCalculationService>>().Object);
            services.AddSingleton(new Mock<ILogger<QAService>>().Object);

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Should_Grant_Full_Bonus_When_100Percent_OnTime_NoDefects()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var testProvider = GetTestServiceProvider(db);
            var bonusService = testProvider.GetRequiredService<BonusCalculationService>();

            // Arrange
            var productId = await CreateTestProduct(db, 2.50m);
            var orderId = await CreateTestOrder(db, productId, 100, DateTime.UtcNow.AddDays(1), 2.50m);
            await CompleteOrder(db, orderId, 100, DateTime.UtcNow);

            // Act
            var report = await bonusService.CalculateUserBonusAsync(1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            // Assert
            Assert.True(report.FinalBonusPercentage > 0);
            Assert.Equal(250.00m, report.TotalAmount); 
        }

        [Fact]
        public async Task Should_Deny_Bonus_When_Lacks_Pieces()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var testProvider = GetTestServiceProvider(db);
            var bonusService = testProvider.GetRequiredService<BonusCalculationService>();

            // Arrange
            var productId = await CreateTestProduct(db, 2.50m);
            var orderId = await CreateTestOrder(db, productId, 100, DateTime.UtcNow.AddDays(1), 2.50m);
            await CompleteOrder(db, orderId, 99, DateTime.UtcNow); 

            // Act
            var report = await bonusService.CalculateUserBonusAsync(1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            // Assert
            Assert.Equal(0, report.FinalBonusPercentage);
            Assert.True(report.IsAtomicFailure);
            Assert.Equal("MISSING_PIECES", report.AtomicFailureReason);
        }

        [Fact]
        public async Task Should_Deny_Bonus_When_Late()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var testProvider = GetTestServiceProvider(db);
            var bonusService = testProvider.GetRequiredService<BonusCalculationService>();

            // Arrange
            var productId = await CreateTestProduct(db, 2.50m);
            var orderId = await CreateTestOrder(db, productId, 100, DateTime.UtcNow.AddDays(-1), 2.50m); 
            await CompleteOrder(db, orderId, 100, DateTime.UtcNow); 

            // Act
            var report = await bonusService.CalculateUserBonusAsync(1, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(1));

            // Assert
            Assert.Equal(0, report.FinalBonusPercentage);
            Assert.True(report.IsAtomicFailure);
            Assert.Equal("LATE_DELIVERY", report.AtomicFailureReason);
        }

        // Helpers
        private async Task<int> CreateTestProduct(AppDbContext db, decimal bonus)
        {
            var p = new Product { 
                Name = "Test", 
                InternalCode = "T1", 
                MainSku = "SKU-T1", 
                FabricType = "Test",
                DefaultBonusPerPiece = bonus 
            };
            db.Products.Add(p);
            await db.SaveChangesAsync();
            return p.Id;
        }

        private async Task<int> CreateTestOrder(AppDbContext db, int prodId, int qty, DateTime deadline, decimal bonus)
        {
            var o = new ProductionOrder { 
                ProductId = prodId, 
                Quantity = qty, 
                EstimatedCompletionAt = deadline, 
                AppliedBonusPerPiece = bonus,
                UserId = 1,
                CurrentStage = ProductionStage.Cutting,
                CurrentStatus = ProductionStatus.InProduction
            };
            db.ProductionOrders.Add(o);
            await db.SaveChangesAsync();
            return o.Id;
        }

        private async Task CompleteOrder(AppDbContext db, int orderId, int qty, DateTime finishedAt)
        {
            db.ProductionOrderOutputs.Add(new ProductionOrderOutput {
                ProductionOrderId = orderId,
                Quantity = qty,
                CreatedAt = finishedAt,
                UserId = 1,
                Stage = ProductionStage.Packaging
            });
            var o = await db.ProductionOrders.FindAsync(orderId);
            if (o != null) {
                o.CurrentStatus = ProductionStatus.Completed;
                o.CompletedAt = finishedAt;
            }
            await db.SaveChangesAsync();
        }
    }
}
