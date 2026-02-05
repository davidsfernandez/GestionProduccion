using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

public class AtualizarStatusRequest
{
    public StatusProducao NovoStatus { get; set; }
    public string Observacao { get; set; } = string.Empty;
}
