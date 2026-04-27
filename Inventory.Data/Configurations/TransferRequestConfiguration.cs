using Inventory.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Data.Configurations;

public class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
{
    public void Configure(EntityTypeBuilder<TransferRequest> builder)
    {
        builder.HasKey(tr => tr.Id);

        // Path 1: From Warehouse
        builder.HasOne(tr => tr.FromWarehouse)
               .WithMany()
               .HasForeignKey(tr => tr.FromWarehouseId)
               .OnDelete(DeleteBehavior.Restrict); // The Fix

        // Path 2: To Warehouse
        builder.HasOne(tr => tr.ToWarehouse)
               .WithMany()
               .HasForeignKey(tr => tr.ToWarehouseId)
               .OnDelete(DeleteBehavior.Restrict); // The Fix

        builder.Property(tr => tr.Status).IsRequired().HasMaxLength(20);
    }
}