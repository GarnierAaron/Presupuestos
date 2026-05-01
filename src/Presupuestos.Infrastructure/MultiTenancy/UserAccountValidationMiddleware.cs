using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;

namespace Presupuestos.Infrastructure.MultiTenancy;

/// <summary>
/// Tras validar la firma JWT, comprueba en base de datos que la cuenta siga activa y no haya vencido.
/// </summary>
public class UserAccountValidationMiddleware
{
    private readonly RequestDelegate _next;

    public UserAccountValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
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

        using var scope = scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await users.GetByIdGlobalAsync(userId, context.RequestAborted);
        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ErrorBody("Usuario no encontrado.", 401), context.RequestAborted);
            return;
        }

        if (!user.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorBody("La cuenta está desactivada.", 403), context.RequestAborted);
            return;
        }

        if (user.ExpirationDate.HasValue && user.ExpirationDate.Value < DateTimeOffset.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorBody("La cuenta ha expirado.", 403), context.RequestAborted);
            return;
        }

        await _next(context);
    }

    private sealed record ErrorBody(string title, int status);
}
