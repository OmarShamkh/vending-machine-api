using Microsoft.AspNetCore.Mvc;
using Serilog;
using VendingMachine.API.Services;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Enums;
using VendingMachine.Core.Services;
using VendingMachine.Infrastructure.Services;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(VendingMachine.Infrastructure.Services.UserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.RegisterAsync(createUserDto.Username, createUserDto.Password, createUserDto.Role);
            var userResponse = new UserResponseDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Deposit = user.Deposit,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                RowVersion = user.RowVersion
            };
            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during user registration for {Username}", createUserDto.Username);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await _userService.AuthenticateAsync(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            var token = _jwtService.GenerateToken(user);
            var userResponse = new UserResponseDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Deposit = user.Deposit,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                RowVersion = user.RowVersion
            };
            return Ok(new { token, user = userResponse });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during login for user {Username}", loginDto.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            var userResponse = new UserResponseDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Deposit = user.Deposit,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                RowVersion = user.RowVersion
            };
            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 