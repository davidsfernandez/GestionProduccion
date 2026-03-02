using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ISewingTeamService
{
    Task<List<SewingTeamDto>> GetAllTeamsAsync();
    Task<SewingTeamDto?> GetTeamByIdAsync(int id);
    Task<SewingTeamDto> CreateTeamAsync(CreateSewingTeamRequest request);
    Task<SewingTeamDto> UpdateTeamAsync(int id, SewingTeamDto dto);
    Task<bool> DeleteTeamAsync(int id);
    Task<bool> ToggleTeamStatusAsync(int id);
}
