using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Products;

public class GetProductByIdRequest : IRequest<ProductResponse?>   // ← nullable
{
    public int Id { get; set; }
}