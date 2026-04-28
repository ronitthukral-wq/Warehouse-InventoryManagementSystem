using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Inventory.ServiceLogic.Handlers.Users;

public class CreateStoreManagerHandler : IRequestHandler<CreateStoreManagerRequest, ActionResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateStoreManagerHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ActionResponse> Handle(CreateStoreManagerRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            WarehouseId = request.WarehouseId
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "StoreManager");
            return new ActionResponse { Success = true, Message = "Store Manager created and assigned to warehouse." };
        }

        return new ActionResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
    }
}