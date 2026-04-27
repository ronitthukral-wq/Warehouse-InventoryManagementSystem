namespace Inventory.ServiceLogic.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}