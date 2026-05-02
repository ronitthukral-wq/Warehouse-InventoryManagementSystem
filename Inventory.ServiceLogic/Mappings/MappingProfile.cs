using AutoMapper;
using Inventory.Contracts.Responses;
using Inventory.Models.Entities;

namespace Inventory.ServiceLogic.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<Warehouse, WarehouseResponse>();

        CreateMap<Stock, StockLevelResponse>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.SKU, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.LowStockThreshold, o => o.MapFrom(s => s.Product.LowStockThreshold));

        CreateMap<StockMovement, MovementHistoryResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.MovementType, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.CreatedDate))
            .ForMember(d => d.PerformedBy, o => o.MapFrom(s => s.CreatedBy ?? string.Empty));

        CreateMap<TransferRequest, TransferRequestResponse>()
            .ForMember(d => d.Product, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.FromWarehouseName, o => o.MapFrom(s => s.FromWarehouse.Name))
            .ForMember(d => d.ToWarehouseName, o => o.MapFrom(s => s.ToWarehouse.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Direction, o => o.Ignore())
            .ForMember(d => d.IsActionable, o => o.Ignore());

        CreateMap<ApplicationUser, UserResponse>()
            .ForMember(d => d.AssignedWarehouseName,
                opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(d => d.Role, opt => opt.Ignore());
    }
}