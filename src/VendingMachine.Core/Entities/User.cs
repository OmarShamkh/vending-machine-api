using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using VendingMachine.Core.Enums;

namespace VendingMachine.Core.Entities;

public class User : IdentityUser
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Deposit cannot be negative")]
    public decimal Deposit { get; set; } = 0;

    [Required]
    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
} 