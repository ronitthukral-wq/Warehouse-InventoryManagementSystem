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

    public GetStockByWarehouseHandler(InventoryDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<StockLevelResponse>> Handle(GetStockByWarehouseRequest request, CancellationToken cancellationToken)
    {
        var stockLevels = await _db.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == request.WarehouseId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<StockLevelResponse>>(stockLevels);
    }
}