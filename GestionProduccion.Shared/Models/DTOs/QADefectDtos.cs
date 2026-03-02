using System;

namespace GestionProduccion.Models.DTOs;

public class QADefectDto
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime ReportedAt { get; set; }
}

public class CreateQADefectDto
{
    public int ProductionOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReportedByUserId { get; set; }
}
