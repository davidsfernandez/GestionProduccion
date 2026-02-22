using GestionProduccion.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class BulkUpdateStatusRequest
{
    [Required]
    public List<int> OrderIds { get; set; } = new List<int>();

    [Required]
    public ProductionStatus NewStatus { get; set; }

    public string Note { get; set; } = string.Empty;
}
