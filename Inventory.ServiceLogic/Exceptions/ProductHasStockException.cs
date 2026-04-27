namespace Inventory.ServiceLogic.Exceptions;

public class ProductHasStockException : Exception
{
    public ProductHasStockException(string message) : base(message) { }
}