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
}

public class CreateProductDto
{
    [Required(ErrorMessage = "O nome do produto Ã© obrigatÃ³rio")]
    [StringLength(100, ErrorMessage = "O nome nÃ£o puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O cÃ³digo interno es obrigatÃ³rio")]
    [StringLength(50, ErrorMessage = "O cÃ³digo nÃ£o puede exceder 50 caracteres")]
    public string InternalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo de tecido es obrigatÃ³rio")]
    [StringLength(50, ErrorMessage = "O tipo de tecido nÃ£o puede exceder 50 caracteres")]
    public string FabricType { get; set; } = string.Empty;

    [Required(ErrorMessage = "O SKU principal es obrigatÃ³rio")]
    [StringLength(50, ErrorMessage = "O SKU nÃ£o puede exceder 50 caracteres")]
    public string MainSku { get; set; } = string.Empty;

    [Range(0.1, double.MaxValue, ErrorMessage = "O tempo mÃ©dio deve ser maior que zero")]
    public double AverageProductionTimeMinutes { get; set; }

    [Required(ErrorMessage = "O preÃ§o estimado es obrigatÃ³rio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preÃ§o deve ser maior que zero")]
    public decimal EstimatedSalePrice { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}


