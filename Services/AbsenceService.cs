using GestionProduccion.Data;
using GestionProduccion.Domain.Entities.HR;
using GestionProduccion.Domain.Enums.HR;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class AbsenceService : IAbsenceService
{
    private readonly AppDbContext _context;

    public AbsenceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeLeaveDto> RequestLeaveAsync(CreateLeaveDto dto, CancellationToken ct = default)
    {
        var profile = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == dto.UserId, ct)
            ?? throw new KeyNotFoundException("Employee profile not found.");

        var leave = new EmployeeLeave
        {
            EmployeeProfileId = profile.Id,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending
        };

        _context.EmployeeLeaves.Add(leave);
        await _context.SaveChangesAsync(ct);

        return MapToDto(leave);
    }

    public async Task<EmployeeLeaveDto> ProcessLeaveAsync(int leaveId, bool approved, int processedByUserId, string? reason = null, CancellationToken ct = default)
    {
        var leave = await _context.EmployeeLeaves.FindAsync(new object[] { leaveId }, ct)
            ?? throw new KeyNotFoundException("Leave request not found.");

        leave.Status = approved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ApprovedByUserId = processedByUserId;
        if (!string.IsNullOrEmpty(reason)) leave.Reason = $"{leave.Reason} | Review: {reason}";

        await _context.SaveChangesAsync(ct);
        return MapToDto(leave);
    }

    public async Task<VacationBalanceDto> GetVacationBalanceAsync(int userId, CancellationToken ct = default)
    {
        var profile = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Employee profile not found.");

        // Adquisitivo period usually starts on JoinedDate and resets every 12 months.
        var today = DateTime.UtcNow;
        int yearsSinceJoined = (int)((today - profile.JoinedDate).TotalDays / 365.25);

        var periodStart = profile.JoinedDate.AddYears(yearsSinceJoined);
        var periodEnd = periodStart.AddYears(1);

        // Count unjustified absences in the current period
        var unjustifiedCount = await _context.EmployeeLeaves
            .Where(l => l.EmployeeProfileId == profile.Id &&
                        l.Type == LeaveType.Unjustified &&
                        l.StartDate >= periodStart && l.StartDate < periodEnd)
            .CountAsync(ct);

        // CLT scale for vacation days
        int daysAvailable = unjustifiedCount switch
        {
            <= 5 => 30,
            <= 14 => 24,
            <= 23 => 18,
            <= 32 => 12,
            _ => 0
        };

        return new VacationBalanceDto
        {
            EmployeeProfileId = profile.Id,
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd,
            UnjustifiedAbsences = unjustifiedCount,
            DaysAvailable = daysAvailable,
            IsEligible = yearsSinceJoined >= 1
        };
    }

    public async Task<List<EmployeeLeaveDto>> GetEmployeeLeavesAsync(int userId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.EmployeeLeaves
            .Where(l => l.Employee!.UserId == userId && l.StartDate >= start && l.EndDate <= end)
            .Select(l => MapToDto(l))
            .ToListAsync(ct);
    }

    private static EmployeeLeaveDto MapToDto(EmployeeLeave entity)
    {
        return new EmployeeLeaveDto
        {
            Id = entity.Id,
            EmployeeProfileId = entity.EmployeeProfileId,
            Type = entity.Type,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            Reason = entity.Reason,
            ApprovedByUserId = entity.ApprovedByUserId,
            CreatedAt = entity.CreatedAt
        };
    }
}