namespace Inventory.Contracts.Responses;

public class WarehouseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}