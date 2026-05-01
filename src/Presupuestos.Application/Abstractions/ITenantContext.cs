namespace Presupuestos.Application.Abstractions;

public interface ITenantContext
{
    /// <summary>Null en sesión de super administrador (JWT sin <c>tenant_id</c>).</summary>
    Guid? TenantId { get; }

    Guid? UserId { get; }
}
