using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;
using Presupuestos.Application.Dto.Auth;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct) =>
        Ok(await _auth.RegisterAsync(dto, ct));

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct) =>
        Ok(await _auth.LoginAsync(dto, ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct) =>
        Ok(await _auth.RefreshAsync(dto, ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        await _auth.LogoutAsync(dto, ct);
        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var sub = AuthClaims.FindUserId(User);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();
        await _auth.LogoutAllAsync(userId, ct);
        return NoContent();
    }
}
