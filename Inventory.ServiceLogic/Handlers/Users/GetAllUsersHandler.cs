using AutoMapper;
using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
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
        // Explicitly include Warehouse to get the AssignedWarehouseName
        var users = await _db.Users
            .Include(u => u.Warehouse)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<UserResponse>>(users);
    }
}