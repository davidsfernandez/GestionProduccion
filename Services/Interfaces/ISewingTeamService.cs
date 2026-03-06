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

public interface ISewingTeamService
{
    Task<List<SewingTeamDto>> GetAllTeamsAsync();
    Task<SewingTeamDto?> GetTeamByIdAsync(int id);
    Task<SewingTeamDto> CreateTeamAsync(CreateSewingTeamRequest request);
    Task<SewingTeamDto> UpdateTeamAsync(int id, SewingTeamDto dto);
    Task<bool> DeleteTeamAsync(int id);
    Task<bool> ToggleTeamStatusAsync(int id);
}


