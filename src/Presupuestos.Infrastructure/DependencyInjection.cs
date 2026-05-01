using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Options;
using Presupuestos.Infrastructure.MultiTenancy;
using Presupuestos.Infrastructure.Persistence;
using Presupuestos.Infrastructure.Security;

namespace Presupuestos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DeviceControlOptions>(configuration.GetSection(DeviceControlOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IAppConfigRepository, AppConfigRepository>();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantResolutionMiddleware>();

    public static IApplicationBuilder UseUserAccountValidation(this IApplicationBuilder app) =>
        app.UseMiddleware<UserAccountValidationMiddleware>();

    public static IApplicationBuilder UseDeviceValidation(this IApplicationBuilder app) =>
        app.UseMiddleware<DeviceValidationMiddleware>();
}
