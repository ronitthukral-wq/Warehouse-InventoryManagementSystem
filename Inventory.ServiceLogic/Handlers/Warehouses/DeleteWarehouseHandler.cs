using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class DeleteWarehouseHandler : IRequestHandler<DeleteWarehouseRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    public DeleteWarehouseHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(DeleteWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses.FindAsync(new object[] { request.Id }, ct);
        if (warehouse == null) return ActionResponse.Failure("Warehouse not found.");

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(ct);

        return ActionResponse.Successful("Warehouse deleted successfully.");
    }
}