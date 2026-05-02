using AutoMapper;
using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsRequest, List<ProductResponse>>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetAllProductsHandler(InventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProductResponse>> Handle(GetAllProductsRequest request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        // Sum stock across ALL warehouses for each product so the Products list
        // shows a real total instead of always showing 0.
        var stockTotals = await _context.Stocks
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(s => s.Quantity) })
            .ToListAsync(cancellationToken);

        var stockDict = stockTotals.ToDictionary(s => s.ProductId, s => s.Total);

        var response = _mapper.Map<List<ProductResponse>>(products);
        foreach (var r in response)
        {
            r.TotalAvailableQuantity = stockDict.TryGetValue(r.Id, out var qty) ? qty : 0;
        }

        return response;
    }
}
