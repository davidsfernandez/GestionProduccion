using System.Security.Claims;
using GestionProduccion.Application.Mappers;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services.ProductionOrders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class ProductionOrderHotfixTests
{
    [Fact]
    public async Task ChangeStageAsync_ShouldReopenCompletedOrder_WhenMovedBackToOperationalStage()
    {
        var order = new ProductionOrder
        {
            Id = 501,
            LotCode = "OP-REOPEN-001",
            ProductId = 1,
            Quantity = 10,
            CurrentStage = ProductionStage.Packaging,
            CurrentStatus = ProductionStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow.AddHours(-3),
            CreatedAt = DateTime.UtcNow.AddHours(-5),
            UpdatedAt = DateTime.UtcNow
        };

        var orderRepo = new Mock<IProductionOrderRepository>();
        orderRepo.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var userRepo = new Mock<IUserRepository>();
        var productRepo = new Mock<IProductRepository>();
        var outputRepo = new Mock<IProductionOrderOutputRepository>();
        var notification = new Mock<INotificationService>();
        var httpContext = new Mock<IHttpContextAccessor>();
        var financial = new Mock<IFinancialCalculatorService>();
        var productService = new Mock<IProductService>();
        var taskService = new Mock<ITaskService>();

        var service = new ProductionOrderLifecycleService(
            orderRepo.Object,
            userRepo.Object,
            productRepo.Object,
            outputRepo.Object,
            notification.Object,
            httpContext.Object,
            financial.Object,
            productService.Object,
            taskService.Object,
            new MainMapper());

        var changed = await service.ChangeStageAsync(order.Id, ProductionStage.Sewing, "Reopen", 1);

        Assert.True(changed);
        Assert.Equal(ProductionStage.Sewing, order.CurrentStage);
        Assert.Equal(ProductionStatus.InProduction, order.CurrentStatus);
        Assert.Null(order.CompletedAt);
    }

    [Fact]
    public async Task UpdateProductionOrderAsync_ShouldRecalculateFinancials_WhenDatesChange()
    {
        var order = new ProductionOrder
        {
            Id = 601,
            LotCode = "OP-DATE-001",
            ProductId = 1,
            Quantity = 20,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddHours(-8),
            UpdatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow.AddHours(-5),
            CompletedAt = DateTime.UtcNow.AddHours(-1),
            AppliedBonusPerPiece = 2m
        };

        var orderRepo = new Mock<IProductionOrderRepository>();
        orderRepo.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var userRepo = new Mock<IUserRepository>();
        var productRepo = new Mock<IProductRepository>();
        var notification = new Mock<INotificationService>();
        var httpContext = new Mock<IHttpContextAccessor>();
        var lockService = new Mock<IDistributedLockService>();
        var financial = new Mock<IFinancialCalculatorService>();

        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContext.Setup(x => x.HttpContext).Returns(context);

        var service = new ProductionOrderMutationService(
            orderRepo.Object,
            userRepo.Object,
            productRepo.Object,
            notification.Object,
            httpContext.Object,
            lockService.Object,
            financial.Object,
            new MainMapper());

        var request = new UpdateProductionOrderRequest
        {
            Id = order.Id,
            ClientName = order.ClientName,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(1),
            UserId = order.UserId,
            SewingTeamId = order.SewingTeamId,
            AppliedBonusPerPiece = 4m,
            StartedAt = DateTime.UtcNow.AddHours(-6),
            CompletedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        await service.UpdateProductionOrderAsync(request, 1);

        financial.Verify(x => x.UpdateIntermediateCostAsync(It.Is<ProductionOrder>(o =>
            o.Id == order.Id &&
            o.StartedAt == request.StartedAt &&
            o.CompletedAt == request.CompletedAt &&
            o.AppliedBonusPerPiece == request.AppliedBonusPerPiece)), Times.Once);
    }

    [Fact]
    public async Task UpdateProductionOrderAsync_ShouldRejectCompletedAt_ForInProgressOrder()
    {
        var order = new ProductionOrder
        {
            Id = 602,
            LotCode = "OP-DATE-002",
            ProductId = 1,
            Quantity = 12,
            CurrentStage = ProductionStage.Sewing,
            CurrentStatus = ProductionStatus.InProduction,
            CreatedAt = DateTime.UtcNow.AddHours(-4),
            UpdatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow.AddHours(-2)
        };

        var orderRepo = new Mock<IProductionOrderRepository>();
        orderRepo.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var userRepo = new Mock<IUserRepository>();
        var productRepo = new Mock<IProductRepository>();
        var notification = new Mock<INotificationService>();
        var httpContext = new Mock<IHttpContextAccessor>();
        var lockService = new Mock<IDistributedLockService>();
        var financial = new Mock<IFinancialCalculatorService>();

        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContext.Setup(x => x.HttpContext).Returns(context);

        var service = new ProductionOrderMutationService(
            orderRepo.Object,
            userRepo.Object,
            productRepo.Object,
            notification.Object,
            httpContext.Object,
            lockService.Object,
            financial.Object,
            new MainMapper());

        var request = new UpdateProductionOrderRequest
        {
            Id = order.Id,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(1),
            StartedAt = order.StartedAt,
            CompletedAt = DateTime.UtcNow
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateProductionOrderAsync(request, 1));
        Assert.Equal("Completion date can only be edited for completed lots.", ex.Message);
        financial.Verify(x => x.UpdateIntermediateCostAsync(It.IsAny<ProductionOrder>()), Times.Never);
    }
}
