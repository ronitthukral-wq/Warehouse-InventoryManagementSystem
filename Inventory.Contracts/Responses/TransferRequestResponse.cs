namespace Inventory.Contracts.Responses;

public class TransferRequestResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    public int FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;

    public int ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    /// <summary>Pending / Accepted / Rejected.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
