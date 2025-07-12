using System.ComponentModel.DataAnnotations;
using VendingMachine.Core.Enums;

namespace VendingMachine.Core.Entities;

public class User
{
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal Deposit { get; set; } = 0;
    [Required]
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
} 