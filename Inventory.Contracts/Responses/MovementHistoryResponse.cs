namespace Inventory.Contracts.Responses;

public class MovementHistoryResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty; // Purchase, TransferIn, etc.
    public int Quantity { get; set; }
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}