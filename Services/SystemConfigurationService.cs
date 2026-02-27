using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using System.Text.RegularExpressions;

namespace GestionProduccion.Services;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _repo;

    public SystemConfigurationService(ISystemConfigurationRepository repo)
    {
        _repo = repo;
    }

    public async Task<SystemConfigurationDto> GetConfigurationAsync()
    {
        var config = await _repo.GetAsync();
        if (config == null)
        {
            return new SystemConfigurationDto
            {
                CompanyName = "My Factory",
                DailyFixedCost = 0,
                OperationalHourlyCost = 0
            };
        }

        return new SystemConfigurationDto
        {
            CompanyName = config.CompanyName,
            CompanyTaxId = config.CompanyTaxId,
            LogoBase64 = config.LogoBase64,
            DailyFixedCost = config.DailyFixedCost,
            OperationalHourlyCost = config.OperationalHourlyCost
        };
    }

    public async Task<PublicConfigurationDto> GetPublicConfigurationAsync()
    {
        var config = await _repo.GetAsync();
        return new PublicConfigurationDto
        {
            CompanyName = config?.CompanyName ?? "Serona ERP",
            LogoBase64 = config?.LogoBase64
        };
    }

    public async Task SaveConfigurationAsync(SystemConfigurationDto dto)
    {
        var config = await _repo.GetAsync();
        if (config == null)
        {
            config = new SystemConfiguration();
        }

        // Validation for Logo Base64 (Basic safety check)
        if (!string.IsNullOrEmpty(dto.LogoBase64) && !IsValidImage(dto.LogoBase64))
        {
            throw new ArgumentException("Invalid image format. Only PNG and JPG are allowed.");
        }

        config.CompanyName = dto.CompanyName;
        config.CompanyTaxId = dto.CompanyTaxId;
        config.LogoBase64 = dto.LogoBase64;
        config.DailyFixedCost = dto.DailyFixedCost;
        config.OperationalHourlyCost = dto.OperationalHourlyCost;

        await _repo.UpdateAsync(config);
    }

    public async Task<string> GetLogoAsync()
    {
        var config = await _repo.GetAsync();
        return config?.LogoBase64 ?? string.Empty;
    }

    public async Task UpdateLogoAsync(string base64Logo)
    {
        var config = await _repo.GetAsync();
        if (config == null) config = new SystemConfiguration();

        if (!string.IsNullOrEmpty(base64Logo) && !IsValidImage(base64Logo))
        {
            throw new ArgumentException("Invalid image format.");
        }

        config.LogoBase64 = base64Logo;
        await _repo.UpdateAsync(config);
    }

    private bool IsValidImage(string base64)
    {
        // Check for common data URI schemes for images
        return Regex.IsMatch(base64, @"^data:image/(png|jpeg|jpg);base64,");
    }
}
