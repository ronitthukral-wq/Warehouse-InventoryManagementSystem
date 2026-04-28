using System.Security.Claims;
using Inventory.Models.Entities;
using Inventory.ServiceLogic.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Inventory.Web.Services;

/// <summary>
/// HTTP-aware implementation of <see cref="ICurrentUserService"/>. Reads the
/// authenticated principal from the current request and looks up the user's
/// assigned warehouse via Identity. Result is cached for the scoped lifetime.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    private int? _cachedWarehouseId;
    private bool _warehouseLoaded;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.Identity?.Name;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => IsInRole("Admin");

    public bool IsStoreManager => IsInRole("StoreManager");

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) ?? false;

    public async Task<int?> GetWarehouseIdAsync(CancellationToken cancellationToken = default)
    {
        if (_warehouseLoaded) return _cachedWarehouseId;

        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
        {
            _warehouseLoaded = true;
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);
        _cachedWarehouseId = user?.WarehouseId;
        _warehouseLoaded = true;
        return _cachedWarehouseId;
    }
}
