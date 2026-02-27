using GestionProduccion.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Domain.Entities;
using System.Security.Cryptography;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordResetTokenRepository _passwordResetRepo;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigurationService _configService;

    public AuthController(
        IConfiguration configuration,
        IUserService userService,
        ILogger<AuthController> logger,
        IUserRefreshTokenRepository refreshTokenRepo,
        IPasswordResetTokenRepository passwordResetRepo,
        IEmailService emailService,
        ISystemConfigurationService configService)
    {
        _configuration = configuration;
        _userService = userService;
        _logger = logger;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordResetRepo = passwordResetRepo;
        _emailService = emailService;
        _configService = configService;
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
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = await _userService.GetUserByEmailAsync(login.Email.Trim());

            if (user == null)
            {
                _logger.LogWarning("LOGIN FAILED: User not found with email {Email}", login.Email);
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                _logger.LogWarning("LOGIN FAILED: Password mismatch for user {Email}", login.Email);
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("LOGIN FAILED: User {Email} is inactive", login.Email);
                return Unauthorized(new { message = "Inactive user." });
            }

            var tokenDuration = login.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8);
            var token = GenerateJwtToken(user, tokenDuration);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepo.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("LOGIN SUCCESS: User {Email} logged in successfully", login.Email);
            return Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                AvatarUrl = user.AvatarUrl,
                FullName = user.FullName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LOGIN CRITICAL ERROR: {Message}", ex.Message);
            return StatusCode(500, new
            {
                message = "An error occurred during login.",
                error = ex.Message
            });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("Invalid client request");
        }

        var storedToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        var user = storedToken.User;
        var newToken = GenerateJwtToken(user, TimeSpan.FromDays(7));
        var newRefreshToken = GenerateRefreshToken();

        storedToken.IsRevoked = true;
        await _refreshTokenRepo.UpdateAsync(storedToken);

        await _refreshTokenRepo.AddAsync(new UserRefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        return Ok(new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            AvatarUrl = user.AvatarUrl,
            FullName = user.FullName
        });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var token = await _userService.RequestPasswordResetAsync(request.Email);

        if (token == null) return Ok(new { message = "If the email exists, a reset link has been sent." });

        var resetLink = $"https://tu-dominio.com/reset-password?token={token}&email={Uri.EscapeDataString(request.Email)}";
        var emailBody = $"<p>Click <a href='{resetLink}'>here</a> to reset your password. This link expires in 15 minutes.</p>";

        await _emailService.SendEmailAsync(request.Email, "Reset Password", emailBody);

        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _userService.CompletePasswordResetAsync(request.Email, request.Token, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired token, or user mismatch." });
        }

        return Ok(new { message = "Password reset successfully." });
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
            // 1. Save Company Configuration
            await _configService.SaveConfigurationAsync(new SystemConfigurationDto
            {
                CompanyName = request.CompanyName,
                CompanyTaxId = request.CompanyTaxId,
                LogoBase64 = request.LogoBase64,
                DailyFixedCost = 0,
                OperationalHourlyCost = 45.0m // Default fallback
            });

            // 2. Create Admin User
            var user = new Domain.Entities.User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = Domain.Enums.UserRole.Administrator,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                AvatarUrl = "/img/avatars/avatar.jpg"
            };

            await _userService.CreateUserAsync(user);
            return Ok(new { message = "Administrator created and system configured successfully. You can now login." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error during setup", error = ex.Message });
        }
    }

    private string GenerateJwtToken(Domain.Entities.User user, TimeSpan duration)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey == "REPLACE_WITH_SECURE_KEY_IN_ENVIRONMENT_VARIABLES")
        {
            jwtKey = "SUPER_SECRET_KEY_FOR_GESTION_PRODUCCION_2024_!@#";
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("AvatarUrl", user.AvatarUrl ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "GestionProduccion",
            audience: _configuration["Jwt:Audience"] ?? "GestionProduccionAPI",
            claims: claims,
            expires: DateTime.UtcNow.Add(duration),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
