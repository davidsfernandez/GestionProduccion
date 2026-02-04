using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IOpService
{
    Task<OrdemProducao> CriarOP(OrdemProducao op);
    Task<OrdemProducao> DelegarTarefa(int opId, int usuarioId);
    Task<OrdemProducao> AtualizarStatus(int opId, StatusProducao novoStatus, string observacao);
    Task<OrdemProducao> AvancarEtapa(int opId);
    Task<DashboardDto> ObterDashboard();
}
