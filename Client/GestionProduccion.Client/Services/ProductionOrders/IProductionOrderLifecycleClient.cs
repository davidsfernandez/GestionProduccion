using GestionProduccion.Client.Models.DTOs;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GestionProduccion.Client.Services.ProductionOrders;

public interface IProductionOrderLifecycleClient
{
    Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default);
    Task<bool> AdvanceStageAsync(int orderId, CancellationToken ct = default);
    Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, CancellationToken ct = default);
    Task<BulkUpdateResult?> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, CancellationToken ct = default);
}
