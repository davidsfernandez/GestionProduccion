using System.Text.Json.Serialization;

namespace GestionProduccion.Client.Models
{
    public class LoginResult
    {
        [JsonPropertyName("Token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("RefreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("AvatarUrl")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("UserName")]
        public string? UserName { get; set; }
    }
}
