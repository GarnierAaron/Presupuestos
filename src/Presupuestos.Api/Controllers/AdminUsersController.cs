using Microsoft.AspNetCore.Mvc;
using Presupuestos.Api.Authorization;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Auth;
using Presupuestos.Application.Dto.Admin;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[RequireSuperAdmin]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _admin;

    public AdminUsersController(IAdminUserService admin) => _admin = admin;

    /// <summary>Lista todos los usuarios del sistema.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserListItemDto>>> List(CancellationToken ct) =>
        Ok(await _admin.ListUsersAsync(ct));

    /// <summary>Detalle de un usuario (tenant, rol, dispositivos registrados).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _admin.GetUserAsync(id, ct);
        return dto == null ? NotFound() : Ok(dto);
    }

    /// <summary>Activa o desactiva la cuenta. Un usuario inactivo recibe 403 en las APIs autenticadas.</summary>
    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<ActionResult<AdminToggleActiveResponseDto>> ToggleActive(Guid id, CancellationToken ct)
    {
        var sub = AuthClaims.FindUserId(User);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var actorId))
            return Unauthorized();

        var result = await _admin.ToggleActiveAsync(id, actorId, ct);
        if (result == null) return NotFound();
        return Ok(new AdminToggleActiveResponseDto { IsActive = result.Value });
    }
}
