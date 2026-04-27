using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

public class GetStockByWarehouseRequest : IRequest<List<StockLevelResponse>>
{
    public int WarehouseId { get; set; }
}