/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.ProductionOrders;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class ProductionOrderServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<INotificationService> _mockNotification;
    private readonly Mock<IDistributedLockService> _mockLock;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IFinancialCalculatorService> _mockFinancialCalc;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ISystemConfigurationService> _mockConfigService;

    private readonly ProductionOrderQueryService _queryService;
    private readonly ProductionOrderMutationService _mutationService;
    private readonly ProductionOrderLifecycleService _lifecycleService;

    public ProductionOrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockNotification = new Mock<INotificationService>();
        _mockLock = new Mock<IDistributedLockService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockFinancialCalc = new Mock<IFinancialCalculatorService>();
        _mockProductService = new Mock<IProductService>();
        _mockTaskService = new Mock<ITaskService>();
        _mockConfigService = new Mock<ISystemConfigurationService>();

        _mockLock.Setup(l => l.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        var orderRepo = new ProductionOrderRepository(_context);
        var userRepo = new UserRepository(_context);
        var outputRepo = new ProductionOrderOutputRepository(_context);
        var mapper = new MainMapper();

        _mockProductRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        _queryService = new ProductionOrderQueryService(orderRepo, userRepo, _mockHttpContextAccessor.Object, outputRepo, _mockConfigService.Object, mapper);

        _mutationService = new ProductionOrderMutationService(
            orderRepo,
            userRepo,
            _mockProductRepo.Object,
            _mockNotification.Object,
            _mockHttpContextAccessor.Object,
            _mockLock.Object,
            _mockFinancialCalc.Object,
            mapper);

        _lifecycleService = new ProductionOrderLifecycleService(
            orderRepo,
            userRepo,
            _mockProductRepo.Object,
            outputRepo,
            _mockNotification.Object,
            _mockHttpContextAccessor.Object,
            _mockFinancialCalc.Object,
            _mockProductService.Object,
            _mockTaskService.Object,
            mapper);
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
        var product = new Product { Id = 1, Name = "Test Product", InternalCode = "P001", FabricType = "Cotton", MainSku = "SKU001", AverageProductionTimeMinutes = 60, EstimatedSalePrice = 100 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

        var request = new CreateProductionOrderRequest
        {
            ProductId = 1,
            Quantity = 100,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(7),
            Size = "M"
        };

        var result = await _mutationService.CreateProductionOrderAsync(request, 1);

        Assert.NotNull(result);
        Assert.StartsWith("OP-", result.LotCode);
    }

    [Fact]
    public async Task AdvanceStageAsync_ShouldChangeStage_FromCuttingToSewing()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);

        var user = new User { Id = 1, FullName = "Tester", Email = "test@test.com", Role = UserRole.Operational, IsActive = true };
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var order = new ProductionOrder
        {
            Id = 1,
            LotCode = "OP-ADV-001",
            ProductId = 1,
            Quantity = 50,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = 1 
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _lifecycleService.AdvanceStageAsync(order.Id, 1);

        // Assert
        Assert.NotNull(result);
        var updatedOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(ProductionStage.Sewing, updatedOrder!.CurrentStage);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCorrectMetrics()
    {
        var orders = new List<ProductionOrder>
        {
            new() { Id = 101, LotCode = "OP-DB-1", Quantity = 100, CurrentStage = ProductionStage.Cutting, CurrentStatus = ProductionStatus.Completed, CreatedAt = DateTime.UtcNow, EstimatedCompletionAt = DateTime.UtcNow.AddDays(1), CompletedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" } },
            new() { Id = 102, LotCode = "OP-DB-2", Quantity = 50, CurrentStage = ProductionStage.Sewing, CurrentStatus = ProductionStatus.InProduction, CreatedAt = DateTime.UtcNow, EstimatedCompletionAt = DateTime.UtcNow.AddDays(2), UpdatedAt = DateTime.UtcNow, Product = new Product { Id = 2, Name = "P2", InternalCode = "C2", FabricType = "F2", MainSku = "S2" } }
        };

        _context.ProductionOrders.AddRange(orders);
        await _context.SaveChangesAsync();

        var result = await _queryService.GetDashboardAsync();

        Assert.NotNull(result);
        Assert.Equal(50.0m, result.CompletionRate);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnSumOfQuantitiesForCompletedToday()
    {
        // Arrange
        var brazilToday = DateTime.UtcNow.AddHours(-3).Date;
        var startUtc = brazilToday.AddHours(3);
        var yesterday = startUtc.AddDays(-1);

        var orders = new List<ProductionOrder>
        {
            new() { 
                Id = 201, LotCode = "OP-SUM-1", Quantity = 100, 
                CurrentStatus = ProductionStatus.Completed, 
                CompletedAt = startUtc.AddHours(2), 
                CreatedAt = yesterday, UpdatedAt = startUtc 
            },
            new() { 
                Id = 202, LotCode = "OP-SUM-2", Quantity = 50, 
                CurrentStatus = ProductionStatus.Completed, 
                CompletedAt = startUtc.AddHours(5), 
                CreatedAt = yesterday, UpdatedAt = startUtc 
            }
        };
        _context.ProductionOrders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queryService.GetDashboardAsync();

        // Assert
        Assert.Equal(150, result.CompletedToday);
    }

    [Fact]
    public async Task GetTvDashboardAsync_ShouldIgnorePackagingOutputs_ForReopenedOrders()
    {
        var today = DateTime.UtcNow.Date;

        var order = new ProductionOrder
        {
            Id = 203,
            LotCode = "OP-TV-REOPEN",
            Quantity = 40,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.InProduction,
            CreatedAt = today.AddDays(-1),
            UpdatedAt = today,
            Product = new Product { Id = 3, Name = "P3", InternalCode = "C3", FabricType = "F3", MainSku = "S3" }
        };

        _context.ProductionOrders.Add(order);
        _context.ProductionOrderOutputs.Add(new ProductionOrderOutput
        {
            ProductionOrderId = order.Id,
            Stage = ProductionStage.Packaging,
            Quantity = 40,
            CreatedAt = today.AddHours(4)
        });
        await _context.SaveChangesAsync();

        var result = await _queryService.GetTvDashboardAsync();

        Assert.Equal(0, result.CompletedToday);
        Assert.Contains(result.ProductionItems, x => x.LotCode == order.LotCode && x.Status == ProductionStatus.InProduction.ToString());
    }

    [Fact]
    public async Task RegisterPartialOutputAsync_ShouldAttributeToOrderUser_WhenAssigned()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);

        var order = new ProductionOrder
        {
            Id = 301,
            LotCode = "OP-ATTR-001",
            ProductId = 1,
            Quantity = 10,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.InProduction,
            UserId = 99 // Assigned to user 99
        };
        order.Sizes.Add(new ProductionOrderSize { Id = 1, Size = "M", Quantity = 10 });
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var sizeOutputs = new Dictionary<int, int> { { 1, 5 } };

        // Act
        // User 1 (Admin/Manager) records the output
        await _lifecycleService.RegisterPartialOutputAsync(order.Id, sizeOutputs, 1);

        // Assert
        var outputs = await _context.ProductionOrderOutputs.Where(o => o.ProductionOrderId == order.Id).ToListAsync();
        Assert.Single(outputs);
        Assert.Equal(99, outputs[0].UserId); // Should be attributed to 99, not 1
    }

    [Fact]
    public async Task RegisterPartialOutputAsync_ShouldAttributeToModifiedByUser_WhenUnassigned()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);

        var order = new ProductionOrder
        {
            Id = 302,
            LotCode = "OP-ATTR-002",
            ProductId = 1,
            Quantity = 10,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.InProduction,
            UserId = null // Unassigned
        };
        order.Sizes.Add(new ProductionOrderSize { Id = 2, Size = "M", Quantity = 10 });
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var sizeOutputs = new Dictionary<int, int> { { 2, 5 } };

        // Act
        // User 1 records the output
        await _lifecycleService.RegisterPartialOutputAsync(order.Id, sizeOutputs, 1);

        // Assert
        var outputs = await _context.ProductionOrderOutputs.Where(o => o.ProductionOrderId == order.Id).ToListAsync();
        Assert.Single(outputs);
        Assert.Equal(1, outputs[0].UserId); // Should be attributed to 1
    }

    [Fact]
    public async Task AdvanceStageAsync_ShouldAttributeRemainingToOrderUser()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);

        var order = new ProductionOrder
        {
            Id = 401,
            LotCode = "OP-ATTR-003",
            ProductId = 1,
            Quantity = 10,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            UserId = 88 // Assigned to user 88
        };
        order.Sizes.Add(new ProductionOrderSize { Id = 3, Size = "M", Quantity = 10 });
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        // Advance from Cutting to Sewing. This should trigger RecordRemainingOutputsAsync for Cutting.
        await _lifecycleService.AdvanceStageAsync(order.Id, 1);

        // Assert
        var outputs = await _context.ProductionOrderOutputs.Where(o => o.ProductionOrderId == order.Id && o.Stage == ProductionStage.Cutting).ToListAsync();
        Assert.Single(outputs);
        Assert.Equal(88, outputs[0].UserId); // Should be attributed to 88
    }

    [Fact]
    public async Task BulkAdvanceStageAsync_ShouldAdvanceMultipleOrders()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);
        _context.Users.Add(new User { Id = 1, FullName = "Tester", Email = "test@test.com", Role = UserRole.Administrator, IsActive = true });
        
        var orders = new List<ProductionOrder>
        {
            new() { LotCode = "OP-B-1", Quantity = 10, CurrentStage = ProductionStage.Cutting, CurrentStatus = ProductionStatus.InProduction, ProductId = 1 },
            new() { LotCode = "OP-B-2", Quantity = 10, CurrentStage = ProductionStage.Sewing, CurrentStatus = ProductionStatus.InProduction, ProductId = 1 }
        };
        _context.ProductionOrders.AddRange(orders);
        await _context.SaveChangesAsync();

        var id1 = orders[0].Id;
        var id2 = orders[1].Id;

        // Act
        var result = await _lifecycleService.BulkAdvanceStageAsync(new List<int> { id1, id2 }, 1);

        // Assert
        Assert.Equal(2, result.SuccessCount);
        var o1 = await _context.ProductionOrders.FindAsync(id1);
        var o2 = await _context.ProductionOrders.FindAsync(id2);
        Assert.Equal(ProductionStage.Sewing, o1!.CurrentStage);
        Assert.Equal(ProductionStage.Review, o2!.CurrentStage);
    }

    [Fact]
    public async Task BulkChangeStageAsync_ShouldUpdateMultipleOrders()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", FabricType = "F1", MainSku = "S1" };
        _context.Products.Add(product);
        _context.Users.Add(new User { Id = 1, FullName = "Tester", Email = "test@test.com", Role = UserRole.Administrator, IsActive = true });

        var orders = new List<ProductionOrder>
        {
            new() { LotCode = "OP-C-1", Quantity = 10, CurrentStage = ProductionStage.Cutting, CurrentStatus = ProductionStatus.InProduction, ProductId = 1 },
            new() { LotCode = "OP-C-2", Quantity = 10, CurrentStage = ProductionStage.Cutting, CurrentStatus = ProductionStatus.InProduction, ProductId = 1 }
        };
        _context.ProductionOrders.AddRange(orders);
        await _context.SaveChangesAsync();

        var id1 = orders[0].Id;
        var id2 = orders[1].Id;

        // Act
        var result = await _lifecycleService.BulkChangeStageAsync(new List<int> { id1, id2 }, ProductionStage.Packaging, "Bulk manual", 1);

        // Assert
        Assert.Equal(2, result.SuccessCount);
        var o1 = await _context.ProductionOrders.FindAsync(id1);
        var o2 = await _context.ProductionOrders.FindAsync(id2);
        Assert.Equal(ProductionStage.Packaging, o1!.CurrentStage);
        Assert.Equal(ProductionStage.Packaging, o2!.CurrentStage);
    }
}
