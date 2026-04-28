using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Inventory.ServiceLogic.Handlers.Users;

public class UpdateStoreManagerHandler : IRequestHandler<UpdateStoreManagerRequest, ActionResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateStoreManagerHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ActionResponse> Handle(UpdateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new ActionResponse { Success = false, Message = "Store manager not found." };
        }

        // Only allow editing StoreManager accounts to avoid Admins being silently re-assigned
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("StoreManager"))
        {
            return new ActionResponse { Success = false, Message = "Only Store Manager accounts can be edited here." };
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
