using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Services.Interfaces;

public interface IFinancialCalculatorService
{
    Task CalculateFinalOrderCostAsync(ProductionOrder order);
}
