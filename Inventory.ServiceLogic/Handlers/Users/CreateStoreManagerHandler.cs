using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class CreateStoreManagerHandler : IRequestHandler<CreateStoreManagerRequest, ActionResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;

    public CreateStoreManagerHandler(
        UserManager<ApplicationUser> userManager,
        InventoryDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<ActionResponse> Handle(CreateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate that the chosen warehouse exists.
        var warehouseExists = await _db.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (!warehouseExists)
        {
            return new ActionResponse { Success = false, Message = "Selected warehouse does not exist." };
        }

        // 2. Enforce business rule: only one Store Manager per warehouse.
        var existingManager = await _db.Users
            .FirstOrDefaultAsync(u => u.WarehouseId == request.WarehouseId, cancellationToken);
        if (existingManager is not null)
        {
            return new ActionResponse
            {
                Success = false,
                Message = $"This warehouse already has a Store Manager assigned ({existingManager.Email})."
            };
        }

        // 3. Create the Identity user.
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
            return new ActionResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        await _userManager.AddToRoleAsync(user, "StoreManager");
        return new ActionResponse { Success = true, Message = "Store Manager created and assigned to warehouse." };
    }
}
