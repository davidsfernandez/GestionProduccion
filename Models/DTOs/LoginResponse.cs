using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Models.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public User? User { get; set; }
}