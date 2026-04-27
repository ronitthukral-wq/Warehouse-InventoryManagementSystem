using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // One Warehouse can have many Managers, but a Manager has one Warehouse
        builder.HasOne(u => u.Warehouse)
               .WithMany(w => w.Managers)
               .HasForeignKey(u => u.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}