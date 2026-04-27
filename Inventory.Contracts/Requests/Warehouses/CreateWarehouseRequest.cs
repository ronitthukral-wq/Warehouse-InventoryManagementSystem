using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Warehouses;

public class CreateWarehouseRequest : IRequest<ActionResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}