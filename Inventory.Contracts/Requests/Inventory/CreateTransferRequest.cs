using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Inventory;

public class CreateTransferRequest : IRequest<ActionResponse>
{
    public int ProductId { get; set; }

    /// <summary>
    /// The warehouse the requesting Store Manager wants to PULL stock FROM.
    /// The destination is always the requester's own warehouse, so it isn't
    /// part of the contract — it's resolved server-side from the current user.
    /// </summary>
    public int FromWarehouseId { get; set; }

    public int Quantity { get; set; }
}
