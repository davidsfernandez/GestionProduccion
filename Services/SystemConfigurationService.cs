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
    private readonly ISystemConfigurationRepository _repository;
    private const string CONFIG_KEY = "MainConfig";
    private const string TV_ANNOUNCEMENT_KEY = "TvAnnouncement";

    public SystemConfigurationService(ISystemConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemConfigurationDto> GetConfigurationAsync()
    {
        var config = await _repository.GetByKeyAsync(CONFIG_KEY);
        var announcement = await _repository.GetValueByKeyAsync(TV_ANNOUNCEMENT_KEY);

        if (config == null)
        {
            return new SystemConfigurationDto 
            { 
                CompanyName = "Serona ERP", 
                TvAnnouncement = announcement ?? "Foco total na produção!" 
            };
        }

        return new SystemConfigurationDto
        {
            CompanyName = config.CompanyName,
            CompanyTaxId = config.CompanyTaxId,
            LogoBase64 = config.LogoBase64,
            DailyFixedCost = config.DailyFixedCost,
            OperationalHourlyCost = config.OperationalHourlyCost,
            ThemeName = config.ThemeName,
            TvAnnouncement = announcement
        };
    }

    public async Task<PublicConfigurationDto> GetPublicConfigurationAsync()
    {
        var config = await _repository.GetByKeyAsync(CONFIG_KEY);
        if (config == null) return new PublicConfigurationDto { CompanyName = "Serona ERP" };

        return new PublicConfigurationDto
        {
            CompanyName = config.CompanyName,
            LogoBase64 = config.LogoBase64,
            ThemeName = config.ThemeName
        };
    }

    public async Task<string> GetLogoAsync()
    {
        var config = await _repository.GetByKeyAsync(CONFIG_KEY);
        return config?.LogoBase64 ?? string.Empty;
    }

    public async Task<bool> SaveConfigurationAsync(SystemConfigurationDto dto)
    {
        if (dto == null) return false;

        // 1. Business Validations
        if (dto.DailyFixedCost < 0 || dto.OperationalHourlyCost < 0)
            throw new InvalidOperationException("Os custos financeiros não podem ser negativos.");

        if (!string.IsNullOrEmpty(dto.LogoBase64) && !IsValidImage(dto.LogoBase64))
            throw new InvalidOperationException("O formato do logotipo é inválido (use PNG/JPG).");

        // 2. Persist Main Config
        var existing = await _repository.GetByKeyAsync(CONFIG_KEY);
        if (existing == null)
        {
            existing = new SystemConfiguration { Key = CONFIG_KEY };
            await _repository.AddAsync(existing);
        }

        existing.CompanyName = dto.CompanyName;
        existing.CompanyTaxId = dto.CompanyTaxId;
        existing.LogoBase64 = dto.LogoBase64;
        existing.DailyFixedCost = dto.DailyFixedCost;
        existing.OperationalHourlyCost = dto.OperationalHourlyCost;
        existing.ThemeName = dto.ThemeName;

        await _repository.UpdateAsync(existing);

        // 3. Persist TV Announcement separately (Side channel for Igor's request)
        if (dto.TvAnnouncement != null)
        {
            await _repository.SaveOrUpdateValueAsync(TV_ANNOUNCEMENT_KEY, dto.TvAnnouncement);
        }

        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLogoAsync(string base64Logo)
    {
        if (string.IsNullOrEmpty(base64Logo) || !IsValidImage(base64Logo)) return false;

        var config = await _repository.GetByKeyAsync(CONFIG_KEY);
        if (config == null)
        {
            config = new SystemConfiguration { Key = CONFIG_KEY };
            await _repository.AddAsync(config);
        }

        config.LogoBase64 = base64Logo;
        await _repository.UpdateAsync(config);
        await _repository.SaveChangesAsync();
        return true;
    }

    private bool IsValidImage(string base64)
    {
        // Basic check for PNG/JPEG headers in Base64
        return base64.Contains("data:image/png;base64") || 
               base64.Contains("data:image/jpeg;base64") || 
               base64.Contains("data:image/jpg;base64");
    }
}
