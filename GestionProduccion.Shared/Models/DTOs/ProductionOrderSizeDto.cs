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

/// <summary>
/// Represents a size and its quantity within a Production Order (DTO).
/// </summary>
public class ProductionOrderSizeDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Quantity { get; set; }
    
    /// <summary>
    /// Quantity already completed in the current stage of the production order.
    /// </summary>
    public int CompletedInCurrentStage { get; set; }
}


