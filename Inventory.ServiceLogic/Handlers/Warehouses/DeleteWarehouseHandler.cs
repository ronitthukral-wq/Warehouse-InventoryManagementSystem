using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class DeleteWarehouseHandler : IRequestHandler<DeleteWarehouseRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    public DeleteWarehouseHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(DeleteWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Managers)
            .Include(w => w.Stocks)
            .FirstOrDefaultAsync(w => w.Id == request.Id, ct);

        if (warehouse == null) return ActionResponse.Failure("Warehouse not found.");

        if (warehouse.Managers.Any())
        {
            return ActionResponse.Failure("This warehouse has an assigned store manager. Remove the manager first.");
        }

        if (warehouse.Stocks.Any(s => s.Quantity > 0))
        {
            return ActionResponse.Failure("This warehouse still has stock. Clear all stock before deleting the warehouse.");
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(ct);

        return ActionResponse.Successful("Warehouse deleted successfully.");
    }
}
