using Inventory.Models.Enums;

namespace Inventory.Models.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public string? Note { get; set; }
}
