using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Registra el historial de cambios de estado y etapa de una Orden de Producción.
/// </summary>
public class HistoricoProducao
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrdemProducaoId { get; set; }
    [ForeignKey("OrdemProducaoId")]
    public virtual OrdemProducao OrdemProducao { get; set; } = null!;

    // El estado anterior puede ser nulo para el primer registro del historial
    public EtapaProducao? EtapaAnterior { get; set; }
    
    [Required]
    public EtapaProducao EtapaNova { get; set; }

    // El estado anterior puede ser nulo para el primer registro del historial
    public StatusProducao? StatusAnterior { get; set; }
    
    [Required]
    public StatusProducao StatusNovo { get; set; }

    [Required]
    public int UsuarioId { get; set; } // Usuario que realizó el cambio
    [ForeignKey("UsuarioId")]
    public virtual Usuario UsuarioResponsavel { get; set; } = null!;

    [Required]
    public DateTime DataModificacao { get; set; }
}
