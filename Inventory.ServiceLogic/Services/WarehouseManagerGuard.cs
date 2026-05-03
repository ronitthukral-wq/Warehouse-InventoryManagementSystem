using Inventory.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Services;

internal sealed class WarehouseManagerGuard : IWarehouseManagerGuard
{
    private const string StoreManagerRole = "StoreManager";
    private readonly InventoryDbContext _db;

    public WarehouseManagerGuard(InventoryDbContext db) => _db = db;

    public async Task<bool> WarehouseAlreadyHasManagerAsync(
        int warehouseId, string? excludeUserId, CancellationToken ct = default)
    {
        var managerRoleId = await _db.Roles
            .Where(r => r.Name == StoreManagerRole)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(managerRoleId)) return false;

        var query = from u in _db.Users
                    join ur in _db.UserRoles on u.Id equals ur.UserId
                    where u.WarehouseId == warehouseId && ur.RoleId == managerRoleId
                    select u.Id;

        if (!string.IsNullOrEmpty(excludeUserId))
            query = query.Where(id => id != excludeUserId);

        return await query.AnyAsync(ct);
    }
}