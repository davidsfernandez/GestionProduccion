using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

public class CreateLeadDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }
}

public class LeadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Message { get; set; }
    public LeadStatus Status { get; set; }
    public LeadSource Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CommercialNotes { get; set; }
}

public class UpdateLeadStatusRequest
{
    public LeadStatus NewStatus { get; set; }
    public string? Note { get; set; }
}