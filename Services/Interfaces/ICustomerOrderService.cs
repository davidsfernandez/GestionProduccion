using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

public interface ICustomerOrderService
{
    Task<List<ProductionOrderDto>> GetCustomerOrdersAsync(int customerUserId, CancellationToken ct = default);
    Task<ProductionOrderDto> GetCustomerOrderDetailsAsync(int orderId, int customerUserId, CancellationToken ct = default);
}