using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

/// <summary>
/// Returns the stock levels for the warehouse managed by the currently
/// signed-in Store Manager. The handler resolves the warehouse from
/// ICurrentUserService, so callers don't need to pass it in.
/// </summary>
public class GetMyStockRequest : IRequest<List<StockLevelResponse>>
{
}
