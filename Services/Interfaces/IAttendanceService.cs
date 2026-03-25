using GestionProduccion.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

/// <summary>
/// Service for managing employee attendance and time tracking following Brazilian CLT regulations.
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// Records a clock-in event for an employee.
    /// </summary>
    Task<AttendanceDto> ClockInAsync(int userId, string? note = null, CancellationToken ct = default);

    /// <summary>
    /// Records a clock-out event for an employee.
    /// Calculates worked hours considering CLT break rules.
    /// </summary>
    Task<AttendanceDto> ClockOutAsync(int userId, string? note = null, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the current attendance status for an employee.
    /// </summary>
    Task<AttendanceStatusDto> GetCurrentStatusAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Calculates overtime hours based on the 44h weekly / 8h daily CLT standard.
    /// </summary>
    Task<decimal> CalculateOvertimeAsync(int userId, DateTime date, CancellationToken ct = default);
}