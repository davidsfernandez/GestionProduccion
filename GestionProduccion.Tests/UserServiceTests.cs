using System.Threading.Tasks;
using GestionProduccion.Data;
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
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Name = "User 1", Email = "user1@test.com", Role = UserRole.Operator, IsActive = true });
            context.Users.Add(new User { Name = "User 2", Email = "user2@test.com", Role = UserRole.Leader, IsActive = true });
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.GetActiveUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Name = "Test User", Email = "test@test.com", Role = UserRole.Operator, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

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
            var service = new UserService(context);
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

            var service = new UserService(context);
            user.Name = "Updated Name";

            // Act
            await service.UpdateUserAsync(user);
            var updatedUser = await context.Users.FindAsync(user.Id);

            // Assert
            Assert.Equal("Updated Name", updatedUser.Name);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUser_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User { Name = "Delete Me", Email = "delete@test.com", Role = UserRole.Operator, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            await service.DeactivateUserAsync(user.Id);
            var deletedUser = await context.Users.FindAsync(user.Id);

            // Assert
            Assert.False(deletedUser.IsActive);
        }
    }
}
