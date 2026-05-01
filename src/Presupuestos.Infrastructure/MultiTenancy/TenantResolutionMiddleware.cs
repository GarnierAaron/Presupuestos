using Microsoft.AspNetCore.Http;
using Presupuestos.Application.Auth;

namespace Presupuestos.Infrastructure.MultiTenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (ShouldBypass(path))
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdStr = AuthClaims.FindUserId(context.User);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Token sin identificador de usuario (sub).",
                    status = 401
                });
                return;
            }

            var isSuperAdmin = string.Equals(
                context.User.FindFirst(AuthConstants.SuperAdminClaim)?.Value,
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (isSuperAdmin)
            {
                HttpTenantContext.SetContext(context, tenantId: null, userId);
                await _next(context);
                return;
            }

            var tenantClaim = context.User.FindFirst(AuthConstants.TenantIdClaim)?.Value;
            if (string.IsNullOrEmpty(tenantClaim) || !Guid.TryParse(tenantClaim, out var tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Token sin claim de tenant (tenant_id).",
                    status = 401
                });
                return;
            }

            HttpTenantContext.SetContext(context, tenantId, userId);
            await _next(context);
            return;
        }

        // Bearer enviado pero JWT no autenticó (caducado, firma distinta, issuer distinto, etc.).
        // Sin esto parece "falta tenant" y devolvemos 400, que no dispara refresh en el cliente.
        if (context.Request.Headers.TryGetValue("Authorization", out var authValues))
        {
            var auth = authValues.ToString();
            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
                auth.Length > "Bearer ".Length)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "El token JWT no es válido o caducó. Inicia sesión de nuevo o espera la renovación automática.",
                    status = 401
                });
                return;
            }
        }

        if (!context.Request.Headers.TryGetValue(HttpTenantContext.TenantHeader, out var raw) ||
            !Guid.TryParse(raw, out var headerTenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = $"Se requiere el encabezado {HttpTenantContext.TenantHeader} o un bearer token JWT.",
                status = 400
            });
            return;
        }

        Guid? headerUserId = null;
        if (context.Request.Headers.TryGetValue(HttpTenantContext.UserHeader, out var userRaw) &&
            Guid.TryParse(userRaw, out var parsedUser))
            headerUserId = parsedUser;

        HttpTenantContext.SetContext(context, headerTenantId, headerUserId);
        await _next(context);
    }

    private static bool ShouldBypass(PathString path)
    {
        if (path.StartsWithSegments("/swagger"))
            return true;
        if (path.StartsWithSegments("/api/CalculationExample", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWithSegments("/api/Auth", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWithSegments("/api/app-config", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
