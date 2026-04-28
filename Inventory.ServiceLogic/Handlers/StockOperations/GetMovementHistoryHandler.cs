using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetMovementHistoryHandler : IRequestHandler<GetMovementHistoryRequest, List<MovementHistoryResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetMovementHistoryHandler(
        InventoryDbContext db,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<MovementHistoryResponse>> Handle(
        GetMovementHistoryRequest request,
        CancellationToken cancellationToken)
    {
        // A Store Manager can only see their own warehouse's history. Any
        // explicit filter on the request is ignored for them so they cannot
        // browse another warehouse's audit log.
        int? warehouseFilter = request.WarehouseId;
        if (_currentUser.IsStoreManager)
        {
            warehouseFilter = await _currentUser.GetWarehouseIdAsync(cancellationToken);
            if (warehouseFilter is null)
            {
                return new List<MovementHistoryResponse>();
            }
        }

        var query = _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .AsNoTracking();

        if (warehouseFilter.HasValue)
        {
            query = query.Where(m => m.WarehouseId == warehouseFilter.Value);
        }

        var movements = await query
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MovementHistoryResponse>>(movements);
    }
}
