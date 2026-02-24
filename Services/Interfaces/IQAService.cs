using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IQAService
{
    Task<QADefect> RegisterDefectAsync(CreateQADefectDto dto, IFormFile? photoFile = null);
    Task<List<QADefect>> GetDefectsByOrderAsync(int orderId);
    Task DeleteDefectAsync(int id);
}
