using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Users;

public class GetUserByIdRequest : IRequest<UserResponse?>
{
    public string Id { get; set; } = string.Empty;
}
