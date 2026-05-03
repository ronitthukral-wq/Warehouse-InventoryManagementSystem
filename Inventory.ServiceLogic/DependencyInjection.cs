using AutoMapper;
using Inventory.Models.Entities;
using Inventory.ServiceLogic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.ServiceLogic;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceLogic(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(assembly);
        });

        // Required by ICurrentUserContext (and any handler that needs request context)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IWarehouseManagerGuard, WarehouseManagerGuard>();

        return services;
    }
}

/// <summary>
/// Lightweight abstraction so that handlers can resolve "who is calling me right now"
/// without taking direct dependencies on HttpContext or UserManager. Keeps handlers
/// testable and aligned with SOLID (specifically D - depend on abstractions).
/// </summary>
public interface ICurrentUserContext
{
    Task<CurrentUserInfo> GetAsync(CancellationToken ct = default);
}

public sealed record CurrentUserInfo(
    string? UserId,
    string UserName,
    int? WarehouseId,
    bool IsAuthenticated,
    bool IsAdmin,
    bool IsStoreManager);

internal sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _http;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserContext(IHttpContextAccessor http, UserManager<ApplicationUser> userManager)
    {
        _http = http;
        _userManager = userManager;
    }

    public async Task<CurrentUserInfo> GetAsync(CancellationToken ct = default)
    {
        var principal = _http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return new CurrentUserInfo(null, "System", null, false, false, false);
        }

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return new CurrentUserInfo(null, principal.Identity.Name ?? "System", null, false, false, false);
        }

        var isAdmin = principal.IsInRole("Admin");
        var isStoreManager = principal.IsInRole("StoreManager");

        return new CurrentUserInfo(
            user.Id,
            user.UserName ?? user.Email ?? "User",
            user.WarehouseId,
            true,
            isAdmin,
            isStoreManager);
    }
}
