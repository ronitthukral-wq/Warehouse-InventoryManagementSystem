namespace Inventory.Contracts.Responses;

public class ActionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    // This method handles successful results
    public static ActionResponse Successful(string message = "Success", object? data = null)
    {
        return new ActionResponse
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    // This method handles error results
    public static ActionResponse Failure(string message)
    {
        return new ActionResponse
        {
            Success = false,
            Message = message
        };
    }
}