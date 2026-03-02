using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QAController : ControllerBase
{
    private readonly IQAService _qaService;

    public QAController(IQAService qaService)
    {
        _qaService = qaService;
    }

    [HttpPost]
    public async Task<ActionResult<QADefectDto>> RegisterDefect([FromForm] int ProductionOrderId, [FromForm] string Reason, [FromForm] int Quantity, IFormFile? PhotoFile)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var dto = new CreateQADefectDto
        {
            ProductionOrderId = ProductionOrderId,
            Reason = Reason,
            Quantity = Quantity,
            ReportedByUserId = userId
        };

        var defect = await _qaService.RegisterDefectAsync(dto, PhotoFile);
        return Ok(new QADefectDto
        {
            Id = defect.Id,
            Reason = defect.Reason,
            Quantity = defect.Quantity,
            ReportedAt = defect.ReportedAt,
            PhotoUrl = defect.PhotoUrl
        });
    }

    [HttpGet("orders/{orderId}/defects")]
    public async Task<ActionResult<ApiResponse<List<QADefectDto>>>> GetDefectsByOrder(int orderId)
    {
        var defects = await _qaService.GetDefectsByOrderAsync(orderId);
        var dtos = defects.Select(d => new QADefectDto
        {
            Id = d.Id,
            Reason = d.Reason,
            Quantity = d.Quantity,
            ReportedAt = d.ReportedAt
        }).ToList();

        return Ok(new ApiResponse<List<QADefectDto>> { Success = true, Data = dtos });
    }

    [HttpDelete("defects/{id}")]
    public async Task<IActionResult> DeleteDefect(int id)
    {
        await _qaService.DeleteDefectAsync(id);
        return NoContent();
    }
}
