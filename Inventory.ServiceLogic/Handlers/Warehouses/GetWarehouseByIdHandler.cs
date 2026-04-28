using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class GetWarehouseByIdHandler : IRequestHandler<GetWarehouseByIdRequest, WarehouseResponse?>
{
    private readonly InventoryDbContext _context;
    public GetWarehouseByIdHandler(InventoryDbContext context) => _context = context;

    public async Task<WarehouseResponse?> Handle(GetWarehouseByIdRequest request, CancellationToken ct)
    {
        var w = await _context.Warehouses.FindAsync(new object[] { request.Id }, ct);
        if (w == null) return null;

        return new WarehouseResponse
        {
            Id = w.Id,
            Name = w.Name,
            Location = w.Location
        };
    }
}