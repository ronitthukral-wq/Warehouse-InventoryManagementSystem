namespace Inventory.Models.Entities;

public class Stock
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; } = 0;
}