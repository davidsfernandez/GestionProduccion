/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

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

    public async Task<SystemConfiguration?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.Key == key);
    }

    public async Task<string?> GetValueByKeyAsync(string key)
    {
        var config = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Key == key);
        return config?.Value;
    }

    public async Task AddAsync(SystemConfiguration config)
    {
        await _context.SystemConfigurations.AddAsync(config);
    }

    public async Task UpdateAsync(SystemConfiguration config)
    {
        _context.SystemConfigurations.Update(config);
        await Task.CompletedTask;
    }

    public async Task SaveOrUpdateValueAsync(string key, string? value)
    {
        var existing = await _context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.Key == key);

        if (existing == null)
        {
            await _context.SystemConfigurations.AddAsync(new SystemConfiguration { Key = key, Value = value });
        }
        else
        {
            existing.Value = value;
            _context.SystemConfigurations.Update(existing);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
