/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;
using System.Threading;

namespace GestionProduccion.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken ct = default);
    Task<ProductDto> UpdateProductAsync(ProductDto productDto, CancellationToken ct = default);
    Task DeleteProductAsync(int id, CancellationToken ct = default);
    Task RecalculateAverageTimeAsync(int productId, CancellationToken ct = default);
}


