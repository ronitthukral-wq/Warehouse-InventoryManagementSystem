using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Users;

public class GetAllUsersRequest : IRequest<List<UserResponse>>
{
}