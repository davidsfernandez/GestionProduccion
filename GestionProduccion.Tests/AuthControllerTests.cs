using System.Collections.Generic;
using System.Threading.Tasks;
using GestionProduccion.Controllers;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Tests
{
    public class AuthControllerTests
    {
        private IConfiguration GetMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"Jwt:Key", "super_secret_testing_key_1234567890123456"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<AuthController>>();
            var mockRefreshTokenRepo = new Mock<IUserRefreshTokenRepository>();
            var mockPasswordResetRepo = new Mock<IPasswordResetTokenRepository>();
            var mockEmailService = new Mock<IEmailService>();

            var user = new User
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = Domain.Enums.UserRole.Administrator,
                IsActive = true
            };

            mockUserService.Setup(s => s.GetUserByEmailAsync("admin@test.com")).ReturnsAsync(user);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object
            );

            var loginDto = new LoginDto { Email = "admin@test.com", Password = "password123" };

            var result = await controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
        {
            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<AuthController>>();
            var mockRefreshTokenRepo = new Mock<IUserRefreshTokenRepository>();
            var mockPasswordResetRepo = new Mock<IPasswordResetTokenRepository>();
            var mockEmailService = new Mock<IEmailService>();

            var user = new User
            {
                Id = 1,
                FullName = "User",
                Email = "user@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = Domain.Enums.UserRole.Operator,
                IsActive = true
            };

            mockUserService.Setup(s => s.GetUserByEmailAsync("user@test.com")).ReturnsAsync(user);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object
            );

            var loginDto = new LoginDto { Email = "user@test.com", Password = "wrongpassword" };

            var result = await controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<AuthController>>();
            var mockRefreshTokenRepo = new Mock<IUserRefreshTokenRepository>();
            var mockPasswordResetRepo = new Mock<IPasswordResetTokenRepository>();
            var mockEmailService = new Mock<IEmailService>();

            mockUserService.Setup(s => s.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object
            );

            var loginDto = new LoginDto { Email = "nonexistent@test.com", Password = "password123" };

            var result = await controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
