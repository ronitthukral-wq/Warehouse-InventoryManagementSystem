using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Warehouses;

public class DeleteWarehouseRequest : IRequest<ActionResponse>
{
    public int Id { get; set; }
}