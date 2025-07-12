using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.Core.DTOs;
using VendingMachine.Infrastructure.Services;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts() => Ok(await _productService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var sellerId = int.Parse(User.FindFirst("nameid")!.Value);
        var product = await _productService.CreateAsync(dto, sellerId);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product); // product includes RowVersion
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        if (dto.RowVersion == null)
            return BadRequest(new { message = "RowVersion is required for concurrency control." });
        var sellerId = int.Parse(User.FindFirst("nameid")!.Value);
        try
        {
            var product = await _productService.UpdateAsync(id, dto, sellerId);
            if (product == null) return Forbid();
            return Ok(product); // product includes RowVersion
        }
        catch (Exception ex) when (ex.Message.Contains("concurrent update"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var sellerId = int.Parse(User.FindFirst("nameid")!.Value);
        // Optionally fetch product before delete to return RowVersion
        var product = await _productService.GetByIdAsync(id);
        var result = await _productService.DeleteAsync(id, sellerId);
        if (!result) return Forbid();
        return Ok(new { deleted = true, rowVersion = product?.RowVersion });
    }
} 