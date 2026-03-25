using GestionProduccion.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

/// <summary>
/// Service for calculating complex Brazilian payroll (Holerite) integrating production bonuses.
/// </summary>
public interface IPayrollService
{
    /// <summary>
    /// Performs a full payroll calculation for an employee for a specific month.
    /// Includes Base Salary, Bonuses, INSS, IRRF, FGTS, and CLT deductions.
    /// </summary>
    Task<PayrollSlipDto> CalculateMonthlyPayrollAsync(int userId, int year, int month, CancellationToken ct = default);

    /// <summary>
    /// Records the final payroll slip for historical tracking and receipt generation.
    /// </summary>
    Task<PayrollSlipDto> ProcessFinalPayrollAsync(int userId, int year, int month, CancellationToken ct = default);
}