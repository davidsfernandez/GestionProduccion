using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using System.Text.RegularExpressions;

namespace GestionProduccion.Services;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _repo;

    public SystemConfigurationService(ISystemConfigurationRepository repo)
    {
        _repo = repo;
    }

    public async Task<string> GetLogoAsync()
    {
        return await _repo.GetValueAsync("LogoBase64") ?? string.Empty;
    }

    public async Task UpdateLogoAsync(string base64Logo)
    {
        if (string.IsNullOrEmpty(base64Logo))
        {
            await _repo.SetValueAsync("LogoBase64", null);
            return;
        }

        // Validate format (PNG/JPG) using magic numbers or headers in Base64
        if (!IsValidImage(base64Logo))
        {
            throw new ArgumentException("Invalid image format. Only PNG and JPG are allowed.");
        }

        await _repo.SetValueAsync("LogoBase64", base64Logo);
    }

    public async Task UpdateFinancialConfigAsync(decimal dailyFixed, decimal hourlyOp)
    {
        await _repo.SetValueAsync("DailyFixedCost", dailyFixed.ToString());
        await _repo.SetValueAsync("OperationalHourlyCost", hourlyOp.ToString());
    }

    private bool IsValidImage(string base64)
    {
        // Simple regex check for data URI scheme
        var match = Regex.Match(base64, @"^data:image/(png|jpeg|jpg);base64,");
        return match.Success;
    }
}
