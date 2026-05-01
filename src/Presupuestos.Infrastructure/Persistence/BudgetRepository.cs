using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;

    public BudgetRepository(AppDbContext db) => _db = db;

    public async Task<Budget?> GetByIdWithDetailsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Budgets
            .Include(b => b.Details).ThenInclude(d => d.Service)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<Budget>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Budgets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.Details).ThenInclude(d => d.Service)
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Budget budget) => _db.Budgets.Add(budget);

    public void Update(Budget budget) => _db.Budgets.Update(budget);

    public void Remove(Budget budget) => _db.Budgets.Remove(budget);
}
