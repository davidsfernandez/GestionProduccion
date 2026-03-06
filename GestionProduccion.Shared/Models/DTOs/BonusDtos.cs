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

public class BonusRuleDto
{
    public int Id { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlineBonusPercentage { get; set; }
    public decimal DefectLimitPercentage { get; set; }
    public decimal DelayPenaltyPercentage { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class UpdateBonusRuleDto
{
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlineBonusPercentage { get; set; }
    public decimal DefectLimitPercentage { get; set; }
    public decimal DelayPenaltyPercentage { get; set; }
}

