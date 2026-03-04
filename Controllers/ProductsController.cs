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
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync(HttpContext.RequestAborted);
        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            FabricType = p.FabricType,
            MainSku = p.MainSku,
            AverageProductionTimeMinutes = p.AverageProductionTimeMinutes,
            EstimatedSalePrice = p.EstimatedSalePrice
        }).ToList();

        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = dtos });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var p = await _productService.GetProductByIdAsync(id, HttpContext.RequestAborted);
        if (p == null) return NotFound(new ApiResponse<object?> { Success = false, Message = "Product not found" });

        var dto = new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            FabricType = p.FabricType,
            MainSku = p.MainSku,
            AverageProductionTimeMinutes = p.AverageProductionTimeMinutes,
            EstimatedSalePrice = p.EstimatedSalePrice
        };

        return Ok(new ApiResponse<ProductDto> { Success = true, Data = dto });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                InternalCode = dto.InternalCode,
                FabricType = dto.FabricType,
                MainSku = dto.MainSku,
                AverageProductionTimeMinutes = dto.AverageProductionTimeMinutes,
                EstimatedSalePrice = dto.EstimatedSalePrice
            };

            var created = await _productService.CreateProductAsync(product, HttpContext.RequestAborted);

            var result = new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                MainSku = created.MainSku,
                EstimatedSalePrice = created.EstimatedSalePrice
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<ProductDto> { Success = true, Data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        if (id != dto.Id) return BadRequest(new ApiResponse<object?> { Success = false, Message = "ID mismatch" });

        try
        {
            var product = new Product
            {
                Id = id,
                Name = dto.Name,
                InternalCode = dto.InternalCode,
                FabricType = dto.FabricType,
                MainSku = dto.MainSku,
                AverageProductionTimeMinutes = dto.AverageProductionTimeMinutes,
                EstimatedSalePrice = dto.EstimatedSalePrice
            };

            await _productService.UpdateProductAsync(product, HttpContext.RequestAborted);
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<object?> { Success = false, Message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id, HttpContext.RequestAborted);
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<object?> { Success = false, Message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetStats(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, HttpContext.RequestAborted);
        if (product == null) return NotFound(new ApiResponse<object?> { Success = false, Message = "Product not found" });

        var stats = new
        {
            ProductId = product.Id,
            AverageMinutes = product.AverageProductionTimeMinutes,
            EstimatedDays = Math.Round(product.AverageProductionTimeMinutes / 60 / 8, 1)
        };

        return Ok(new ApiResponse<object> { Success = true, Data = stats });
    }
}
