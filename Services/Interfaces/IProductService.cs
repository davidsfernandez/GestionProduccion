using GestionProduccion.Domain.Entities;
using System.Threading;

namespace GestionProduccion.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<Product> CreateProductAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateProductAsync(Product product, CancellationToken ct = default);
    Task DeleteProductAsync(int id, CancellationToken ct = default);
    Task RecalculateAverageTimeAsync(int productId, CancellationToken ct = default);
}
