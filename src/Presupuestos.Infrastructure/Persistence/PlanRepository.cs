using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _db;

    public PlanRepository(AppDbContext db) => _db = db;

    public async Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var key = name.Trim();
        return await _db.Plans.AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Name.ToLower() == key.ToLower(),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Plans.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);
}
