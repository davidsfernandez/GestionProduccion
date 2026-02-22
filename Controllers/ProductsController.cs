using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            FabricType = p.FabricType,
            MainSku = p.MainSku,
            AverageProductionTimeMinutes = p.AverageProductionTimeMinutes,
            EstimatedSalePrice = p.EstimatedSalePrice,
            Sizes = p.Sizes.Select(s => new ProductSizeDto { Id = s.Id, SizeName = s.SizeName }).ToList()
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await _productService.GetProductByIdAsync(id);
        if (p == null) return NotFound();

        return Ok(new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            FabricType = p.FabricType,
            MainSku = p.MainSku,
            AverageProductionTimeMinutes = p.AverageProductionTimeMinutes,
            EstimatedSalePrice = p.EstimatedSalePrice,
            Sizes = p.Sizes.Select(s => new ProductSizeDto { Id = s.Id, SizeName = s.SizeName }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                InternalCode = dto.InternalCode,
                FabricType = dto.FabricType,
                MainSku = dto.MainSku,
                EstimatedSalePrice = dto.EstimatedSalePrice,
                Sizes = dto.Sizes.Select(s => new ProductSize { SizeName = s }).ToList()
            };

            var created = await _productService.CreateProductAsync(product);
            
            // Map back to DTO manually for simplicity
            var result = new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                MainSku = created.MainSku,
                EstimatedSalePrice = created.EstimatedSalePrice,
                Sizes = created.Sizes.Select(s => new ProductSizeDto { Id = s.Id, SizeName = s.SizeName }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        if (id != dto.Id) return BadRequest();

        try
        {
            var product = new Product
            {
                Id = id,
                Name = dto.Name,
                InternalCode = dto.InternalCode,
                FabricType = dto.FabricType,
                MainSku = dto.MainSku,
                EstimatedSalePrice = dto.EstimatedSalePrice,
                Sizes = dto.Sizes.Select(s => new ProductSize { SizeName = s }).ToList()
            };

            await _productService.UpdateProductAsync(product);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<object>> GetStats(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        return Ok(new 
        { 
            ProductId = product.Id,
            AverageMinutes = product.AverageProductionTimeMinutes,
            // Calculated estimation logic can be expanded here
            EstimatedDays = Math.Round(product.AverageProductionTimeMinutes / 60 / 8, 1) // Assuming 8h work day
        });
    }
}
