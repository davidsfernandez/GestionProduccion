using FluentAssertions;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProduccion.Tests;

public class OperationalTaskServiceTests
{
    private readonly AppDbContext _context;
    private readonly OperationalTaskService _service;

    public OperationalTaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new OperationalTaskService(_context);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldInitializeStatusToPending()
    {
        // Arrange
        var dto = new CreateTaskDto { Title = "Buy Coffee", AssignedUserId = 1 };

        // Act
        var result = await _service.CreateTaskAsync(dto);

        // Assert
        result.Status.Should().Be(OpTaskStatus.Pending);
        result.CompletionDate.Should().BeNull();

        var dbTask = await _context.OperationalTasks.FindAsync(result.Id);
        dbTask.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldSetStatusAndDate()
    {
        // Arrange
        var task = new OperationalTask { Title = "Test Task", Status = OpTaskStatus.Pending };
        _context.OperationalTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        await _service.CompleteTaskAsync(task.Id);

        // Assert
        var updatedTask = await _context.OperationalTasks.FindAsync(task.Id);
        updatedTask!.Status.Should().Be(OpTaskStatus.Completed);
        updatedTask.CompletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
