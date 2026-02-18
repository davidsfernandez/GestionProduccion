using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

public interface IProductionOrderService
{
    // CREATE
    Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId);
    
    // READ
    Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id);
    Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter);
    
    // ASSIGN TASK
    Task<bool> AssignTaskAsync(int orderId, int userId);
    
    // WORKFLOW: ADVANCE STAGE
    Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId);

    // WORKFLOW: CHANGE STAGE (Allows rework/rollback)
    Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId);
    
    // UPDATE STATUS
    Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId);

    // BULK UPDATE
    Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId);
    
    // DASHBOARD
    Task<DashboardDto> GetDashboardAsync();
    
    // HISTORY
    Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId);
}
