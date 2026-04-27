namespace Inventory.ServiceLogic.Exceptions;

public class WarehouseAlreadyAssignedException : Exception
{
    public WarehouseAlreadyAssignedException(string message) : base(message) { }
}