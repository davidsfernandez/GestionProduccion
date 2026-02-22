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

    [Required]
    public string Description { get; set; } = string.Empty;

    public int AssignedUserId { get; set; }
    [ForeignKey("AssignedUserId")]
    public virtual User AssignedUser { get; set; } = null!;

    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? Deadline { get; set; }

    public OpTaskStatus Status { get; set; } = OpTaskStatus.Pending;

    public DateTime? CompletionDate { get; set; }
}

public enum OpTaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
