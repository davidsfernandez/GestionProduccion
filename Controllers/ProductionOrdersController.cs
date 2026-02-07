using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication for all controller endpoints
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _productionOrderService;

    public ProductionOrdersController(IProductionOrderService productionOrderService)
    {
        _productionOrderService = productionOrderService;
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Leader")] // Only specific roles can create
    public async Task<IActionResult> CreateProductionOrder([FromBody] CreateProductionOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var createdByUserId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid." });
            }

            var newOrder = await _productionOrderService.CreateProductionOrderAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetProductionOrderById), new { id = newOrder.Id }, newOrder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating production order", error = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductionOrderById(int id)
    {
        try
        {
            var order = await _productionOrderService.GetProductionOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Production order with ID {id} not found." });
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving production order", error = ex.Message });
        }
    }

    [HttpPost("{orderId}/assign")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> AssignTask(int orderId, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedOrder = await _productionOrderService.AssignTaskAsync(orderId, request.UserId);
            return Ok(updatedOrder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error assigning task", error = ex.Message });
        }
    }

    [HttpPatch("{orderId}/status")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid." });
            }

            var updatedOrder = await _productionOrderService.UpdateStatusAsync(orderId, request.NewStatus, request.Note, modifiedByUserId);
            return Ok(updatedOrder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{orderId}/advance-stage")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> AdvanceStage(int orderId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid." });
            }

            var updatedOrder = await _productionOrderService.AdvanceStageAsync(orderId, modifiedByUserId);
            return Ok(updatedOrder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboardData = await _productionOrderService.GetDashboardAsync();
        
        // Map to match client expectations
        var response = new
        {
            OperationsByStage = dashboardData.OperationsByStage,
            StoppedOperations = dashboardData.StoppedOperations.Select(po => new
            {
                po.Id,
                po.UniqueCode,
                po.ProductDescription,
                po.EstimatedDeliveryDate
            }).ToList(),
            WorkloadByUser = dashboardData.WorkloadByUser.Select(w => new 
            { 
                w.UserName, 
                w.TotalOrders,
                w.PendingOrders
            }).ToList()
        };
        
        return Ok(response);
    }
}
