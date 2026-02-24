using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default)
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken ct = default)
    {
        if (await _productRepository.ExistsAsync(product.MainSku))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: SKU '{product.MainSku}'");
        }

        if (await _productRepository.ExistsByInternalCodeAsync(product.InternalCode))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: InternalCode '{product.InternalCode}'");
        }

        await _productRepository.AddAsync(product);
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product, CancellationToken ct = default)
    {
        var existing = await _productRepository.GetByIdAsync(product.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException(ErrorMessages.ElementNotFound);
        }

        if (existing.MainSku != product.MainSku && await _productRepository.ExistsAsync(product.MainSku))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: SKU '{product.MainSku}'");
        }

        if (existing.InternalCode != product.InternalCode && await _productRepository.ExistsByInternalCodeAsync(product.InternalCode))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: InternalCode '{product.InternalCode}'");
        }

        // Update properties
        existing.Name = product.Name;
        existing.InternalCode = product.InternalCode;
        existing.FabricType = product.FabricType;
        existing.MainSku = product.MainSku;
        existing.AverageProductionTimeMinutes = product.AverageProductionTimeMinutes;
        existing.EstimatedSalePrice = product.EstimatedSalePrice;
        
        await _productRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException(ErrorMessages.ElementNotFound);

        try
        {
            await _productRepository.DeleteAsync(product);
        }
        catch (Exception)
        {
            throw new InvalidOperationException(ErrorMessages.CannotDeleteByBusinessRules);
        }
    }
}
