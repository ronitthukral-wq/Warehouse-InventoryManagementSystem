using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Warehouses;

public class UpdateWarehouseRequest : IRequest<ActionResponse>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}