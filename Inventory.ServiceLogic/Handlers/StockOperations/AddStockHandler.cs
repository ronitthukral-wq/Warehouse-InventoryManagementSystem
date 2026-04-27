using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class AddStockHandler : IRequestHandler<AddStockRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public AddStockHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(AddStockRequest request, CancellationToken cancellationToken)
    {
        // 1. Logic: Identify the Store Manager's warehouse (simplified for this context)
        // In a real scenario, you'd fetch the WarehouseId from the logged-in user's claims
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(cancellationToken);

        if (warehouse == null) return new ActionResponse { Success = false, Message = "No warehouse found." };

        // 2. Update or Create Stock record
        var stock = await _context.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == request.ProductId && s.WarehouseId == warehouse.Id, cancellationToken);

        if (stock == null)
        {
            stock = new Stock { ProductId = request.ProductId, WarehouseId = warehouse.Id, Quantity = request.Quantity };
            _context.Stocks.Add(stock);
        }
        else
        {
            stock.Quantity += request.Quantity;
        }

        // 3. Log the Movement
        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            WarehouseId = warehouse.Id,
            Quantity = request.Quantity,
            Type = MovementType.Purchase,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "StoreManager"
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = $"Added {request.Quantity} units to stock." };
    }
}