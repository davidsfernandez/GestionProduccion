using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QAController : ControllerBase
{
    private readonly IQAService _qaService;

    public QAController(IQAService qaService)
    {
        _qaService = qaService;
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<QADefectDto>>> RegisterDefect([FromForm] CreateQADefectDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        dto.ReportedByUserId = userId;

        var defect = await _qaService.RegisterDefectAsync(dto);
        
        var result = new QADefectDto
        {
            Id = defect.Id,
            ProductionOrderId = defect.ProductionOrderId,
            Reason = defect.Reason,
            Quantity = defect.Quantity,
            PhotoUrl = defect.PhotoUrl,
            RegistrationDate = defect.RegistrationDate
        };

        return Ok(new ApiResponse<QADefectDto> { Success = true, Message = "Defect registered successfully", Data = result });
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<List<QADefectDto>>>> GetByOrder(int orderId)
    {
        var defects = await _qaService.GetDefectsByOrderAsync(orderId);
        var result = defects.Select(d => new QADefectDto
        {
            Id = d.Id,
            ProductionOrderId = d.ProductionOrderId,
            Reason = d.Reason,
            Quantity = d.Quantity,
            PhotoUrl = d.PhotoUrl,
            RegistrationDate = d.RegistrationDate
        }).ToList();

        return Ok(new ApiResponse<List<QADefectDto>> { Success = true, Data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        await _qaService.DeleteDefectAsync(id);
        return Ok(new ApiResponse<string> { Success = true, Message = "Defect deleted" });
    }
}
