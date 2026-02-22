using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

/// <summary>
/// DTO for requesting assignment of a Production Order to a user.
/// </summary>
public class AssignTaskRequest
{
    [Required(ErrorMessage = "User ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be valid.")]
    public int UserId { get; set; }
}
