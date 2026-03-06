/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Models.DTOs;

public class SystemConfigurationDto
{
    public string? CompanyName { get; set; }
    public string? CompanyTaxId { get; set; }
    public string? LogoBase64 { get; set; }
    public decimal DailyFixedCost { get; set; }
    public decimal OperationalHourlyCost { get; set; }
    public string? ThemeName { get; set; }
    public string? TvAnnouncement { get; set; }
}

public class LogoDto
{
    public string? Base64Image { get; set; }
}


