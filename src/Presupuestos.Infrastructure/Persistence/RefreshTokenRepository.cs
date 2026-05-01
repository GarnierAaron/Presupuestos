using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenHash, cancellationToken);

    public void Add(RefreshToken token) => _db.RefreshTokens.Add(token);

    public void Remove(RefreshToken token) => _db.RefreshTokens.Remove(token);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _db.RefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RevokeAllForDeviceAsync(Guid deviceRecordId, CancellationToken cancellationToken = default)
    {
        await _db.RefreshTokens.Where(x => x.DeviceRecordId == deviceRecordId).ExecuteDeleteAsync(cancellationToken);
    }
}
