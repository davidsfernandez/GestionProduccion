using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ISystemConfigurationService
{
    Task<SystemConfigurationDto> GetConfigurationAsync();
    Task SaveConfigurationAsync(SystemConfigurationDto dto);
    
    // Legacy support
    Task<string> GetLogoAsync();
    Task UpdateLogoAsync(string base64Logo);
}
