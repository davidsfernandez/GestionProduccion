using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
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

        // Repositories
        var orderRepo = new ProductionOrderRepository(_context);
        var userRepo = new UserRepository(_context);

        _service = new ProductionOrderService(orderRepo, userRepo, _mockHubContext.Object, _mockHttpContextAccessor.Object);
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
        var user = new User { Id = 10, Name = "Operator 1", Email = "op1@test.com", Role = UserRole.Operator, IsActive = true };
        
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

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCorrectMetrics()
    {
        // Arrange
        var orders = new List<ProductionOrder>
        {
            new() { Id = 101, UniqueCode = "OP-DB-1", Quantity = 100, CurrentStage = ProductionStage.Cutting, CurrentStatus = ProductionStatus.Completed, CreationDate = DateTime.UtcNow, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1) },
            new() { Id = 102, UniqueCode = "OP-DB-2", Quantity = 50, CurrentStage = ProductionStage.Sewing, CurrentStatus = ProductionStatus.InProduction, CreationDate = DateTime.UtcNow, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(2) },
            new() { Id = 103, UniqueCode = "OP-DB-3", Quantity = 50, CurrentStage = ProductionStage.Sewing, CurrentStatus = ProductionStatus.InProduction, CreationDate = DateTime.UtcNow, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3) },
            new() { Id = 104, UniqueCode = "OP-DB-4", Quantity = 200, CurrentStage = ProductionStage.Packaging, CurrentStatus = ProductionStatus.Stopped, CreationDate = DateTime.UtcNow, EstimatedDeliveryDate = DateTime.UtcNow.AddDays(4) }
        };
        
        // Add mocked history for volume calculation (Last 7 days)
        var history = new ProductionHistory 
        { 
            ProductionOrderId = 101, 
            NewStatus = ProductionStatus.Completed, 
            ModificationDate = DateTime.UtcNow.AddDays(-1),
            UserId = 1
        };

        _context.ProductionOrders.AddRange(orders);
        _context.ProductionHistories.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert
        Assert.NotNull(result);
        // Note: With current simplistic Service-side dashboard implementation (due to repository pattern limit),
        // filtering complex stuff without dedicated repository methods might return slightly different results or work fine if logic was preserved.
        // We preserved the logic using Queryable in service.
        
        Assert.Equal(25.0m, result.CompletionRate); // 1 out of 4 is completed
        
        Assert.Contains("Sewing", result.OrdersByStage.Keys);
        Assert.Equal(2, result.OrdersByStage["Sewing"]); // OP-DB-2 and OP-DB-3
        
        Assert.Single(result.StoppedOperations); // OP-DB-4
        Assert.Equal("OP-DB-4", result.StoppedOperations[0].UniqueCode);
    }

    [Fact]
    public async Task AssignTaskAsync_ShouldLogHistoryWithCorrectUserId()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 5,
            UniqueCode = "OP-LOG-001",
            ProductDescription = "Log Test",
            Quantity = 10,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        var targetUser = new User { Id = 20, Name = "Operator 2", Role = UserRole.Operator, IsActive = true };
        
        _context.ProductionOrders.Add(order);
        _context.Users.Add(targetUser);
        await _context.SaveChangesAsync();

        // Mock HttpContext to return UserId = 99 (Admin/Manager who assigns)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Name, "Manager User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        // Act
        await _service.AssignTaskAsync(order.Id, targetUser.Id);

        // Assert
        var histories = await _context.ProductionHistories
            .Where(h => h.ProductionOrderId == order.Id)
            .ToListAsync();

        var history = histories.FirstOrDefault(h => h.Note.Contains($"Atribuído a {targetUser.Name}"));
            
        if (history == null)
        {
            var actualNotes = string.Join(", ", histories.Select(h => $"'{h.Note}'"));
            Assert.True(false, $"Expected note containing 'Atribuído a {targetUser.Name}' not found. Actual notes: {actualNotes}");
        }

        Assert.NotNull(history);
        Assert.Equal(99, history.UserId);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus_WhenValidTransition()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = 6,
            UniqueCode = "OP-STATUS-002",
            ProductDescription = "Status Change Test",
            Quantity = 10,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        int modifierUserId = 55;

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, ProductionStatus.Stopped, "Machine broke", modifierUserId);

        // Assert
        Assert.True(result);
        
        var dbOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(ProductionStatus.Stopped, dbOrder.CurrentStatus);
        
        var history = await _context.ProductionHistories
            .FirstOrDefaultAsync(h => h.ProductionOrderId == order.Id && h.NewStatus == ProductionStatus.Stopped);
        Assert.NotNull(history);
        Assert.Equal(modifierUserId, history.UserId);
        Assert.Equal("Machine broke", history.Note);
    }
}
