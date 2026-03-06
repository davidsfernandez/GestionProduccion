/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ISystemConfigurationService
{
    Task<SystemConfigurationDto> GetConfigurationAsync();
    Task<PublicConfigurationDto> GetPublicConfigurationAsync();
    Task SaveConfigurationAsync(SystemConfigurationDto dto);

    // Legacy support
    Task<string> GetLogoAsync();
    Task UpdateLogoAsync(string base64Logo);
}

