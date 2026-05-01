using System.Security.Claims;

namespace Presupuestos.Application.Auth;

/// <summary>
/// El JWT usa el claim estándar <c>sub</c>; ASP.NET a veces lo expone como <see cref="ClaimTypes.NameIdentifier"/> y a veces no.
/// </summary>
public static class AuthClaims
{
    public const string JwtSubjectClaimType = "sub";

    public static string? FindUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(id))
            return id;
        return user.FindFirst(JwtSubjectClaimType)?.Value;
    }
}
