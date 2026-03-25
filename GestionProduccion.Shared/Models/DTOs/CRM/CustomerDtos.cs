using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class CustomerProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? TaxId { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
}

public class RegisterCustomerRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
}