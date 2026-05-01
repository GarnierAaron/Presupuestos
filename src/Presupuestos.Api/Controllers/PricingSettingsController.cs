using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Dto.Pricing;
using Presupuestos.Application.Services;

namespace Presupuestos.Api.Controllers;

/// <summary>Activa el motor flexible por tenant. Sin JWT + tenant, no aplica.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PricingSettingsController : ControllerBase
{
    private readonly FlexiblePricingSettingsService _settings;

    public PricingSettingsController(FlexiblePricingSettingsService settings) => _settings = settings;

    [HttpGet]
    public async Task<ActionResult<FlexiblePricingSettingsDto>> Get(CancellationToken ct) =>
        Ok(await _settings.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] FlexiblePricingSettingsDto dto, CancellationToken ct)
    {
        await _settings.SetAsync(dto, ct);
        return NoContent();
    }
}
