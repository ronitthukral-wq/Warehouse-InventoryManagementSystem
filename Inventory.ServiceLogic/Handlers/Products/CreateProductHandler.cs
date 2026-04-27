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

        return new ActionResponse { Success = true, Message = "Product created successfully." };
    }
}