using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Exceptions;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly VendingMachineDbContext _context;
    private readonly UserManager<User> _userManager;

    public ProductsController(VendingMachineDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            Log.Information("Retrieving all products");

            var products = await _context.Products
                .Include(p => p.Seller)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    AmountAvailable = p.AmountAvailable,
                    Cost = p.Cost,
                    SellerId = p.SellerId,
                    SellerName = p.Seller.UserName ?? "",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            Log.Information("Retrieving product with ID: {ProductId}", id);

            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                Log.Warning("Product with ID {ProductId} not found", id);
                return NotFound(new { message = "Product not found" });
            }

            var productResponse = new ProductResponseDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                AmountAvailable = product.AmountAvailable,
                Cost = product.Cost,
                SellerId = product.SellerId,
                SellerName = product.Seller.UserName ?? "",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Ok(productResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            Log.Information("Creating new product: {ProductName}", createProductDto.ProductName);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var product = new Product
            {
                ProductName = createProductDto.ProductName,
                AmountAvailable = createProductDto.AmountAvailable,
                Cost = createProductDto.Cost,
                SellerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            Log.Information("Product {ProductName} created successfully with ID: {ProductId}", 
                createProductDto.ProductName, product.Id);

            var productResponse = new ProductResponseDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                AmountAvailable = product.AmountAvailable,
                Cost = product.Cost,
                SellerId = product.SellerId,
                SellerName = User.Identity?.Name ?? "",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating product {ProductName}", createProductDto.ProductName);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            Log.Information("Updating product with ID: {ProductId}", id);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                Log.Warning("Product with ID {ProductId} not found", id);
                return NotFound(new { message = "Product not found" });
            }

            // Check if the user is the seller of this product
            if (product.SellerId != userId)
            {
                Log.Warning("User {UserId} attempted to update product {ProductId} owned by {SellerId}", 
                    userId, id, product.SellerId);
                return Forbid();
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateProductDto.ProductName))
                product.ProductName = updateProductDto.ProductName;

            if (updateProductDto.AmountAvailable.HasValue)
                product.AmountAvailable = updateProductDto.AmountAvailable.Value;

            if (updateProductDto.Cost.HasValue)
                product.Cost = updateProductDto.Cost.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Log.Information("Product {ProductId} updated successfully", id);

            var productResponse = new ProductResponseDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                AmountAvailable = product.AmountAvailable,
                Cost = product.Cost,
                SellerId = product.SellerId,
                SellerName = User.Identity?.Name ?? "",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Ok(productResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            Log.Information("Deleting product with ID: {ProductId}", id);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                Log.Warning("Product with ID {ProductId} not found", id);
                return NotFound(new { message = "Product not found" });
            }

            // Check if the user is the seller of this product
            if (product.SellerId != userId)
            {
                Log.Warning("User {UserId} attempted to delete product {ProductId} owned by {SellerId}", 
                    userId, id, product.SellerId);
                return Forbid();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            Log.Information("Product {ProductId} deleted successfully", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
} 