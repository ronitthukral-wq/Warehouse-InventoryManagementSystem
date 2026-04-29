using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class CreateTransferHandler : IRequestHandler<CreateTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateTransferHandler(InventoryDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(CreateTransferRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return ActionResponse.Failure("Quantity must be greater than zero.");
        }

        var actor = await _currentUser.GetAsync(cancellationToken);
        if (!actor.IsStoreManager || actor.WarehouseId is null)
        {
            return ActionResponse.Failure("Only Store Managers assigned to a warehouse can create transfers.");
        }
        var fromWarehouseId = actor.WarehouseId.Value;

        if (request.ToWarehouseId == fromWarehouseId)
        {
            return ActionResponse.Failure("Source and destination warehouse cannot be the same.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return ActionResponse.Failure("Selected product does not exist.");
        }

        var destinationExists = await _context.Warehouses
            .AnyAsync(w => w.Id == request.ToWarehouseId, cancellationToken);
        if (!destinationExists)
        {
            return ActionResponse.Failure("Destination warehouse does not exist.");
        }

        var sourceStock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == fromWarehouseId, cancellationToken);

        var available = sourceStock?.Quantity ?? 0;
        if (available < request.Quantity)
        {
            return ActionResponse.Failure(
                $"Insufficient stock. You currently have {available} units of '{product.Name}'.");
        }

        var transfer = new TransferRequest
        {
            ProductId = request.ProductId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            Status = TransferStatus.Pending,
            CreatedBy = actor.UserName
        };

        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful("Transfer request sent and is pending approval.");
    }
}
