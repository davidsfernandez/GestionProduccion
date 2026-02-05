using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints del controlador
public class OpsController : ControllerBase
{
    private readonly IOpService _opService;

    public OpsController(IOpService opService)
    {
        _opService = opService;
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Lider")] // Solo roles específicos pueden crear
    public async Task<IActionResult> CriarOP([FromBody] Domain.Entities.OrdemProducao op)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var novaOp = await _opService.CriarOP(op);
        return CreatedAtAction(nameof(GetOpById), new { id = novaOp.Id }, novaOp);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOpById(int id)
    {
        // Esta lógica debería existir en el servicio, pero para simplicidad la omitimos.
        // En una app real, tendríamos un método _opService.GetById(id);
        return Ok($"Endpoint para obtener OP con Id {id}. Implementación pendiente.");
    }

    [HttpPost("{opId}/delegar/{usuarioId}")]
    [Authorize(Roles = "Administrador,Lider")]
    public async Task<IActionResult> DelegarTarefa(int opId, int usuarioId)
    {
        try
        {
            var opAtualizada = await _opService.DelegarTarefa(opId, usuarioId);
            return Ok(opAtualizada);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{opId}/status")]
    [Authorize(Roles = "Administrador,Lider")]
    public async Task<IActionResult> AtualizarStatus(int opId, [FromBody] AtualizarStatusRequest request)
    {
        try
        {
            var opAtualizada = await _opService.AtualizarStatus(opId, request.NovoStatus, request.Observacao);
            return Ok(opAtualizada);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{opId}/avancar")]
    [Authorize(Roles = "Administrador,Lider")]
    public async Task<IActionResult> AvancarEtapa(int opId)
    {
        try
        {
            var opAtualizada = await _opService.AvancarEtapa(opId);
            return Ok(opAtualizada);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.InvalidOperationException ex)
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
