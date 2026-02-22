namespace GestionProduccion.Client.Models.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? UserName { get; set; }
    }
}