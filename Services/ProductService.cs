/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly MainMapper _mapper;

    public ProductService(IProductRepository productRepository, IProductionOrderRepository orderRepository, MainMapper mapper)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.ToDtoList(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? _mapper.ToDto(product) : null;
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken ct = default)
    {
        if (await _productRepository.ExistsAsync(productDto.MainSku))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: SKU '{productDto.MainSku}'");
        }

        if (await _productRepository.ExistsByInternalCodeAsync(productDto.InternalCode))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: InternalCode '{productDto.InternalCode}'");
        }

        var product = productDto.ToEntity();
        await _productRepository.AddAsync(product);
        return _mapper.ToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(ProductDto productDto, CancellationToken ct = default)
    {
        var existing = await _productRepository.GetByIdAsync(productDto.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException(ErrorMessages.ElementNotFound);
        }

        if (existing.MainSku != productDto.MainSku && await _productRepository.ExistsAsync(productDto.MainSku))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: SKU '{productDto.MainSku}'");
        }

        if (existing.InternalCode != productDto.InternalCode && await _productRepository.ExistsByInternalCodeAsync(productDto.InternalCode))
        {
            throw new InvalidOperationException($"{ErrorMessages.DuplicateCode}: InternalCode '{productDto.InternalCode}'");
        }

        // Update properties
        existing.Name = productDto.Name;
        existing.InternalCode = productDto.InternalCode;
        existing.FabricType = productDto.FabricType;
        existing.MainSku = productDto.MainSku;
        existing.AverageProductionTimeMinutes = productDto.AverageProductionTimeMinutes;
        existing.EstimatedSalePrice = productDto.EstimatedSalePrice;

        await _productRepository.UpdateAsync(existing);
        return _mapper.ToDto(existing);
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException(ErrorMessages.ElementNotFound);

        try
        {
            await _productRepository.DeleteAsync(product);
        }
        catch (Exception)
        {
            throw new InvalidOperationException(ErrorMessages.CannotDeleteByBusinessRules);
        }
    }

    public async Task RecalculateAverageTimeAsync(int productId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return;

        var query = await _orderRepository.GetQueryableAsync();
        var completedOrders = await query
            .AsNoTracking()
            .Where(o => o.ProductId == productId && o.CurrentStatus == Domain.Enums.ProductionStatus.Completed)
            .Select(o => new { o.EffectiveMinutes, o.Quantity })
            .ToListAsync(ct);

        if (!completedOrders.Any())
        {
            product.AverageProductionTimeMinutes = 0;
        }
        else
        {
            double totalMinutes = completedOrders.Sum(o => o.EffectiveMinutes);
            int totalProduced = completedOrders.Sum(o => o.Quantity);
            
            // Average time PER UNIT produced
            product.AverageProductionTimeMinutes = totalProduced > 0 
                ? Math.Round(totalMinutes / totalProduced, 2) 
                : 0;
        }

        await _productRepository.UpdateAsync(product);
    }
}


