using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachine.Core.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BuyerId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be at least 1")]
    public int Amount { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Total spent cannot be negative")]
    public decimal TotalSpent { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Change cannot be negative")]
    public decimal Change { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("BuyerId")]
    public virtual User Buyer { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
} 