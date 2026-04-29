using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetMovementHistoryHandler : IRequestHandler<GetMovementHistoryRequest, List<MovementHistoryResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;

    public GetMovementHistoryHandler(InventoryDbContext db, IMapper mapper, ICurrentUserContext currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<MovementHistoryResponse>> Handle(GetMovementHistoryRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);

        var query = _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .AsNoTracking();

        // Store managers are silently scoped to their own warehouse, regardless of what they request.
        // Admins may pass a WarehouseId filter or leave it null to see everything.
        if (actor.IsStoreManager && actor.WarehouseId is int wid)
        {
            query = query.Where(m => m.WarehouseId == wid);
        }
        else if (request.WarehouseId.HasValue)
        {
            query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
        }

        var movements = await query
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MovementHistoryResponse>>(movements);
    }
}
