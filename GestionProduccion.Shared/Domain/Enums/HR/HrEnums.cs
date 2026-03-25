namespace GestionProduccion.Domain.Enums.HR;

public enum ContractType
{
    CLT,        // Consolidação das Leis do Trabalho (Standard employment)
    PJ,         // Pessoa Jurídica (B2B / Service Provider)
    Internship, // Estágio
    Apprentice  // Aprendiz
}

public enum LeaveType
{
    Vacation,       // Férias
    SickLeave,      // Auxílio-doença
    Maternity,      // Licença Maternidade
    Paternity,      // Licença Paternidade
    Justified,      // Faltas abonadas (Marriage, Bereavement, etc.)
    Unjustified     // Faltas injustificadas (Deducted from salary/vacation)
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}