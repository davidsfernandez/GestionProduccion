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
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Tests;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductService _service;
    private readonly ProductRepository _productRepo;
    private readonly ProductionOrderRepository _orderRepo;
    private readonly MainMapper _mapper;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _productRepo = new ProductRepository(_context);
        _orderRepo = new ProductionOrderRepository(_context);
        _mapper = new MainMapper();

        _service = new ProductService(_productRepo, _orderRepo, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RecalculateAverageTimeAsync_ShouldReturnCorrectAverage_BasedOnHistory()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Shirt", InternalCode = "TS-01", MainSku = "SKU-01", FabricType = "Silk" };
        _context.Products.Add(product);

        var now = DateTime.UtcNow;
        var orders = new List<ProductionOrder>
        {
            new() { LotCode = "OP1", ProductId = 1, Quantity = 10, CurrentStatus = ProductionStatus.Completed, StartedAt = now.AddMinutes(-60), CompletedAt = now, EffectiveMinutes = 600 },
            new() { LotCode = "OP2", ProductId = 1, Quantity = 10, CurrentStatus = ProductionStatus.Completed, StartedAt = now.AddMinutes(-120), CompletedAt = now, EffectiveMinutes = 1200 },
            new() { LotCode = "OP3", ProductId = 1, Quantity = 10, CurrentStatus = ProductionStatus.Completed, StartedAt = now.AddMinutes(-180), CompletedAt = now, EffectiveMinutes = 1800 }
        };
        _context.ProductionOrders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        await _service.RecalculateAverageTimeAsync(1);

        // Assert
        var updatedProduct = await _context.Products.FindAsync(1);
        updatedProduct!.AverageProductionTimeMinutes.Should().Be(120);
    }

    [Fact]
    public async Task RecalculateAverageTimeAsync_ShouldReturnDefault_WhenNoHistoryExists()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "New Product", InternalCode = "NP-01", MainSku = "SKU-NP", FabricType = "Cotton" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        await _service.RecalculateAverageTimeAsync(1);

        // Assert
        var updatedProduct = await _context.Products.FindAsync(1);
        updatedProduct!.AverageProductionTimeMinutes.Should().Be(0);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowException_WhenInternalCodeIsDuplicated()
    {
        // Arrange
        var existing = new Product { Name = "P1", InternalCode = "DUP-01", MainSku = "SKU-1", FabricType = "F1" };
        _context.Products.Add(existing);
        await _context.SaveChangesAsync();

        var newProduct = new Product { Name = "P2", InternalCode = "DUP-01", MainSku = "SKU-2", FabricType = "F2" };

        // Act
        Func<Task> act = async () => await _service.CreateProductAsync(newProduct.ToDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*InternalCode*");
    }
}


