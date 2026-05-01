using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Dto.Admin;

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? PlanName { get; set; }
    public DateTime? PlanEndDate { get; set; }
}

public class AdminUserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? PlanName { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public int DeviceCount { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}

public class AdminToggleActiveResponseDto
{
    public bool IsActive { get; set; }
}
