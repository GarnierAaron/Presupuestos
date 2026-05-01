using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    void Add(RefreshToken token);
    void Remove(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllForDeviceAsync(Guid deviceRecordId, CancellationToken cancellationToken = default);
}
