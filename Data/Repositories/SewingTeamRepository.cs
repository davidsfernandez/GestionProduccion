using Microsoft.EntityFrameworkCore;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;

namespace GestionProduccion.Data.Repositories;

public class SewingTeamRepository : Repository<SewingTeam>, ISewingTeamRepository
{
    public SewingTeamRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<SewingTeam>> GetActiveTeamsAsync()
    {
        return await _dbSet.Where(t => t.IsActive).ToListAsync();
    }

    public async Task<SewingTeam?> GetTeamWithMembersAsync(int id)
    {
        return await _dbSet.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == id);
    }
}
