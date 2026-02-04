using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class OpService : IOpService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ProducaoHub> _hubContext;

    public OpService(AppDbContext context, IHubContext<ProducaoHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<OrdemProducao> CriarOP(OrdemProducao op)
    {
        op.EtapaAtual = EtapaProducao.Corte;
        op.StatusAtual = StatusProducao.EmProducao;
        op.DataCriacao = DateTime.UtcNow;

        _context.OrdensProducao.Add(op);

        // El usuario responsable del cambio inicial podría ser un usuario del sistema o un ID predefinido.
        // Aquí usaremos un ID ficticio 1 para el usuario 'Sistema/Admin'.
        // En una aplicación real, esto debería obtenerse del usuario autenticado.
        await AdicionarHistorico(op, null, op.EtapaAtual, null, op.StatusAtual, 1, "Criação da OP");

        await _context.SaveChangesAsync();
        return op;
    }

    public async Task<OrdemProducao> DelegarTarefa(int opId, int usuarioId)
    {
        var op = await _context.OrdensProducao.FindAsync(opId);
        if (op == null)
        {
            throw new KeyNotFoundException("Ordem de Produção não encontrada.");
        }

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado.");
        }
        
        if (usuario.Perfil != PerfilUsuario.Costureira && usuario.Perfil != PerfilUsuario.Oficina)
        {
            throw new InvalidOperationException("A tarefa só pode ser delegada a uma 'Costureira' ou 'Oficina'.");
        }

        op.UsuarioId = usuarioId;
        _context.Entry(op).State = EntityState.Modified;
        
        // Asumimos que el usuario que delega la tarea es el responsable del cambio.
        // En un escenario real, este Id vendría del contexto de autenticación.
        await AdicionarHistorico(op, op.EtapaAtual, op.EtapaAtual, op.StatusAtual, op.StatusAtual, 1, $"Delegada para {usuario.Nome}");

        await _context.SaveChangesAsync();
        return op;
    }

    public async Task<OrdemProducao> AtualizarStatus(int opId, StatusProducao novoStatus, string observacao)
    {
        var op = await _context.OrdensProducao.FindAsync(opId);
        if (op == null)
        {
            throw new KeyNotFoundException("Ordem de Produção não encontrada.");
        }

        var statusAnterior = op.StatusAtual;
        op.StatusAtual = novoStatus;

        _context.Entry(op).State = EntityState.Modified;
        await AdicionarHistorico(op, op.EtapaAtual, op.EtapaAtual, statusAnterior, novoStatus, 1, observacao);

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", op.Id, op.EtapaAtual.ToString(), op.StatusAtual.ToString());

        return op;
    }

    public async Task<OrdemProducao> AvancarEtapa(int opId)
    {
        var op = await _context.OrdensProducao.FindAsync(opId);
        if (op == null)
        {
            throw new KeyNotFoundException("Ordem de Produção não encontrada.");
        }

        var etapaAnterior = op.EtapaAtual;
        var statusAnterior = op.StatusAtual;

        var novaEtapa = etapaAnterior switch
        {
            EtapaProducao.Corte => EtapaProducao.Costura,
            EtapaProducao.Costura => EtapaProducao.Revisao,
            EtapaProducao.Revisao => EtapaProducao.Embalagem,
            EtapaProducao.Embalagem => throw new InvalidOperationException("A OP já está na etapa final."),
            _ => throw new InvalidOperationException("Etapa de produção desconhecida.")
        };
        
        op.EtapaAtual = novaEtapa;
        op.StatusAtual = StatusProducao.EmProducao; // Resetea el status

        _context.Entry(op).State = EntityState.Modified;
        await AdicionarHistorico(op, etapaAnterior, novaEtapa, statusAnterior, op.StatusAtual, 1, $"Avançou para {novaEtapa}");

        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", op.Id, op.EtapaAtual.ToString(), op.StatusAtual.ToString());

        return op;
    }

    public async Task<DashboardDto> ObterDashboard()
    {
        var dashboard = new DashboardDto
        {
            OpsPorEtapa = await _context.OrdensProducao
                .GroupBy(op => op.EtapaAtual)
                .Select(g => new { Etapa = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Etapa, x => x.Count),

            OpsParadas = await _context.OrdensProducao
                .Where(op => op.StatusAtual == StatusProducao.Parado)
                .ToListAsync(),

            OpsPorUsuario = await _context.OrdensProducao
                .Where(op => op.UsuarioId.HasValue)
                .Include(op => op.UsuarioAtribuido)
                .GroupBy(op => op.UsuarioAtribuido!.Nome)
                .ToDictionaryAsync(g => g.Key, g => g.ToList())
        };

        return dashboard;
    }
    
    private async Task AdicionarHistorico(OrdemProducao op, EtapaProducao? etapaAnterior, EtapaProducao etapaNova, StatusProducao? statusAnterior, StatusProducao statusNovo, int usuarioId, string observacao)
    {
        var historico = new HistoricoProducao
        {
            OrdemProducaoId = op.Id,
            EtapaAnterior = etapaAnterior,
            EtapaNova = etapaNova,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            UsuarioId = usuarioId, // El ID del usuario que realiza la acción
            DataModificacao = DateTime.UtcNow
            // Podríamos agregar un campo 'Observacao' al histórico si fuera necesario
        };
        _context.HistoricoProducoes.Add(historico);
    }
}
