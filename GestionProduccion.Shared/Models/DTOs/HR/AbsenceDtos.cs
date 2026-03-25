using GestionProduccion.Domain.Enums.HR;

namespace GestionProduccion.Models.DTOs;

public class CreateLeaveDto
{
    public int UserId { get; set; }
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public class EmployeeLeaveDto
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string? Reason { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VacationBalanceDto
{
    public int EmployeeProfileId { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public int UnjustifiedAbsences { get; set; }
    public int DaysAvailable { get; set; } // CLT: Standard 30, but decreases with absences.
    public bool IsEligible { get; set; } // Adquisitivo period complete?
}