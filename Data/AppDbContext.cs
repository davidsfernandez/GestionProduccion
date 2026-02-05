using Microsoft.EntityFrameworkCore;
using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Data;

/// <summary>
/// Database context for the Production Management application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // --- DBSETS ---
    public DbSet<User> Users { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionHistory> ProductionHistories { get; set; }

    /// <summary>
    /// Configures the data model using EF Core Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- TABLE AND COLUMN MAPPINGS ---
        // Map DbSets to existing Portuguese table names from migrations
        modelBuilder.Entity<User>().ToTable("Usuarios");
        modelBuilder.Entity<User>().Property(u => u.Name).HasColumnName("Nome");
        modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("HashPassword");
        modelBuilder.Entity<User>().Property(u => u.Role).HasColumnName("Perfil");
        modelBuilder.Entity<User>().Property(u => u.IsActive).HasColumnName("Ativo");

        modelBuilder.Entity<ProductionOrder>().ToTable("OrdensProducao");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.UniqueCode).HasColumnName("CodigoUnico");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.ProductDescription).HasColumnName("DescricaoProduto");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.Quantity).HasColumnName("Quantidade");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.CurrentStage).HasColumnName("EtapaAtual");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.CurrentStatus).HasColumnName("StatusAtual");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.CreationDate).HasColumnName("DataCriacao");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.EstimatedDeliveryDate).HasColumnName("DataEstimadaEntrega");
        modelBuilder.Entity<ProductionOrder>().Property(po => po.UserId).HasColumnName("UsuarioId");

        modelBuilder.Entity<ProductionHistory>().ToTable("HistoricoProducoes");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.ProductionOrderId).HasColumnName("OrdemProducaoId");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.PreviousStage).HasColumnName("EtapaAnterior");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.NewStage).HasColumnName("EtapaNova");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.PreviousStatus).HasColumnName("StatusAnterior");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.NewStatus).HasColumnName("StatusNovo");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.UserId).HasColumnName("UsuarioId");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.ModificationDate).HasColumnName("DataModificacao");
        modelBuilder.Entity<ProductionHistory>().Property(h => h.Note).HasColumnName("Observacao");

        // --- ENUM TO STRING CONVERSION ---

        // Converts UserRole enum in User entity
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Converts enums in ProductionOrder entity
        modelBuilder.Entity<ProductionOrder>()
            .Property(po => po.CurrentStage)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<ProductionOrder>()
            .Property(po => po.CurrentStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Converts enums in ProductionHistory entity
        modelBuilder.Entity<ProductionHistory>()
            .Property(h => h.PreviousStage)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<ProductionHistory>()
            .Property(h => h.NewStage)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<ProductionHistory>()
            .Property(h => h.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<ProductionHistory>()
            .Property(h => h.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        // --- INDEX AND CONSTRAINT CONFIGURATION ---

        // Ensures that the production order code is unique
        modelBuilder.Entity<ProductionOrder>()
            .HasIndex(po => po.UniqueCode)
            .IsUnique();

        // --- RELATIONSHIP AND DELETE BEHAVIOR CONFIGURATION ---

        // Relationship: User -> ProductionOrder (1 to N)
        // Prevents deleting a User if they have assigned production orders.
        modelBuilder.Entity<ProductionOrder>()
            .HasOne(po => po.AssignedUser)
            .WithMany(u => u.AssignedOrders)
            .HasForeignKey(po => po.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: User -> ProductionHistory (1 to N)
        // Prevents deleting a User if they have made changes in history.
        modelBuilder.Entity<ProductionHistory>()
            .HasOne(h => h.ResponsibleUser)
            .WithMany(u => u.HistoryChanges)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relationship: ProductionOrder -> ProductionHistory (1 to N)
        // Deletes all history records when a ProductionOrder is deleted (cascade).
        modelBuilder.Entity<ProductionHistory>()
            .HasOne(h => h.ProductionOrder)
            .WithMany(po => po.History)
            .HasForeignKey(h => h.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- SEED DATA ---
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Administrator",
                Email = "admin@local.host",
                // Password is "admin", hashed with BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = Domain.Enums.UserRole.Administrator,
                IsActive = true
            }
        );
    }
}
