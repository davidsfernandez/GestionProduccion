using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Client.Models;

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
