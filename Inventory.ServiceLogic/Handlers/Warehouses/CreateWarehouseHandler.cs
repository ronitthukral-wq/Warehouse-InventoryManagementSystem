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

            // Seed 0-quantity Stock rows for every existing product so the
            // new warehouse immediately has the full catalog at zero stock.
            var productIds = await _context.Products
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (productIds.Count > 0)
            {
                var seedStocks = productIds.Select(pid => new Stock
                {
                    ProductId = pid,
                    WarehouseId = warehouse.Id,
                    Quantity = 0
                });

                _context.Stocks.AddRange(seedStocks);
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
