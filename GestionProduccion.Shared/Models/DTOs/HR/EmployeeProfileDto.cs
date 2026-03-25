namespace GestionProduccion.Models.DTOs;

public class EmployeeProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public string? RNM { get; set; }
    public string? PIS { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public DateTime JoinedDate { get; set; }
    public decimal BaseSalary { get; set; }
    public bool OptsForTransportationVoucher { get; set; }
    public bool IsActive { get; set; }
}