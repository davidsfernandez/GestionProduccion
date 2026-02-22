using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Client.Models;

public class ResetPasswordModel
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
