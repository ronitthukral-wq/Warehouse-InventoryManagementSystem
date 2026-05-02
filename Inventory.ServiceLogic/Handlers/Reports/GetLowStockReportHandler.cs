using AutoMapper;
using Inventory.Contracts.Requests.Reports;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Reports;

public class GetLowStockReportHandler : IRequestHandler<GetLowStockReportRequest, List<StockLevelResponse>>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetLowStockReportHandler(InventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<StockLevelResponse>> Handle(GetLowStockReportRequest request, CancellationToken cancellationToken)
    {
        // Show every stock row where:
        //   • A threshold is configured (> 0), AND
        //   • Current quantity is at or below that threshold.
        //
        // Importantly we do NOT filter out Quantity == 0. A warehouse whose stock
        // was fully drained by an accepted transfer still has a Stock row (created
        // when it first received stock) — that row with Quantity = 0 is a valid
        // low-stock (actually out-of-stock) alert and MUST be surfaced.
        //
        // Products that were never stocked in a warehouse simply have no Stock row
        // in this table, so they can never appear here — no false-positives.
        var lowStockItems = await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product.LowStockThreshold > 0 && s.Quantity <= s.Product.LowStockThreshold)
            .OrderBy(s => s.Warehouse.Name)
            .ThenBy(s => s.Product.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<StockLevelResponse>>(lowStockItems);
    }
}
