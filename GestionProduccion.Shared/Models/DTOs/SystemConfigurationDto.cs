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

public class SystemConfigurationDto
{
    [StringLength(100, ErrorMessage = "O nome da empresa não pode exceder 100 caracteres")]
    public string? CompanyName { get; set; }

    [StringLength(20, ErrorMessage = "O CNPJ/TaxId não pode exceder 20 caracteres")]
    public string? CompanyTaxId { get; set; }

    public string? LogoBase64 { get; set; }

    [Range(0, 1000000, ErrorMessage = "O custo fixo diário deve ser entre 0 e 1.000.000")]
    public decimal DailyFixedCost { get; set; }

    [Range(0, 10000, ErrorMessage = "O custo operacional por hora deve ser entre 0 e 10.000")]
    public decimal OperationalHourlyCost { get; set; }

    [StringLength(50)]
    public string? ThemeName { get; set; }

    [Range(1, 10000, ErrorMessage = "A meta diária deve ser entre 1 e 10.000")]
    public int DailyGoal { get; set; } = 500;

    [StringLength(500, ErrorMessage = "O anúncio de TV não puede exceder 500 caracteres")]
    public string? TvAnnouncement { get; set; }
}

public class LogoDto
{
    public string? Base64Image { get; set; }
}
