using Microsoft.EntityFrameworkCore;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.Infrastructure.Services;

public class ProductService
{
    private readonly VendingMachineDbContext _context;

    public ProductService(VendingMachineDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                AmountAvailable = p.AmountAvailable,
                Cost = p.Cost,
                SellerId = p.SellerId,
                SellerName = p.Seller.Username,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToListAsync();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var p = await _context.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return null;
        return new ProductResponseDto
        {
            Id = p.Id,
            ProductName = p.ProductName,
            AmountAvailable = p.AmountAvailable,
            Cost = p.Cost,
            SellerId = p.SellerId,
            SellerName = p.Seller.Username,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto, int sellerId)
    {
        var product = new Product
        {
            ProductName = dto.ProductName,
            AmountAvailable = dto.AmountAvailable,
            Cost = dto.Cost,
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(product.Id) ?? throw new Exception("Product creation failed");
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto, int sellerId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
        if (product == null) 
            return null;

        if (!string.IsNullOrEmpty(dto.ProductName)) 
            product.ProductName = dto.ProductName;

        if (dto.AmountAvailable.HasValue) 
            product.AmountAvailable = dto.AmountAvailable.Value;

        if (dto.Cost.HasValue) 
            product.Cost = dto.Cost.Value;

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(product.Id);
    }

    public async Task<bool> DeleteAsync(int id, int sellerId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
} 