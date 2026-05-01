using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);

    public async Task<User?> GetByIdGlobalAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> ListAllForAdminAsync(CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking()
            .Include(u => u.Tenant)
                .ThenInclude(t => t!.Subscriptions.Where(s => s.Status == SubscriptionStatuses.Active))
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

    public async Task<User?> GetByIdWithTenantForAdminAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking()
            .Include(u => u.Tenant)
                .ThenInclude(t => t!.Subscriptions.Where(s => s.Status == SubscriptionStatuses.Active))
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByIdGlobalTrackedAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<(int Total, int Active, int Inactive)> GetAdminUserStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var total = await _db.Users.CountAsync(cancellationToken);
        var active = await _db.Users.CountAsync(u => u.IsActive, cancellationToken);
        return (total, active, total - active);
    }

    public Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Users.Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.LastLogin, DateTimeOffset.UtcNow),
                cancellationToken);

    public void Add(User user) => _db.Users.Add(user);

    public void Update(User user) => _db.Users.Update(user);
}
