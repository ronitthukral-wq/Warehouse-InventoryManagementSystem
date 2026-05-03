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
    public CreateWarehouseHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse { Name = request.Name, Location = request.Location };
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        var productIds = await _context.Products
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var pid in productIds)
            _context.Stocks.Add(new Stock { ProductId = pid, WarehouseId = warehouse.Id, Quantity = 0 });

        if (productIds.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful("Warehouse created successfully.");
    }
}