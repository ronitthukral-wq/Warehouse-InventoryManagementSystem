using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetStockByWarehouseHandler : IRequestHandler<GetStockByWarehouseRequest, List<StockLevelResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;

    public GetStockByWarehouseHandler(InventoryDbContext db, IMapper mapper, ICurrentUserContext currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<StockLevelResponse>> Handle(GetStockByWarehouseRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);

        // Store managers can ONLY ever see their own warehouse, regardless of what they request.
        var effectiveWarehouseId = actor.IsStoreManager && actor.WarehouseId is int wid
            ? wid
            : request.WarehouseId;

        if (effectiveWarehouseId <= 0)
        {
            return new List<StockLevelResponse>();
        }

        var stockLevels = await _db.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == effectiveWarehouseId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<StockLevelResponse>>(stockLevels);
    }
}
