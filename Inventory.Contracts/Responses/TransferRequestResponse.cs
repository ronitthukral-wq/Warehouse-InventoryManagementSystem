namespace Inventory.Contracts.Responses;

public class TransferRequestResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string Product { get; set; } = string.Empty;

    public int FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;

    public int ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    /// <summary>"Incoming" or "Outgoing" relative to the current Store Manager.</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>True if the current Store Manager can Accept/Reject this transfer.</summary>
    public bool IsActionable { get; set; }
}