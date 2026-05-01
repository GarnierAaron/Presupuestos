using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _db;

    public SubscriptionRepository(AppDbContext db) => _db = db;

    public void Add(Subscription subscription) => _db.Subscriptions.Add(subscription);

    public async Task<Subscription?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Subscription?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        var active = await _db.Subscriptions.FirstOrDefaultAsync(
            s => s.ExternalPaymentId == externalPaymentId && s.Status == SubscriptionStatuses.Active,
            cancellationToken);
        if (active != null)
            return active;

        return await _db.Subscriptions.FirstOrDefaultAsync(
            s => s.ExternalPaymentId == externalPaymentId,
            cancellationToken);
    }

    public async Task<Subscription?> GetActiveByTenantAndPlanAsync(
        Guid tenantId,
        string planName,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Where(s => s.PlanName == planName)
            .Where(s => s.Status == SubscriptionStatuses.Active)
            .Where(s => !s.EndDate.HasValue || s.EndDate.Value >= now)
            .OrderByDescending(s => s.EndDate ?? DateTime.MaxValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _db.Subscriptions.AsNoTracking()
            .AnyAsync(
                s => s.TenantId == tenantId
                     && s.Status == SubscriptionStatuses.Active
                     && (!s.EndDate.HasValue || s.EndDate.Value >= now),
                cancellationToken);
    }

    public async Task<Subscription?> GetCurrentForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var active = await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Where(s => s.Status == SubscriptionStatuses.Active)
            .Where(s => !s.EndDate.HasValue || s.EndDate.Value >= now)
            .OrderByDescending(s => s.EndDate == null)
            .ThenByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);
        if (active != null)
            return active;

        return await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Where(s => s.Status == SubscriptionStatuses.Pending)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CancelPendingForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Subscriptions
            .Where(s => s.TenantId == tenantId && s.Status == SubscriptionStatuses.Pending)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.Status, SubscriptionStatuses.Cancelled),
                cancellationToken);

    public async Task CancelOtherActiveForTenantAsync(
        Guid tenantId,
        Guid exceptSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Subscriptions.Where(s => s.TenantId == tenantId && s.Status == SubscriptionStatuses.Active);
        if (exceptSubscriptionId != Guid.Empty)
            q = q.Where(s => s.Id != exceptSubscriptionId);

        await q.ExecuteUpdateAsync(
            s => s.SetProperty(x => x.Status, SubscriptionStatuses.Cancelled),
            cancellationToken);
    }
}
