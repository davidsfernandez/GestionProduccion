/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a size and its quantity within a Production Order.
/// </summary>
public class ProductionOrderSize
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductionOrderId { get; set; }

    [ForeignKey("ProductionOrderId")]
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Size { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }
}


