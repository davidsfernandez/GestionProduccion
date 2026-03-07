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
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public async Task<ActionResult<ApiResponse<QADefectDto>>> RegisterDefect([FromForm] int ProductionOrderId, [FromForm] string Reason, [FromForm] int Quantity, IFormFile? PhotoFile)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<QADefectDto>.FailureResult("Unauthorized access"));
            }

            var dto = new CreateQADefectDto
            {
                ProductionOrderId = ProductionOrderId,
                Reason = Reason,
                Quantity = Quantity,
                ReportedByUserId = userId
            };

            var defect = await _qaService.RegisterDefectAsync(dto, PhotoFile);
            var result = new QADefectDto
            {
                Id = defect.Id,
                Reason = defect.Reason,
                Quantity = defect.Quantity,
                ReportedAt = defect.ReportedAt,
                PhotoUrl = defect.PhotoUrl
            };

            return Ok(ApiResponse<QADefectDto>.SuccessResult(result, "Defect registered successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<QADefectDto>.FailureResult("Error registering defect", new List<string> { ex.Message }));
        }
    }

    [HttpGet("orders/{orderId}/defects")]
    public async Task<ActionResult<ApiResponse<List<QADefectDto>>>> GetDefectsByOrder(int orderId)
    {
        try
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            var dtos = defects.Select(d => new QADefectDto
            {
                Id = d.Id,
                Reason = d.Reason,
                Quantity = d.Quantity,
                ReportedAt = d.ReportedAt
            }).ToList();

            return Ok(ApiResponse<List<QADefectDto>>.SuccessResult(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<QADefectDto>>.FailureResult("Error retrieving defects", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("defects/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDefect(int id)
    {
        try
        {
            await _qaService.DeleteDefectAsync(id);
            return Ok(ApiResponse<object>.SuccessResult(null, "Defect deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResult("Error deleting defect", new List<string> { ex.Message }));
        }
    }
}
