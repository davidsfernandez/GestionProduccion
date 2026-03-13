/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class ProductionOrderOutputRepository : IProductionOrderOutputRepository
{
    private readonly AppDbContext _context;

    public ProductionOrderOutputRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ProductionOrderOutput output)
    {
        await _context.ProductionOrderOutputs.AddAsync(output);
    }

    public async Task<IEnumerable<ProductionOrderOutput>> GetByOrderIdAsync(int orderId)
    {
        return await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.ProductionOrderId == orderId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderOutput>> GetByTeamAndDateRangeAsync(int teamId, DateTime startDate, DateTime endDate)
    {
        // Normalize dates to ensure we cover the full range
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);

        return await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Join(_context.Users,
                output => output.UserId,
                user => user.Id,
                (output, user) => new { output, user })
            .Where(x => x.user.SewingTeamId == teamId &&
                        x.output.CreatedAt >= start && x.output.CreatedAt <= end)
            .Select(x => x.output)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderOutput>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        // Normalize dates to ensure we cover the full range (inclusive)
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);

        return await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.UserId == userId &&
                        o.CreatedAt >= start && o.CreatedAt <= end)
            .ToListAsync();
    }

    public async Task<int> GetTotalQuantityByOrderAndStageAsync(int orderId, ProductionStage stage)
    {
        return await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.ProductionOrderId == orderId && o.Stage == stage)
            .SumAsync(o => o.Quantity);
    }

    public async Task<IQueryable<ProductionOrderOutput>> GetQueryableAsync()
    {
        return await Task.FromResult(_context.ProductionOrderOutputs.AsQueryable());
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


