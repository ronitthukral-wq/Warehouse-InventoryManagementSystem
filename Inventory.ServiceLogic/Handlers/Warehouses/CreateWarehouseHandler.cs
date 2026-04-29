using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class CreateWarehouseHandler : IRequestHandler<CreateWarehouseRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public CreateWarehouseHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse> Handle(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var warehouse = new Warehouse
            {
                Name = request.Name,
                Location = request.Location
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync(cancellationToken);

            // New warehouse starts with every existing product at qty 0 so the
            // assigned Store Manager can immediately stock anything in the catalog.
            var productIds = await _context.Products
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            foreach (var productId in productIds)
            {
                _context.Stocks.Add(new Stock
                {
                    ProductId = productId,
                    WarehouseId = warehouse.Id,
                    Quantity = 0
                });
            }

            if (productIds.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ActionResponse.Successful("Warehouse created successfully.");
        }
        catch (Exception ex)
        {
            return ActionResponse.Failure($"Failed to create warehouse: {ex.Message}");
        }
    }
}
