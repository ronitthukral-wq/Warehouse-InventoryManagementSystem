using AutoMapper;
using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Enums;
using Inventory.ServiceLogic.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetPendingTransfersHandler : IRequestHandler<GetPendingTransfersRequest, List<TransferRequestResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetPendingTransfersHandler(
        InventoryDbContext db,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<TransferRequestResponse>> Handle(
        GetPendingTransfersRequest request,
        CancellationToken cancellationToken)
    {
        var query = _db.TransferRequests
            .Include(t => t.Product)
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Where(t => t.Status == TransferStatus.Pending)
            .AsNoTracking();

        // Store Managers see only the transfers landing in THEIR warehouse,
        // i.e. the inbox of approvals they need to make. Admin sees all.
        if (_currentUser.IsStoreManager)
        {
            var warehouseId = await _currentUser.GetWarehouseIdAsync(cancellationToken);
            if (warehouseId is null)
            {
                return new List<TransferRequestResponse>();
            }
            query = query.Where(t => t.ToWarehouseId == warehouseId.Value);
        }

        var pending = await query
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TransferRequestResponse>>(pending);
    }
}
