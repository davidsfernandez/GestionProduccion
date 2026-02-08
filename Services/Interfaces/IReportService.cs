using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateProductionOrderReportAsync(int orderId);
    Task<byte[]> GenerateDailyProductionReportAsync();
}
