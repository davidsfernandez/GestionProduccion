/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
            var mockConfigService = new Mock<ISystemConfigurationService>();

            var userDto = new UserDto
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@test.com",
                Role = Domain.Enums.UserRole.Administrator,
                IsActive = true
            };

            mockUserService.Setup(s => s.ValidateCredentialsAsync("admin@test.com", "password123")).ReturnsAsync(userDto);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object,
                mockConfigService.Object
            );

            var loginDto = new LoginDto { Email = "admin@test.com", Password = "password123" };

            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
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
            var mockConfigService = new Mock<ISystemConfigurationService>();

            mockUserService.Setup(s => s.ValidateCredentialsAsync("user@test.com", "wrongpassword")).ReturnsAsync((UserDto)null!);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object,
                mockConfigService.Object
            );

            var loginDto = new LoginDto { Email = "user@test.com", Password = "wrongpassword" };

            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponse>>>(result);
            Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
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
            var mockConfigService = new Mock<ISystemConfigurationService>();

            mockUserService.Setup(s => s.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((UserDto)null!);

            var controller = new AuthController(
                config,
                mockUserService.Object,
                mockLogger.Object,
                mockRefreshTokenRepo.Object,
                mockPasswordResetRepo.Object,
                mockEmailService.Object,
                mockConfigService.Object
            );

            var loginDto = new LoginDto { Email = "nonexistent@test.com", Password = "password123" };

            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponse>>>(result);
            Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        }
    }
}


