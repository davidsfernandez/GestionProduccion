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

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        if (await _productRepository.ExistsAsync(product.MainSku))
        {
            throw new InvalidOperationException($"Product with SKU '{product.MainSku}' already exists.");
        }

        await _productRepository.AddAsync(product);
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        var existing = await _productRepository.GetByIdAsync(product.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (existing.MainSku != product.MainSku && await _productRepository.ExistsAsync(product.MainSku))
        {
            throw new InvalidOperationException($"Product with SKU '{product.MainSku}' already exists.");
        }

        // Update properties
        existing.Name = product.Name;
        existing.InternalCode = product.InternalCode;
        existing.FabricType = product.FabricType;
        existing.MainSku = product.MainSku;
        existing.EstimatedSalePrice = product.EstimatedSalePrice;
        
        // Update sizes logic (simplistic replace for now, could be improved)
        existing.Sizes.Clear();
        foreach (var size in product.Sizes)
        {
            existing.Sizes.Add(size);
        }

        await _productRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            await _productRepository.DeleteAsync(product);
        }
    }
}
