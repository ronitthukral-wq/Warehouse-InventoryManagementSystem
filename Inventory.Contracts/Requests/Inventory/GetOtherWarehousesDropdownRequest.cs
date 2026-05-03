using MediatR;
using Inventory.Contracts.Responses;

namespace Inventory.Contracts.Requests.Inventory;

public class GetOtherWarehousesDropdownRequest : IRequest<List<DropdownItemResponse>>
{
    public int ExcludeWarehouseId { get; set; }
}