using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface ISewingTeamRepository : IRepository<SewingTeam>
{
    Task<List<SewingTeam>> GetActiveTeamsAsync();
    Task<SewingTeam?> GetTeamWithMembersAsync(int id);
    Task<User?> GetMemberByIdAsync(int userId);
}
