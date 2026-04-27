using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

public class CreateTransferRequest : IRequest<ActionResponse>
{
    public int ProductId { get; set; }
    public int ToWarehouseId { get; set; }
    public int Quantity { get; set; }
}