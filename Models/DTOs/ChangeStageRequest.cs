using GestionProduccion.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class ChangeStageRequest
{
    [Required]
    public ProductionStage NewStage { get; set; }
    
    public string Note { get; set; } = string.Empty;
}
