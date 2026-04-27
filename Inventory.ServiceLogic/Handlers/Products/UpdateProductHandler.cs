using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public UpdateProductHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product == null)
            return new ActionResponse { Success = false, Message = "Product not found." };

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.Description = request.Description;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedDate = DateTime.UtcNow;
        product.UpdatedBy = "System"; // Ideally from IHttpContextAccessor

        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = "Product updated successfully." };
    }
}