using Inventory.Models.Enums;

namespace Inventory.Models.Entities;

public class TransferRequest : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int FromWarehouseId { get; set; }
    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public int ToWarehouseId { get; set; }
    public virtual Warehouse ToWarehouse { get; set; } = null!;

    public int Quantity { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
}