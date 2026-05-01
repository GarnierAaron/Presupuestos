using Microsoft.AspNetCore.Http;
using Presupuestos.Application.Abstractions;

namespace Presupuestos.Infrastructure.MultiTenancy;

public class HttpTenantContext : ITenantContext
{
    public const string TenantHeader = "X-Tenant-Id";
    public const string UserHeader = "X-User-Id";

    private static readonly object StateKey = typeof(HttpTenantContext);

    private readonly IHttpContextAccessor _http;

    public HttpTenantContext(IHttpContextAccessor http) => _http = http;

    public Guid? TenantId
    {
        get
        {
            var state = GetState();
            return state?.TenantId;
        }
    }

    public Guid? UserId
    {
        get
        {
            var state = GetState();
            return state?.UserId;
        }
    }

    public static void SetContext(HttpContext http, Guid? tenantId, Guid? userId) =>
        http.Items[StateKey] = new TenantResolutionState(tenantId, userId);

    private TenantResolutionState? GetState()
    {
        var http = _http.HttpContext;
        if (http == null) return null;
        return http.Items.TryGetValue(StateKey, out var v) ? v as TenantResolutionState : null;
    }
}
