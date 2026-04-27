using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Products;

public class DeleteProductRequest : IRequest<ActionResponse>
{
    public int Id { get; set; }
}