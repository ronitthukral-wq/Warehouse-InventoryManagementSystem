using AutoMapper;
using Inventory.Contracts.Responses;
using Inventory.Models.Entities;

namespace Inventory.ServiceLogic.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ----- Products -----
        CreateMap<Product, ProductResponse>();

        // ----- Warehouses -----
        CreateMap<Warehouse, WarehouseResponse>();

        // ----- Stock movements -----
        CreateMap<StockMovement, MovementHistoryResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.MovementType, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.CreatedDate))
            .ForMember(d => d.PerformedBy, o => o.MapFrom(s => s.CreatedBy ?? string.Empty));

        // ----- Stock levels (per warehouse) -----
        CreateMap<Stock, StockLevelResponse>()
            .ForMember(d => d.Id, o => o.Ignore()) // composite key entity
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.SKU, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.LowStockThreshold, o => o.MapFrom(s => s.Product.LowStockThreshold));

        // ----- Transfer requests -----
        CreateMap<TransferRequest, TransferRequestResponse>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.Sku, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.FromWarehouseName, o => o.MapFrom(s => s.FromWarehouse.Name))
            .ForMember(d => d.ToWarehouseName, o => o.MapFrom(s => s.ToWarehouse.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // ----- Users -----
        // Role is intentionally Ignored: Identity stores roles in a join table,
        // so the handler populates Role via UserManager.GetRolesAsync (SRP).
        CreateMap<ApplicationUser, UserResponse>()
            .ForMember(d => d.AssignedWarehouseName,
                o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.Name : null))
            .ForMember(d => d.Role, o => o.Ignore());
    }
}
