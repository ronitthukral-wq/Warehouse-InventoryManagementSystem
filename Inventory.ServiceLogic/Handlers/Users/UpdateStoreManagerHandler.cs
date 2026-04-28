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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;

    public UpdateStoreManagerHandler(
        UserManager<ApplicationUser> userManager,
        InventoryDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<ActionResponse> Handle(UpdateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new ActionResponse { Success = false, Message = "Store manager not found." };
        }

        // Only allow editing StoreManager accounts to avoid Admins being silently re-assigned.
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("StoreManager"))
        {
            return new ActionResponse { Success = false, Message = "Only Store Manager accounts can be edited here." };
        }

        // If the warehouse is being changed, enforce one-SM-per-warehouse.
        if (request.WarehouseId.HasValue && request.WarehouseId.Value != user.WarehouseId)
        {
            var occupied = await _db.Users.AnyAsync(
                u => u.WarehouseId == request.WarehouseId.Value && u.Id != user.Id,
                cancellationToken);

            if (occupied)
            {
                return new ActionResponse
                {
                    Success = false,
                    Message = "Target warehouse already has a Store Manager assigned."
                };
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

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return new ActionResponse { Success = true, Message = "Store Manager updated successfully." };
        }

        return new ActionResponse
        {
            Success = false,
            Message = string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }
}
