using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _db;

    public TenantRepository(AppDbContext db) => _db = db;

    public void Add(Tenant tenant) => _db.Tenants.Add(tenant);

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<bool> IsFlexiblePricingEnabledAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Tenants.AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => t.FlexiblePricingEnabled)
            .FirstOrDefaultAsync(cancellationToken);

    public Task SetFlexiblePricingEnabledAsync(Guid tenantId, bool enabled, CancellationToken cancellationToken = default) =>
        _db.Tenants
            .Where(t => t.Id == tenantId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.FlexiblePricingEnabled, enabled), cancellationToken);
}
