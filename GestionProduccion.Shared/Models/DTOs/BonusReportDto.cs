using System;
using System.Collections.Generic;

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
    
    // Details for transparency
    public int CompletedOrders { get; set; }
    public int OnTimeOrders { get; set; }
    public int TotalProduced { get; set; }
    public int TotalDefects { get; set; }
    public List<OrderBonusDetail> Orders { get; set; } = new List<OrderBonusDetail>();
}

public class OrderBonusDetail
{
    public string UniqueCode { get; set; } = string.Empty;
    public bool IsOnTime { get; set; }
    public int Defects { get; set; }
    public decimal Contribution { get; set; }
}
