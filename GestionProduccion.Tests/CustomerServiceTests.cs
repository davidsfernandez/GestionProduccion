using Moq;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProduccion.Tests
{
    public class CustomerServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterCustomerAsync_ShouldCreateUserAndProfile()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockUserService = new Mock<IUserService>();
            var service = new CustomerService(context, mockUserService.Object);

            var request = new RegisterCustomerRequest
            {
                FullName = "Test Customer",
                Email = "customer@test.com",
                Password = "password123",
                CompanyName = "Test Corp"
            };

            var createdUser = new UserDto { Id = 1, FullName = request.FullName, Email = request.Email, Role = UserRole.Customer };
            mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<UserDto>(), request.Password))
                           .ReturnsAsync(createdUser);

            // Act
            var result = await service.RegisterCustomerAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            
            var profile = await context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == result.Id);
            Assert.NotNull(profile);
            Assert.Equal(request.CompanyName, profile.CompanyName);
        }

        [Fact]
        public async Task GetCustomerProfileAsync_ShouldCreateProfile_IfDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Id = 1, FullName = "B2B Client", Email = "b2b@test.com", Role = UserRole.Customer };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockUserService = new Mock<IUserService>();
            var service = new CustomerService(context, mockUserService.Object);

            // Act
            var profile = await service.GetCustomerProfileAsync(user.Id);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(user.Id, profile.UserId);
            
            var profileInDb = await context.CustomerProfiles.AnyAsync(p => p.UserId == user.Id);
            Assert.True(profileInDb);
        }
    }
}