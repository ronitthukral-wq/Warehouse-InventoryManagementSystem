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
            return ActionResponse.Failure("Only Store Managers assigned to a warehouse can request transfers.");
        }

        // NEW SEMANTICS:
        //   - The requesting Store Manager is the DESTINATION (they want stock IN).
        //   - request.FromWarehouseId is the SOURCE (the warehouse being asked to give stock).
        //   - The Store Manager of the SOURCE warehouse is the one who must approve.
        var toWarehouseId = actor.WarehouseId.Value;
        var fromWarehouseId = request.FromWarehouseId;

        if (fromWarehouseId == toWarehouseId)
        {
            return ActionResponse.Failure("Source and destination warehouse cannot be the same.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return ActionResponse.Failure("Selected product does not exist.");
        }

        var sourceExists = await _context.Warehouses
            .AnyAsync(w => w.Id == fromWarehouseId, cancellationToken);
        if (!sourceExists)
        {
            return ActionResponse.Failure("Source warehouse does not exist.");
        }

        // Pre-flight check: tell the requester upfront if the source clearly cannot
        // fulfil this. The acceptance handler also re-validates at acceptance time.
        var sourceStock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == fromWarehouseId, cancellationToken);

        var available = sourceStock?.Quantity ?? 0;
        if (available < request.Quantity)
        {
            return ActionResponse.Failure(
                $"Source warehouse only has {available} units of '{product.Name}' available.");
        }

        var transfer = new TransferRequest
        {
            ProductId = request.ProductId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            Quantity = request.Quantity,
            Status = TransferStatus.Pending,
            CreatedBy = actor.UserName
        };

        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful("Transfer request sent and is pending approval by the source warehouse manager.");
    }
}
