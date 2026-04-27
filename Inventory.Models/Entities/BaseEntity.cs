namespace Inventory.Models.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    // Use string for 'By' fields to store the Identity User's Id or Username
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}