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

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface ISystemConfigurationRepository
{
    Task<SystemConfiguration?> GetByKeyAsync(string key);
    Task<string?> GetValueByKeyAsync(string key);
    Task AddAsync(SystemConfiguration config);
    Task UpdateAsync(SystemConfiguration config);
    Task SaveOrUpdateValueAsync(string key, string? value);
    Task SaveChangesAsync();
}


