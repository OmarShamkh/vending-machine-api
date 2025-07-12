using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Services;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.Infrastructure.Services;

public class BuyerService
{
    private readonly VendingMachineDbContext _context;
    private readonly ICoinService _coinService;

    public BuyerService(VendingMachineDbContext context, ICoinService coinService)
    {
        _context = context;
        _coinService = coinService;
    }

    public async Task<decimal> DepositAsync(int userId, int amount)
    {
        if (!_coinService.IsValidCoin(amount))
            throw new Exception("Invalid coin denomination. Only 5, 10, 20, 50, and 100 cent coins are accepted");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) 
            throw new Exception("User not found");

        user.Deposit += amount;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user.Deposit;
    }

    public async Task<BuyResponseDto> BuyAsync(int userId, int productId, int amount)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) 
            throw new Exception("User not found");

        var product = await _context.Products.FindAsync(productId);
        if (product == null) 
            throw new Exception("Product not found");

        if (product.AmountAvailable < amount) 
            throw new Exception("Insufficient stock for this product");

        var totalCost = product.Cost * amount;
        if (user.Deposit < totalCost) 
            throw new Exception("Insufficient funds for this purchase");

        var change = user.Deposit - totalCost;
        var changeBreakdown = _coinService.CalculateChange(change);

        product.AmountAvailable -= amount;
        product.UpdatedAt = DateTime.UtcNow;
        user.Deposit = 0;
        user.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            BuyerId = userId,
            ProductId = productId,
            Amount = amount,
            TotalSpent = totalCost,
            Change = change,
            CreatedAt = DateTime.UtcNow
        };

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Products.Update(product);
            _context.Users.Update(user);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        return new BuyResponseDto
        {
            TotalSpent = totalCost,
            Product = new ProductResponseDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                AmountAvailable = product.AmountAvailable,
                Cost = product.Cost,
                SellerId = product.SellerId,
                SellerName = "",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            },
            AmountPurchased = amount,
            Change = change,
            ChangeBreakdown = changeBreakdown
        };
    }

    public async Task<ResetResponseDto> ResetAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) 
            throw new Exception("User not found");

        var previousDeposit = user.Deposit;
        user.Deposit = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return new ResetResponseDto
        {
            PreviousDeposit = previousDeposit,
            NewDeposit = user.Deposit
        };
    }

    public int[] GetValidCoins()
    {
        return _coinService.GetValidCoins();
    }
} 