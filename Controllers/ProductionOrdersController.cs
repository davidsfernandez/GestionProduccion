/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services.ProductionOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderQueryService _queryService;
    private readonly IProductionOrderMutationService _mutationService;
    private readonly IProductionOrderLifecycleService _lifecycleService;
    private readonly IUserService _userService;
    private readonly IExcelExportService _excelExportService;

    public ProductionOrdersController(
        IProductionOrderQueryService queryService,
        IProductionOrderMutationService mutationService,
        IProductionOrderLifecycleService lifecycleService,
        IUserService userService,
        IExcelExportService excelExportService)
    {
        _queryService = queryService;
        _mutationService = mutationService;
        _lifecycleService = lifecycleService;
        _userService = userService;
        _excelExportService = excelExportService;
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> CreateProductionOrder([FromBody] CreateProductionOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ProductionOrderDto>.FailureResult("Validation failed"));
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var createdByUserId))
            {
                return Unauthorized(ApiResponse<ProductionOrderDto>.FailureResult("Unauthorized access"));
            }

            var newOrder = await _mutationService.CreateProductionOrderAsync(request, createdByUserId, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetProductionOrderById), new { id = newOrder.Id }, ApiResponse<ProductionOrderDto>.SuccessResult(newOrder, "Order created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Error creating production order", new List<string> { ex.Message }));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>>> GetProductionOrders(
        [FromQuery] FilterProductionOrderDto? filter, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var orders = await _queryService.ListProductionOrdersAsync(filter, page, pageSize, HttpContext.RequestAborted);
            return Ok(ApiResponse<PaginatedResponseDto<ProductionOrderDto>>.SuccessResult(orders));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponseDto<ProductionOrderDto>>.FailureResult("Error retrieving production orders", new List<string> { ex.Message }));
        }
    }

    [HttpGet("assignable")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAssignableUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            var assignable = users
                .Where(u => u.Role == UserRole.Operational || u.Role == UserRole.Leader)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    ExternalId = u.ExternalId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                }).ToList();
            return Ok(ApiResponse<List<UserDto>>.SuccessResult(assignable));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserDto>>.FailureResult("Error retrieving assignable users", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> GetProductionOrderById(int id)
    {
        try
        {
            var order = await _queryService.GetProductionOrderByIdAsync(id, HttpContext.RequestAborted);
            if (order == null)
            {
                return NotFound(ApiResponse<ProductionOrderDto>.FailureResult($"Production order with ID {id} not found."));
            }
            return Ok(ApiResponse<ProductionOrderDto>.SuccessResult(order));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Error retrieving production order", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<ApiResponse<List<ProductionHistoryDto>>>> GetOrderHistory(int id)
    {
        try
        {
            var history = await _queryService.GetHistoryByProductionOrderIdAsync(id, HttpContext.RequestAborted);
            return Ok(ApiResponse<List<ProductionHistoryDto>>.SuccessResult(history));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ProductionHistoryDto>>.FailureResult("Error retrieving order history", new List<string> { ex.Message }));
        }
    }

    [HttpPost("{orderId}/assign")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> AssignTask(int orderId, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ProductionOrderDto>.FailureResult("Validation failed"));
        }

        try
        {
            var updatedOrder = await _lifecycleService.AssignTaskAsync(orderId, request.UserId, HttpContext.RequestAborted);
            return Ok(ApiResponse<ProductionOrderDto>.SuccessResult(updatedOrder, "Task assigned successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Error assigning task", new List<string> { ex.Message }));
        }
    }

    [HttpPatch("{orderId}/status")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> UpdateStatus(int orderId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(ApiResponse<ProductionOrderDto>.FailureResult("Unauthorized access"));
            }

            var updatedOrder = await _lifecycleService.UpdateStatusAsync(orderId, request.NewStatus, request.Note, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(ApiResponse<ProductionOrderDto>.SuccessResult(updatedOrder, "Status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Error updating status", new List<string> { ex.Message }));
        }
    }

    [HttpPost("bulk-status")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<BulkUpdateResult>>> BulkUpdateStatus([FromBody] BulkUpdateStatusRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(ApiResponse<BulkUpdateResult>.FailureResult("Unauthorized access"));
            }

            var result = await _lifecycleService.BulkUpdateStatusAsync(request.OrderIds, request.NewStatus, request.Note, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(ApiResponse<BulkUpdateResult>.SuccessResult(result, "Bulk update completed"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkUpdateResult>.FailureResult("Error processing bulk update", new List<string> { ex.Message }));
        }
    }

    [HttpPost("{orderId}/advance-stage")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> AdvanceStage(int orderId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(ApiResponse<ProductionOrderDto>.FailureResult("Unauthorized access"));
            }

            var updatedOrder = await _lifecycleService.AdvanceStageAsync(orderId, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(ApiResponse<ProductionOrderDto>.SuccessResult(updatedOrder, "Stage advanced successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProductionOrderDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Error advancing stage", new List<string> { ex.Message }));
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        try
        {
            var dashboardData = await _queryService.GetDashboardAsync(HttpContext.RequestAborted);
            return Ok(ApiResponse<DashboardDto>.SuccessResult(dashboardData));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DashboardDto>.FailureResult("Error retrieving dashboard", new List<string> { ex.Message }));
        }
    }

    [HttpGet("tv-stats")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetTvStats([FromServices] ISystemConfigurationService configService)
    {
        try
        {
            var dashboardData = await _queryService.GetDashboardAsync(HttpContext.RequestAborted);
            var config = await configService.GetConfigurationAsync();
            dashboardData.TvAnnouncement = config.TvAnnouncement;
            return Ok(ApiResponse<DashboardDto>.SuccessResult(dashboardData));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DashboardDto>.FailureResult("Error retrieving TV stats", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetOrderPdf(int id, [FromServices] IReportService reportService)
    {
        try
        {
            var order = await _queryService.GetProductionOrderByIdAsync(id, HttpContext.RequestAborted);
            if (order == null) return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var pdfBytes = await reportService.GenerateProductionOrderReportAsync(id, baseUrl);
            if (pdfBytes == null || pdfBytes.Length == 0) return NotFound();

            return File(pdfBytes, "application/pdf", $"Order_{order.LotCode}.pdf");
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
