using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Users;

public class UpdateStoreManagerRequest : IRequest<ActionResponse>
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? WarehouseId { get; set; }
}
