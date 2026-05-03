using MediatR;
using Inventory.Contracts.Responses;

namespace Inventory.Contracts.Requests.Inventory;

public class GetProductDropdownRequest : IRequest<List<DropdownItemResponse>> { }