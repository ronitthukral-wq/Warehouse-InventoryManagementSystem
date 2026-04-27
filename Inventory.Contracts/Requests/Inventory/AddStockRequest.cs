using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

public class AddStockRequest : IRequest<ActionResponse>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}