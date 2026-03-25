using GestionProduccion.Domain.Entities.CRM;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface ILeadRepository : IRepository<Lead>
{
    Task<List<Lead>> GetActiveLeadsAsync();
}