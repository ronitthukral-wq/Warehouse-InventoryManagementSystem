using AutoMapper;
using Inventory.Contracts.Requests.Dashboard;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Dashboard;

public class GetDashboardHandler : IRequestHandler<GetDashboardRequest, DashboardMetricsResponse>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardHandler(
        InventoryDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<DashboardMetricsResponse> Handle(GetDashboardRequest request, CancellationToken cancellationToken)
    {
        // Resolve scope: a Store Manager only sees their own warehouse.
        int? scopedWarehouseId = null;
        if (_currentUser.IsStoreManager)
        {
            scopedWarehouseId = await _currentUser.GetWarehouseIdAsync(cancellationToken);
        }

        var stocksQuery = _context.Stocks.Include(s => s.Product).AsQueryable();
        var movementsQuery = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsQueryable();
        var pendingQuery = _context.TransferRequests
            .Where(tr => tr.Status == TransferStatus.Pending);

        if (scopedWarehouseId.HasValue)
        {
            stocksQuery = stocksQuery.Where(s => s.WarehouseId == scopedWarehouseId.Value);
            movementsQuery = movementsQuery.Where(sm => sm.WarehouseId == scopedWarehouseId.Value);
            // SM only cares about transfers coming INTO their warehouse.
            pendingQuery = pendingQuery.Where(tr => tr.ToWarehouseId == scopedWarehouseId.Value);
        }

        var response = new DashboardMetricsResponse
        {
            TotalProducts = await _context.Products.CountAsync(cancellationToken),
            TotalStockQuantity = await stocksQuery.AnyAsync(cancellationToken)
                ? await stocksQuery.SumAsync(s => s.Quantity, cancellationToken)
                : 0,
            LowStockAlertsCount = await stocksQuery
                .Where(s => s.Quantity <= s.Product.LowStockThreshold)
                .CountAsync(cancellationToken),
            PendingTransfersCount = await pendingQuery.CountAsync(cancellationToken)
        };

        var recentMovements = await movementsQuery
            .OrderByDescending(sm => sm.CreatedDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        response.RecentMovements = _mapper.Map<List<MovementHistoryResponse>>(recentMovements);

        return response;
    }
}
