using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IProductionOrderService
{
    Task<ProductionOrder> CreateProductionOrder(ProductionOrder order);
    Task<ProductionOrder?> GetProductionOrderById(int orderId);
    Task<ProductionOrder> AssignTask(int orderId, int userId);
    Task<ProductionOrder> UpdateStatus(int orderId, ProductionStatus newStatus, string note);
    Task<ProductionOrder> AdvanceStage(int orderId);
    Task<DashboardDto> GetDashboard();
}
