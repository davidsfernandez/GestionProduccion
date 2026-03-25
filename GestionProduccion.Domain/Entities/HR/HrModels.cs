using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums.HR;

namespace GestionProduccion.Domain.Entities.HR;

public class EmployeeProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the primary User entity in MySQL.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    // --- Brazilian Identification ---
    [Required]
    [MaxLength(14)]
    public string CPF { get; set; } = string.Empty; // Cadastro de Pessoas Físicas

    [MaxLength(20)]
    public string? RG { get; set; } // Registro Geral

    [MaxLength(20)]
    public string? RNM { get; set; } // Registro Nacional Migratório (for foreigners)

    [MaxLength(20)]
    public string? PIS { get; set; } // Programa de Integração Social

    public DateTime? BirthDate { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    // --- Financial Config ---
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// Indicates if the employee receives "Vale Transporte" (usually 6% deduction).
    /// </summary>
    public bool OptsForTransportationVoucher { get; set; }

    // Navigation properties
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

public class Contract
{
    [Key]
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    [ForeignKey(nameof(EmployeeProfileId))]
    public virtual EmployeeProfile? Employee { get; set; }

    [Required]
    public ContractType Type { get; set; } = ContractType.CLT;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // --- Probation Period (Experiência - standard 90 days total) ---
    public int? FirstProbationDays { get; set; } // e.g., 45 days
    public int? SecondProbationDays { get; set; } // e.g., 45 days

    public string? DocumentUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class Attendance
{
    [Key]
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    [ForeignKey(nameof(EmployeeProfileId))]
    public virtual EmployeeProfile? Employee { get; set; }

    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }

    public decimal HoursWorked { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }
}

public class EmployeeLeave
{
    [Key]
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    [ForeignKey(nameof(EmployeeProfileId))]
    public virtual EmployeeProfile? Employee { get; set; }

    [Required]
    public LeaveType Type { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public string? Reason { get; set; }
    public string? AttachmentUrl { get; set; } // For medical certificates, etc.

    public int? ApprovedByUserId { get; set; }

    [ForeignKey(nameof(ApprovedByUserId))]
    public virtual User? ApprovedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EmployeeDocument
{
    [Key]
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    [ForeignKey(nameof(EmployeeProfileId))]
    public virtual EmployeeProfile? Employee { get; set; }

    [Required]
    [MaxLength(100)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}