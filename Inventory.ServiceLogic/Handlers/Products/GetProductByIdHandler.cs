using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdRequest, ProductResponse?>
{
    private readonly InventoryDbContext _context;

    public GetProductByIdHandler(InventoryDbContext context) => _context = context;

    public async Task<ProductResponse?> Handle(
        GetProductByIdRequest request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => p.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}