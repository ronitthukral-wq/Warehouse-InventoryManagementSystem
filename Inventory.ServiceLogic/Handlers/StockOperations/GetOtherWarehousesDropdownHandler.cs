using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetOtherWarehousesDropdownHandler
    : IRequestHandler<GetOtherWarehousesDropdownRequest, List<DropdownItemResponse>>
{
    private readonly InventoryDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetOtherWarehousesDropdownHandler(InventoryDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<DropdownItemResponse>> Handle(
        GetOtherWarehousesDropdownRequest request, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        var excludeId = actor.WarehouseId ?? request.ExcludeWarehouseId;

        return await _db.Warehouses
            .Where(w => w.Id != excludeId)
            .OrderBy(w => w.Name)
            .Select(w => new DropdownItemResponse
            {
                Value = w.Id.ToString(),
                Display = w.Name + " - " + w.Location
            })
            .ToListAsync(cancellationToken);
    }
}