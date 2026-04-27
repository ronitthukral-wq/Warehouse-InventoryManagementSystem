using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(p => p.SKU).IsUnique(); // SKU must be unique in DB
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
    }
}