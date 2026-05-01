using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _db;

    public ServiceRepository(AppDbContext db) => _db = db;

    public async Task<Service?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Services.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

    public Task DeleteItemsForServiceAsync(Guid serviceId, Guid tenantId, CancellationToken cancellationToken = default) =>
        _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DELETE FROM ServiceItems
             WHERE ServiceId = {serviceId}
               AND EXISTS (
                 SELECT 1 FROM Services
                 WHERE Id = {serviceId} AND TenantId = {tenantId})
             """,
            cancellationToken);

    public async Task<bool> TryReplaceServiceContentAsync(
        Guid serviceId,
        Guid tenantId,
        string name,
        decimal? basePrice,
        decimal? marginPercent,
        IReadOnlyList<(Guid ItemId, decimal QuantityUsed)> lines,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var rows = await _db.Services
            .Where(s => s.Id == serviceId && s.TenantId == tenantId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(s => s.Name, name)
                    .SetProperty(s => s.BasePrice, basePrice)
                    .SetProperty(s => s.MarginPercent, marginPercent),
                cancellationToken);

        if (rows == 0)
        {
            await tx.RollbackAsync(cancellationToken);
            return false;
        }

        await DeleteItemsForServiceAsync(serviceId, tenantId, cancellationToken);

        foreach (var line in lines)
        {
            _db.ServiceItems.Add(new ServiceItem
            {
                Id = Guid.NewGuid(),
                ServiceId = serviceId,
                ItemId = line.ItemId,
                QuantityUsed = line.QuantityUsed
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<Service?> GetByIdWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Services
            .Include(s => s.ServiceItems).ThenInclude(si => si.Item)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<Service>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Services
            .AsNoTracking()
            .Include(s => s.ServiceItems).ThenInclude(si => si.Item)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public void Add(Service service) => _db.Services.Add(service);

    public void Update(Service service) => _db.Services.Update(service);

    public void Remove(Service service) => _db.Services.Remove(service);
}
