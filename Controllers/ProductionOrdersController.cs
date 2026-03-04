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
            return BadRequest(new ApiResponse<object?> { Success = false, Message = "Invalid model state", Data = ModelState });
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var createdByUserId))
            {
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "User ID claim not found or invalid." });
            }

            var newOrder = await _mutationService.CreateProductionOrderAsync(request, createdByUserId, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetProductionOrderById), new { id = newOrder.Id }, new ApiResponse<ProductionOrderDto> { Success = true, Data = newOrder });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error creating production order", Data = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>>> GetProductionOrders([FromQuery] FilterProductionOrderDto? filter)
    {
        try
        {
            var result = await _queryService.ListProductionOrdersAsync(filter, HttpContext.RequestAborted);
            return Ok(new ApiResponse<PaginatedResponseDto<ProductionOrderDto>> { Success = true, Data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving production orders", Data = ex.Message });
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
            return Ok(new ApiResponse<List<UserDto>> { Success = true, Data = assignable });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving assignable users", Data = ex.Message });
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
                return NotFound(new ApiResponse<object?> { Success = false, Message = $"Production order with ID {id} not found." });
            }
            return Ok(new ApiResponse<ProductionOrderDto> { Success = true, Data = order });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving production order", Data = ex.Message });
        }
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<ApiResponse<List<ProductionHistoryDto>>>> GetOrderHistory(int id)
    {
        try
        {
            var history = await _queryService.GetHistoryByProductionOrderIdAsync(id, HttpContext.RequestAborted);
            return Ok(new ApiResponse<List<ProductionHistoryDto>> { Success = true, Data = history });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving order history", Data = ex.Message });
        }
    }

    [HttpPost("{orderId}/assign")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignTask(int orderId, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = "Invalid model state", Data = ModelState });
        }

        try
        {
            var result = await _lifecycleService.AssignTaskAsync(orderId, request.UserId, HttpContext.RequestAborted);
            return Ok(new ApiResponse<bool> { Success = true, Data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error assigning task", Data = ex.Message });
        }
    }

    [HttpPatch("{orderId}/status")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int orderId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "User ID claim not found or invalid." });
            }

            var result = await _lifecycleService.UpdateStatusAsync(orderId, request.NewStatus, request.Note, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(new ApiResponse<bool> { Success = true, Data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object?> { Success = false, Message = ex.Message });
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
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "User ID claim not found or invalid." });
            }

            var result = await _lifecycleService.BulkUpdateStatusAsync(request.OrderIds, request.NewStatus, request.Note, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(new ApiResponse<BulkUpdateResult> { Success = true, Data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error processing bulk update", Data = ex.Message });
        }
    }

    [HttpPost("{orderId}/change-stage")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeStage(int orderId, [FromBody] ChangeStageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = "Invalid model state", Data = ModelState });
        }
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "User ID claim not found or invalid." });
            }

            var result = await _lifecycleService.ChangeStageAsync(orderId, request.NewStage, request.Note, modifiedByUserId, HttpContext.RequestAborted);
            if (!result) return NotFound(new ApiResponse<object?> { Success = false, Message = "Order not found." });
            return Ok(new ApiResponse<bool> { Success = true, Data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error changing stage", Data = ex.Message });
        }
    }

    [HttpPost("{orderId}/advance-stage")]
    [Authorize(Roles = "Administrator,Leader,Operational")]
    public async Task<ActionResult<ApiResponse<bool>>> AdvanceStage(int orderId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var modifiedByUserId))
            {
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "User ID claim not found or invalid." });
            }

            var result = await _lifecycleService.AdvanceStageAsync(orderId, modifiedByUserId, HttpContext.RequestAborted);
            return Ok(new ApiResponse<bool> { Success = true, Data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProductionOrder(int id)
    {
        try
        {
            var result = await _mutationService.DeleteProductionOrderAsync(id, HttpContext.RequestAborted);
            if (!result) return NotFound(new ApiResponse<object?> { Success = false, Message = "Order not found." });
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error deleting order", Data = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        var dashboardData = await _queryService.GetDashboardAsync(HttpContext.RequestAborted);
        return Ok(new ApiResponse<DashboardDto> { Success = true, Data = dashboardData });
    }

    [HttpGet("tv-stats")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetTvStats([FromServices] ISystemConfigurationService configService)
    {
        var dashboardData = await _queryService.GetDashboardAsync(HttpContext.RequestAborted);
        var config = await configService.GetConfigurationAsync();
        dashboardData.TvAnnouncement = config.TvAnnouncement;
        return Ok(new ApiResponse<DashboardDto> { Success = true, Data = dashboardData });
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetOrderPdf(int id, [FromServices] IReportService reportService)
    {
        try
        {
            var order = await _queryService.GetProductionOrderByIdAsync(id, HttpContext.RequestAborted);
            if (order == null) return NotFound(new { message = "Order not found." });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var pdfBytes = await reportService.GenerateProductionOrderReportAsync(id, baseUrl);
            if (pdfBytes == null || pdfBytes.Length == 0) return NotFound(new { message = "Could not generate PDF content." });

            return File(pdfBytes, "application/pdf", $"Order_{order.LotCode}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating Individual PDF", details = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetOrderReport(int id, [FromServices] IReportService reportService)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var pdfBytes = await reportService.GenerateProductionOrderReportAsync(id, baseUrl);
            if (pdfBytes == null || pdfBytes.Length == 0) return NotFound(new { message = "Report generation failed." });

            return File(pdfBytes, "application/pdf", $"ProductionOrder_{id}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating Order Report", details = ex.Message });
        }
    }

    [HttpGet("daily-report")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> GetDailyReport([FromServices] IReportService reportService)
    {
        try
        {
            var pdfBytes = await reportService.GenerateDailyProductionReportAsync();
            if (pdfBytes == null || pdfBytes.Length == 0) return StatusCode(500, new { message = "Daily report content is empty." });
            return File(pdfBytes, "application/pdf", $"DailyReport_{DateTime.Today:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating Daily PDF Report", details = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] FilterProductionOrderDto? filter, [FromServices] IReportService reportService)
    {
        try
        {
            // Ensure we get all records if it's for export
            if (filter != null) filter.PageSize = 100000; 
            
            var result = await _queryService.ListProductionOrdersAsync(filter, HttpContext.RequestAborted);
            var csvBytes = await reportService.GenerateOrdersCsvAsync(result.Items);
            return File(csvBytes, "text/csv", $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating CSV", error = ex.Message });
        }
    }

    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] List<ProductionOrderDto> orders)
    {
        try
        {
            var excelBytes = await _excelExportService.ExportProductionOrdersToExcelAsync(orders);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating Excel", error = ex.Message });
        }
    }
}
