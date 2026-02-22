using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class CreateQADefectDto
{
    [Required]
    public int ProductionOrderId { get; set; }

    [Required]
    [StringLength(255)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }



    public int? ReportedByUserId { get; set; }
}

public class QADefectDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime RegistrationDate { get; set; }
}
