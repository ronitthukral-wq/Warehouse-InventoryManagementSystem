using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class DeleteStoreManagerHandler : IRequestHandler<DeleteStoreManagerRequest, ActionResponse>
{
    private readonly InventoryDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteStoreManagerHandler(InventoryDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ActionResponse> Handle(DeleteStoreManagerRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return ActionResponse.Failure("Store manager not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("StoreManager"))
        {
            return ActionResponse.Failure("Selected user is not a store manager.");
        }

        await _userManager.DeleteAsync(user);
        return ActionResponse.Successful("Store manager deleted successfully.");
    }
}
