/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardBIService _biService;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "DashboardComplete";

    public DashboardController(IDashboardBIService biService, IMemoryCache cache)
    {
        _biService = biService;
        _cache = cache;
    }

    [HttpGet("complete")]
    public async Task<ActionResult<ApiResponse<DashboardCompleteResponse>>> GetComplete()
    {
        try
        {
            // 1. Memory Cache implementation (Optimized for industrial workloads)
            var dashboard = await _cache.GetOrCreateAsync(CACHE_KEY, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return _biService.GetCompleteDashboardAsync(HttpContext.RequestAborted);
            });

            if (dashboard == null) return NotFound(ApiResponse<DashboardCompleteResponse>.FailureResult("No data found"));

            // 2. Field-level Security: Hide financial data from non-admins
            if (!User.IsInRole("Administrator"))
            {
                // Create a clone/copy to avoid modifying the cached instance
                var secureView = new DashboardCompleteResponse
                {
                    MonthProductionQuantity = dashboard.MonthProductionQuantity,
                    DelayedOrdersCount = dashboard.DelayedOrdersCount,
                    ProductionByWorkshop = dashboard.ProductionByWorkshop,
                    TeamRanking = dashboard.TeamRanking,
                    WeeklyVolumeData = dashboard.WeeklyVolumeData,
                    WeeklyLabels = dashboard.WeeklyLabels,
                    TopProfitableModels = new List<ProductProfitabilityDto>(), // Financial info cleared
                    BottomProfitableModels = new List<ProductProfitabilityDto>(),
                    StalledStock = dashboard.StalledStock,
                    // Clear financial costs/margins for privacy
                    MonthAverageCostPerPiece = 0,
                    MonthAverageMargin = 0
                };
                return Ok(ApiResponse<DashboardCompleteResponse>.SuccessResult(secureView));
            }

            return Ok(ApiResponse<DashboardCompleteResponse>.SuccessResult(dashboard!));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DashboardCompleteResponse>.FailureResult("Error retrieving dashboard", new List<string> { ex.Message }));
        }
    }

    [HttpPost("refresh")]
    [Authorize(Roles = "Administrator")]
    public IActionResult RefreshCache()
    {
        _cache.Remove(CACHE_KEY);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Cache cleared"));
    }

    [HttpPost("seed-audit-bi")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SeedAuditBI([FromServices] GestionProduccion.Data.AppDbContext context)
    {
        try
        {
            // 0. Clean previous attempts
            var oldOrder = context.ProductionOrders.FirstOrDefault(o => o.LotCode == "LOT-BI-SUCCESS-2026");
            if (oldOrder != null) { context.ProductionOrders.Remove(oldOrder); await context.SaveChangesAsync(); }

            // 1. Asegurar Producto
            var product = context.Products.FirstOrDefault(p => p.MainSku == "SKU-BI-AUDIT");
            if (product == null)
            {
                product = new Domain.Entities.Product { Name = "Camiseta Audit", InternalCode = "AUD-001", MainSku = "SKU-BI-AUDIT", FabricType = "Algodão", AverageProductionTimeMinutes = 10, EstimatedSalePrice = 150 };
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            // 2. Asegurar Equipo
            var team = context.SewingTeams.FirstOrDefault(t => t.Name == "Equipe Alpha Audit");
            if (team == null)
            {
                team = new Domain.Entities.SewingTeam { Name = "Equipe Alpha Audit", IsActive = true };
                context.SewingTeams.Add(team);
                await context.SaveChangesAsync();
            }

            // 3. Vincular Usuario david
            var user = context.Users.FirstOrDefault(u => u.Email == "administrador@serona.com");
            if (user != null) { user.SewingTeamId = team.Id; await context.SaveChangesAsync(); }

            // 4. Crear Orden Marzo 2026
            var order = new Domain.Entities.ProductionOrder
            {
                LotCode = "LOT-BI-SUCCESS-2026",
                ProductId = product.Id,
                Quantity = 100,
                CurrentStage = Domain.Enums.ProductionStage.Packaging,
                CurrentStatus = Domain.Enums.ProductionStatus.Completed,
                CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2026, 3, 1, 17, 0, 0, DateTimeKind.Utc),
                EstimatedCompletionAt = new DateTime(2026, 3, 1, 18, 0, 0, DateTimeKind.Utc),
                EffectiveMinutes = 500, // Efficiency 200% (Standard is 1000 min)
                SewingTeamId = team.Id,
                UserId = user?.Id ?? 1,
                ProfitMargin = 25.5m,
                TotalCost = 4500m,
                AverageCostPerPiece = 45m
            };
            context.ProductionOrders.Add(order);
            await context.SaveChangesAsync();

            // 5. Registrar Tamaños
            var size = new Domain.Entities.ProductionOrderSize { ProductionOrderId = order.Id, Size = "M", Quantity = 100 };
            context.ProductionOrderSizes.Add(size);
            await context.SaveChangesAsync();

            // 6. Registrar Producción (Output) - CRUCIAL PARA AGREGACIÓN BI
            context.ProductionOrderOutputs.Add(new Domain.Entities.ProductionOrderOutput
            {
                ProductionOrderId = order.Id,
                ProductionOrderSizeId = size.Id,
                Quantity = 100,
                Stage = Domain.Enums.ProductionStage.Packaging,
                CreatedAt = new DateTime(2026, 3, 1, 17, 30, 0, DateTimeKind.Utc),
                UserId = user?.Id ?? 1
            });
            await context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(null!, "BI Seed Successful"));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
}
