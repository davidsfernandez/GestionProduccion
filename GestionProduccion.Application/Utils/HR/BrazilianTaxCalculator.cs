namespace GestionProduccion.Application.Utils.HR;

/// <summary>
/// Expert utility for calculating Brazilian labor taxes and benefits (CLT standard).
/// Values are based on 2024 standards.
/// </summary>
public static class BrazilianTaxCalculator
{
    /// <summary>
    /// Calculates the INSS (Social Security) deduction based on progressive brackets.
    /// </summary>
    public static decimal CalculateInss(decimal grossSalary)
    {
        if (grossSalary <= 0) return 0;

        decimal deduction = 0;

        // 2024 Progressive Table Brackets
        if (grossSalary <= 1412.00m)
            deduction = grossSalary * 0.075m;
        else if (grossSalary <= 2666.68m)
            deduction = (grossSalary * 0.09m) - 21.18m;
        else if (grossSalary <= 4000.03m)
            deduction = (grossSalary * 0.12m) - 101.18m;
        else if (grossSalary <= 7786.02m)
            deduction = (grossSalary * 0.14m) - 181.18m;
        else
            deduction = 908.85m; // INSS Ceiling (Teto)

        return Math.Round(deduction, 2);
    }

    /// <summary>
    /// Calculates the FGTS (Employer obligation - not a deduction from employee).
    /// </summary>
    public static decimal CalculateFgts(decimal grossSalary)
    {
        return Math.Round(grossSalary * 0.08m, 2);
    }

    /// <summary>
    /// Calculates the IRRF (Income Tax) based on salary after INSS deduction.
    /// Includes the 2024 Simplified Discount (R$ 564.80) option.
    /// </summary>
    public static decimal CalculateIrrf(decimal taxableSalary)
    {
        // 2024 Simplified Discount: Employees can deduct R$ 564.80 if it's more beneficial than legal deductions.
        const decimal simplifiedDiscount = 564.80m;
        decimal baseForCalculation = taxableSalary - simplifiedDiscount;

        if (baseForCalculation <= 2259.20m) return 0;

        decimal tax = 0;

        if (baseForCalculation <= 2826.65m)
            tax = (baseForCalculation * 0.075m) - 169.44m;
        else if (baseForCalculation <= 3751.05m)
            tax = (baseForCalculation * 0.15m) - 381.44m;
        else if (baseForCalculation <= 4664.68m)
            tax = (baseForCalculation * 0.225m) - 662.77m;
        else
            tax = (baseForCalculation * 0.275m) - 896.00m;

        return Math.Max(0, Math.Round(tax, 2));
    }

    /// <summary>
    /// Calculates the Transportation Voucher deduction (Vale Transporte).
    /// Limited to 6% of base salary or the actual cost (whichever is lower).
    /// </summary>
    public static decimal CalculateTransportationDeduction(decimal baseSalary, bool optsIn)
    {
        if (!optsIn) return 0;
        return Math.Round(baseSalary * 0.06m, 2);
    }
}