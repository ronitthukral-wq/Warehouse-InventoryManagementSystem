using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Products;

public class CreateProductRequest : IRequest<ActionResponse>
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LowStockThreshold { get; set; }
}