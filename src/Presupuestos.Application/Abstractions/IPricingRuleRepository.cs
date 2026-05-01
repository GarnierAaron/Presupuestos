using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IPricingRuleRepository
{
    Task<IReadOnlyList<PricingRule>> ListOrderedForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<PricingRule?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    void Add(PricingRule rule);
    void Update(PricingRule rule);
    void Remove(PricingRule rule);
}
