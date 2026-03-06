/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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

