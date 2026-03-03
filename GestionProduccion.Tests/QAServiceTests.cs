using FluentAssertions;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class QAServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly QAService _service;
    private readonly Repository<QADefect> _defectRepo;

    public QAServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockFileStorage = new Mock<IFileStorageService>();
        _defectRepo = new Repository<QADefect>(_context); // Using generic repo implementation

        _service = new QAService(_defectRepo, _mockFileStorage.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterDefect_ShouldSaveDefect_AndUploadPhoto()
    {
        // Arrange
        var dto = new CreateQADefectDto { ProductionOrderId = 1, Reason = "Torn Fabric", Quantity = 5, ReportedByUserId = 1 };
        var mockFile = new Mock<IFormFile>();
        _mockFileStorage.Setup(fs => fs.UploadAsync(It.IsAny<IFormFile>(), "defects")).ReturnsAsync("http://storage/img.jpg");

        // Act
        var result = await _service.RegisterDefectAsync(dto, mockFile.Object);

        // Assert
        result.Should().NotBeNull();
        result.PhotoUrl.Should().Be("http://storage/img.jpg");
        result.Reason.Should().Be("Torn Fabric");

        var dbDefect = await _context.QADefects.FindAsync(result.Id);
        dbDefect.Should().NotBeNull();

        _mockFileStorage.Verify(fs => fs.UploadAsync(mockFile.Object, "defects"), Times.Once);
    }

    [Fact]
    public async Task DeleteDefect_ShouldRemoveRecord_AndTriggerFileDeletion()
    {
        // Arrange
        var defect = new QADefect
        {
            ProductionOrderId = 1,
            Reason = "Stain",
            Quantity = 1,
            ReportedByUserId = 1,
            PhotoUrl = "http://storage/stain.jpg"
        };
        _context.QADefects.Add(defect);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteDefectAsync(defect.Id);

        // Assert
        var dbDefect = await _context.QADefects.FindAsync(defect.Id);
        dbDefect.Should().BeNull();

        _mockFileStorage.Verify(fs => fs.DeleteAsync("stain.jpg", "defects"), Times.Once);
    }
}
