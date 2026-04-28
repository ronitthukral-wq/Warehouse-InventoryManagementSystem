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

    public CreateProductHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Reject duplicate SKUs early - SKU is the business identifier.
            var skuTaken = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU, cancellationToken);
            if (skuTaken)
            {
                return ActionResponse.Failure($"A product with SKU '{request.SKU}' already exists.");
            }

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

            // Per business rule: every warehouse starts with 0 stock for a new product.
            // Store Managers will then top up their own warehouse via AddStock.
            var warehouseIds = await _context.Warehouses
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            if (warehouseIds.Count > 0)
            {
                var seedStocks = warehouseIds.Select(wid => new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = wid,
                    Quantity = 0
                });

                _context.Stocks.AddRange(seedStocks);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ActionResponse.Successful($"Product {product.Name} added successfully.");
        }
        catch (Exception ex)
        {
            return ActionResponse.Failure($"Error: {ex.Message}");
        }
    }
}
