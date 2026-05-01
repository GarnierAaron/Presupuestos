using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface ISubscriptionRepository
{
    void Add(Subscription subscription);

    Task<Subscription?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);

    Task<Subscription?> GetActiveByTenantAndPlanAsync(
        Guid tenantId,
        string planName,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Suscripción vigente más reciente del tenant (prioriza Active válida por fecha).</summary>
    Task<Subscription?> GetCurrentForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task CancelPendingForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task CancelOtherActiveForTenantAsync(Guid tenantId, Guid exceptSubscriptionId, CancellationToken cancellationToken = default);
}
