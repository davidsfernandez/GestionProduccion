/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using FluentAssertions;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Hubs;
using GestionProduccion.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using System.Security.Claims;
using GestionProduccion.Application.Mappers;

namespace GestionProduccion.Tests;

public class OperationalTaskServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHubContext<ProductionHub>> _mockHubContext;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly OperationalTaskService _service;
    private readonly MainMapper _mapper;

    public OperationalTaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockCache = new Mock<IMemoryCache>();
        _mockHubContext = new Mock<IHubContext<ProductionHub>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mapper = new MainMapper();

        // Setup mock user context
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _service = new OperationalTaskService(_context, _mockCache.Object, _mockHubContext.Object, _mockHttpContextAccessor.Object, _mapper);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldInitializeStatusToPending()
    {
        // Arrange
        var dto = new CreateTaskDto { Title = "Buy Coffee", AssignedUserId = 1 };

        // Act
        var result = await _service.CreateTaskAsync(dto);

        // Assert
        result.Status.Should().Be("Pending");

        var dbTask = await _context.OperationalTasks.FindAsync(result.Id);
        dbTask.Should().NotBeNull();
        dbTask!.CompletionDate.Should().BeNull();
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldSetStatusAndDateAndLogHistory()
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
        updatedTask.LastModifiedByUserId.Should().Be(1);

        // Verify history entry
        var history = await _context.OperationalTaskHistories
            .FirstOrDefaultAsync(h => h.OperationalTaskId == task.Id);
        history.Should().NotBeNull();
        history!.NewStatus.Should().Be(OpTaskStatus.Completed);
        history.UserId.Should().Be(1);
    }
}
