using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Inventory.ServiceLogic.Handlers.Users;

public class CreateStoreManagerHandler : IRequestHandler<CreateStoreManagerRequest, ActionResponse>
{
    private const string StoreManagerRole = "StoreManager";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;

    public CreateStoreManagerHandler(UserManager<ApplicationUser> userManager, InventoryDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<ActionResponse> Handle(CreateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        if (request.WarehouseId <= 0)
        {
            return ActionResponse.Failure("A warehouse must be selected for the Store Manager.");
        }

        // 1. Make sure the warehouse exists
        var warehouseExists = await _db.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (!warehouseExists)
        {
            return ActionResponse.Failure("Selected warehouse does not exist.");
        }

        // 2. Enforce: ONE Store Manager per warehouse
        if (await WarehouseAlreadyHasManagerAsync(request.WarehouseId, excludeUserId: null))
        {
            return ActionResponse.Failure("This warehouse already has a Store Manager assigned.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            WarehouseId = request.WarehouseId,
            CreatedBy = "Admin"
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ActionResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, StoreManagerRole);
        if (!roleResult.Succeeded)
        {
            // Rollback the created user so we don't leave it orphaned
            await _userManager.DeleteAsync(user);
            return ActionResponse.Failure(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        return ActionResponse.Successful("Store Manager created and assigned to the warehouse.");
    }

    private async Task<bool> WarehouseAlreadyHasManagerAsync(int warehouseId, string? excludeUserId)
    {
        // We need to check both the WarehouseId column AND that the user is in the StoreManager role.
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
