using System.Collections.Generic;
using System.Threading.Tasks;
using GestionProduccion.Controllers;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

using Moq;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Tests
{
    public class AuthControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration GetMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
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
            // Arrange
            using var context = GetInMemoryDbContext();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            context.Users.Add(new User { Name = "Admin", Email = "admin@test.com", PasswordHash = hashedPassword, Role = Domain.Enums.UserRole.Administrator, IsActive = true });
            await context.SaveChangesAsync();

            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var controller = new AuthController(context, config, mockUserService.Object);

            var loginDto = new LoginDto { Email = "admin@test.com", Password = "password123" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            context.Users.Add(new User { Name = "User", Email = "user@test.com", PasswordHash = hashedPassword, Role = Domain.Enums.UserRole.Operator, IsActive = true });
            await context.SaveChangesAsync();

            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var controller = new AuthController(context, config, mockUserService.Object);

            var loginDto = new LoginDto { Email = "user@test.com", Password = "wrongpassword" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var config = GetMockConfiguration();
            var mockUserService = new Mock<IUserService>();
            var controller = new AuthController(context, config, mockUserService.Object);

            var loginDto = new LoginDto { Email = "nonexistent@test.com", Password = "password123" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
