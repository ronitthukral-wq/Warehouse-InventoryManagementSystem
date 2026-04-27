using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(sm => sm.Id);

        builder.HasOne(sm => sm.Warehouse)
               .WithMany(w => w.StockMovements)
               .HasForeignKey(sm => sm.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict); // The Fix

        builder.HasOne(sm => sm.Product)
               .WithMany()
               .HasForeignKey(sm => sm.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sm => sm.Type).IsRequired().HasMaxLength(50);
    }
}