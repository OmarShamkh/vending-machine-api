using System.ComponentModel.DataAnnotations;

namespace VendingMachine.Core.DTOs;

public class CreateProductDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Amount available cannot be negative")]
    public int AmountAvailable { get; set; }

    [Required]
    [Range(5, double.MaxValue, ErrorMessage = "Cost must be at least 5 cents")]
    public decimal Cost { get; set; }
}

public class UpdateProductDto
{
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string? ProductName { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Amount available cannot be negative")]
    public int? AmountAvailable { get; set; }

    [Range(5, double.MaxValue, ErrorMessage = "Cost must be at least 5 cents")]
    public decimal? Cost { get; set; }
}

public class ProductResponseDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int AmountAvailable { get; set; }
    public decimal Cost { get; set; }
    public string SellerId { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 