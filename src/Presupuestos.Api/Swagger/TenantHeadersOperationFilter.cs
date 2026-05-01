using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presupuestos.Api.Swagger;

public class TenantHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        if (path.Contains("CalculationExample", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("Auth", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("app-config", StringComparison.OrdinalIgnoreCase))
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        if (operation.Parameters.All(p => p.Name != "X-Tenant-Id"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                Description = "Requerido solo si no envías JWT. Con Bearer token se toma tenant_id del token."
            });
        }

        if (operation.Parameters.All(p => p.Name != "X-User-Id"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                Description = "Opcional sin JWT (margen global). Con JWT se usa el usuario del token."
            });
        }

        if (operation.Parameters.All(p => p.Name != "X-Device-Id"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Si DeviceControl está habilitado, debe coincidir con el deviceId enviado en login/registro."
            });
        }
    }
}
