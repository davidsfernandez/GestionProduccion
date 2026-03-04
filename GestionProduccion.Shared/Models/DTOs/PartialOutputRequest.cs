using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

/// <summary>
/// Request DTO for registering partial production output.
/// </summary>
public class PartialOutputRequest
{
    /// <summary>
    /// A dictionary where the key is the SizeId and the value is the quantity completed.
    /// </summary>
    [Required]
    public Dictionary<int, int> SizeOutputs { get; set; } = new();

    /// <summary>
    /// Optional note for this partial output.
    /// </summary>
    [StringLength(255)]
    public string? Note { get; set; }
}
