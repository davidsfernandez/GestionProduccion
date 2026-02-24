using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

public class OperationalTask
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? Deadline { get; set; }

    public DateTime? CompletionDate { get; set; }

    public OpTaskStatus Status { get; set; } = OpTaskStatus.Pending;

    public int? AssignedUserId { get; set; }
    [ForeignKey("AssignedUserId")]
    public virtual User? AssignedUser { get; set; }
}
