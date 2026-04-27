using Inventory.Contracts.Responses;
using MediatR;

namespace Inventory.Contracts.Requests.Warehouses;

public class GetAllWarehousesRequest : IRequest<List<WarehouseResponse>> { }