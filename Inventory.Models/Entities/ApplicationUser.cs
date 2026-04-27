using Microsoft.AspNetCore.Identity;

namespace Inventory.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public int? WarehouseId { get; set; }
    public virtual Warehouse? Warehouse { get; set; }

    // Manually matching the BaseEntity "Contract"
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}