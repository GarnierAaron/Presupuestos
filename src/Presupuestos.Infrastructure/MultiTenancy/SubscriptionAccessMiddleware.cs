using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;
using Presupuestos.Application.Options;

namespace Presupuestos.Infrastructure.MultiTenancy;

/// <summary>
/// Tras resolver tenant, exige suscripción activa para la organización (configurable).
/// </summary>
public class SubscriptionAccessMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionAccessMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IServiceScopeFactory scopeFactory,
        IOptions<SubscriptionAccessOptions> options)
    {
        if (!options.Value.Enforce)
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        if (ShouldBypass(context.Request.Path, context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (string.Equals(
                context.User.FindFirst(AuthConstants.SuperAdminClaim)?.Value,
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        var tenantId = tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
        {
            await _next(context);
            return;
        }

        var subscriptions = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        if (await subscriptions.HasActiveSubscriptionAsync(tenantId.Value, context.RequestAborted))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Se requiere una suscripción activa para esta organización. Activá el plan Free o completá el pago en Mercado Pago.",
            status = 403,
        }, context.RequestAborted);
    }

    private static bool ShouldBypass(PathString path, string method)
    {
        var p = path.Value ?? "";
        if (p.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return true;
        if (p.StartsWith("/api/Auth", StringComparison.OrdinalIgnoreCase))
            return true;
        if (p.StartsWith("/api/app-config", StringComparison.OrdinalIgnoreCase))
            return true;
        if (p.StartsWith("/api/CalculationExample", StringComparison.OrdinalIgnoreCase))
            return true;
        if (p.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
            return true;
        if (p.StartsWith("/api/webhooks", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(p, "/api/Subscriptions/me", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(p, "/api/Subscriptions/create", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
