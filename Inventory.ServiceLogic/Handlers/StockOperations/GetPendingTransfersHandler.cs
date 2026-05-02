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
    private readonly ICurrentUserContext _currentUser;

    public GetPendingTransfersHandler(InventoryDbContext db, IMapper mapper, ICurrentUserContext currentUser)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<TransferRequestResponse>> Handle(GetPendingTransfersRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);

        // Base query: include navigation props so the mapper can resolve names
        var query = _db.TransferRequests
            .Include(t => t.Product)
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .AsNoTracking()
            .AsQueryable();

        // Store Manager: see ALL transfers involving their warehouse (incoming + outgoing,
        // any status) so the page doubles as their transfer history.
        // Admin: see everything pending so they can monitor activity.
        if (actor.IsStoreManager && actor.WarehouseId is int warehouseId)
        {
            query = query.Where(t => t.ToWarehouseId == warehouseId || t.FromWarehouseId == warehouseId);
        }
        else
        {
            query = query.Where(t => t.Status == TransferStatus.Pending);
        }

        var transfers = await query
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

        var mapped = _mapper.Map<List<TransferRequestResponse>>(transfers);

        // Decorate Direction + IsActionable for the current user
        for (int i = 0; i < mapped.Count; i++)
        {
            var entity = transfers[i];
            var dto = mapped[i];

            if (actor.IsStoreManager && actor.WarehouseId is int wid)
            {
                // Direction = what's happening to MY stock when accepted:
                //   - I'm the destination (ToWarehouseId == me) → stock comes IN
                //   - I'm the source      (FromWarehouseId == me) → stock goes OUT
                dto.Direction = entity.ToWarehouseId == wid ? "Incoming" : "Outgoing";

                // The SOURCE manager is the approver under the new request semantics.
                dto.IsActionable = entity.Status == TransferStatus.Pending && entity.FromWarehouseId == wid;
            }
            else
            {
                dto.Direction = "-";
                dto.IsActionable = false;
            }
        }

        return mapped;
    }
}
