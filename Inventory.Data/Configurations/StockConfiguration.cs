using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Data.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // Composite Key: A warehouse cannot have the same product twice.
        builder.HasKey(s => new { s.ProductId, s.WarehouseId });

        builder.Property(s => s.Quantity)
               .IsRequired()
               .HasDefaultValue(0);

        builder.HasOne(s => s.Product)
               .WithMany(p => p.Stocks)
               .HasForeignKey(s => s.ProductId);

        builder.HasOne(s => s.Warehouse)
               .WithMany(w => w.Stocks)
               .HasForeignKey(s => s.WarehouseId);
    }
}