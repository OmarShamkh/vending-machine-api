using System.ComponentModel.DataAnnotations;

namespace VendingMachine.Core.DTOs;

public class BuyDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be positive")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be at least 1")]
    public int Amount { get; set; }
}

public class BuyResponseDto
{
    public decimal TotalSpent { get; set; }
    public ProductResponseDto Product { get; set; } = null!;
    public int AmountPurchased { get; set; }
    public decimal Change { get; set; }
    public Dictionary<int, int> ChangeBreakdown { get; set; } = new();
}

public class ResetResponseDto
{
    public decimal PreviousDeposit { get; set; }
    public decimal NewDeposit { get; set; }
} 