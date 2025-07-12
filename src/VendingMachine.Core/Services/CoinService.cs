namespace VendingMachine.Core.Services;

public class CoinService : ICoinService
{
    private readonly int[] _validCoins = { 5, 10, 20, 50, 100 };

    public bool IsValidCoin(int amount)
    {
        return _validCoins.Contains(amount);
    }

    public Dictionary<int, int> CalculateChange(decimal amount)
    {
        if (amount <= 0)
            return new Dictionary<int, int>();

        var change = new Dictionary<int, int>();
        var remainingAmount = (int)amount; 

        var sortedCoins = _validCoins.OrderByDescending(c => c).ToArray();

        foreach (var coin in sortedCoins)
        {
            if (remainingAmount >= coin)
            {
                var count = remainingAmount / coin;
                change[coin] = count;
                remainingAmount %= coin;
            }
        }

        return change;
    }

    public int[] GetValidCoins()
    {
        return _validCoins;
    }
} 