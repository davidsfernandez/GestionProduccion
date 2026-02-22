using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;

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

    public async Task<QADefect> RegisterDefectAsync(CreateQADefectDto dto)
    {
        string? photoUrl = null;
        if (dto.PhotoFile != null)
        {
            // Validate Image
            var extension = Path.GetExtension(dto.PhotoFile.FileName).ToLower();
            if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                throw new ArgumentException("Invalid file type. Only JPG and PNG allowed.");

            if (dto.PhotoFile.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit.");

            var fileName = await _fileStorage.UploadAsync(dto.PhotoFile, "defects");
            photoUrl = _fileStorage.GetUrl(fileName, "defects");
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
        var all = await _defectRepo.GetAllAsync();
        return all.Where(d => d.ProductionOrderId == orderId).ToList();
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
