using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Lead>> GetActiveLeadsAsync()
    {
        return await _context.Leads
            .Where(l => l.Status != Domain.Enums.LeadStatus.Won && l.Status != Domain.Enums.LeadStatus.Lost)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}