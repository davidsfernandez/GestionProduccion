using System.Threading.Tasks;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProduccion.Tests
{
    public class UserServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnActiveUsers()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Name = "User 1", Email = "user1@test.com", Role = UserRole.Operator, IsActive = true });
            context.Users.Add(new User { Name = "User 2", Email = "user2@test.com", Role = UserRole.Leader, IsActive = true });
            context.Users.Add(new User { Name = "Inactive", Email = "inactive@test.com", Role = UserRole.Operator, IsActive = false });
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo);

            // Act
            var result = await service.GetActiveUsersAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Name = "Test User", Email = "test@test.com", Role = UserRole.Operator, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo);

            // Act
            var result = await service.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldAddUser_WhenValid()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo);
            
            var newUser = new User { Name = "New User", Email = "new@test.com", PasswordHash = "hash", Role = UserRole.Operator };

            // Act
            var result = await service.CreateUserAsync(newUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New User", result.Name);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldModifyUser_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Name = "Old Name", Email = "update@test.com", Role = UserRole.Operator, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo);
            
            user.Name = "Updated Name";

            // Act
            await service.UpdateUserAsync(user);
            var updatedUser = await context.Users.FindAsync(user.Id);

            // Assert
            Assert.Equal("Updated Name", updatedUser.Name);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldSetIsActiveFalse_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Name = "Delete Me", Email = "delete@test.com", Role = UserRole.Operator, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo);

            // Act
            await service.DeactivateUserAsync(user.Id);
            var deletedUser = await context.Users.FindAsync(user.Id);

            // Assert
            Assert.False(deletedUser.IsActive);
        }
    }
}
