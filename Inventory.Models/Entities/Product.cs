using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.Entities;

public class Product : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int LowStockThreshold { get; set; } = 10;

    // Navigation Properties
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}