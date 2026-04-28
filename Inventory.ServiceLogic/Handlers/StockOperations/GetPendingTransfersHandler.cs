using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetPendingTransfersHandler : IRequestHandler<GetPendingTransfersRequest, List<TransferRequestResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;

    public GetPendingTransfersHandler(InventoryDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<TransferRequestResponse>> Handle(GetPendingTransfersRequest request, CancellationToken cancellationToken)
    {
        // 1. Fetch only 'Pending' status transfers to match TransferStatus enum logic
        // 2. Include Product and Warehouse entities so AutoMapper can fill 'ProductName' and 'ToWarehouseName'
        var pendingTransfers = await _db.TransferRequests
            .Include(t => t.Product)
            .Include(t => t.ToWarehouse)
            .Where(t => t.Status == TransferStatus.Pending)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 3. Map to the TransferRequestResponse list required by the request interface
        return _mapper.Map<List<TransferRequestResponse>>(pendingTransfers);
    }
}