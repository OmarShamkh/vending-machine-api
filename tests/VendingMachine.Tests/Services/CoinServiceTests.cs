using FluentAssertions;
using VendingMachine.Core.Services;
using Xunit;

namespace VendingMachine.Tests.Services;

public class CoinServiceTests
{
    private readonly ICoinService _coinService;

    public CoinServiceTests()
    {
        _coinService = new CoinService();
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(1, false)]
    [InlineData(25, false)]
    [InlineData(75, false)]
    [InlineData(200, false)]
    public void IsValidCoin_ShouldReturnExpectedResult(int amount, bool expected)
    {
        // Act
        var result = _coinService.IsValidCoin(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 1)]
    [InlineData(10, 1)]
    [InlineData(15, 2)] // 10 + 5
    [InlineData(25, 2)] // 20 + 5
    [InlineData(30, 2)] // 20 + 10
    [InlineData(35, 3)] // 20 + 10 + 5
    [InlineData(50, 1)]
    [InlineData(100, 1)]
    [InlineData(125, 3)] // 100 + 20 + 5
    [InlineData(150, 2)] // 100 + 50
    public void CalculateChange_ShouldReturnCorrectBreakdown(int amount, int expectedTotalCoins)
    {
        // Act
        var result = _coinService.CalculateChange(amount);

        // Assert
        var totalCoins = result.Values.Sum();
        totalCoins.Should().Be(expectedTotalCoins);

        var totalValue = result.Sum(kvp => kvp.Key * kvp.Value);
        totalValue.Should().Be(amount);
    }

    [Fact]
    public void GetValidCoins_ShouldReturnExpectedCoins()
    {
        // Act
        var result = _coinService.GetValidCoins();

        // Assert
        result.Should().BeEquivalentTo(new[] { 5, 10, 20, 50, 100 });
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(35)]
    [InlineData(125)]
    [InlineData(150)]
    public void CalculateChange_ShouldReturnOptimalBreakdown(int amount)
    {
        // Act
        var result = _coinService.CalculateChange(amount);

        // Assert
        var sortedCoins = result.Keys.OrderByDescending(k => k).ToList();
        var resultCoins = result.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).OrderByDescending(k => k).ToList();
        
        resultCoins.Should().BeEquivalentTo(sortedCoins.Take(resultCoins.Count));
    }
} 