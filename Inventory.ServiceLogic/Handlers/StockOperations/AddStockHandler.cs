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
    private readonly ICurrentUserContext _currentUser;

    public AddStockHandler(InventoryDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(AddStockRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return ActionResponse.Failure("Quantity must be greater than zero.");
        }

        var actor = await _currentUser.GetAsync(cancellationToken);
        if (!actor.IsStoreManager || actor.WarehouseId is null)
        {
            return ActionResponse.Failure("Only Store Managers assigned to a warehouse can add stock.");
        }
        var warehouseId = actor.WarehouseId.Value;

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return ActionResponse.Failure("Selected product does not exist.");
        }

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == warehouseId, cancellationToken);

        if (stock is null)
        {
            stock = new Stock
            {
                ProductId = request.ProductId,
                WarehouseId = warehouseId,
                Quantity = request.Quantity
            };
            _context.Stocks.Add(stock);
        }
        else
        {
            stock.Quantity += request.Quantity;
        }

        _context.StockMovements.Add(new StockMovement
        {
            ProductId = request.ProductId,
            WarehouseId = warehouseId,
            Quantity = request.Quantity,
            Type = MovementType.Purchase,
            Note = string.IsNullOrWhiteSpace(request.Note) ? "Stock added by manager" : request.Note,
            CreatedBy = actor.UserName
        });

        await _context.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful($"Added {request.Quantity} units of '{product.Name}' to your warehouse.");
    }
}
