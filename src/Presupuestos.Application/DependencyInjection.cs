using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Services;

namespace Presupuestos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<ItemService>();
        services.AddScoped<ServiceCatalogService>();
        services.AddScoped<BudgetAppService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IRemoteAppConfigService, RemoteAppConfigService>();
        services.AddScoped<IFlexiblePricingEngine, FlexiblePricingEngine>();
        services.AddScoped<PricingRuleService>();
        services.AddScoped<FlexiblePricingSettingsService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        return services;
    }
}
