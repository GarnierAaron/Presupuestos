using Microsoft.AspNetCore.Authorization;
using Presupuestos.Application.Auth;

namespace Presupuestos.Api.Authorization;

/// <summary>Requiere JWT con claim <see cref="AuthConstants.SuperAdminClaim"/> = true.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSuperAdminAttribute : AuthorizeAttribute
{
    public RequireSuperAdminAttribute() => Policy = AuthPolicies.SuperAdmin;
}
