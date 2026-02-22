using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface ISystemConfigurationRepository
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string? value);
}
