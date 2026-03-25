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

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string InternalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FabricType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MainSku { get; set; } = string.Empty;

    public double AverageProductionTimeMinutes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedSalePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultBonusPerPiece { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [MaxLength(200)]
    public string? AvailableSizes { get; set; } // e.g. "P, M, G, GG"

    [MaxLength(200)]
    public string? AvailableColors { get; set; } // e.g. "Preto, Branco, Azul"
}
