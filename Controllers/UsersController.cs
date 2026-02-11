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
    private readonly IWebHostEnvironment _environment;

    public UsersController(IUserService userService, IWebHostEnvironment environment)
    {
        _userService = userService;
        _environment = environment;
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
            PublicId = u.PublicId,
            Role = u.Role,
            AvatarUrl = u.AvatarUrl,
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
            PublicId = user.PublicId,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
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
            PublicId = u.PublicId,
            Role = u.Role,
            IsActive = u.IsActive
        }).ToList();
    }

    [HttpPost("upload-avatar")]
    [Authorize] // Allow any authenticated user
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Get user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        // Validate image
        if (!file.ContentType.StartsWith("image/"))
            return BadRequest("Only image files are allowed.");

        try
        {
            var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "img", "avatars");
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Create unique filename
            var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update user URL (root relative path)
            user.AvatarUrl = $"/img/avatars/{fileName}";
            await _userService.UpdateUserAsync(user);

            return Ok(new { avatarUrl = user.AvatarUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error uploading avatar", error = ex.Message });
        }
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
                PublicId = request.PublicId,
                IsActive = true
            };

            var createdUser = await _userService.CreateUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new UserDto
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                PublicId = createdUser.PublicId,
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