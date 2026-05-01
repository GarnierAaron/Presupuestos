using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class PricingRuleRepository : IPricingRuleRepository
{
    private readonly AppDbContext _db;

    public PricingRuleRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PricingRule>> ListOrderedForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.PricingRules.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public async Task<PricingRule?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.PricingRules.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, cancellationToken);

    public void Add(PricingRule rule) => _db.PricingRules.Add(rule);

    public void Update(PricingRule rule) => _db.PricingRules.Update(rule);

    public void Remove(PricingRule rule) => _db.PricingRules.Remove(rule);
}
