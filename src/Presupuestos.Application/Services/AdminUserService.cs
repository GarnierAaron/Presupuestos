using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Common.Exceptions;
using Presupuestos.Application.Dto.Admin;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IDeviceRepository _devices;

    public AdminUserService(IUserRepository users, IUnitOfWork uow, IDeviceRepository devices)
    {
        _users = users;
        _uow = uow;
        _devices = devices;
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var list = await _users.ListAllForAdminAsync(cancellationToken);
        return list.Select(MapListItem).ToList();
    }

    public async Task<AdminUserDetailDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdWithTenantForAdminAsync(id, cancellationToken);
        if (user == null) return null;

        var deviceCount = await _devices.CountForUserAsync(user.Id, cancellationToken);
        return MapDetail(user, deviceCount);
    }

    public async Task<bool?> ToggleActiveAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (id == actorUserId)
            throw new AuthException("No puedes activar ni desactivar tu propia cuenta desde el panel.");

        var user = await _users.GetByIdGlobalTrackedAsync(id, cancellationToken);
        if (user == null) return null;

        user.IsActive = !user.IsActive;
        await _uow.SaveChangesAsync(cancellationToken);
        return user.IsActive;
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var (total, active, inactive) = await _users.GetAdminUserStatsAsync(cancellationToken);
        return new AdminStatsDto
        {
            TotalUsers = total,
            ActiveUsers = active,
            InactiveUsers = inactive
        };
    }

    private static AdminUserListItemDto MapListItem(User u) =>
        new()
        {
            Id = u.Id,
            Email = u.Email,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin,
            TenantId = u.TenantId,
            TenantName = u.Tenant?.Name,
            PlanName = u.Tenant?.Subscriptions.FirstOrDefault()?.PlanName,
            PlanEndDate = u.Tenant?.Subscriptions.FirstOrDefault()?.EndDate
        };

    private static AdminUserDetailDto MapDetail(User u, int deviceCount) =>
        new()
        {
            Id = u.Id,
            Email = u.Email,
            IsActive = u.IsActive,
            IsSuperAdmin = u.IsSuperAdmin,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin,
            ExpirationDate = u.ExpirationDate,
            TenantId = u.TenantId,
            TenantName = u.Tenant?.Name,
            PlanName = u.Tenant?.Subscriptions.FirstOrDefault()?.PlanName,
            PlanEndDate = u.Tenant?.Subscriptions.FirstOrDefault()?.EndDate,
            DeviceCount = deviceCount
        };
}
