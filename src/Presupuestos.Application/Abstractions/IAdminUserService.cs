using Presupuestos.Application.Dto.Admin;

namespace Presupuestos.Application.Abstractions;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserListItemDto>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool?> ToggleActiveAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);

    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
