using GestionProduccion.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

/// <summary>
/// Service for managing employee leaves and vacations (Férias) according to Brazilian CLT rules.
/// </summary>
public interface IAbsenceService
{
    /// <summary>
    /// Submits a new leave or vacation request.
    /// </summary>
    Task<EmployeeLeaveDto> RequestLeaveAsync(CreateLeaveDto dto, CancellationToken ct = default);

    /// <summary>
    /// Approves or rejects a pending leave request.
    /// </summary>
    Task<EmployeeLeaveDto> ProcessLeaveAsync(int leaveId, bool approved, int processedByUserId, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Calculates the current vacation balance for an employee.
    /// CLT rule: 30 days per 12-month period, reduced by unjustified absences.
    /// </summary>
    Task<VacationBalanceDto> GetVacationBalanceAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Lists all leaves for an employee within a date range.
    /// </summary>
    Task<List<EmployeeLeaveDto>> GetEmployeeLeavesAsync(int userId, DateTime start, DateTime end, CancellationToken ct = default);
}