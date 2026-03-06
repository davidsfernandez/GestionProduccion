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

public class QADefectDto
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime ReportedAt { get; set; }
}

public class CreateQADefectDto
{
    public int ProductionOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReportedByUserId { get; set; }
}

