namespace GestionProduccion.Services.Interfaces;

public interface ISystemConfigurationService
{
    Task<string> GetLogoAsync();
    Task UpdateLogoAsync(string base64Logo);
    Task UpdateFinancialConfigAsync(decimal dailyFixed, decimal hourlyOp);
}
