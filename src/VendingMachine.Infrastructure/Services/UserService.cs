using Microsoft.EntityFrameworkCore;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Enums;
using VendingMachine.Core.Services;
using VendingMachine.Infrastructure.Data;

namespace VendingMachine.Infrastructure.Services;

public class UserService
{
    private readonly VendingMachineDbContext _context;
    private readonly PasswordHasher _passwordHasher;

    public UserService(VendingMachineDbContext context, PasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(string username, string password, UserRole role)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            throw new Exception("Username already exists");
        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = role,
            Deposit = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash)) return null;
        return user;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
} 