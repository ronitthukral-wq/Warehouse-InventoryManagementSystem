using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Warehouses;

public class GetWarehouseByIdRequest : IRequest<WarehouseResponse>
{
    public int Id { get; set; }
}