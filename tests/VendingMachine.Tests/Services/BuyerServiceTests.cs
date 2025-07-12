using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Core.Services;
using VendingMachine.Infrastructure.Data;
using VendingMachine.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class BuyerServiceTests
{
    private readonly Mock<ICoinService> _coinServiceMock = new();
    private readonly DbContextOptions<VendingMachineDbContext> _dbOptions;

    public BuyerServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<VendingMachineDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task DepositAsync_ValidCoin_IncreasesDeposit()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var user = new User { Id = 1, Username = "buyer", Deposit = 0, Role = VendingMachine.Core.Enums.UserRole.Buyer, PasswordHash = "hash" };
        context.Users.Add(user);
        context.SaveChanges();
        _coinServiceMock.Setup(x => x.IsValidCoin(10)).Returns(true);
        var service = new BuyerService(context, _coinServiceMock.Object);
        var rowVersion = user.RowVersion;
        var result = await service.DepositAsync(1, 10, rowVersion);
        result.Should().Be(10);
    }

    [Fact]
    public async Task DepositAsync_InvalidCoin_Throws()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var user = new User { Id = 1, Username = "buyer", Deposit = 0, Role = VendingMachine.Core.Enums.UserRole.Buyer, PasswordHash = "hash" };
        context.Users.Add(user);
        context.SaveChanges();
        _coinServiceMock.Setup(x => x.IsValidCoin(3)).Returns(false);
        var service = new BuyerService(context, _coinServiceMock.Object);
        Func<Task> act = async () => await service.DepositAsync(1, 3, user.RowVersion);
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid coin denomination*");
    }

    [Fact]
    public async Task ResetAsync_ResetsDeposit()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var user = new User { Id = 1, Username = "buyer", Deposit = 50, Role = VendingMachine.Core.Enums.UserRole.Buyer, PasswordHash = "hash" };
        context.Users.Add(user);
        context.SaveChanges();
        var service = new BuyerService(context, _coinServiceMock.Object);
        var rowVersion = user.RowVersion;
        var result = await service.ResetAsync(1, rowVersion);
        result.PreviousDeposit.Should().Be(50);
        result.NewDeposit.Should().Be(0);
    }

    [Fact]
    public void GetValidCoins_ReturnsCoins()
    {
        _coinServiceMock.Setup(x => x.GetValidCoins()).Returns(new[] { 5, 10, 20 });
        using var context = new VendingMachineDbContext(_dbOptions);
        var service = new BuyerService(context, _coinServiceMock.Object);
        var coins = service.GetValidCoins();
        coins.Should().BeEquivalentTo(new[] { 5, 10, 20 });
    }

    [Fact]
    public async Task DepositAsync_ConcurrencyConflict_Throws()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var user = new User { Id = 1, Username = "buyer", Deposit = 0, Role = VendingMachine.Core.Enums.UserRole.Buyer, PasswordHash = "hash" };
        context.Users.Add(user);
        context.SaveChanges();
        _coinServiceMock.Setup(x => x.IsValidCoin(10)).Returns(true);
        var service = new BuyerService(context, _coinServiceMock.Object);
        // Simulate concurrency conflict by passing wrong RowVersion
        var wrongRowVersion = new byte[] { 1, 2, 3 };
        Func<Task> act = async () => await service.DepositAsync(1, 10, wrongRowVersion);
        await act.Should().ThrowAsync<Exception>().WithMessage("Deposit failed due to a concurrent update*");
    }
} 