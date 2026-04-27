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
        var products = await _context.Products.ToListAsync(cancellationToken);
        return _mapper.Map<List<ProductResponse>>(products);
    }
}