using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _db;

    public ItemRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsInTenantAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        _db.Items.AnyAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public async Task<Item?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Items.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<Item>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Items.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public void Add(Item item) => _db.Items.Add(item);

    public void Update(Item item)
    {
        _db.Items.Update(item);
    }

    public void Remove(Item item) => _db.Items.Remove(item);
}
