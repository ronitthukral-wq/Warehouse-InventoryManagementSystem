namespace Inventory.Contracts.Responses;

public class ActionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}