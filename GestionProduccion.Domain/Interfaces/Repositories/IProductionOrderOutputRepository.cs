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
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IProductionOrderOutputRepository
{
    Task AddAsync(ProductionOrderOutput output);
    Task<IEnumerable<ProductionOrderOutput>> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<ProductionOrderOutput>> GetByTeamAndDateRangeAsync(int teamId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionOrderOutput>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<int> GetTotalQuantityByOrderAndStageAsync(int orderId, ProductionStage stage);
    Task SaveChangesAsync();
}


