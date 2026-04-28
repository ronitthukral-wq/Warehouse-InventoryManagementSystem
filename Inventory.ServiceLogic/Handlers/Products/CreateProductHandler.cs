using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Products;

public class CreateProductHandler : IRequestHandler<CreateProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public CreateProductHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Product
            {
                Name = request.Name,
                SKU = request.SKU,
                Description = request.Description,
                LowStockThreshold = request.LowStockThreshold,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Admin"
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            // Use the static helper method
            return ActionResponse.Successful($"Product {product.Name} added successfully.");
        }
        catch (Exception ex)
        {
            return ActionResponse.Failure($"Error: {ex.Message}");
        }
    }
}