using GestionProduccion.Data;
using GestionProduccion.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthController(AppDbContext context, IConfiguration configuration, IUserService userService)
    {
        _context = context;
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
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Inactive user." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new LoginResponse { Token = token, User = user });
        }
        catch (Exception ex)
        {
            // Log the error for the developer to see in the console
            Console.WriteLine($"LOGIN ERROR: {ex.Message}");
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

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "SUPER_SECRET_KEY_FOR_GESTION_PRODUCCION_2024_!@#";
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
