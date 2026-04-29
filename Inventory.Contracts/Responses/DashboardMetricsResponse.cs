namespace Inventory.Contracts.Responses;

public class DashboardMetricsResponse
{
    /// <summary>True when the response is scoped to a single Store Manager's warehouse.</summary>
    public bool IsStoreManager { get; set; }

    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public int LowStockAlertsCount { get; set; }
    public int PendingTransfersCount { get; set; }

    public List<MovementHistoryResponse> RecentMovements { get; set; } = new();

    /// <summary>Per-product stock for the manager's warehouse (Store Manager view only).</summary>
    public List<StockLevelResponse> MyInventory { get; set; } = new();
}
