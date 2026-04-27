using Inventory.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Reflection.Emit;

namespace Inventory.Data.Context;

// We inherit from IdentityDbContext to include all AspNet security tables
public class InventoryDbContext : IdentityDbContext<ApplicationUser>
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Critical for Identity tables!

        // This line automatically finds all classes implementing IEntityTypeConfiguration 
        // in this project and applies them. Clean and Scalable.
        builder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }

    // Senior Pro Tip: Automatically handle CreatedDate and UpdatedDate
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
            }
            else
            {
                ((BaseEntity)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;
                // Prevent overriding CreatedDate during an update
                entityEntry.Property("CreatedDate").IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}