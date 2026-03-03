using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class QAService : IQAService
{
    private readonly IRepository<QADefect> _defectRepo;
    private readonly IFileStorageService _fileStorage;

    public QAService(IRepository<QADefect> defectRepo, IFileStorageService fileStorage)
    {
        _defectRepo = defectRepo;
        _fileStorage = fileStorage;
    }

    public async Task<QADefect> RegisterDefectAsync(CreateQADefectDto dto, IFormFile? photoFile = null)
    {
        string? photoUrl = null;

        if (photoFile != null)
        {
            photoUrl = await _fileStorage.UploadAsync(photoFile, "defects");
        }

        var defect = new QADefect
        {
            ProductionOrderId = dto.ProductionOrderId,
            Reason = dto.Reason,
            Quantity = dto.Quantity,
            PhotoUrl = photoUrl,
            ReportedByUserId = dto.ReportedByUserId
        };

        await _defectRepo.AddAsync(defect);
        await _defectRepo.SaveChangesAsync();
        return defect;
    }

    public async Task<List<QADefect>> GetDefectsByOrderAsync(int orderId)
    {
        var query = await _defectRepo.GetQueryableAsync();
        return await query.AsNoTracking()
            .Where(d => d.ProductionOrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<QADefect>> GetDefectsByOrdersAsync(IEnumerable<int> orderIds)
    {
        if (orderIds == null || !orderIds.Any()) return new List<QADefect>();

        var query = await _defectRepo.GetQueryableAsync();
        return await query.AsNoTracking()
            .Where(d => orderIds.Contains(d.ProductionOrderId))
            .ToListAsync();
    }

    public async Task DeleteDefectAsync(int id)
    {
        var defect = await _defectRepo.GetByIdAsync(id);
        if (defect != null)
        {
            if (!string.IsNullOrEmpty(defect.PhotoUrl))
            {
                var fileName = Path.GetFileName(defect.PhotoUrl);
                await _fileStorage.DeleteAsync(fileName, "defects");
            }
            await _defectRepo.DeleteAsync(defect);
            await _defectRepo.SaveChangesAsync();
        }
    }
}
