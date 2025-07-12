using System.ComponentModel.DataAnnotations;
using VendingMachine.Core.Enums;

namespace VendingMachine.Core.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public decimal Deposit { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class UpdateUserDto
{
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class DepositDto
{
    public int Amount { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class ResetDto
{
    public byte[]? RowVersion { get; set; }
} 