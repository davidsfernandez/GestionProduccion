using GestionProduccion.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class ManualStageChangeRequest
{
    [Required]
    public ProductionStage NewStage { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
