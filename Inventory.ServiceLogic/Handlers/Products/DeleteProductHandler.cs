using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class DeleteProductHandler : IRequestHandler<DeleteProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public DeleteProductHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object?[] { request.Id }, cancellationToken);
        if (product == null)
        {
            return ActionResponse.Failure("Product not found.");
        }

        // ERD rule: cannot delete a product that still has stock anywhere.
        // Returning a friendly ActionResponse keeps the response shape consistent
        // with every other handler.
        var hasStock = await _context.Stocks
            .AnyAsync(s => s.ProductId == request.Id && s.Quantity > 0, cancellationToken);
        if (hasStock)
        {
            return ActionResponse.Failure(
                "This product still has stock in one or more warehouses. Please clear all stock before deleting.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful("Product deleted.");
    }
}
