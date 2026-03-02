using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface ISystemConfigurationRepository
{
    Task<SystemConfiguration?> GetAsync();
    Task UpdateAsync(SystemConfiguration config);

    // Legacy support
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string? value);
}
