using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsRequest, List<ProductResponse>>
{
    private readonly InventoryDbContext _context;

    public GetAllProductsHandler(InventoryDbContext context) => _context = context;

    public async Task<List<ProductResponse>> Handle(
        GetAllProductsRequest request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Description = p.Description ?? string.Empty,
                LowStockThreshold = p.LowStockThreshold,
                TotalAvailableQuantity = p.Stocks.Sum(s => s.Quantity)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}