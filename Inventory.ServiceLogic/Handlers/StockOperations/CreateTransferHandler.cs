using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Models.Enums;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class CreateTransferHandler : IRequestHandler<CreateTransferRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public CreateTransferHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(CreateTransferRequest request, CancellationToken cancellationToken)
    {
        // Logic: StoreManager 1 requests stock from their warehouse to another
        var transfer = new TransferRequest
        {
            ProductId = request.ProductId,
            ToWarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            Status = TransferStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActionResponse { Success = true, Message = "Transfer request sent and is pending approval." };
    }
}