using Moq;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GestionProduccion.Tests.Integration
{
    public class CustomerPortalIntegrationTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CustomerOrderService_ShouldOnlyReturnOwnedOrders()
        {
            // 1. Setup
            using var context = GetInMemoryDbContext();
            var customer1Id = 10;
            var customer2Id = 20;

            var product = new Product { Name = "Test Product", MainSku = "T1", InternalCode = "I1", FabricType = "Silk" };
            context.Products.Add(product);

            context.ProductionOrders.Add(new ProductionOrder { LotCode = "ORD1", CustomerUserId = customer1Id, Product = product, Quantity = 10 });
            context.ProductionOrders.Add(new ProductionOrder { LotCode = "ORD2", CustomerUserId = customer1Id, Product = product, Quantity = 5 });
            context.ProductionOrders.Add(new ProductionOrder { LotCode = "ORD3", CustomerUserId = customer2Id, Product = product, Quantity = 100 });
            await context.SaveChangesAsync();

            var service = new CustomerOrderService(context);

            // 2. Act
            var ordersC1 = await service.GetCustomerOrdersAsync(customer1Id);
            var ordersC2 = await service.GetCustomerOrdersAsync(customer2Id);

            // 3. Assert
            Assert.Equal(2, ordersC1.Count);
            Assert.Single(ordersC2);
            Assert.All(ordersC1, o => Assert.Equal(customer1Id, o.CustomerUserId));
        }
    }
}