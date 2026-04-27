using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums;
using Inventory.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class RespondToTransferHandler : IRequestHandler<RespondToTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _db;

    public RespondToTransferHandler(InventoryDbContext db) => _db = db;

    public async Task<ActionResponse> Handle(RespondToTransferRequest request, CancellationToken cancellationToken)
    {
        var transfer = await _db.TransferRequests
            .FirstOrDefaultAsync(t => t.Id == request.TransferRequestId, cancellationToken);

        if (transfer == null || transfer.Status != TransferStatus.Pending)
            return new ActionResponse { Success = false, Message = "Transfer request not found or already processed." };

        if (!request.Accept)
        {
            transfer.Status = TransferStatus.Rejected;
            await _db.SaveChangesAsync(cancellationToken);
            return new ActionResponse { Success = true, Message = "Transfer request rejected." };
        }

        // Logic: Execute the stock move if accepted
        // 1. Logic for source warehouse deduction usually happens at request time 
        // or here depending on your business rules. 

        // 2. Add stock to destination
        var destinationStock = await _db.Stocks.FirstOrDefaultAsync(s =>
            s.ProductId == transfer.ProductId && s.WarehouseId == transfer.ToWarehouseId, cancellationToken);

        if (destinationStock == null)
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
            destinationStock.Quantity += transfer.Quantity;
        }

        transfer.Status = TransferStatus.Accepted;
        await _db.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = "Transfer completed successfully." };
    }
}