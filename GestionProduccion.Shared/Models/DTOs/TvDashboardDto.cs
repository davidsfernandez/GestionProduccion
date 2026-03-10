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

/// <summary>
/// DTO optimizado para el Modo TV. 
/// Excluye PII (Información Personal Identificable) y datos financieros.
/// </summary>
public class TvDashboardDto
{
    public int CompletedToday { get; set; }
    public int DailyGoal { get; set; }
    public double CompletionRate { get; set; } // Eficiencia del día
    public double AverageTimePerPieceMinutes { get; set; }
    public int ActiveOrders { get; set; }
    public string? TvAnnouncement { get; set; } // Mensaje motivador/urgente solicitado por Igor
    public List<TvProductionItemDto> ProductionItems { get; set; } = new();
}

public class TvProductionItemDto
{
    public string LotCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
