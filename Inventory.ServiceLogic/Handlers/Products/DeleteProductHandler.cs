using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.ServiceLogic.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class DeleteProductHandler : IRequestHandler<DeleteProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public DeleteProductHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.Id);
        if (product == null) return new ActionResponse { Success = false, Message = "Product not found." };

        // Logic check: ERD rule - cannot delete if stock exists
        var hasStock = await _context.Stocks.AnyAsync(s => s.ProductId == request.Id && s.Quantity > 0);
        if (hasStock) throw new ProductHasStockException("Cannot delete product with existing stock.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = "Product deleted." };
    }
}