using System.Text.Json.Serialization;

namespace GestionProduccion.Client.Models
{
    public class LoginResult
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }
    }
}
