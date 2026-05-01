using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface ITenantRepository
{
    void Add(Tenant tenant);

    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> IsFlexiblePricingEnabledAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task SetFlexiblePricingEnabledAsync(Guid tenantId, bool enabled, CancellationToken cancellationToken = default);
}
