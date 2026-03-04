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

            return Ok(new ApiResponse<List<UserDto>> { Success = true, Data = dtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving users", Data = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                return Unauthorized(new ApiResponse<object?> { Success = false, Message = "Unauthorized" });

            if (currentUserId != id && !User.IsInRole("Administrator") && !User.IsInRole("Leader"))
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new ApiResponse<object?> { Success = false, Message = "User not found" });

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

            return Ok(new ApiResponse<UserDto> { Success = true, Data = dto });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error retrieving user", Data = ex.Message });
        }
    }

    [HttpPost("upload-avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<object?> { Success = false, Message = "No file uploaded." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new ApiResponse<object?> { Success = false, Message = "Unauthorized" });

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest(new ApiResponse<object?> { Success = false, Message = "Only image files are allowed." });

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

            if (!success) return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Failed to update user record in database." });

            return Ok(new ApiResponse<object> { Success = true, Data = new { url = newAvatarUrl } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error uploading avatar", Data = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse<object?> { Success = false, Message = "Invalid data", Data = ModelState });

        try
        {
            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new ApiResponse<object?> { Success = false, Message = $"User with email '{request.Email}' already exists." });
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

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new ApiResponse<UserDto> { Success = true, Data = userDto });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error creating user", Data = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new ApiResponse<object?> { Success = false, Message = "User not found" });

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Role = request.Role;
            user.SewingTeamId = request.SewingTeamId;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _userService.UpdateUserAsync(user);
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error updating user", Data = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Security Check: Block deactivation if user has active production orders
            var hasActiveOrders = await _userService.HasActiveOrdersAsync(id);
            if (hasActiveOrders)
            {
                return Conflict(new ApiResponse<object?> { Success = false, Message = "Não é possível desativar este usuário pois ele possui Ordens de Produção ativas atribuídas. Finalize ou reatribua as ordens primeiro." });
            }

            var success = await _userService.DeactivateUserAsync(id);
            if (!success) return NotFound(new ApiResponse<object?> { Success = false, Message = "User not found" });
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object?> { Success = false, Message = "Error deactivating user", Data = ex.Message });
        }
    }
}
