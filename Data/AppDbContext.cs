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
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<SewingTeam> SewingTeams { get; set; }
    public DbSet<BonusRule> BonusRules { get; set; }
    public DbSet<QADefect> QADefects { get; set; }
    public DbSet<OperationalTask> OperationalTasks { get; set; }

    /// <summary>
    /// Configures the data model using EF Core Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- QA AND TASKS CONFIGURATION ---
        modelBuilder.Entity<QADefect>()
            .HasOne(d => d.ProductionOrder)
            .WithMany()
            .HasForeignKey(d => d.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QADefect>()
            .HasOne(d => d.ReportedByUser)
            .WithMany()
            .HasForeignKey(d => d.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OperationalTask>()
            .HasOne(t => t.AssignedUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OperationalTask>()
            .Property(t => t.Status)
            .HasMaxLength(50);

        // --- PRODUCT CONFIGURATION ---
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.MainSku)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.InternalCode)
            .IsUnique();

        // Relationship: ProductionOrder -> Product (N to 1)
        modelBuilder.Entity<ProductionOrder>()
            .HasOne(po => po.Product)
            .WithMany()
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- ENUM TO STRING CONVERSION ---

        // User Refresh Token
        modelBuilder.Entity<UserRefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();
        modelBuilder.Entity<UserRefreshToken>()
            .Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(255);
        modelBuilder.Entity<UserRefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Password Reset Token
        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(pt => pt.TokenHash)
            .IsUnique();
        modelBuilder.Entity<PasswordResetToken>()
            .Property(pt => pt.TokenHash)
            .IsRequired()
            .HasMaxLength(255);
        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(pt => pt.User)
            .WithMany()
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Converts UserRole enum in User entity
        modelBuilder.Entity<User>()
            .HasIndex(u => u.ExternalId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        // System Configuration
        modelBuilder.Entity<SystemConfiguration>()
            .HasIndex(sc => sc.Key)
            .IsUnique();
        modelBuilder.Entity<SystemConfiguration>()
            .Property(sc => sc.Key)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<SystemConfiguration>()
            .Property(sc => sc.LogoBase64)
            .HasColumnType("longtext"); // For base64 storage

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
            .HasIndex(po => po.LotCode)
            .IsUnique();

        // --- RELATIONSHIP AND DELETE BEHAVIOR CONFIGURATION ---

        // Relationship: User -> ProductionOrder (1 to N)
        modelBuilder.Entity<ProductionOrder>()
            .HasOne(po => po.AssignedUser)
            .WithMany(u => u.AssignedOrders)
            .HasForeignKey(po => po.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: User -> ProductionHistory (1 to N)
        modelBuilder.Entity<ProductionHistory>()
            .HasOne(h => h.ResponsibleUser)
            .WithMany(u => u.HistoryChanges)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: ProductionOrder -> ProductionHistory (1 to N)
        modelBuilder.Entity<ProductionHistory>()
            .HasOne(h => h.ProductionOrder)
            .WithMany(po => po.History)
            .HasForeignKey(h => h.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- SEWING TEAM AND BONUS RULES ---
        modelBuilder.Entity<User>()
            .HasOne(u => u.SewingTeam)
            .WithMany(t => t.Members)
            .HasForeignKey(u => u.SewingTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductionOrder>()
            .HasOne(po => po.AssignedTeam)
            .WithMany(t => t.AssignedOrders)
            .HasForeignKey(po => po.SewingTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
