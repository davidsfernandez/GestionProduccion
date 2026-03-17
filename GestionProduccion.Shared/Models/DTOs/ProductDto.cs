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
using GestionProduccion.Resources;

namespace GestionProduccion.Models.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string FabricType { get; set; } = string.Empty;
    public string MainSku { get; set; } = string.Empty;
    public double AverageProductionTimeMinutes { get; set; }
    public decimal EstimatedSalePrice { get; set; }
    public decimal DefaultBonusPerPiece { get; set; }
}

public class CreateProductDto
{
    [Required(ErrorMessage = Portuguese.Prod_ErrNameRequired)]
    [StringLength(100, ErrorMessage = Portuguese.Prod_ErrNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = Portuguese.Prod_ErrInternalCodeRequired)]
    [StringLength(50, ErrorMessage = Portuguese.Prod_ErrInternalCodeLength)]
    public string InternalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = Portuguese.Prod_ErrFabricRequired)]
    [StringLength(50, ErrorMessage = Portuguese.Prod_ErrFabricLength)]
    public string FabricType { get; set; } = string.Empty;

    [Required(ErrorMessage = Portuguese.Prod_ErrSkuRequired)]
    [StringLength(50, ErrorMessage = Portuguese.Prod_ErrSkuLength)]
    public string MainSku { get; set; } = string.Empty;

    [Range(0.1, double.MaxValue, ErrorMessage = Portuguese.Prod_ErrTimePositive)]
    public double AverageProductionTimeMinutes { get; set; }

    [Required(ErrorMessage = Portuguese.Prod_ErrPriceRequired)]
    [Range(0.01, double.MaxValue, ErrorMessage = Portuguese.Prod_ErrPricePositive)]
    public decimal EstimatedSalePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Bonus must be positive.")]
    public decimal DefaultBonusPerPiece { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}
