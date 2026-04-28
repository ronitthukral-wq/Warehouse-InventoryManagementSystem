using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetMyStockHandler : IRequestHandler<GetMyStockRequest, List<StockLevelResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetMyStockHandler(
        InventoryDbContext db,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<StockLevelResponse>> Handle(
        GetMyStockRequest request,
        CancellationToken cancellationToken)
    {
        var warehouseId = await _currentUser.GetWarehouseIdAsync(cancellationToken);
        if (warehouseId is null)
        {
            return new List<StockLevelResponse>();
        }

        var stocks = await _db.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == warehouseId.Value)
            .OrderBy(s => s.Product.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<StockLevelResponse>>(stocks);
    }
}
