using AutoMapper;
using Inventory.Models.Entities;
using Inventory.Contracts.Responses;

namespace Inventory.ServiceLogic.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<Warehouse, WarehouseResponse>();
        CreateMap<StockMovement, MovementHistoryResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreatedDate));
    }
}