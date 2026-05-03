using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Products;

public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public UpdateProductHandler(InventoryDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product is null)
            return ActionResponse.Failure("Product not found.");

        var actor = await _currentUser.GetAsync(cancellationToken);

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.Description = request.Description;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedDate = DateTime.UtcNow;
        product.UpdatedBy = actor.UserName;

        await _context.SaveChangesAsync(cancellationToken);
        return ActionResponse.Successful("Product updated successfully.");
    }
}