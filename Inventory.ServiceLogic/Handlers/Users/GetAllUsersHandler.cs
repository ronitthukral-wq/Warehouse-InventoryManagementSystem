using AutoMapper;
using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, List<UserResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;

    public GetAllUsersHandler(InventoryDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<UserResponse>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        // Single query: join users → userRoles → roles in one shot — no N+1
        var usersWithRoles = await (
            from u in _db.Users.Include(u => u.Warehouse)
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
        ).AsNoTracking().ToListAsync(cancellationToken);

        return usersWithRoles;
    }
}