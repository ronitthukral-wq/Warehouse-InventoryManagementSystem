using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Warehouses;

public class GetAllWarehousesHandler : IRequestHandler<GetAllWarehousesRequest, List<WarehouseResponse>>
{
    private readonly InventoryDbContext _context;

    public GetAllWarehousesHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseResponse>> Handle(GetAllWarehousesRequest request, CancellationToken cancellationToken)
    {
        // Fetch warehouses from DB and map them to the response model
        return await _context.Warehouses
            .Select(w => new WarehouseResponse
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location
            })
            .ToListAsync(cancellationToken);
    }
}