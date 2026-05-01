namespace Presupuestos.Infrastructure.MultiTenancy;

internal sealed record TenantResolutionState(Guid? TenantId, Guid? UserId);
