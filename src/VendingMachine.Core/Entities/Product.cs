using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachine.Core.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Amount available cannot be negative")]
    public int AmountAvailable { get; set; }

    [Required]
    [Range(5, double.MaxValue, ErrorMessage = "Cost must be at least 5 cents")]
    public decimal Cost { get; set; }

    [Required]
    public int SellerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SellerId")]
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
} 