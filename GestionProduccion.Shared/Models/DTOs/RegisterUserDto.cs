using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class RegisterUserDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? CompanyTaxId { get; set; }
    public string? LogoBase64 { get; set; }
}
