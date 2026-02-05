using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

public class UpdateStatusRequest
{
    [Required]
    public ProductionStatus NewStatus { get; set; }
    
    public string Note { get; set; } = string.Empty;
}