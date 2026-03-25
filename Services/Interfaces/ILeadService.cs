using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ILeadService
{
    Task<LeadDto> CreateLeadAsync(CreateLeadDto dto, CancellationToken ct = default);
    Task<List<LeadDto>> GetLeadsAsync(CancellationToken ct = default);
    Task<LeadDto> UpdateLeadStatusAsync(int leadId, Domain.Enums.LeadStatus newStatus, string? note = null, CancellationToken ct = default);
}