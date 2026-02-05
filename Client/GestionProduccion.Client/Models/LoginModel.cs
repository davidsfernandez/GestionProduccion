using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Client.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "admin@local.host";

        [Required]
        public string Password { get; set; } = "admin";
    }
}
