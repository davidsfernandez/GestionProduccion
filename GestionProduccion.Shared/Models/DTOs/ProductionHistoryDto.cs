/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System;

namespace GestionProduccion.Models.DTOs;

public class ProductionHistoryDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string PreviousStage { get; set; } = string.Empty;
    public string NewStage { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string Note { get; set; } = string.Empty;
}

