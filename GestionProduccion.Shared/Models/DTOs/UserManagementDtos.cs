using GestionProduccion.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? Password { get; set; } // Optional
    public UserRole Role { get; set; }
}
