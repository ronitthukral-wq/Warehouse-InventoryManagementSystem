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

        // ApplicationUser -> UserResponse
        // Role is intentionally Ignored here because Identity stores roles in a separate
        // join table (AspNetUserRoles). The handler is responsible for populating Role
        // via UserManager.GetRolesAsync to keep this mapping pure (SRP).
        CreateMap<ApplicationUser, UserResponse>()
            .ForMember(dest => dest.AssignedWarehouseName,
                opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}
