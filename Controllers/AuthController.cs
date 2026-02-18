using GestionProduccion.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthController(IConfiguration configuration, IUserService userService)
    {
        _configuration = configuration;
        _userService = userService;
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var success = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!success)
        {
            return BadRequest(new { message = "Invalid current password or user not found." });
        }

        return Ok(new { message = "Password updated successfully." });
    }

    [HttpPost("login")]
    [AllowAnonymous] // FIX: Allow unauthenticated users to attempt login
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        // ... existing login logic ...
        try
        {
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = await _userService.GetUserByEmailAsync(login.Email.Trim());

            if (user == null)
            {
                Console.WriteLine($"LOGIN FAILED: User not found with email {login.Email}");
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                Console.WriteLine($"LOGIN FAILED: Password mismatch for user {login.Email}");
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!user.IsActive)
            {
                Console.WriteLine($"LOGIN FAILED: User {login.Email} is inactive");
                return Unauthorized(new { message = "Inactive user." });
            }

            var token = GenerateJwtToken(user);
            Console.WriteLine($"LOGIN SUCCESS: User {login.Email} logged in successfully");
            return Ok(new LoginResponse { Token = token, User = user });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LOGIN CRITICAL ERROR: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
            }

            return StatusCode(500, new { 
                message = "An error occurred during login.", 
                error = ex.Message,
                detail = ex.InnerException?.Message 
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("setup-required")]
    public async Task<ActionResult<bool>> IsSetupRequired()
    {
        return await _userService.IsSetupRequiredAsync();
    }

    [AllowAnonymous]
    [HttpPost("setup")]
    public async Task<IActionResult> FirstTimeSetup([FromBody] RegisterUserDto request)
    {
        if (await _userService.IsSetupRequiredAsync() == false)
        {
            return BadRequest(new { message = "Setup is not required. Users already exist." });
        }

        try
        {
            var user = new Domain.Entities.User
            {
                Name = request.Name,
                Email = request.Email,
                Role = Domain.Enums.UserRole.Administrator, // Force Admin
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                AvatarUrl = "/img/avatars/avatar.jpg"
            };

            await _userService.CreateUserAsync(user);
            return Ok(new { message = "Administrator created successfully. You can now login." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error during setup", error = ex.Message });
        }
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey == "REPLACE_WITH_SECURE_KEY_IN_ENVIRONMENT_VARIABLES")
        {
            jwtKey = "SUPER_SECRET_KEY_FOR_GESTION_PRODUCCION_2024_!@#";
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // USE FULL URI CLAIM NAMES (Microsoft Default)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "GestionProduccion",
            audience: _configuration["Jwt:Audience"] ?? "GestionProduccionAPI",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
