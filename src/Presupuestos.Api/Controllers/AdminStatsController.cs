using Microsoft.AspNetCore.Mvc;
using Presupuestos.Api.Authorization;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.Admin;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/admin/stats")]
[RequireSuperAdmin]
public class AdminStatsController : ControllerBase
{
    private readonly IAdminUserService _admin;

    public AdminStatsController(IAdminUserService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<AdminStatsDto>> Get(CancellationToken ct) =>
        Ok(await _admin.GetStatsAsync(ct));
}
