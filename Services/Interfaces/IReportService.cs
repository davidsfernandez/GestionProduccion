using System.Threading.Tasks;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;

namespace GestionProduccion.Services.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateProductionOrderReportAsync(int orderId, string baseUrl);
    Task<byte[]> GenerateDailyProductionReportAsync();
    Task<byte[]> GenerateOrdersCsvAsync(List<ProductionOrderDto> orders);
}
