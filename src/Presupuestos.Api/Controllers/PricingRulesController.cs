using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Dto.Pricing;
using Presupuestos.Application.Services;

namespace Presupuestos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PricingRulesController : ControllerBase
{
    private readonly PricingRuleService _rules;

    public PricingRulesController(PricingRuleService rules) => _rules = rules;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PricingRuleDto>>> GetAll(CancellationToken ct) =>
        Ok(await _rules.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PricingRuleDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _rules.GetAsync(id, ct);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<PricingRuleDto>> Create([FromBody] CreatePricingRuleDto dto, CancellationToken ct)
    {
        var created = await _rules.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PricingRuleDto>> Update(Guid id, [FromBody] UpdatePricingRuleDto dto, CancellationToken ct)
    {
        var updated = await _rules.UpdateAsync(id, dto, ct);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _rules.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
