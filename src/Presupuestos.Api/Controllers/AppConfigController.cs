using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.App;

namespace Presupuestos.Api.Controllers;

/// <summary>
/// Control remoto (kill switch) sin autenticación. Ruta: GET /api/app-config
/// </summary>
[ApiController]
[Route("api/app-config")]
[AllowAnonymous]
public class AppConfigController : ControllerBase
{
    private readonly IRemoteAppConfigService _config;

    public AppConfigController(IRemoteAppConfigService config) => _config = config;

    /// <summary>
    /// Versión del cliente: query <c>?version=1.2.3</c> o cabecera <c>X-App-Version</c>.
    /// </summary>
    /// <remarks>
    /// Ejemplo de respuesta (HTTP 200):
    /// <code>
    /// {
    ///   "id": "00000000-0000-4000-8000-000000000001",
    ///   "appEnabled": true,
    ///   "maintenanceMode": false,
    ///   "forceUpdate": false,
    ///   "blocked": false,
    ///   "clientVersion": "1.2.0",
    ///   "minimumVersion": "1.0.0",
    ///   "blockedVersions": ["1.0.5"],
    ///   "message": "",
    ///   "status": "Ok"
    /// }
    /// </code>
    /// Estados típicos de <c>status</c>: Ok, Disabled, Maintenance, VersionBlocked, ForceUpdateRequired.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? version, CancellationToken ct)
    {
        string? fromHeader = null;
        if (Request.Headers.TryGetValue(AppVersionHeader.Name, out var hv))
            fromHeader = hv.FirstOrDefault();

        var clientVersion = version ?? fromHeader;
        var dto = await _config.GetStatusAsync(clientVersion, ct);
        return Ok(dto);
    }
}
