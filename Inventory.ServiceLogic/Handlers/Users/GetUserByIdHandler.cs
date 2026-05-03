using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, UserResponse?>
{
    private readonly InventoryDbContext _db;

    public GetUserByIdHandler(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task<UserResponse?> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var result = await (
            from u in _db.Users.Include(u => u.Warehouse)
            where u.Id == request.Id
            join ur in _db.UserRoles on u.Id equals ur.UserId into userRoles
            from ur in userRoles.DefaultIfEmpty()
            join r in _db.Roles on ur.RoleId equals r.Id into roles
            from r in roles.DefaultIfEmpty()
            select new UserResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                WarehouseId = u.WarehouseId,
                AssignedWarehouseName = u.Warehouse != null ? u.Warehouse.Name : null,
                Role = r != null ? r.Name ?? string.Empty : string.Empty
            }
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}