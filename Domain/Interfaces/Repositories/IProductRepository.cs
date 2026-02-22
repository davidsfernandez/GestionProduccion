using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<List<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(string sku);
}
