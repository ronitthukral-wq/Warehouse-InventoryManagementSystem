using Inventory.Contracts.Responses;
using MediatR;
using System.Collections.Generic;

namespace Inventory.Contracts.Requests.Inventory;

public class GetMovementHistoryRequest : IRequest<List<MovementHistoryResponse>>
{
    // Adding this property resolves error CS1061
    public int? WarehouseId { get; set; }
}