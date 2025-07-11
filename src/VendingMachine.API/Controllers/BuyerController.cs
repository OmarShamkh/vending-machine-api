using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Exceptions;
using VendingMachine.Core.Services;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BuyerOnly")]
public class BuyerController : ControllerBase
{
    private readonly VendingMachineDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ICoinService _coinService;

    public BuyerController(
        VendingMachineDbContext context,
        UserManager<User> userManager,
        ICoinService coinService)
    {
        _context = context;
        _userManager = userManager;
        _coinService = coinService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositDto depositDto)
    {
        try
        {
            Log.Information("Deposit attempt: {Amount} cents", depositDto.Amount);

            // Validate coin denomination
            if (!_coinService.IsValidCoin(depositDto.Amount))
            {
                Log.Warning("Invalid coin denomination: {Amount}", depositDto.Amount);
                return BadRequest(new { message = "Invalid coin denomination. Only 5, 10, 20, 50, and 100 cent coins are accepted" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update deposit
            user.Deposit += depositDto.Amount;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            Log.Information("Deposit successful: {Amount} cents added to user {UserId}. New balance: {NewBalance}", 
                depositDto.Amount, userId, user.Deposit);

            return Ok(new
            {
                message = "Deposit successful",
                deposit = user.Deposit
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during deposit operation for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyDto buyDto)
    {
        try
        {
            Log.Information("Purchase attempt: Product {ProductId}, Amount {Amount}", buyDto.ProductId, buyDto.Amount);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == buyDto.ProductId);
            if (product == null)
            {
                Log.Warning("Product {ProductId} not found", buyDto.ProductId);
                return NotFound(new { message = "Product not found" });
            }

            // Check stock availability
            if (product.AmountAvailable < buyDto.Amount)
            {
                Log.Warning("Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}", 
                    buyDto.ProductId, buyDto.Amount, product.AmountAvailable);
                return BadRequest(new { message = "Insufficient stock for this product" });
            }

            var totalCost = product.Cost * buyDto.Amount;

            // Check if user has enough funds
            if (user.Deposit < totalCost)
            {
                Log.Warning("Insufficient funds for user {UserId}. Required: {Required}, Available: {Available}", 
                    userId, totalCost, user.Deposit);
                return BadRequest(new { message = "Insufficient funds for this purchase" });
            }

            // Calculate change
            var change = user.Deposit - totalCost;
            var changeBreakdown = _coinService.CalculateChange(change);

            // Update product stock
            product.AmountAvailable -= buyDto.Amount;
            product.UpdatedAt = DateTime.UtcNow;

            // Update user deposit
            user.Deposit = 0; // Reset to 0 since we're giving change
            user.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new Transaction
            {
                BuyerId = userId,
                ProductId = buyDto.ProductId,
                Amount = buyDto.Amount,
                TotalSpent = totalCost,
                Change = change,
                CreatedAt = DateTime.UtcNow
            };

            // Save all changes in a transaction
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Products.Update(product);
                await _userManager.UpdateAsync(user);
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                Log.Information("Purchase successful: User {UserId} bought {Amount} of product {ProductId} for {TotalCost}. Change: {Change}", 
                    userId, buyDto.Amount, buyDto.ProductId, totalCost, change);

                var productResponse = new ProductResponseDto
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    AmountAvailable = product.AmountAvailable,
                    Cost = product.Cost,
                    SellerId = product.SellerId,
                    SellerName = "", // We don't need to load the seller for this response
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                var buyResponse = new BuyResponseDto
                {
                    TotalSpent = totalCost,
                    Product = productResponse,
                    AmountPurchased = buyDto.Amount,
                    Change = change,
                    ChangeBreakdown = changeBreakdown
                };

                return Ok(buyResponse);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during purchase operation for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        try
        {
            Log.Information("Reset deposit request");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var previousDeposit = user.Deposit;
            user.Deposit = 0;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            Log.Information("Deposit reset for user {UserId}. Previous deposit: {PreviousDeposit}", 
                userId, previousDeposit);

            var resetResponse = new ResetResponseDto
            {
                PreviousDeposit = previousDeposit,
                NewDeposit = user.Deposit
            };

            return Ok(resetResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during deposit reset for user {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("coins")]
    public IActionResult GetValidCoins()
    {
        try
        {
            var validCoins = _coinService.GetValidCoins();
            return Ok(new { validCoins });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving valid coins");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
} 