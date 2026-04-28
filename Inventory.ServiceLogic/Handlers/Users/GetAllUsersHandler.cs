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
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersHandler(
        InventoryDbContext db,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<List<UserResponse>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        // Explicitly include Warehouse to get the AssignedWarehouseName
        var users = await _db.Users
            .Include(u => u.Warehouse)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var responses = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            var dto = _mapper.Map<UserResponse>(user);

            // Identity stores roles in AspNetUserRoles. Resolve the primary role here
            // so the AutoMapper profile remains free of side-effects.
            var roles = await _userManager.GetRolesAsync(user);
            dto.Role = roles.FirstOrDefault() ?? string.Empty;

            responses.Add(dto);
        }

        return responses;
    }
}
