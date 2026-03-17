using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class UpdateProductionOrderRequest
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? ClientName { get; set; }

    [Required]
    public DateTime EstimatedCompletionAt { get; set; }

    public int? UserId { get; set; }
    public int? SewingTeamId { get; set; }
    public decimal AppliedBonusPerPiece { get; set; }

    /// <summary>
    /// If provided, updates the sizes/quantities. 
    /// Note: Business rules might prevent this if production has already started.
    /// </summary>
    public List<ProductionOrderSizeDto>? Sizes { get; set; }
}
