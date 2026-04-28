using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class UpdateWarehouseHandler : IRequestHandler<UpdateWarehouseRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;
    public UpdateWarehouseHandler(InventoryDbContext context) => _context = context;

    public async Task<ActionResponse> Handle(UpdateWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses.FindAsync(new object[] { request.Id }, ct);
        if (warehouse == null) return ActionResponse.Failure("Warehouse not found.");

        warehouse.Name = request.Name;
        warehouse.Location = request.Location;

        await _context.SaveChangesAsync(ct);
        return ActionResponse.Successful("Warehouse updated successfully.");
    }
}