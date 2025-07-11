using VendingMachine.Core.DTOs;

namespace VendingMachine.Core.Services;

public interface ICoinService
{
    bool IsValidCoin(int amount);
    Dictionary<int, int> CalculateChange(decimal amount);
    int[] GetValidCoins();
} 