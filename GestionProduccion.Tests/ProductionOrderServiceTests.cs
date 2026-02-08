using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class ProductionOrderServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IHubContext<ProductionHub>> _mockHubContext;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly ProductionOrderService _service;

    public ProductionOrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockHubContext = new Mock<IHubContext<ProductionHub>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // Setup mock SignalR
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);

        _service = new ProductionOrderService(_context, _mockHubContext.Object, _mockHttpContextAccessor.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateProductionOrderAsync_ShouldCreateOrder_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            UniqueCode = "OP-TEST-001",
            ProductDescription = "Test Product",
            Quantity = 100,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7)
        };
        int userId = 1;

        // Act
        var result = await _service.CreateProductionOrderAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.UniqueCode, result.UniqueCode);
        Assert.Equal("Cutting", result.CurrentStage);
        Assert.Equal("InProduction", result.CurrentStatus);
        
        var orderInDb = await _context.ProductionOrders.FirstOrDefaultAsync(o => o.UniqueCode == "OP-TEST-001");
        Assert.NotNull(orderInDb);
        Assert.Equal(1, await _context.ProductionHistories.CountAsync());
    }

    [Fact]
    public async Task CreateProductionOrderAsync_ShouldThrowException_WhenQuantityIsZero()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            UniqueCode = "OP-TEST-002",
            ProductDescription = "Test Product",
            Quantity = 0,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateProductionOrderAsync(request, 1));
    }

    [Fact]
    public async Task AdvanceStageAsync_ShouldChangeStage_FromCuttingToSewing()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 1,
            UniqueCode = "OP-ADV-001",
            ProductDescription = "Advancing Test",
            Quantity = 50,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AdvanceStageAsync(order.Id, 1);

        // Assert
        Assert.True(result);
        var updatedOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(ProductionStage.Sewing, updatedOrder.CurrentStage);
    }

    [Fact]
    public async Task AdvanceStageAsync_ShouldThrowException_WhenInPackaging()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 2,
            UniqueCode = "OP-ADV-002",
            ProductDescription = "Final Stage Test",
            Quantity = 50,
            CurrentStage = ProductionStage.Packaging,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceStageAsync(order.Id, 1));
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldFail_WhenMarkingCompletedFromCutting()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 3,
            UniqueCode = "OP-STATUS-001",
            ProductDescription = "Status Test",
            Quantity = 50,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, ProductionStatus.Completed, "Done", 1);

        // Assert
        Assert.False(result); // Should fail because it's not in Packaging stage
        var dbOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(ProductionStatus.InProduction, dbOrder.CurrentStatus);
    }

    [Fact]
    public async Task AssignTaskAsync_ShouldUpdateUserId_WhenUserIsOperator()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 4,
            UniqueCode = "OP-ASSIGN-001",
            ProductDescription = "Assign Test",
            Quantity = 10,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        var user = new User { Id = 10, Name = "Operator 1", Email = "op1@test.com", Role = UserRole.Operator };
        
        _context.ProductionOrders.Add(order);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AssignTaskAsync(order.Id, user.Id);

        // Assert
        Assert.True(result);
        var updatedOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(user.Id, updatedOrder.UserId);
    }
}
