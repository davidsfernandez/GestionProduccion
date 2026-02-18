using System.Text.Json.Serialization;

namespace GestionProduccion.Client.Models.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Administrator,
    Leader,
    Operator,
    Workshop,
    Sewer
}
