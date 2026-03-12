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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorage;

    public UsersController(IUserService userService, IFileStorageService fileStorage)
    {
        _userService = userService;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers([FromQuery] string? role = null)
    {
        try
        {
            List<UserDto> dtos;
            if (!string.IsNullOrEmpty(role))
            {
                dtos = await _userService.GetUsersByRoleAsync(role);
            }
            else
            {
                dtos = await _userService.GetActiveUsersAsync();
            }
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

            var dto = await _userService.GetUserByIdAsync(id);
            if (dto == null) return NotFound(ApiResponse<UserDto>.FailureResult("User not found"));

            return Ok(ApiResponse<UserDto>.SuccessResult(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResult("Error retrieving user", new List<string> { ex.Message }));
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(ApiResponse<UserDto>.FailureResult("Name is required."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(ApiResponse<UserDto>.FailureResult("Unauthorized"));

        try
        {
            var updated = await _userService.UpdateProfileAsync(userId, request.FullName);
            return Ok(ApiResponse<UserDto>.SuccessResult(updated, "Profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResult("Error updating profile", new List<string> { ex.Message }));
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
            // Standardized File Storage usage
            string avatarUrl = await _fileStorage.UploadAsync(file, "avatars");

            var success = await _userService.UpdateUserAvatarAsync(userId, avatarUrl);
            if (!success) return StatusCode(500, ApiResponse<string>.FailureResult("Failed to update database record."));

            return Ok(ApiResponse<string>.SuccessResult(avatarUrl, "Avatar uploaded successfully"));
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

            var userDto = new UserDto
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                IsActive = true,
                SewingTeamId = request.SewingTeamId
            };

            var createdUser = await _userService.CreateUserAsync(userDto, request.Password);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, ApiResponse<UserDto>.SuccessResult(createdUser, "User created successfully"));
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
            var userDto = await _userService.GetUserByIdAsync(id);
            if (userDto == null) return NotFound(ApiResponse<object>.FailureResult("User not found"));

            userDto.FullName = request.FullName;
            userDto.Email = request.Email;
            userDto.Role = request.Role;
            userDto.SewingTeamId = request.SewingTeamId;

            if (!string.IsNullOrEmpty(request.Password))
            {
                await _userService.ResetPasswordAsync(id, request.Password);
            }

            await _userService.UpdateUserAsync(userDto);
            return Ok(ApiResponse<object>.SuccessResult(null!, "User updated successfully"));
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
                return Conflict(ApiResponse<object>.FailureResult("Não é posible desativar este usuário pois ele possui Ordens de Produção ativas atribuídas."));
            }

            var success = await _userService.DeactivateUserAsync(id);
            if (!success) return NotFound(ApiResponse<object>.FailureResult("User not found"));
            return Ok(ApiResponse<object>.SuccessResult(null!, "User deactivated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResult("Error deactivating user", new List<string> { ex.Message }));
        }
    }

    [HttpGet("orders/{orderId}/eligible-users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetEligibleUsers(int orderId)
    {
        try
        {
            var users = await _userService.GetUsersForOrderAsync(orderId);
            return Ok(ApiResponse<List<UserDto>>.SuccessResult(users));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserDto>>.FailureResult("Error retrieving eligible users", new List<string> { ex.Message }));
        }
    }
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
}
