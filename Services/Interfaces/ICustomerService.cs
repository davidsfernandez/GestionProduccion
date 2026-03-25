using GestionProduccion.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto> GetCustomerProfileAsync(int userId, CancellationToken ct = default);
    Task<CustomerProfileDto> UpdateCustomerProfileAsync(CustomerProfileDto dto, CancellationToken ct = default);
    Task<UserDto> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken ct = default);
}