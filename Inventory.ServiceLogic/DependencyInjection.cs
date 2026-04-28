using AutoMapper;
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

        // FIX: use cfg.AddMaps() instead of passing assembly directly
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(assembly);
        });

        return services;
    }
}