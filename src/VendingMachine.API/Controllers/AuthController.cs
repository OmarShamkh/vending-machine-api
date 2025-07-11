using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VendingMachine.API.Services;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Exceptions;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            Log.Information("Attempting to register user: {Username}", createUserDto.Username);

            var existingUser = await _userManager.FindByNameAsync(createUserDto.Username);
            if (existingUser != null)
            {
                Log.Warning("Registration failed: Username {Username} already exists", createUserDto.Username);
                return BadRequest(new { message = "Username already exists" });
            }

            var user = new User
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Username + "@vendingmachine.com", // Simple email generation
                Role = createUserDto.Role,
                Deposit = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                Log.Warning("Registration failed for user {Username}: {Errors}", 
                    createUserDto.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { message = "Registration failed", errors = result.Errors.Select(e => e.Description) });
            }

            // Add role
            await _userManager.AddToRoleAsync(user, createUserDto.Role.ToString());

            Log.Information("User {Username} registered successfully", createUserDto.Username);

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Deposit = user.Deposit,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during user registration for {Username}", createUserDto.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            Log.Information("Login attempt for user: {Username}", loginDto.Username);

            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                Log.Warning("Login failed: User {Username} not found", loginDto.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                Log.Warning("Login failed: Invalid password for user {Username}", loginDto.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtService.GenerateToken(user);

            Log.Information("User {Username} logged in successfully", loginDto.Username);

            return Ok(new
            {
                token,
                user = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    Deposit = user.Deposit,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during login for user {Username}", loginDto.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Deposit = user.Deposit,
                Role = user.Role,
                CreatedAt = user.CreatedAt
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