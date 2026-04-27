using AutoMapper;
using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.ServiceLogic.Handlers.Products;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdRequest, ProductResponse>
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(InventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductResponse> Handle(GetProductByIdRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }
}