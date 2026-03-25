using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public CustomerService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<CustomerProfileDto> GetCustomerProfileAsync(int userId, CancellationToken ct = default)
    {
        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (profile == null)
        {
            // Auto-create profile if user exists but has no profile record (common for B2B added users)
            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) throw new KeyNotFoundException("User not found.");

            profile = new CustomerProfile { UserId = userId };
            _context.CustomerProfiles.Add(profile);
            await _context.SaveChangesAsync(ct);
        }

        return MapToDto(profile);
    }

    public async Task<CustomerProfileDto> UpdateCustomerProfileAsync(CustomerProfileDto dto, CancellationToken ct = default)
    {
        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(c => c.UserId == dto.UserId, ct)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        profile.TaxId = dto.TaxId;
        profile.CompanyName = dto.CompanyName;
        profile.Phone = dto.Phone;
        profile.Address = dto.Address;
        profile.City = dto.City;
        profile.State = dto.State;
        profile.PostalCode = dto.PostalCode;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return MapToDto(profile);
    }

    public async Task<UserDto> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken ct = default)
    {
        // 1. Register base user via existing UserService
        var userDto = new UserDto
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.Customer,
            IsActive = true
        };

        var createdUser = await _userService.CreateUserAsync(userDto, request.Password);

        // 2. Create customer profile
        var profile = new CustomerProfile
        {
            UserId = createdUser.Id,
            CompanyName = request.CompanyName,
            TaxId = request.TaxId,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CustomerProfiles.Add(profile);
        await _context.SaveChangesAsync(ct);

        return createdUser;
    }

    private static CustomerProfileDto MapToDto(CustomerProfile entity)
    {
        return new CustomerProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TaxId = entity.TaxId,
            CompanyName = entity.CompanyName,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode
        };
    }
}