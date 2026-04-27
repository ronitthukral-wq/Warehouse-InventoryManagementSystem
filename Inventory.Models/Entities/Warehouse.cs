using Inventory.Models.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    // These are "Navigations" - they don't create columns, they just allow linking in C#
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public virtual ICollection<ApplicationUser> Managers { get; set; } = new List<ApplicationUser>();
}