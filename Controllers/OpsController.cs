using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpsController : ControllerBase
{
    private readonly IOpService _opService;

    public OpsController(IOpService opService)
    {
        _opService = opService;
    }

    [HttpPost]
    public async Task<IActionResult> CriarOP([FromBody] OrdemProducao op)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var novaOp = await _opService.CriarOP(op);
        return CreatedAtAction(nameof(GetOpById), new { id = novaOp.Id }, novaOp);
    }
    
    // Endpoint auxiliar para obtener una OP por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOpById(int id)
    {
        // Esta lógica debería existir en el servicio, pero para simplicidad la omitimos.
        // En una app real, tendríamos un método _opService.GetById(id);
        return Ok($"Endpoint para obtener OP con Id {id}. Implementación pendiente.");
    }


    [HttpPost("{opId}/delegar/{usuarioId}")]
    public async Task<IActionResult> DelegarTarefa(int opId, int usuarioId)
    {
        try
        {
            var opAtualizada = await _opService.DelegarTarefa(opId, usuarioId);
            return Ok(opAtualizada);
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

    [HttpPatch("{opId}/status")]
    public async Task<IActionResult> AtualizarStatus(int opId, [FromBody] AtualizarStatusRequest request)
    {
        try
        {
            var opAtualizada = await _opService.AtualizarStatus(opId, request.NovoStatus, request.Observacao);
            return Ok(opAtualizada);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{opId}/avancar")]
    public async Task<IActionResult> AvancarEtapa(int opId)
    {
        try
        {
            var opAtualizada = await _opService.AvancarEtapa(opId);
            return Ok(opAtualizada);
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
        var dashboard = await _opService.ObterDashboard();
        return Ok(dashboard);
    }
}

// Clase DTO para el request de actualización de status
public class AtualizarStatusRequest
{
    public StatusProducao NovoStatus { get; set; }
    public string Observacao { get; set; } = string.Empty;
}
