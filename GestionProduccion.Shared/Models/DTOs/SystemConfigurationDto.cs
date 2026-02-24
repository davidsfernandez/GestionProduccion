namespace GestionProduccion.Models.DTOs;

public class SystemConfigurationDto
{
    public string? CompanyName { get; set; }
    public string? CompanyTaxId { get; set; }
    public string? LogoBase64 { get; set; }
    public decimal DailyFixedCost { get; set; }
    public decimal OperationalHourlyCost { get; set; }
}

public class LogoDto
{
    public string? Base64Image { get; set; }
}
