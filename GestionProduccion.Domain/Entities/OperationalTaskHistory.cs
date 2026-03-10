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
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

public class OperationalTaskHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OperationalTaskId { get; set; }
    [ForeignKey("OperationalTaskId")]
    public virtual OperationalTask OperationalTask { get; set; } = null!;

    public OpTaskStatus? PreviousStatus { get; set; }

    [Required]
    public OpTaskStatus NewStatus { get; set; }

    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User ResponsibleUser { get; set; } = null!;

    [StringLength(500)]
    public string? Note { get; set; }
}
