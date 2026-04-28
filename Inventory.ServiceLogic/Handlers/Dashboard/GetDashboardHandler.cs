using AutoMapper;
using Inventory.Contracts.Requests.Dashboard;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums; // Ensure this is imported for TransferStatus
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Dashboard;

public class GetDashboardHandler : IRequestHandler<GetDashboardRequest, DashboardMetricsResponse>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;

    // Removed the underscore from the parameter name to fix ambiguity
    public GetDashboardHandler(InventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardMetricsResponse> Handle(GetDashboardRequest request, CancellationToken cancellationToken)
    {
        var response = new DashboardMetricsResponse
        {
            TotalProducts = await _context.Products.CountAsync(cancellationToken),
            TotalStockQuantity = await _context.Stocks.AnyAsync()
                ? await _context.Stocks.SumAsync(s => s.Quantity, cancellationToken)
                : 0,
            LowStockAlertsCount = await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Quantity <= s.Product.LowStockThreshold)
                .CountAsync(cancellationToken),

            // FIXED: Comparing TransferStatus enum to its correct enum value
            PendingTransfersCount = await _context.TransferRequests
                .Where(tr => tr.Status == TransferStatus.Pending)
                .CountAsync(cancellationToken)
        };

        var recentMovements = await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .OrderByDescending(sm => sm.CreatedDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        response.RecentMovements = _mapper.Map<List<MovementHistoryResponse>>(recentMovements);

        return response;
    }
}