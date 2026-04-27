namespace Inventory.Contracts.Responses;

// Add the 'public' keyword here
public class TransferRequestResponse
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public string ToWarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}