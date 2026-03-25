namespace GestionProduccion.Models.DTOs;

public class PayrollSlipDto
{
    public int UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }

    // --- Earnings (Proventos) ---
    public decimal BaseSalary { get; set; }
    public decimal ProductionBonus { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal DsrPay { get; set; } // Descanso Semanal Remunerado
    public decimal TotalEarnings => BaseSalary + ProductionBonus + OvertimePay + DsrPay;

    // --- Deductions (Descontos) ---
    public decimal InssDeduction { get; set; }
    public decimal IrrfDeduction { get; set; }
    public decimal TransportationDeduction { get; set; }
    public decimal AbsenceDeduction { get; set; }
    public decimal TotalDeductions => InssDeduction + IrrfDeduction + TransportationDeduction + AbsenceDeduction;

    // --- Net Total ---
    public decimal NetSalary => TotalEarnings - TotalDeductions;

    // --- Company Obligations (Informative) ---
    public decimal FgtsDeposit { get; set; }
    
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}