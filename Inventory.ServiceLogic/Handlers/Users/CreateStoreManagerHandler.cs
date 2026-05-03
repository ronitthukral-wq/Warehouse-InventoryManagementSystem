using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.ServiceLogic.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class CreateStoreManagerHandler : IRequestHandler<CreateStoreManagerRequest, ActionResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;
    private readonly IWarehouseManagerGuard _guard;

    public CreateStoreManagerHandler(
        UserManager<ApplicationUser> userManager,
        InventoryDbContext db,
        IWarehouseManagerGuard guard)
    {
        _userManager = userManager;
        _db = db;
        _guard = guard;
    }

    public async Task<ActionResponse> Handle(CreateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        if (request.WarehouseId <= 0)
            return ActionResponse.Failure("A warehouse must be selected for the Store Manager.");

        var warehouseExists = await _db.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (!warehouseExists)
            return ActionResponse.Failure("Selected warehouse does not exist.");

        if (await _guard.WarehouseAlreadyHasManagerAsync(request.WarehouseId, excludeUserId: null, cancellationToken))
            return ActionResponse.Failure("This warehouse already has a Store Manager assigned.");

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
            return ActionResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, "StoreManager");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ActionResponse.Failure(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        return ActionResponse.Successful("Store Manager created and assigned to the warehouse.");
    }
}