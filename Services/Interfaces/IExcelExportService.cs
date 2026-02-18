using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

public interface IExcelExportService
{
    Task<byte[]> ExportProductionOrdersToExcelAsync(FilterProductionOrderDto? filter);
}
