using GestionProduccion.Application.Mapping;
using GestionProduccion.Domain.Entities.HR;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestionProduccion.Data;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HRController : ControllerBase
{
    private readonly IAbsenceService _absenceService;
    private readonly ILeadService _leadService;
    private readonly IQuoteService _quoteService;
    private readonly IReportService _reportService;
    private readonly AppDbContext _context;
    private readonly ILogger<HRController> _logger;

    public HRController(IAbsenceService absenceService, ILeadService leadService, IQuoteService quoteService, IReportService reportService, AppDbContext context, ILogger<HRController> logger)
    {
        _absenceService = absenceService;
        _leadService = leadService;
        _quoteService = quoteService;
        _reportService = reportService;
        _context = context;
        _logger = logger;
    }

    // --- QUOTES ---
    [Authorize(Roles = "Administrator,Leader")]
    [HttpPost("quotes")]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> CreateQuote([FromBody] CreateQuoteRequest request)
    {
        try
        {
            var result = await _quoteService.CreateQuoteAsync(request, HttpContext.RequestAborted);
            return Ok(ApiResponse<QuoteDto>.SuccessResult(result, "Orçamento criado com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quote");
            return StatusCode(500, ApiResponse<QuoteDto>.FailureResult("Erro ao criar orçamento."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpGet("leads/{id}/quotes")]
    public async Task<ActionResult<ApiResponse<List<QuoteDto>>>> GetLeadQuotes(int id)
    {
        try
        {
            var result = await _quoteService.GetLeadQuotesAsync(id, HttpContext.RequestAborted);
            return Ok(ApiResponse<List<QuoteDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lead quotes");
            return StatusCode(500, ApiResponse<List<QuoteDto>>.FailureResult("Erro ao carregar orçamentos."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpPost("quotes/{id}/status")]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> UpdateQuoteStatus(int id, [FromBody] dynamic request)
    {
        try
        {
            string newStatusStr = request.GetProperty("newStatus").GetString();
            if (!Enum.TryParse<QuoteStatus>(newStatusStr, true, out var newStatus)) return BadRequest("Status inválido.");

            var result = await _quoteService.UpdateQuoteStatusAsync(id, newStatus, HttpContext.RequestAborted);
            return Ok(ApiResponse<QuoteDto>.SuccessResult(result, "Status atualizado."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quote status");
            return StatusCode(500, ApiResponse<QuoteDto>.FailureResult("Erro ao atualizar status."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpGet("quotes/{id}/pdf")]
    public async Task<IActionResult> DownloadQuotePdf(int id)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var pdfBytes = await _reportService.GenerateQuotePdfAsync(id, baseUrl);
            return File(pdfBytes, "application/pdf", $"Orcamento_{id:D6}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quote PDF");
            return StatusCode(500);
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpGet("leads")]
    public async Task<ActionResult<ApiResponse<List<LeadDto>>>> GetLeads()
    {
        try
        {
            var result = await _leadService.GetLeadsAsync(HttpContext.RequestAborted);
            return Ok(ApiResponse<List<LeadDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leads");
            return StatusCode(500, ApiResponse<List<LeadDto>>.FailureResult("Erro ao carregar leads."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpPost("leads/{id}/status")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> UpdateLeadStatus(int id, [FromBody] UpdateLeadStatusRequest request)
    {
        try
        {
            var result = await _leadService.UpdateLeadStatusAsync(id, request.NewStatus, request.Note, HttpContext.RequestAborted);
            return Ok(ApiResponse<LeadDto>.SuccessResult(result, "Status do lead atualizado."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead status {Id}", id);
            return StatusCode(500, ApiResponse<LeadDto>.FailureResult("Erro ao atualizar status."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpGet("employees")]
    public async Task<ActionResult<ApiResponse<List<EmployeeProfileDto>>>> GetEmployees()
    {
        try
        {
            var employees = await _context.EmployeeProfiles
                .Include(e => e.User)
                .Select(e => new EmployeeProfileDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    FullName = e.User!.FullName,
                    CPF = e.CPF,
                    IsActive = e.User.IsActive,
                    JoinedDate = e.JoinedDate,
                    BaseSalary = e.BaseSalary
                })
                .ToListAsync();

            return Ok(ApiResponse<List<EmployeeProfileDto>>.SuccessResult(employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing employees");
            return StatusCode(500, ApiResponse<List<EmployeeProfileDto>>.FailureResult("Erro ao listar colaboradores."));
        }
    }

    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<ApiResponse<EmployeeProfileDto>>> GetProfile(int userId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId)) return Unauthorized();

            if (userId != currentUserId && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
            {
                return Forbid();
            }

            var e = await _context.EmployeeProfiles
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (e == null) return NotFound(ApiResponse<EmployeeProfileDto>.FailureResult("Perfil não encontrado."));

            var dto = new EmployeeProfileDto
            {
                Id = e.Id,
                UserId = e.UserId,
                FullName = e.User!.FullName,
                CPF = e.CPF,
                RG = e.RG,
                RNM = e.RNM,
                PIS = e.PIS,
                BirthDate = e.BirthDate,
                Address = e.Address,
                JoinedDate = e.JoinedDate,
                BaseSalary = e.BaseSalary,
                OptsForTransportationVoucher = e.OptsForTransportationVoucher,
                IsActive = e.User.IsActive
            };

            return Ok(ApiResponse<EmployeeProfileDto>.SuccessResult(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for {UserId}", userId);
            return StatusCode(500, ApiResponse<EmployeeProfileDto>.FailureResult("Erro ao carregar perfil."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpPut("profile/{userId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile(int userId, [FromBody] EmployeeProfileDto dto)
    {
        try
        {
            var e = await _context.EmployeeProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (e == null) return NotFound();

            e.CPF = dto.CPF;
            e.RG = dto.RG;
            e.RNM = dto.RNM;
            e.PIS = dto.PIS;
            e.BirthDate = dto.BirthDate;
            e.Address = dto.Address;
            e.BaseSalary = dto.BaseSalary;
            e.OptsForTransportationVoucher = dto.OptsForTransportationVoucher;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.SuccessResult(true, "Perfil atualizado com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for {UserId}", userId);
            return StatusCode(500, ApiResponse<bool>.FailureResult("Erro ao salvar alterações."));
        }
    }

    [HttpPost("leave-request")]
    public async Task<ActionResult<ApiResponse<EmployeeLeaveDto>>> RequestLeave([FromBody] CreateLeaveDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId)) return Unauthorized();

            // Ensure users only request leave for themselves unless they are Admin/HR
            if (dto.UserId != currentUserId && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
            {
                return Forbid();
            }

            var result = await _absenceService.RequestLeaveAsync(dto, HttpContext.RequestAborted);
            return Ok(ApiResponse<EmployeeLeaveDto>.SuccessResult(result, "Solicitação de ausência registrada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting leave");
            return StatusCode(500, ApiResponse<EmployeeLeaveDto>.FailureResult("Erro ao registrar solicitação."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpPost("leave/{id}/process")]
    public async Task<ActionResult<ApiResponse<EmployeeLeaveDto>>> ProcessLeave(int id, [FromQuery] bool approved, [FromQuery] string? note)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId)) return Unauthorized();

            var result = await _absenceService.ProcessLeaveAsync(id, approved, currentUserId, note, HttpContext.RequestAborted);
            return Ok(ApiResponse<EmployeeLeaveDto>.SuccessResult(result, approved ? "Ausência aprovada." : "Ausência rejeitada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing leave {Id}", id);
            return StatusCode(500, ApiResponse<EmployeeLeaveDto>.FailureResult("Erro ao processar solicitação."));
        }
    }

    [HttpGet("vacation-balance/{userId}")]
    public async Task<ActionResult<ApiResponse<VacationBalanceDto>>> GetVacationBalance(int userId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId)) return Unauthorized();

            if (userId != currentUserId && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
            {
                return Forbid();
            }

            var result = await _absenceService.GetVacationBalanceAsync(userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<VacationBalanceDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacation balance for {UserId}", userId);
            return StatusCode(500, ApiResponse<VacationBalanceDto>.FailureResult("Erro al obtener saldo de vacaciones."));
        }
    }
}