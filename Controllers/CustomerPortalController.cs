using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class CustomerPortalController : ControllerBase
{
    private readonly ICustomerOrderService _orderService;
    private readonly IQuoteService _quoteService;
    private readonly ILeadService _leadService;
    private readonly ILogger<CustomerPortalController> _logger;

    public CustomerPortalController(ICustomerOrderService orderService, IQuoteService quoteService, ILeadService leadService, ILogger<CustomerPortalController> logger)
    {
        _orderService = orderService;
        _quoteService = quoteService;
        _leadService = leadService;
        _logger = logger;
    }

    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<List<ProductionOrderDto>>>> GetMyOrders()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _orderService.GetCustomerOrdersAsync(userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<List<ProductionOrderDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders");
            return StatusCode(500, ApiResponse<List<ProductionOrderDto>>.FailureResult("Erro ao carregar seus pedidos."));
        }
    }

    [HttpGet("orders/{id}")]
    public async Task<ActionResult<ApiResponse<ProductionOrderDto>>> GetOrderDetails(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _orderService.GetCustomerOrderDetailsAsync(id, userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<ProductionOrderDto>.SuccessResult(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ProductionOrderDto>.FailureResult("Pedido não encontrado."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for customer");
            return StatusCode(500, ApiResponse<ProductionOrderDto>.FailureResult("Erro ao carregar detalhes do pedido."));
        }
    }

    [HttpPost("orders/{id}/reorder")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> Reorder(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            // Verify ownership
            var order = await _orderService.GetCustomerOrderDetailsAsync(id, userId, HttpContext.RequestAborted);
            
            // Create a new lead based on the previous order
            var leadDto = new CreateLeadDto
            {
                Name = order.ClientName ?? User.Identity?.Name ?? "Cliente",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Message = $"RE-PEDIDO do lote #{order.LotCode}. Producto: {order.ProductName} (Qtd: {order.Quantity})"
            };

            var result = await _leadService.CreateLeadAsync(leadDto, HttpContext.RequestAborted);
            return Ok(ApiResponse<LeadDto>.SuccessResult(result, "Sua solicitação de re-pedido foi enviada! Igor entrará em contato em breve."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reorder");
            return StatusCode(500, ApiResponse<LeadDto>.FailureResult("Erro ao processar re-pedido."));
        }
    }

    [HttpGet("quotes")]
    public async Task<ActionResult<ApiResponse<List<QuoteDto>>>> GetMyQuotes()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _quoteService.GetCustomerQuotesAsync(userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<List<QuoteDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer quotes");
            return StatusCode(500, ApiResponse<List<QuoteDto>>.FailureResult("Erro ao carregar orçamentos."));
        }
    }

    [HttpPost("quotes/{id}/approve")]
    public async Task<ActionResult<ApiResponse<QuoteDto>>> ApproveQuote(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            // Verify ownership first
            var quote = await _quoteService.GetQuoteByIdAsync(id, HttpContext.RequestAborted);
            if (quote.CustomerUserId != userId) return Forbid();

            // Implementation detail: Quote status transition to Approved
            var result = await _quoteService.UpdateQuoteStatusAsync(id, Domain.Entities.CRM.QuoteStatus.Approved, HttpContext.RequestAborted);
            return Ok(ApiResponse<QuoteDto>.SuccessResult(result, "Orçamento aprovado com sucesso! Igor iniciará a produção em breve."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving quote");
            return StatusCode(500, ApiResponse<QuoteDto>.FailureResult("Erro ao aprovar orçamento."));
        }
    }}