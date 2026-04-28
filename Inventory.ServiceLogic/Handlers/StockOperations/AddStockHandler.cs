using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class AddStockHandler : IRequestHandler<AddStockRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddStockHandler(InventoryDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(AddStockRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ActionResponse { Success = false, Message = "Quantity must be greater than zero." };
        }

        // 1. Resolve the Store Manager's own warehouse.
        var warehouseId = await _currentUser.GetWarehouseIdAsync(cancellationToken);
        if (warehouseId is null)
        {
            return new ActionResponse
            {
                Success = false,
                Message = "You are not assigned to a warehouse, so stock cannot be added."
            };
        }

        // 2. Verify the product exists in the catalog.
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
        {
            return new ActionResponse { Success = false, Message = "Selected product does not exist." };
        }

        // 3. Update or create the Stock row for (product, warehouse).
        var stock = await _context.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == request.ProductId && s.WarehouseId == warehouseId.Value,
            cancellationToken);

        if (stock is null)
        {
            stock = new Stock
            {
                ProductId = request.ProductId,
                WarehouseId = warehouseId.Value,
                Quantity = request.Quantity
            };
            _context.Stocks.Add(stock);
        }
        else
        {
            stock.Quantity += request.Quantity;
        }

        // 4. Audit log.
        _context.StockMovements.Add(new StockMovement
        {
            ProductId = request.ProductId,
            WarehouseId = warehouseId.Value,
            Quantity = request.Quantity,
            Type = MovementType.Purchase,
            Note = request.Note,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "StoreManager"
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse
        {
            Success = true,
            Message = $"Added {request.Quantity} unit(s) to your warehouse."
        };
    }
}
