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
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IFinancialCalculatorService> _mockFinancialCalc;

    private readonly ProductionOrderQueryService _queryService;
    private readonly ProductionOrderMutationService _mutationService;
    private readonly ProductionOrderLifecycleService _lifecycleService;

    public ProductionOrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockHubContext = new Mock<IHubContext<ProductionHub>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockFinancialCalc = new Mock<IFinancialCalculatorService>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);

        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        var orderRepo = new ProductionOrderRepository(_context);
        var userRepo = new UserRepository(_context);

        _mockProductRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        _queryService = new ProductionOrderQueryService(orderRepo, userRepo, _mockHttpContextAccessor.Object);

        _mutationService = new ProductionOrderMutationService(
            orderRepo,
            userRepo,
            _mockProductRepo.Object,
            _mockHubContext.Object,
            _mockHttpContextAccessor.Object,
            _mockFinancialCalc.Object);

        _lifecycleService = new ProductionOrderLifecycleService(
            orderRepo,
            userRepo,
            _mockProductRepo.Object,
            _mockHubContext.Object,
            _mockHttpContextAccessor.Object,
            _mockFinancialCalc.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateProductionOrderAsync_ShouldCreateOrder_WhenRequestIsValid()
    {
        var product = new Product { Id = 1, Name = "Test Product", InternalCode = "P001", FabricType = "Cotton", MainSku = "SKU001" };
        _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

        var request = new CreateProductionOrderRequest
        {
            ProductId = 1,
            Quantity = 100,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(7)
        };

        var result = await _mutationService.CreateProductionOrderAsync(request, 1);

        Assert.NotNull(result);
        Assert.StartsWith("OP-", result.LotCode);
    }

    [Fact]
    public async Task AdvanceStageAsync_ShouldChangeStage_FromCuttingToSewing()
    {
        var order = new ProductionOrder
        {
            Id = 1,
            LotCode = "OP-ADV-001",
            ProductId = 1,
            Quantity = 50,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _lifecycleService.AdvanceStageAsync(order.Id, 1);

        Assert.True(result);
        var updatedOrder = await _context.ProductionOrders.FindAsync(order.Id);
        Assert.Equal(ProductionStage.Sewing, updatedOrder.CurrentStage);
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
}
