using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Representa una Orden de Producción (OP).
/// </summary>
public class OrdemProducao
{
    [Key]
    public int Id { get; set; }

    // NOTA: Para garantizar la unicidad de 'CodigoUnico' a nivel de base de datos,
    // es recomendable configurar un índice único en su DbContext:
    // modelBuilder.Entity<OrdemProducao>().HasIndex(op => op.CodigoUnico).IsUnique();
    [Required]
    [StringLength(50)]
    public string CodigoUnico { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DescricaoProduto { get; set; } = string.Empty;

    [Required]
    public int Quantidade { get; set; }

    [Required]
    public EtapaProducao EtapaAtual { get; set; }

    [Required]
    public StatusProducao StatusAtual { get; set; }

    [Required]
    public DateTime DataCriacao { get; set; }

    public DateTime DataEstimadaEntrega { get; set; }

    // Relación con Usuario (puede ser nulo)
    public int? UsuarioId { get; set; }
    [ForeignKey("UsuarioId")]
    public virtual Usuario? UsuarioAtribuido { get; set; }

    // Propiedad de navegación para el historial
    public virtual ICollection<HistoricoProducao> Historico { get; set; } = new List<HistoricoProducao>();
}
