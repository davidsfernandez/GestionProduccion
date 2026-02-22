using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Models.DTOs;

public class CreateTaskDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int AssignedUserId { get; set; }

    public DateTime? Deadline { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime? Deadline { get; set; }
    public double ProgressPercentage { get; set; }
}
