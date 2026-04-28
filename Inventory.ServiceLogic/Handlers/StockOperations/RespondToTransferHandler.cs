using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class RespondToTransferHandler : IRequestHandler<RespondToTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RespondToTransferHandler(InventoryDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(RespondToTransferRequest request, CancellationToken cancellationToken)
    {
        var transfer = await _db.TransferRequests
            .FirstOrDefaultAsync(t => t.Id == request.TransferRequestId, cancellationToken);

        if (transfer is null || transfer.Status != TransferStatus.Pending)
        {
            return new ActionResponse { Success = false, Message = "Transfer request not found or already processed." };
        }

        // Authorisation: only the destination warehouse's Store Manager (or an Admin)
        // can accept/reject the transfer. The source SM cannot approve their own request.
        if (_currentUser.IsStoreManager)
        {
            var myWarehouse = await _currentUser.GetWarehouseIdAsync(cancellationToken);
            if (myWarehouse != transfer.ToWarehouseId)
            {
                return new ActionResponse { Success = false, Message = "You can only act on transfers sent to your warehouse." };
            }
        }
        else if (!_currentUser.IsAdmin)
        {
            return new ActionResponse { Success = false, Message = "Not authorised to respond to transfers." };
        }

        // ---------- REJECT ----------
        if (!request.Accept)
        {
            transfer.Status = TransferStatus.Rejected;
            transfer.UpdatedDate = DateTime.UtcNow;
            transfer.UpdatedBy = _currentUser.Email ?? "StoreManager";
            await _db.SaveChangesAsync(cancellationToken);
            return new ActionResponse { Success = true, Message = "Transfer request rejected." };
        }

        // ---------- ACCEPT ----------
        // Verify source still has the stock (could have changed since the request was made).
        var source = await _db.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == transfer.ProductId && s.WarehouseId == transfer.FromWarehouseId,
            cancellationToken);

        if (source is null || source.Quantity < transfer.Quantity)
        {
            return new ActionResponse
            {
                Success = false,
                Message = $"Source warehouse no longer has sufficient stock (available: {source?.Quantity ?? 0})."
            };
        }

        // Deduct from source.
        source.Quantity -= transfer.Quantity;

        // Add to destination (composite key, so seed if absent).
        var destination = await _db.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == transfer.ProductId && s.WarehouseId == transfer.ToWarehouseId,
            cancellationToken);

        if (destination is null)
        {
            _db.Stocks.Add(new Stock
            {
                ProductId = transfer.ProductId,
                WarehouseId = transfer.ToWarehouseId,
                Quantity = transfer.Quantity
            });
        }
        else
        {
            destination.Quantity += transfer.Quantity;
        }

        // Audit log: a TransferOut from source and a TransferIn to destination.
        var now = DateTime.UtcNow;
        var actor = _currentUser.Email ?? "StoreManager";

        _db.StockMovements.Add(new StockMovement
        {
            ProductId = transfer.ProductId,
            WarehouseId = transfer.FromWarehouseId,
            Quantity = transfer.Quantity,
            Type = MovementType.TransferOut,
            CreatedDate = now,
            CreatedBy = actor
        });

        _db.StockMovements.Add(new StockMovement
        {
            ProductId = transfer.ProductId,
            WarehouseId = transfer.ToWarehouseId,
            Quantity = transfer.Quantity,
            Type = MovementType.TransferIn,
            CreatedDate = now,
            CreatedBy = actor
        });

        transfer.Status = TransferStatus.Accepted;
        transfer.UpdatedDate = now;
        transfer.UpdatedBy = actor;

        await _db.SaveChangesAsync(cancellationToken);
        return new ActionResponse { Success = true, Message = "Transfer accepted and stock moved." };
    }
}
