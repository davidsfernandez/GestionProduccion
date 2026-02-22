using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.ProductionOrders;

public interface IProductionOrderLifecycleService
{
    Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default);
    Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
}
