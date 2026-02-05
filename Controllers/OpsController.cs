using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        var order = new ProductionOrder
        {
            UniqueCode = request.UniqueCode,
            ProductDescription = request.ProductDescription,
            Quantity = request.Quantity,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate
        };

        try
        {
            var newOrder = await _productionOrderService.CreateProductionOrder(order);
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
            var order = await _productionOrderService.GetProductionOrderById(id);
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
            var updatedOrder = await _productionOrderService.AssignTask(orderId, request.UserId);
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
            var updatedOrder = await _productionOrderService.UpdateStatus(orderId, request.NewStatus, request.Note);
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
            var updatedOrder = await _productionOrderService.AdvanceStage(orderId);
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
        var dashboardData = await _productionOrderService.GetDashboard();
        
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
            WorkloadByUser = dashboardData.OperationsByUser
                .Select(kv => new
                {
                    UserName = kv.Key,
                    OperationCount = kv.Value.Count
                }).ToList()
        };
        
        return Ok(response);
    }
}
