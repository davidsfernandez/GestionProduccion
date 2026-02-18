using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication for all controller endpoints
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _productionOrderService;
    private readonly IUserService _userService;
    private readonly IExcelExportService _excelExportService;

    public ProductionOrdersController(IProductionOrderService productionOrderService, IUserService userService, IExcelExportService excelExportService)
    {
        _productionOrderService = productionOrderService;
        _userService = userService;
        _excelExportService = excelExportService;
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
    
    [HttpGet]
    public async Task<ActionResult<List<ProductionOrderDto>>> GetProductionOrders([FromQuery] FilterProductionOrderDto? filter)
    {
        try
        {
            var orders = await _productionOrderService.ListProductionOrdersAsync(filter);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving production orders", error = ex.Message });
        }
    }

    [HttpGet("assignable")]
    public async Task<ActionResult<List<UserDto>>> GetAssignableUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            var assignable = users
                .Where(u => u.Role == UserRole.Operator || u.Role == UserRole.Workshop || u.Role == UserRole.Leader)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                }).ToList();
            return Ok(assignable);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving assignable users", error = ex.Message });
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

    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<ProductionHistoryDto>>> GetOrderHistory(int id)
    {
        try
        {
            var history = await _productionOrderService.GetHistoryByProductionOrderIdAsync(id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving order history", error = ex.Message });
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
    [Authorize(Roles = "Administrator,Leader,Operator")]
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

    [HttpPost("bulk-status")]
    [Authorize(Roles = "Administrator,Leader,Operator")]
    public async Task<ActionResult<BulkUpdateResult>> BulkUpdateStatus([FromBody] BulkUpdateStatusRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid." });
            }

            var result = await _productionOrderService.BulkUpdateStatusAsync(request.OrderIds, request.NewStatus, request.Note, modifiedByUserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error processing bulk update", error = ex.Message });
        }
    }

    [HttpPost("{orderId}/change-stage")]
    [Authorize(Roles = "Administrator,Leader,Operator")]
    public async Task<IActionResult> ChangeStage(int orderId, [FromBody] ChangeStageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid." });
            }

            var result = await _productionOrderService.ChangeStageAsync(orderId, request.NewStage, request.Note, modifiedByUserId);
            if (!result) return NotFound(new { message = "Order not found." });
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
             return StatusCode(500, new { message = "Error changing stage", error = ex.Message });
        }
    }

    [HttpPost("{orderId}/advance-stage")]
    [Authorize(Roles = "Administrator,Leader,Operator")]
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
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dashboardData = await _productionOrderService.GetDashboardAsync();
        return Ok(dashboardData);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetOrderPdf(int id, [FromServices] IReportService reportService)
    {
        try
        {
            var order = await _productionOrderService.GetProductionOrderByIdAsync(id);
            if (order == null) return NotFound(new { message = "Order not found." });

            var pdfBytes = await reportService.GenerateProductionOrderReportAsync(id);
            if (pdfBytes == null) return NotFound(new { message = "Could not generate PDF." });
            
            return File(pdfBytes, "application/pdf", $"Orden_{order.UniqueCode}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
        }
    }

    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetOrderReport(int id, [FromServices] IReportService reportService)
    {
        try
        {
            var pdfBytes = await reportService.GenerateProductionOrderReportAsync(id);
            if (pdfBytes == null) return NotFound();
            
            return File(pdfBytes, "application/pdf", $"ProductionOrder_{id}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating report", error = ex.Message });
        }
    }

    [HttpGet("daily-report")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> GetDailyReport([FromServices] IReportService reportService)
    {
        try
        {
            var pdfBytes = await reportService.GenerateDailyProductionReportAsync();
            return File(pdfBytes, "application/pdf", $"DailyReport_{DateTime.Today:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating daily report", error = ex.Message });
        }
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] FilterProductionOrderDto? filter, [FromServices] IReportService reportService)
    {
        try
        {
            var orders = await _productionOrderService.ListProductionOrdersAsync(filter);
            var csvBytes = await reportService.GenerateOrdersCsvAsync(orders);
            return File(csvBytes, "text/csv", $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating CSV", error = ex.Message });
        }
    }

    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] FilterProductionOrderDto? filter)
    {
        try
        {
            var excelBytes = await _excelExportService.ExportProductionOrdersToExcelAsync(filter);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating Excel", error = ex.Message });
        }
    }
}
