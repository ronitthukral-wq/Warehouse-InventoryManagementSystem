using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class CreateTransferHandler : IRequestHandler<CreateTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTransferHandler(InventoryDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionResponse> Handle(CreateTransferRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ActionResponse { Success = false, Message = "Quantity must be greater than zero." };
        }

        // 1. Source warehouse is always the requester's own warehouse.
        var fromWarehouseId = await _currentUser.GetWarehouseIdAsync(cancellationToken);
        if (fromWarehouseId is null)
        {
            return new ActionResponse
            {
                Success = false,
                Message = "Only an assigned Store Manager can request a transfer."
            };
        }

        if (fromWarehouseId.Value == request.ToWarehouseId)
        {
            return new ActionResponse
            {
                Success = false,
                Message = "Source and destination warehouses must be different."
            };
        }

        // 2. Make sure the destination warehouse exists.
        var destExists = await _context.Warehouses
            .AnyAsync(w => w.Id == request.ToWarehouseId, cancellationToken);
        if (!destExists)
        {
            return new ActionResponse { Success = false, Message = "Destination warehouse does not exist." };
        }

        // 3. Make sure the requester actually has enough stock to send.
        var sourceStock = await _context.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == request.ProductId && s.WarehouseId == fromWarehouseId.Value,
            cancellationToken);

        if (sourceStock is null || sourceStock.Quantity < request.Quantity)
        {
            var available = sourceStock?.Quantity ?? 0;
            return new ActionResponse
            {
                Success = false,
                Message = $"Insufficient stock in your warehouse (available: {available})."
            };
        }

        // 4. Create the pending transfer request. Stock is moved on Accept,
        //    keeping the source intact while the destination decides.
        var transfer = new TransferRequest
        {
            ProductId = request.ProductId,
            FromWarehouseId = fromWarehouseId.Value,
            ToWarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            Status = TransferStatus.Pending,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "StoreManager"
        };

        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = "Transfer request sent and is pending approval." };
    }
}
