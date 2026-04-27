namespace Inventory.Contracts.Responses;

public class DashboardMetricsResponse
{
    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public int LowStockAlertsCount { get; set; }
    public int PendingTransfersCount { get; set; }
    public List<MovementHistoryResponse> RecentMovements { get; set; } = new();
}