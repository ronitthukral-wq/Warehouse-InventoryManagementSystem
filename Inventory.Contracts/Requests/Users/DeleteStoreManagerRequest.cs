using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Users;

public class DeleteStoreManagerRequest : IRequest<ActionResponse>
{
    public string Id { get; set; } = string.Empty;
}
