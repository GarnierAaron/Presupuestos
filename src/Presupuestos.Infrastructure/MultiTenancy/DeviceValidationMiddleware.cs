using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;
using Presupuestos.Application.HttpHeaders;
using Presupuestos.Application.Options;

namespace Presupuestos.Infrastructure.MultiTenancy;

/// <summary>
/// Con <see cref="DeviceControlOptions.Enabled"/> y cabecera en peticiones autenticadas, valida el dispositivo.
/// </summary>
public class DeviceValidationMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IServiceScopeFactory scopeFactory,
        IOptions<DeviceControlOptions> options)
    {
        if (!options.Value.Enabled)
        {
            await _next(context);
            return;
        }

        if (ShouldBypassPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
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

        var userIdStr = AuthClaims.FindUserId(context.User);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await _next(context);
            return;
        }

        context.Request.Headers.TryGetValue(DeviceHeaderNames.ClientDeviceId, out var headerValues);
        var header = headerValues.FirstOrDefault();

        using var scope = scopeFactory.CreateScope();
        var devices = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        await devices.ValidateRequestDeviceAsync(userId, header, context.RequestAborted);

        await _next(context);
    }

    private static bool ShouldBypassPath(PathString path)
    {
        if (path.StartsWithSegments("/swagger"))
            return true;
        if (path.StartsWithSegments("/api/CalculationExample", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWithSegments("/api/Auth", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWithSegments("/api/app-config", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWithSegments("/api/admin", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
