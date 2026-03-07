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
        var tvAnnouncement = await _repo.GetValueAsync("TvAnnouncement");

        if (config == null)
        {
            return new SystemConfigurationDto
            {
                CompanyName = "Gestão de Produção",
                DailyFixedCost = 0,
                OperationalHourlyCost = 0,
                TvAnnouncement = tvAnnouncement,
                ThemeName = "default"
            };
        }

        return new SystemConfigurationDto
        {
            CompanyName = config.CompanyName,
            CompanyTaxId = config.CompanyTaxId,
            LogoBase64 = config.LogoBase64,
            DailyFixedCost = config.DailyFixedCost,
            OperationalHourlyCost = config.OperationalHourlyCost,
            ThemeName = config.ThemeName ?? "default",
            TvAnnouncement = tvAnnouncement
        };
    }

    public async Task<PublicConfigurationDto> GetPublicConfigurationAsync()
    {
        var config = await _repo.GetAsync();
        return new PublicConfigurationDto
        {
            CompanyName = config?.CompanyName ?? "Gestão de Produção",
            LogoBase64 = config?.LogoBase64,
            ThemeName = config?.ThemeName ?? "default"
        };
    }

    public async Task SaveConfigurationAsync(SystemConfigurationDto dto)
    {
        var config = await _repo.GetAsync();
        if (config == null)
        {
            config = new SystemConfiguration();
        }

        if (!string.IsNullOrEmpty(dto.LogoBase64) && !IsValidImage(dto.LogoBase64))
        {
            throw new ArgumentException("Invalid image format. Only PNG and JPG are allowed.");
        }

        config.CompanyName = dto.CompanyName;
        config.CompanyTaxId = dto.CompanyTaxId;
        config.LogoBase64 = dto.LogoBase64;
        config.DailyFixedCost = dto.DailyFixedCost;
        config.OperationalHourlyCost = dto.OperationalHourlyCost;
        config.ThemeName = dto.ThemeName;

        await _repo.UpdateAsync(config);
        await _repo.SetValueAsync("TvAnnouncement", dto.TvAnnouncement);
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
        return Regex.IsMatch(base64, @"^data:image/(png|jpeg|jpg);base64,");
    }
}
