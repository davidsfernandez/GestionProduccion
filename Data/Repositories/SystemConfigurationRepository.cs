using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly AppDbContext _context;

    public SystemConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var config = await _context.SystemConfigurations.FirstOrDefaultAsync(c => c.Key == key);
        return config?.Value;
    }

    public async Task SetValueAsync(string key, string? value)
    {
        var config = await _context.SystemConfigurations.FirstOrDefaultAsync(c => c.Key == key);
        if (config == null)
        {
            config = new SystemConfiguration { Key = key, Value = value };
            await _context.SystemConfigurations.AddAsync(config);
        }
        else
        {
            config.Value = value;
            _context.SystemConfigurations.Update(config);
        }
        await _context.SaveChangesAsync();
    }
}
