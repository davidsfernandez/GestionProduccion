using GestionProduccion.Application.Utils.HR;
using GestionProduccion.Data;
using GestionProduccion.Domain.Enums.HR;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IBonusCalculationService _bonusService;
    private readonly IAttendanceService _attendanceService;
    private readonly IAbsenceService _absenceService;

    public PayrollService(
        AppDbContext context,
        IBonusCalculationService bonusService,
        IAttendanceService attendanceService,
        IAbsenceService absenceService)
    {
        _context = context;
        _bonusService = bonusService;
        _attendanceService = attendanceService;
        _absenceService = absenceService;
    }

    public async Task<PayrollSlipDto> CalculateMonthlyPayrollAsync(int userId, int year, int month, CancellationToken ct = default)      
    {
        var employee = await _context.EmployeeProfiles
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Employee profile not found.");

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // 1. Get Production Bonuses from the existing MySQL service
        var bonusReport = await _bonusService.CalculateUserBonusAsync(userId, startDate, endDate);
        decimal productionBonus = bonusReport.IsAtomicFailure ? 0 : bonusReport.TotalAmount;

        // 2. Get Absences for deductions
        var leaves = await _absenceService.GetEmployeeLeavesAsync(userId, startDate, endDate, ct);
        int unjustifiedDays = leaves.Count(l => l.Type == LeaveType.Unjustified);
        decimal dayValue = employee.BaseSalary / 30;
        decimal absenceDeduction = unjustifiedDays * dayValue;

        // 3. Basic Overtime Calculation (To be expanded)
        decimal overtimePay = 0; // Simplified for MVP

        // 4. Taxes using the Expert Utility
        decimal grossForInss = employee.BaseSalary + productionBonus + overtimePay - absenceDeduction;
        decimal inss = BrazilianTaxCalculator.CalculateInss(grossForInss);

        decimal taxableForIrrf = grossForInss - inss;
        decimal irrf = BrazilianTaxCalculator.CalculateIrrf(taxableForIrrf);

        decimal transportDeduction = BrazilianTaxCalculator.CalculateTransportationDeduction(employee.BaseSalary, employee.OptsForTransportationVoucher);

        return new PayrollSlipDto
        {
            UserId = userId,
            EmployeeName = employee.User?.FullName ?? "Unknown",
            CPF = employee.CPF,
            Year = year,
            Month = month,
            BaseSalary = employee.BaseSalary,
            ProductionBonus = productionBonus,
            OvertimePay = overtimePay,
            DsrPay = 0, // Placeholder
            InssDeduction = inss,
            IrrfDeduction = irrf,
            TransportationDeduction = transportDeduction,
            AbsenceDeduction = absenceDeduction,
            FgtsDeposit = BrazilianTaxCalculator.CalculateFgts(grossForInss)
        };
    }

    public async Task<PayrollSlipDto> ProcessFinalPayrollAsync(int userId, int year, int month, CancellationToken ct = default)
    {
        var slip = await CalculateMonthlyPayrollAsync(userId, year, month, ct);

        // Here we would persist the slip to a PayrollHistory table in MySQL
        // For now, return the calculated DTO.

        return slip;
    }
}