using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Reports;

/// <summary>
/// Returns every stock row that is running low (Quantity > 0 and below the
/// product's alert threshold), across all warehouses. Admin-only — enforced
/// in the controller, not the handler, so the same query can be reused if
/// needed by other callers.
/// </summary>
public class GetLowStockReportRequest : IRequest<List<StockLevelResponse>>
{
}
