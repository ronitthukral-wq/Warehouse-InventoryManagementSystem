using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class UpdateStoreManagerHandler : IRequestHandler<UpdateStoreManagerRequest, ActionResponse>
{
    private const string StoreManagerRole = "StoreManager";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;

    public UpdateStoreManagerHandler(UserManager<ApplicationUser> userManager, InventoryDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<ActionResponse> Handle(UpdateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return ActionResponse.Failure("Store manager not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(StoreManagerRole))
        {
            return ActionResponse.Failure("Only Store Manager accounts can be edited here.");
        }

        // If a warehouse is being assigned, ensure it isn't already taken by someone else.
        if (request.WarehouseId.HasValue && request.WarehouseId.Value > 0)
        {
            var warehouseExists = await _db.Warehouses
                .AnyAsync(w => w.Id == request.WarehouseId.Value, cancellationToken);
            if (!warehouseExists)
            {
                return ActionResponse.Failure("Selected warehouse does not exist.");
            }

            if (await WarehouseAlreadyHasManagerAsync(request.WarehouseId.Value, excludeUserId: user.Id))
            {
                return ActionResponse.Failure("This warehouse already has a Store Manager assigned.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        user.WarehouseId = request.WarehouseId;
        user.UpdatedDate = DateTime.UtcNow;
        user.UpdatedBy = "Admin";

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ActionResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return ActionResponse.Successful("Store Manager updated successfully.");
    }

    private async Task<bool> WarehouseAlreadyHasManagerAsync(int warehouseId, string? excludeUserId)
    {
        var managerRoleId = await _db.Roles
            .Where(r => r.Name == StoreManagerRole)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(managerRoleId)) return false;

        var query = from u in _db.Users
                    join ur in _db.UserRoles on u.Id equals ur.UserId
                    where u.WarehouseId == warehouseId && ur.RoleId == managerRoleId
                    select u.Id;

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(id => id != excludeUserId);
        }

        return await query.AnyAsync();
    }
}
