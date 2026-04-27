using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Users;

public class CreateStoreManagerRequest : IRequest<ActionResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int WarehouseId { get; set; } // Required to link SM to a specific location
}