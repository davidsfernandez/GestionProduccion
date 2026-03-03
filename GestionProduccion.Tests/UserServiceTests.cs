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
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { FullName = "User 1", Email = "user1@test.com", Role = UserRole.Operational, IsActive = true });
            context.Users.Add(new User { FullName = "User 2", Email = "user2@test.com", Role = UserRole.Leader, IsActive = true });
            context.Users.Add(new User { FullName = "Inactive", Email = "inactive@test.com", Role = UserRole.Operational, IsActive = false });
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var teamRepo = new SewingTeamRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo, teamRepo);

            var result = await service.GetActiveUsersAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { FullName = "Test User", Email = "test@test.com", Role = UserRole.Operational, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var teamRepo = new SewingTeamRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo, teamRepo);

            var result = await service.GetUserByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("Test User", result.FullName);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldAddUser_WhenValid()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var teamRepo = new SewingTeamRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo, teamRepo);

            var newUser = new User { FullName = "New User", Email = "new@test.com", PasswordHash = "hash", Role = UserRole.Operational };

            var result = await service.CreateUserAsync(newUser);

            Assert.NotNull(result);
            Assert.Equal("New User", result.FullName);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldModifyUser_WhenExists()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { FullName = "Old Name", Email = "update@test.com", Role = UserRole.Operational, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var teamRepo = new SewingTeamRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo, teamRepo);

            user.FullName = "Updated Name";

            await service.UpdateUserAsync(user);
            var updatedUser = await context.Users.FindAsync(user.Id);

            Assert.Equal("Updated Name", updatedUser.FullName);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldSetIsActiveFalse_WhenExists()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { FullName = "Delete Me", Email = "delete@test.com", Role = UserRole.Operational, IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var orderRepo = new ProductionOrderRepository(context);
            var resetRepo = new PasswordResetTokenRepository(context);
            var refreshRepo = new UserRefreshTokenRepository(context);
            var teamRepo = new SewingTeamRepository(context);
            var service = new UserService(userRepo, orderRepo, resetRepo, refreshRepo, teamRepo);

            await service.DeactivateUserAsync(user.Id);
            var deletedUser = await context.Users.FindAsync(user.Id);

            Assert.False(deletedUser.IsActive);
        }
    }
}
