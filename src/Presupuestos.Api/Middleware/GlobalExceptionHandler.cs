using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Common.Exceptions;

namespace Presupuestos.Api.Middleware;

public class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int status, string title) = exception switch
        {
            AuthException a => (StatusCodes.Status400BadRequest, a.Message),
            UnauthorizedAppException u => (StatusCodes.Status401Unauthorized, u.Message),
            ForbiddenAppException f => (StatusCodes.Status403Forbidden, f.Message),
            ValidationException v => (StatusCodes.Status400BadRequest, string.Join(" ", v.Errors.Select(e => e.ErrorMessage))),
            DbUpdateException db => (StatusCodes.Status500InternalServerError, DbMessage(db)),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado");

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";

        if (environment.IsDevelopment() && status == StatusCodes.Status500InternalServerError)
        {
            var detail = exception.GetBaseException().Message;
            await httpContext.Response.WriteAsJsonAsync(
                new { title, status, detail },
                cancellationToken);
            return true;
        }

        await httpContext.Response.WriteAsJsonAsync(
            new { title, status },
            cancellationToken);
        return true;
    }

    private static string DbMessage(DbUpdateException db)
    {
        var msg = db.GetBaseException().Message;
        if (msg.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
            return "Base de datos desactualizada. Ejecuta: dotnet ef database update (proyecto Infrastructure).";
        return "Error al guardar en la base de datos.";
    }
}
