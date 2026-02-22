using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Client.Models.DTOs;

public class UpdateFinancialConfigDto
{
    [Range(0, double.MaxValue)]
    public decimal DailyFixedCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OperationalHourlyCost { get; set; }
}
