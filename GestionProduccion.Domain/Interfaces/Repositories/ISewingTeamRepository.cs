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

public interface ISewingTeamRepository : IRepository<SewingTeam>
{
    Task<List<SewingTeam>> GetActiveTeamsAsync();
    Task<SewingTeam?> GetTeamWithMembersAsync(int id);
    Task<User?> GetMemberByIdAsync(int userId);
}


