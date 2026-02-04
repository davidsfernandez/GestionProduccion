using Microsoft.EntityFrameworkCore;
using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Data;

/// <summary>
/// Contexto de la base de datos para la aplicación de Gestión de Producción.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // --- DBSETS ---
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<OrdemProducao> OrdensProducao { get; set; }
    public DbSet<HistoricoProducao> HistoricoProducoes { get; set; }

    /// <summary>
    /// Configura el modelo de datos usando la Fluent API de EF Core.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- CONFIGURACIÓN DE CONVERSIÓN DE ENUMS A STRING ---

        // Convierte el enum PerfilUsuario en la entidad Usuario
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Perfil)
            .HasConversion<string>()
            .HasMaxLength(50); // Es una buena práctica definir una longitud máxima

        // Convierte los enums en la entidad OrdemProducao
        modelBuilder.Entity<OrdemProducao>()
            .Property(op => op.EtapaAtual)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<OrdemProducao>()
            .Property(op => op.StatusAtual)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Convierte los enums en la entidad HistoricoProducao
        modelBuilder.Entity<HistoricoProducao>()
            .Property(h => h.EtapaAnterior)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<HistoricoProducao>()
            .Property(h => h.EtapaNova)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<HistoricoProducao>()
            .Property(h => h.StatusAnterior)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<HistoricoProducao>()
            .Property(h => h.StatusNovo)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        // --- CONFIGURACIÓN DE ÍNDICES Y RESTRICCIONES ---

        // Asegura que el código de la orden de producción sea único
        modelBuilder.Entity<OrdemProducao>()
            .HasIndex(op => op.CodigoUnico)
            .IsUnique();

        // --- CONFIGURACIÓN DE RELACIONES Y COMPORTAMIENTO DE BORRADO ---

        // Relación: Usuario -> OrdemProducao (1 a N)
        // Evita que se pueda eliminar un Usuario si tiene órdenes de producción asignadas.
        modelBuilder.Entity<OrdemProducao>()
            .HasOne(op => op.UsuarioAtribuido)
            .WithMany(u => u.OrdensAtribuidas)
            .HasForeignKey(op => op.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación: Usuario -> HistoricoProducao (1 a N)
        // Evita que se pueda eliminar un Usuario si ha realizado cambios en el historial.
        modelBuilder.Entity<HistoricoProducao>()
            .HasOne(h => h.UsuarioResponsavel)
            .WithMany(u => u.AlteracoesNoHistorico)
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relación: OrdemProducao -> HistoricoProducao (1 a N)
        // Al eliminar una OrdemProducao, se eliminarán en cascada todos sus registros de historial.
        // Este es el comportamiento por defecto para relaciones requeridas, pero se puede hacer explícito:
        modelBuilder.Entity<HistoricoProducao>()
            .HasOne(h => h.OrdemProducao)
            .WithMany(op => op.Historico)
            .HasForeignKey(h => h.OrdemProducaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
