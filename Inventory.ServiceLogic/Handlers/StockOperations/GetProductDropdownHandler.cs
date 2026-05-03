using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.StockOperations;

public class GetProductDropdownHandler : IRequestHandler<GetProductDropdownRequest, List<DropdownItemResponse>>
{
    private readonly InventoryDbContext _db;
    public GetProductDropdownHandler(InventoryDbContext db) => _db = db;

    public async Task<List<DropdownItemResponse>> Handle(
        GetProductDropdownRequest request, CancellationToken cancellationToken)
    {
        return await _db.Products
            .OrderBy(p => p.Name)
            .Select(p => new DropdownItemResponse
            {
                Value = p.Id.ToString(),
                Display = p.Name + " (" + p.SKU + ")"
            })
            .ToListAsync(cancellationToken);
    }
}