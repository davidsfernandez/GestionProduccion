using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .FirstOrDefaultAsync(p => p.MainSku == sku);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string sku)
    {
        return await _context.Products.AnyAsync(p => p.MainSku == sku);
    }
}
