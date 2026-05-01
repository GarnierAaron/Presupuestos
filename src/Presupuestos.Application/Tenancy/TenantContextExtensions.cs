using Presupuestos.Application.Abstractions;

namespace Presupuestos.Application.Tenancy;

public static class TenantContextExtensions
{
    /// <summary>
    /// Tenant obligatorio para operaciones multi-tenant (servicios, presupuestos, etc.).
    /// </summary>
    public static Guid RequireTenantId(this ITenantContext tenant) =>
        tenant.TenantId
        ?? throw new InvalidOperationException("Esta operación requiere un tenant (no aplica a la sesión de super administrador).");
}
