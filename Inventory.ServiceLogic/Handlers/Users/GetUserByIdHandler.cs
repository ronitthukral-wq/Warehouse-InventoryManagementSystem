using AutoMapper;
using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Users;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, UserResponse?>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdHandler(
        InventoryDbContext db,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<UserResponse?> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Warehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null) return null;

        var dto = _mapper.Map<UserResponse>(user);
        var roles = await _userManager.GetRolesAsync(user);
        dto.Role = roles.FirstOrDefault() ?? string.Empty;

        return dto;
    }
}
