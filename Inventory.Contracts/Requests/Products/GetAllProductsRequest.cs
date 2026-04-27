using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Products;

public class GetAllProductsRequest : IRequest<List<ProductResponse>>
{
}