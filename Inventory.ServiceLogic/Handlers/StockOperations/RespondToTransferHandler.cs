using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class RespondToTransferHandler : IRequestHandler<RespondToTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public RespondToTransferHandler(InventoryDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(RespondToTransferRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (!actor.IsStoreManager || actor.WarehouseId is null)
        {
            return ActionResponse.Failure("Only Store Managers can respond to transfers.");
        }

        var transfer = await _db.TransferRequests
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.Id == request.TransferRequestId, cancellationToken);

        if (transfer is null)
        {
            return ActionResponse.Failure("Transfer request not found.");
        }

        if (transfer.Status != TransferStatus.Pending)
        {
            return ActionResponse.Failure("Transfer request has already been processed.");
        }

        // Only the SOURCE warehouse's manager can approve / reject — they're the
        // one being asked to give up their stock. The requester is the destination.
        if (transfer.FromWarehouseId != actor.WarehouseId.Value)
        {
            return ActionResponse.Failure("You can only respond to transfer requests pulling stock FROM your warehouse.");
        }

        if (!request.Accept)
        {
            transfer.Status = TransferStatus.Rejected;
            transfer.UpdatedBy = actor.UserName;
            transfer.UpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ActionResponse.Successful("Transfer request rejected.");
        }

        // ACCEPT: validate source stock at acceptance time, then move it.
        var sourceStock = await _db.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.FromWarehouseId, cancellationToken);

        if (sourceStock is null || sourceStock.Quantity < transfer.Quantity)
        {
            // Auto-reject if source can no longer fulfil it.
            transfer.Status = TransferStatus.Rejected;
            transfer.UpdatedBy = actor.UserName;
            transfer.UpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ActionResponse.Failure(
                "Source warehouse no longer has enough stock to fulfil this transfer. Request has been rejected.");
        }

        // 1. Deduct from source
        sourceStock.Quantity -= transfer.Quantity;

        // 2. Add to destination
        var destinationStock = await _db.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.ToWarehouseId, cancellationToken);

        if (destinationStock is null)
        {
            destinationStock = new Stock
            {
                ProductId = transfer.ProductId,
                WarehouseId = transfer.ToWarehouseId,
                Quantity = transfer.Quantity
            };
            _db.Stocks.Add(destinationStock);
        }
        else
        {
            destinationStock.Quantity += transfer.Quantity;
        }

        // 3. Audit trail: TransferOut at source, TransferIn at destination
        var transferRef = $"Transfer #{transfer.Id}";
        _db.StockMovements.Add(new StockMovement
        {
            ProductId = transfer.ProductId,
            WarehouseId = transfer.FromWarehouseId,
            Quantity = transfer.Quantity,
            Type = MovementType.TransferOut,
            Note = $"{transferRef} -> Warehouse {transfer.ToWarehouseId}",
            CreatedBy = actor.UserName
        });
        _db.StockMovements.Add(new StockMovement
        {
            ProductId = transfer.ProductId,
            WarehouseId = transfer.ToWarehouseId,
            Quantity = transfer.Quantity,
            Type = MovementType.TransferIn,
            Note = $"{transferRef} <- Warehouse {transfer.FromWarehouseId}",
            CreatedBy = actor.UserName
        });

        transfer.Status = TransferStatus.Accepted;
        transfer.UpdatedBy = actor.UserName;
        transfer.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return ActionResponse.Successful(
            $"Transfer of {transfer.Quantity} units of '{transfer.Product.Name}' completed.");
    }
}
