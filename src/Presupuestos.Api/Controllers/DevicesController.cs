using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;
using Presupuestos.Application.Dto.Devices;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _devices;

    public DevicesController(IDeviceService devices) => _devices = devices;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeviceDto>>> List(CancellationToken ct)
    {
        var sub = AuthClaims.FindUserId(User);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();
        return Ok(await _devices.ListForUserAsync(userId, ct));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var sub = AuthClaims.FindUserId(User);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();
        await _devices.DeactivateAsync(userId, id, ct);
        return NoContent();
    }

    /// <summary>Revoca refresh tokens emitidos para ese dispositivo (sesión cerrada en ese terminal).</summary>
    [HttpPost("{id:guid}/force-logout")]
    public async Task<IActionResult> ForceLogout(Guid id, CancellationToken ct)
    {
        var sub = AuthClaims.FindUserId(User);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();
        await _devices.ForceLogoutDeviceAsync(userId, id, ct);
        return NoContent();
    }
}
