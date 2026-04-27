namespace Inventory.Models.Enums
{
    public enum MovementType
    {
        Purchase = 1,     // Stock coming from a supplier
        TransferIn = 2,   // Stock received from another warehouse
        TransferOut = 3,  // Stock sent to another warehouse
        Adjustment = 4    // Manual correction (e.g., breakage)
    }
}
