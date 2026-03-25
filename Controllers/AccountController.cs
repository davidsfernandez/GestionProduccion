using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ICustomerService customerService, ILogger<AccountController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterCustomerRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<UserDto>.FailureResult("Validation failed"));

            var user = await _customerService.RegisterCustomerAsync(request, HttpContext.RequestAborted);
            return Ok(ApiResponse<UserDto>.SuccessResult(user, "Conta criada com sucesso!"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<UserDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering customer");
            return StatusCode(500, ApiResponse<UserDto>.FailureResult("Erro ao criar conta."));
        }
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var profile = await _customerService.GetCustomerProfileAsync(userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<CustomerProfileDto>.SuccessResult(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer profile");
            return StatusCode(500, ApiResponse<CustomerProfileDto>.FailureResult("Erro ao carregar perfil."));
        }
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> UpdateProfile(CustomerProfileDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            if (dto.UserId != userId) return Forbid();

            var result = await _customerService.UpdateCustomerProfileAsync(dto, HttpContext.RequestAborted);
            return Ok(ApiResponse<CustomerProfileDto>.SuccessResult(result, "Perfil atualizado!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile");
            return StatusCode(500, ApiResponse<CustomerProfileDto>.FailureResult("Erro ao salvar alterações."));
        }
    }
}