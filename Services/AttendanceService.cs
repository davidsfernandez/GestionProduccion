using GestionProduccion.Data;
using GestionProduccion.Domain.Entities.HR;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(AppDbContext context, ILogger<AttendanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AttendanceDto> ClockInAsync(int userId, string? note = null, CancellationToken ct = default)
    {
        var employee = await GetEmployeeByUserIdAsync(userId, ct);
        
        // Check for existing ongoing attendance
        var active = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeProfileId == employee.Id && a.ClockOut == null, ct);

        if (active != null)
        {
            throw new InvalidOperationException("Employee is already clocked in.");
        }

        var attendance = new Attendance
        {
            EmployeeProfileId = employee.Id,
            ClockIn = DateTime.UtcNow,
            Note = note
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(ct);

        return MapToDto(attendance);
    }

    public async Task<AttendanceDto> ClockOutAsync(int userId, string? note = null, CancellationToken ct = default)
    {
        var employee = await GetEmployeeByUserIdAsync(userId, ct);
        
        var active = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeProfileId == employee.Id && a.ClockOut == null, ct);

        if (active == null)
        {
            throw new InvalidOperationException("No active clock-in found for this employee.");
        }

        active.ClockOut = DateTime.UtcNow;
        active.Note = string.IsNullOrEmpty(note) ? active.Note : $"{active.Note} | {note}";
        
        // Calculate total duration
        var duration = active.ClockOut.Value - active.ClockIn;
        decimal totalHours = (decimal)duration.TotalHours;

        // CLT Rule: Inter-journey break (Intervalo Intrajornada)
        // If the shift is > 6 hours, it typically includes a 1-hour mandatory unpaid break.
        // For simplicity in this first version, we assume the user clocks out for lunch.
        // If they didn't, we might need to deduct it or flag it based on company policy.
        active.HoursWorked = Math.Max(0, totalHours);

        await _context.SaveChangesAsync(ct);
        return MapToDto(active);
    }

    public async Task<AttendanceStatusDto> GetCurrentStatusAsync(int userId, CancellationToken ct = default)
    {
        var employee = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employee == null) return new AttendanceStatusDto { IsClockedIn = false };

        var active = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeProfileId == employee.Id && a.ClockOut == null, ct);

        var today = DateTime.UtcNow.Date;
        var todayHours = await _context.Attendances
            .Where(a => a.EmployeeProfileId == employee.Id && a.ClockIn >= today)
            .SumAsync(a => a.HoursWorked, ct);

        return new AttendanceStatusDto
        {
            IsClockedIn = active != null,
            LastClockIn = active?.ClockIn,
            TodayHoursWorked = todayHours
        };
    }

    public Task<decimal> CalculateOvertimeAsync(int userId, DateTime date, CancellationToken ct = default)
    {
        // Standard CLT: 8h per day / 44h per week.
        // To be implemented in next step with Weekly aggregation logic.
        return Task.FromResult(0m);
    }

    private async Task<EmployeeProfile> GetEmployeeByUserIdAsync(int userId, CancellationToken ct)
    {
        var employee = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employee == null)
        {
            // If profile doesn't exist, create one automatically from user data (lazy migration)
            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) throw new KeyNotFoundException("User not found.");

            employee = new EmployeeProfile
            {
                UserId = userId,
                JoinedDate = DateTime.UtcNow,
                BaseSalary = 0 // Needs to be configured by HR
            };
            _context.EmployeeProfiles.Add(employee);
            await _context.SaveChangesAsync(ct);
        }

        return employee;
    }

    private AttendanceDto MapToDto(Attendance entity)
    {
        return new AttendanceDto
        {
            Id = entity.Id,
            EmployeeProfileId = entity.EmployeeProfileId,
            ClockIn = entity.ClockIn,
            ClockOut = entity.ClockOut,
            HoursWorked = entity.HoursWorked,
            Note = entity.Note
        };
    }
}