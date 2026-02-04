using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Representa a un usuario del sistema.
/// </summary>
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string HashPassword { get; set; } = string.Empty;

    [Required]
    public PerfilUsuario Perfil { get; set; }

    public bool Ativo { get; set; } = true;

    // Propiedades de navegación de EF Core
    public virtual ICollection<OrdemProducao> OrdensAtribuidas { get; set; } = new List<OrdemProducao>();
    public virtual ICollection<HistoricoProducao> AlteracoesNoHistorico { get; set; } = new List<HistoricoProducao>();
}
