namespace Inventory.ServiceLogic.Services;

public interface IWarehouseManagerGuard
{
    Task<bool> WarehouseAlreadyHasManagerAsync(int warehouseId, string? excludeUserId, CancellationToken ct = default);
}