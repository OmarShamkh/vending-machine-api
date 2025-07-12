using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.Core.DTOs;
using VendingMachine.Infrastructure.Services;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BuyerOnly")]
public class BuyerController : ControllerBase
{
    private readonly BuyerService _buyerService;

    public BuyerController(BuyerService buyerService)
    {
        _buyerService = buyerService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
    {
        if (dto.RowVersion == null)
            return BadRequest(new { message = "RowVersion is required for concurrency control." });
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        try
        {
            var deposit = await _buyerService.DepositAsync(userId, dto.Amount, dto.RowVersion);
            // Fetch updated user to get new RowVersion
            var user = await _buyerService.GetUserByIdAsync(userId);
            return Ok(new { deposit, rowVersion = user?.RowVersion });
        }
        catch (Exception ex) when (ex.Message.Contains("concurrent update"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyDto dto)
    {
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        var result = await _buyerService.BuyAsync(userId, dto.ProductId, dto.Amount);
        // Fetch updated user to get new RowVersion
        var user = await _buyerService.GetUserByIdAsync(userId);
        return Ok(new { result, rowVersion = user?.RowVersion });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset([FromBody] ResetDto dto)
    {
        if (dto.RowVersion == null)
            return BadRequest(new { message = "RowVersion is required for concurrency control." });
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        try
        {
            var result = await _buyerService.ResetAsync(userId, dto.RowVersion);
            // Fetch updated user to get new RowVersion
            var user = await _buyerService.GetUserByIdAsync(userId);
            return Ok(new { result, rowVersion = user?.RowVersion });
        }
        catch (Exception ex) when (ex.Message.Contains("concurrent update"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("coins")]
    public IActionResult GetValidCoins()
    {
        var validCoins = _buyerService.GetValidCoins();
        return Ok(new { validCoins });
    }
} 