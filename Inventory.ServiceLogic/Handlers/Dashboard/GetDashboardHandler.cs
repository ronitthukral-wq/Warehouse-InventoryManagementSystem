using AutoMapper;
using Inventory.Contracts.Requests.Dashboard;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Dashboard;

public class GetDashboardHandler : IRequestHandler<GetDashboardRequest, DashboardMetricsResponse>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;

    public GetDashboardHandler(InventoryDbContext context, IMapper mapper, ICurrentUserContext currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<DashboardMetricsResponse> Handle(GetDashboardRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);

        var response = new DashboardMetricsResponse
        {
            IsStoreManager = actor.IsStoreManager,
            WarehouseId = actor.IsStoreManager ? actor.WarehouseId : null,
        };

        // Branch the queries: Store Manager sees their warehouse only; everyone else sees totals.
        if (actor.IsStoreManager && actor.WarehouseId is int wid)
        {
            response.WarehouseName = await _context.Warehouses
                .Where(w => w.Id == wid)
                .Select(w => w.Name)
                .FirstOrDefaultAsync(cancellationToken);

            // "Total Products" for an SM = how many products they hold any stock of
            response.TotalProducts = await _context.Stocks
                .Where(s => s.WarehouseId == wid && s.Quantity > 0)
                .CountAsync(cancellationToken);

            response.TotalStockQuantity = await _context.Stocks
                .Where(s => s.WarehouseId == wid)
                .SumAsync(s => (int?)s.Quantity, cancellationToken) ?? 0;

            response.LowStockAlertsCount = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.WarehouseId == wid
                         && s.Product.LowStockThreshold > 0
                         && s.Quantity <= s.Product.LowStockThreshold)
                .CountAsync(cancellationToken);

            // Pending = anything still awaiting their action OR their outgoing requests still pending
            response.PendingTransfersCount = await _context.TransferRequests
                .Where(t => t.Status == TransferStatus.Pending &&
                            (t.ToWarehouseId == wid || t.FromWarehouseId == wid))
                .CountAsync(cancellationToken);

            var recentMovements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Where(m => m.WarehouseId == wid)
                .OrderByDescending(m => m.CreatedDate)
                .Take(5)
                .ToListAsync(cancellationToken);
            response.RecentMovements = _mapper.Map<List<MovementHistoryResponse>>(recentMovements);

            var inventory = await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Where(s => s.WarehouseId == wid)
                .OrderBy(s => s.Product.Name)
                .ToListAsync(cancellationToken);
            response.MyInventory = _mapper.Map<List<StockLevelResponse>>(inventory);
        }
        else
        {
            response.TotalProducts = await _context.Products.CountAsync(cancellationToken);

            response.TotalStockQuantity = await _context.Stocks
                .SumAsync(s => (int?)s.Quantity, cancellationToken) ?? 0;

            response.LowStockAlertsCount = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Product.LowStockThreshold > 0
                         && s.Quantity <= s.Product.LowStockThreshold)
                .CountAsync(cancellationToken);

            response.PendingTransfersCount = await _context.TransferRequests
                .Where(t => t.Status == TransferStatus.Pending)
                .CountAsync(cancellationToken);

            var recentMovements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .OrderByDescending(m => m.CreatedDate)
                .Take(5)
                .ToListAsync(cancellationToken);
            response.RecentMovements = _mapper.Map<List<MovementHistoryResponse>>(recentMovements);
        }

        return response;
    }
}
