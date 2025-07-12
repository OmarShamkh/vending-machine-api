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
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        var deposit = await _buyerService.DepositAsync(userId, dto.Amount);
        return Ok(new { deposit });
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyDto dto)
    {
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        var result = await _buyerService.BuyAsync(userId, dto.ProductId, dto.Amount);
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        var userId = int.Parse(User.FindFirst("nameid")!.Value);
        var result = await _buyerService.ResetAsync(userId);
        return Ok(result);
    }

    [HttpGet("coins")]
    public IActionResult GetValidCoins()
    {
        var validCoins = _buyerService.GetValidCoins();
        return Ok(new { validCoins });
    }
} 