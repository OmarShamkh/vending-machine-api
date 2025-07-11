namespace VendingMachine.Core.Exceptions;

public class VendingMachineException : Exception
{
    public VendingMachineException(string message) : base(message) { }
    public VendingMachineException(string message, Exception innerException) : base(message, innerException) { }
}

public class InsufficientFundsException : VendingMachineException
{
    public InsufficientFundsException(string message = "Insufficient funds for this purchase") : base(message) { }
}

public class InsufficientStockException : VendingMachineException
{
    public InsufficientStockException(string message = "Insufficient stock for this product") : base(message) { }
}

public class InvalidCoinException : VendingMachineException
{
    public InvalidCoinException(string message = "Invalid coin denomination. Only 5, 10, 20, 50, and 100 cent coins are accepted") : base(message) { }
}

public class ProductNotFoundException : VendingMachineException
{
    public ProductNotFoundException(string message = "Product not found") : base(message) { }
}

public class UnauthorizedAccessException : VendingMachineException
{
    public UnauthorizedAccessException(string message = "You are not authorized to perform this action") : base(message) { }
} 