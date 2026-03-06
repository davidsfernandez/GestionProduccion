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

public class BonusReportDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlinePerformance { get; set; }
    public decimal DefectPercentage { get; set; }
    public decimal FinalBonusPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public int CompletedOrders { get; set; }
    public int OnTimeOrders { get; set; }
    public int TotalProduced { get; set; }
    public int TotalDefects { get; set; }
    public string? Message { get; set; }
    public List<OrderBonusDetail> Orders { get; set; } = new();
}

public class OrderBonusDetail
{
    public string LotCode { get; set; } = string.Empty;
    public bool IsOnTime { get; set; }
    public int Defects { get; set; }
    public decimal Contribution { get; set; }
}


