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

    public GetMovementHistoryHandler(InventoryDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<MovementHistoryResponse>> Handle(GetMovementHistoryRequest request, CancellationToken cancellationToken)
    {
        var query = _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .AsNoTracking();

        // If a WarehouseId is provided (e.g., for a Store Manager), filter the results
        if (request.WarehouseId.HasValue)
        {
            query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
        }

        var movements = await query
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MovementHistoryResponse>>(movements);
    }
}