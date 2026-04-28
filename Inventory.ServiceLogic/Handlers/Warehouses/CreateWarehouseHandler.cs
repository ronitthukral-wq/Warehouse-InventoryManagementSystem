using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

// MUST BE PUBLIC and implement IRequestHandler
public class CreateWarehouseHandler : IRequestHandler<CreateWarehouseRequest, ActionResponse>
{
    private readonly InventoryDbContext _context;

    public CreateWarehouseHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse> Handle(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var warehouse = new Warehouse
            {
                Name = request.Name,
                Location = request.Location
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync(cancellationToken);

            return ActionResponse.Successful("Warehouse created successfully.");
        }
        catch (Exception ex)
        {
            return ActionResponse.Failure($"Failed to create warehouse: {ex.Message}");
        }
    }
}