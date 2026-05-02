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

    // Alert when:
    //   • A threshold has been configured (> 0), AND
    //   • Quantity has fallen at or below it (including 0 after a transfer fully drained the warehouse).
    // Products that were NEVER stocked in a warehouse have no Stock row at all, so they
    // never reach this property — no false-positive risk.
    public bool IsLowStock => LowStockThreshold > 0 && Quantity <= LowStockThreshold;
}