using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class CreateProductHandler : IRequestHandler<CreateProductRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateProductHandler(InventoryDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // SKU uniqueness guard (the DB has a unique index, but a friendly message is nicer)
            var skuTaken = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU, cancellationToken);
            if (skuTaken)
            {
                return ActionResponse.Failure($"A product with SKU '{request.SKU}' already exists.");
            }

            var actor = await _currentUser.GetAsync(cancellationToken);

            var product = new Product
            {
                Name = request.Name,
                SKU = request.SKU,
                Description = request.Description,
                LowStockThreshold = request.LowStockThreshold,
                CreatedBy = actor.UserName
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            // Initialise the product to qty 0 in EVERY warehouse so each Store Manager
            // immediately sees the new SKU and can stock it.
            var warehouseIds = await _context.Warehouses
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            foreach (var warehouseId in warehouseIds)
            {
                _context.Stocks.Add(new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = warehouseId,
                    Quantity = 0
                });
            }

            if (warehouseIds.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ActionResponse.Successful($"Product '{product.Name}' added to the catalog.");
        }
        catch (Exception ex)
        {
            return ActionResponse.Failure($"Error: {ex.Message}");
        }
    }
}
