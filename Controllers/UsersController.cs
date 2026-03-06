/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public UsersController(IUserService userService, IWebHostEnvironment environment)
    {
        _userService = userService;
        _environment = environment;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            var dtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                ExternalId = u.ExternalId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive,
                SewingTeamId = u.SewingTeamId,
                SewingTeamName = u.SewingTeam?.Name
            }).ToList();

            return Ok(ApiResponse<List<UserDto>>.SuccessResult(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserDto>>.FailureResult("Error retrieving users", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                return Unauthorized(ApiResponse<UserDto>.FailureResult("Unauthorized"));

            if (currentUserId != id && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(ApiResponse<UserDto>.FailureResult("User not found"));

            var dto = new UserDto
            {
                Id = user.Id,
                ExternalId = user.ExternalId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                SewingTeamId = user.SewingTeamId,
                SewingTeamName = user.SewingTeam?.Name
            };

            return Ok(ApiResponse<UserDto>.SuccessResult(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResult("Error retrieving user", new List<string> { ex.Message }));
        }
    }

    [HttpPost("upload-avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.FailureResult("No file uploaded."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(ApiResponse<string>.FailureResult("Unauthorized"));

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest(ApiResponse<string>.FailureResult("Only image files are allowed."));

        try
        {
            var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "img", "avatars");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var newAvatarUrl = $"/img/avatars/{fileName}";
            var success = await _userService.UpdateUserAvatarAsync(userId, newAvatarUrl);

            if (!success) return StatusCode(500, ApiResponse<string>.FailureResult("Failed to update user record in database."));

            return Ok(ApiResponse<string>.SuccessResult(newAvatarUrl, "Avatar uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.FailureResult("Error uploading avatar", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<UserDto>.FailureResult("Validation failed"));

        try
        {
            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(ApiResponse<UserDto>.FailureResult($"User with email '{request.Email}' already exists."));
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                SewingTeamId = request.SewingTeamId
            };

            var createdUser = await _userService.CreateUserAsync(user);
            var userDto = new UserDto
            {
                Id = createdUser.Id,
                ExternalId = createdUser.ExternalId,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                Role = createdUser.Role,
                IsActive = createdUser.IsActive,
                SewingTeamId = createdUser.SewingTeamId
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, ApiResponse<UserDto>.SuccessResult(userDto, "User created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResult("Error creating user", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(ApiResponse<object>.FailureResult("User not found"));

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Role = request.Role;
            user.SewingTeamId = request.SewingTeamId;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _userService.UpdateUserAsync(user);
            return Ok(ApiResponse<object>.SuccessResult(null, "User updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResult("Error updating user", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            var hasActiveOrders = await _userService.HasActiveOrdersAsync(id);
            if (hasActiveOrders)
            {
                return Conflict(ApiResponse<object>.FailureResult("Não é possível desativar este usuário pois ele possui Ordens de Produção ativas atribuídas."));
            }

            var success = await _userService.DeactivateUserAsync(id);
            if (!success) return NotFound(ApiResponse<object>.FailureResult("User not found"));
            return Ok(ApiResponse<object>.SuccessResult(null, "User deactivated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResult("Error deactivating user", new List<string> { ex.Message }));
        }
    }
}
