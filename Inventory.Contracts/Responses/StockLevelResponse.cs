namespace Inventory.Contracts.Responses;

public class StockLevelResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; }

    // Helper property to highlight rows in the UI
    public bool IsLowStock => Quantity <= LowStockThreshold;
}