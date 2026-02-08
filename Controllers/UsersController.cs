using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Authorize(Roles = "Administrator,Leader")] // Allow Leader too
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _userService.GetActiveUsersAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive
        }).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    [HttpGet("assignable")] // Ensure this endpoint is here too
    public async Task<ActionResult<List<UserDto>>> GetAssignableUsers()
    {
        var users = await _userService.GetActiveUsersAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive
        }).ToList();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")] // Only Admin can create
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true
            };

            var createdUser = await _userService.CreateUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new UserDto
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Role = createdUser.Role,
                IsActive = createdUser.IsActive
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating user", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")] // Only Admin can update
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            user.Name = request.Name;
            user.Email = request.Email;
            user.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _userService.UpdateUserAsync(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating user", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")] // Only Admin can delete
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeactivateUserAsync(id);
            if (!success) return NotFound(new { message = "User not found" });
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deactivating user", error = ex.Message });
        }
    }
}