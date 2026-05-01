using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<User?> GetByIdGlobalAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAllForAdminAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithTenantForAdminAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByIdGlobalTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(int Total, int Active, int Inactive)> GetAdminUserStatsAsync(CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(User user);

    void Update(User user);
}
