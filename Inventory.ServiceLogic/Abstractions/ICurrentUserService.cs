namespace Inventory.ServiceLogic.Abstractions;

/// <summary>
/// Abstraction over the currently authenticated user. Implemented in the Web
/// layer so the ServiceLogic project stays free of any HTTP / Identity types
/// (Dependency Inversion Principle).
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }

    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsStoreManager { get; }
    bool IsInRole(string role);

    /// <summary>
    /// Returns the warehouse the current user manages (Store Manager only).
    /// Cached per scoped request to avoid repeated DB calls.
    /// </summary>
    Task<int?> GetWarehouseIdAsync(CancellationToken cancellationToken = default);
}
