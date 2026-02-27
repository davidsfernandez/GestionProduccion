using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();

            return Ok(users.Select(u => new UserDto
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
            }).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                return Unauthorized();

            if (currentUserId != id && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(new UserDto
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
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
        }
    }

    [HttpPost("upload-avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files are allowed.");

        try
        {
            // Convert to Base64 for persistence in DB (Cloud-native approach)
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            var base64String = $"data:{file.ContentType};base64,{Convert.ToBase64String(fileBytes)}";

            var success = await _userService.UpdateUserAvatarBase64Async(userId, base64String);

            if (!success) return StatusCode(500, "Failed to update user record in database.");

            return Ok(new { url = base64String });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error uploading avatar", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = $"User with email '{request.Email}' already exists." });
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

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating user", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Role = request.Role;
            user.SewingTeamId = request.SewingTeamId;

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
    [Authorize(Roles = "Administrator")]
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
