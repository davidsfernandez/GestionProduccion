using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllActiveAsync();
    Task<List<User>> GetByRoleAsync(string role);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
    Task<int> CountActiveAsync();
}
