using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Location).IsRequired().HasMaxLength(250);

        // Relationship with Managers (ApplicationUser)
        builder.HasMany(w => w.Managers)
               .WithOne(u => u.Warehouse)
               .HasForeignKey(u => u.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}