using System.Text.Json.Serialization;

namespace GestionProduccion.Client.Models
{
    public class LoginResult
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("usuario")]
        public UsuarioInfo? Usuario { get; set; }
    }

    public class UsuarioInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("perfil")]
        public string Perfil { get; set; } = string.Empty;
    }
}
